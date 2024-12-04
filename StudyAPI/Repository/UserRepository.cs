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
    public class UserRepository : Repository<Villa>,  IUserRepository
    {
        private readonly VillaDbContext _db;
        private readonly UserManager<VillaUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IMapper _mapper;

        public UserRepository(VillaDbContext db, IConfiguration configuration, UserManager<VillaUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager) : base(db)
        {
            _db = db;
            secretKey = configuration.GetSection("ApiSettings:Secret").Value;
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.VillaUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null)
                return true;

            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await _db.VillaUsers.FirstOrDefaultAsync(x => x.UserName.ToLower() == loginRequestDTO.UserName.ToLower() );

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            
            if (user == null || !isValid)
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null,
                };
            }



            var roles = await _userManager.GetRolesAsync(user);
            var token = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenObj = token.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                Token = token.WriteToken(tokenObj), //serializado
                User = _mapper.Map<UserDTO>(user),
                Role = roles.FirstOrDefault()
            };
            return loginResponseDTO;
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
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User")); //This  is an awful pratice, you need to seed and define the roles before
                    }
                    await _userManager.AddToRoleAsync(user, "User");
                    var userToReturn = _db.VillaUsers.FirstOrDefault(x => x.UserName == user.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return new UserDTO();
        }
    }
}
