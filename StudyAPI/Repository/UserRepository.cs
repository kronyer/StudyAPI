using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudyAPI.Data;
using StudyAPI.DTOs;
using StudyAPI.Models;
using StudyAPI.Repository.IRepository;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace StudyAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly VillaDbContext _db;
        private readonly UserManager<VillaUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IMapper _mapper;

        public UserRepository(VillaDbContext db, IConfiguration configuration,
            UserManager<VillaUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
        }

        public bool IsUniqueUser(string username)
        {
            if (_db.Users == null)
            {
                throw new InvalidOperationException("Database context is not initialized.");
            }

            var user = _db.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }


        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.Users
                .FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);


            if (user == null || isValid == false)
            {
                return new TokenDTO()
                {
                    AccessToken = "",
                };
            }
            var jwtTokenId = $"JTI{Guid.NewGuid()}";

            var token = await GetAccessToken(user, jwtTokenId);

            var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);

            TokenDTO tokenDto = new TokenDTO()
            {
                AccessToken = token,
                RefreshToken = refreshToken

            };
            return tokenDto;
        }

        public async Task<UserDTO> Register(RegistrationRequestDTO registerRequestDTO)
        {
            VillaUser user = new()
            {
                UserName = registerRequestDTO.UserName,
                Email = registerRequestDTO.UserName,
                NormalizedEmail = registerRequestDTO.UserName.ToUpper(),
                Name = registerRequestDTO.Name,
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerRequestDTO.Password);
                if (result.Succeeded)
                {
                    try
                    {
                        if (!await _roleManager.RoleExistsAsync(registerRequestDTO.Role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(registerRequestDTO.Role)); //This is not the best scenario, there should be a crud for role
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as needed
                        throw new Exception("Error creating role", ex);
                    }

                    try
                    {
                        await _userManager.AddToRoleAsync(user, registerRequestDTO.Role);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as needed
                        throw new Exception("Error adding user to role", ex);
                    }

                    try
                    {
                        var userToReturn = _db.Users.FirstOrDefault(u => u.UserName == registerRequestDTO.UserName);
                        return _mapper.Map<UserDTO>(userToReturn);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as needed
                        throw new Exception("Error retrieving user", ex);
                    }
                }
                else
                {
                    // Handle the case where user creation failed
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"User creation failed: {errors}");
                }
            }
            catch (Exception e)
            {
                // Log the exception or handle it as needed
                throw new Exception("Error during registration", e);
            }

            return new UserDTO();
        }



        public async Task<string> GetAccessToken(VillaUser user, string jwtTokenId)
        {
            //if user was found generate JWT Token
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                                        new Claim(JwtRegisteredClaimNames.Aud, "tedas.com")
                }),
                Expires = DateTime.UtcNow.AddMinutes(1), //1 minute for testing
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
        {
            // Find an existing refresh token
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(u => u.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null)
            {
                return new TokenDTO();
            }

            // Compare data from existing refresh and access token provided and if there is any missmatch then consider it as a fraud
            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            // When someone tries to use not valid refresh token, fraud possible
            if (!existingRefreshToken.IsValid)
            {
                await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            }
            // If just expired then mark as invalid and return empty
            if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            // replace old refresh with a new one with updated expire date
            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);


            // revoke existing refresh token
            await MarkTokenAsInvalid(existingRefreshToken);

            // generate new access token
            var applicationUser = _db.Users.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
            if (applicationUser == null)
                return new TokenDTO();

            var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

            return new TokenDTO()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };

        }

        private async Task<string> CreateNewRefreshToken(string userId, string jwtTokenId)
        {
            var refreshToken = new RefreshToken()
            {
                UserId = userId,
                JwtTokenId = jwtTokenId,
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
                IsValid = true,
                ExpiresAt = DateTime.UtcNow.AddMinutes(1)
            };
            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }
        public Task RevokeRefreshToken(TokenDTO tokenDTO)
        {
            throw new NotImplementedException();
        }

        private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
                return userId == expectedUserId && jwtTokenId == expectedTokenId;

            }
            catch
            {
                return false;
            }
        }


        private async Task MarkAllTokenInChainAsInvalid(string userId, string tokenId)
        {
            await _db.RefreshTokens.Where(u => u.UserId == userId
               && u.JwtTokenId == tokenId)
                   .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false));

        }


        private Task MarkTokenAsInvalid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
            return _db.SaveChangesAsync();
        }
    }
}
