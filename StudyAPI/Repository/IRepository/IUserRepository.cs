using StudyAPI.DTOs;
using StudyAPI.Models;

namespace StudyAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<UserDTO> Register(RegistrationRequestDTO registerRequestDTO);

    }
}
