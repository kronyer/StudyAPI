using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyAPI.Data;
using StudyAPI.DTOs;
using StudyAPI.Models;
using StudyAPI.Repository.IRepository;

namespace StudyAPI.Controllers
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersionNeutral] //é basico, está em qualquer tipo de versão
    public class UsersController : ControllerBase
    {
        protected APIResponse _apiResponse;
        private readonly IMapper _mapper;
        private readonly IUserRepository _dbUser;

        public UsersController(IUserRepository dbUser, IMapper mapper)
        {
            _dbUser = dbUser;
            _mapper = mapper;
            this._apiResponse = new APIResponse(); // Pode ser nos moldes de DI
        }


        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            try
            {
                var loginResponse = await _dbUser.Login(model);
                if (loginResponse.User == null || loginResponse.Token == "")
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Invalid username or password");
                    return BadRequest(_apiResponse);
                }
                _apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
                _apiResponse.Response = loginResponse;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);

            }
            catch (Exception ex)
            {
                _apiResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                    {
                        ex.ToString()
                    };
                return StatusCode(StatusCodes.Status500InternalServerError, _apiResponse);
            }

        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
        {
            try
            {
                bool isUserNameUnique = _dbUser.IsUniqueUser(model.Username);
                if (!isUserNameUnique)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("User already exists");
                    return BadRequest(_apiResponse);
                }

                var user = await _dbUser.Register(model);

                if (user == null)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Registration failed");
                    return BadRequest(_apiResponse);
                }

                _apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);

            }
            catch (Exception ex)
            {
                _apiResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                    {
                        ex.ToString()
                    };
                return StatusCode(StatusCodes.Status500InternalServerError, _apiResponse);

            }
        }
    }
}
