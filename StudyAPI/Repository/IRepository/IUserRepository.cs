using StudyAPI.DTOs;
using StudyAPI.Models;

namespace StudyAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<UserDTO> Register(RegistrationRequestDTO registerRequestDTO);
        Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDto);
        Task RevokeRefreshToken(TokenDTO tokenDTO);

    }
}
