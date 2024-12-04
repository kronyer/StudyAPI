using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyAPI.Data;
using StudyAPI.DTOs;
using StudyAPI.Models;
using StudyAPI.Repository.IRepository;
using System.ComponentModel.DataAnnotations;
using System.Net;

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
                var tokenDTO = await _dbUser.Login(model);
                if (tokenDTO == null || tokenDTO.AccessToken == "")
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Invalid username or password");
                    return BadRequest(_apiResponse);
                }
                _apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
                _apiResponse.Response = tokenDTO;
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
                var passwordRegex = new RegularExpressionAttribute(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$");
                if (!passwordRegex.IsValid(model.Password))
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Invalid password format. Password must be at least 6 characters long and contain at least one uppercase letter, one number, and one special character.");
                    return BadRequest(_apiResponse);
                }

                bool isUserNameUnique = _dbUser.IsUniqueUser(model.UserName);
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

        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDTOResponse = await _dbUser.RefreshAccessToken(tokenDTO);
                if (tokenDTOResponse == null || string.IsNullOrEmpty(tokenDTOResponse.AccessToken))
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Token Invalid");
                    return BadRequest(_apiResponse);
                }
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                _apiResponse.Response = tokenDTOResponse;
                return Ok(_apiResponse);
            }
            else
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Response = "Invalid Input";
                return BadRequest(_apiResponse);
            }

        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
        {

            if (ModelState.IsValid)
            {
                await _dbUser.RevokeRefreshToken(tokenDTO);
                _apiResponse.IsSuccess = true;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);

            }
            _apiResponse.IsSuccess = false;
            _apiResponse.Response = "Invalid Input";
            return BadRequest(_apiResponse);
        }
    }
}
