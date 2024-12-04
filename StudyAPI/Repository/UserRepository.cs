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
            if (_db.VillaUsers == null)
            {
                throw new InvalidOperationException("Database context is not initialized.");
            }

            var user = _db.VillaUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }


        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.VillaUsers
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

        public async Task<UserDTO> Register(RegistrationRequestDTO registerationRequestDTO)
        {
            VillaUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Email = registerationRequestDTO.UserName,
                NormalizedEmail = registerationRequestDTO.UserName.ToUpper(),
                Name = registerationRequestDTO.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(registerationRequestDTO.Role).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role)); //This is not the best scenario, there should be a crud for role
                    }
                    await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                    var userToReturn = _db.VillaUsers
                        .FirstOrDefault(u => u.UserName == registerationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);

                }
            }
            catch (Exception e)
            {

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
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDto)
        {
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Refresh_Token == tokenDto.RefreshToken);
            if (existingRefreshToken == null)
            {
                return new TokenDTO();
            }

            var accessTokenData = GetAccessTokenData(tokenDto.AccessToken);
            if (!accessTokenData.isSuccessful || accessTokenData.userId != existingRefreshToken.UserId || accessTokenData.tokenId != existingRefreshToken.JwtTokenId)
            {
                existingRefreshToken.IsValid = false;
                await _db.SaveChangesAsync();
            }


            if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                existingRefreshToken.IsValid = false;
                await _db.SaveChangesAsync();
            }

            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

            existingRefreshToken.IsValid = false;
            await _db.SaveChangesAsync();

            var villaUser = _db.VillaUsers.FirstOrDefault(x => x.Id == existingRefreshToken.UserId);

            if (villaUser == null)
            {
                return new TokenDTO();
            }

            var newAccessToken = await GetAccessToken(villaUser, existingRefreshToken.JwtTokenId);

            return new TokenDTO()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };  
        }

        private async Task<string> CreateNewRefreshToken(string userId, string jwtTokenId)
        {
            var refreshToken = new RefreshToken()
            {
                UserId = userId,
                JwtTokenId = jwtTokenId,
                Refresh_Token = Guid.NewGuid + "-" + Guid.NewGuid(),
                IsValid = true,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }

        private (bool isSuccessful, string userId, string tokenId) GetAccessTokenData(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var userId = jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
                var tokenId = jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;

                return (true, userId, tokenId);
            }
            catch (Exception e)
            {
                return (false, "", "");
            }
        }
    }
}
