using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyAPI.Data;
using StudyAPI.DTOs;
using StudyAPI.Migrations;
using StudyAPI.Models;
using StudyAPI.Repository.IRepository;
using System.Net;
using System.Text.Json;

namespace StudyAPI.Controllers.v2
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    public class VillaAPIController : ControllerBase
    {
        public ILogger<VillaAPIController> _logger { get; }
        protected APIResponse _apiResponse;
        private readonly IMapper _mapper;
        private readonly IVillaRepository _dbVilla;

        public VillaAPIController(ILogger<VillaAPIController> logger, IVillaRepository dbVilla, IMapper mapper)
        {
            _logger = logger;
            _dbVilla = dbVilla;
            _mapper = mapper;
            this._apiResponse = new APIResponse(); // Pode ser nos moldes de DI
        }


        [HttpGet]
        [Authorize] //metodo autorizado
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(CacheProfileName = "120SecondsDuration")] //isso só funciona no cliente!

        public async Task<ActionResult<APIResponse>> GetAllVillas([FromQuery(Name ="filterOccupancy")] int? occupancy, [FromQuery] string? search, int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                _logger.LogInformation("GetAllVillas was called");

                IEnumerable<Villa> villas = await _dbVilla.GetAllAsync();

                if (occupancy > 0)
                {
                    villas = await _dbVilla.GetAllAsync(u => u.Occupancy == occupancy, pageSize:pageSize, pageNumber:pageNumber); // the filter occurs in the repository, so that's a good pratice
                }
                else
                {
                    villas = await _dbVilla.GetAllAsync(pageSize: pageSize, pageNumber: pageNumber);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    villas = villas.Where(u => u.Amenity.ToLower().Contains(search) || u.Name.ToLower().Contains(search)); // Now here i dont think it's a good pratice to filter
                }

                Page page = new () { PageSize = pageSize, PageNumber = pageNumber,}; // The return should be Page<T>

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(page));

                _apiResponse.Response = _mapper.Map<IEnumerable<VillaDTO>>(villas);
                _apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllVillas was called");
                _apiResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
                return _apiResponse;
            }

        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        //Cadastrar os tipos de respostas que a API pode retornar
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse);

                }

                Villa villa = await _dbVilla.GetAsync(i => i.Id == id);
                if (villa == null)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;

                    return BadRequest(_apiResponse);
                }

                _apiResponse.Response = _mapper.Map<VillaDTO>(villa);
                _apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetVilla was called");
                _apiResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
                return _apiResponse;

            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles ="adm")] //metodo autorizado com ROLE!, precisa do UseAuthentication() na program
        public async Task<ActionResult<APIResponse>> CreateVilla([FromForm] VillaCreateDTO villaDto)
        {
            try
            {
                if (await _dbVilla.GetAsync(i => i.Name == villaDto.Name) != null || !ModelState.IsValid) //Custom validation
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse);
                }

                if (villaDto == null)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse);
                }

                Villa villa = _mapper.Map<Villa>(villaDto);


                await _dbVilla.CreateAsync(villa); // precisa ser mapeado para o modelo
                                                   //O id aqui vai estar disponivel graças ao tracking do EF no model

                if(villaDto.Image != null)
                {
                    string fileName = villa.Id.ToString() + Path.GetExtension(villaDto.Image.FileName);
                    string folder = @"wwwroot\ProductImage\" + fileName;

                    var directory = Path.Combine(Directory.GetCurrentDirectory(), folder);

                    FileInfo file = new FileInfo(directory);

                    if (file.Exists)
                    {
                        file.Directory.Delete();
                    }

                    using (var fileStream = new FileStream(directory, FileMode.Create))
                    {
                        await villaDto.Image.CopyToAsync(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    villaDto.ImageUrl = baseUrl + "/ProductImage/" + fileName;
                    villaDto.ImageLocalPath= baseUrl + "/ProductImage/" + fileName;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }



                await _dbVilla.UpdateAsync(villa);
                _apiResponse.Response = _mapper.Map<VillaDTO>(villa);
                _apiResponse.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateVilla was called");
                _apiResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
                return _apiResponse;
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {

            try
            {
                if (id == 0)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse);
                }
                var villa = await _dbVilla.GetAsync(i => i.Id == id);
                if (villa == null)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse);
                }
                await _dbVilla.DeleteAsync(villa); //Nao existe removeAsync
                _apiResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteVilla was called");
                _apiResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
                return _apiResponse;
            }
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromForm] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
                    return BadRequest();
                }

                Villa model = _mapper.Map<Villa>(updateDTO);
                if (updateDTO.Image != null)
                {
                    if (!string.IsNullOrEmpty(model.ImageLocalPath))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), model.ImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);

                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string fileName = updateDTO.Id + Path.GetExtension(updateDTO.Image.FileName);
                    string filePath = @"wwwroot\ProductImage\" + fileName;

                    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using (var fileStream = new FileStream(directoryLocation, FileMode.Create))
                    {
                        updateDTO.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    model.ImageUrl = baseUrl + "/ProductImage/" + fileName;
                    model.ImageLocalPath = filePath;

                }
                else
                {
                    model.ImageUrl = "https://placehold.co/600x400";
                }


                await _dbVilla.UpdateAsync(model);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _apiResponse;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDto)
        {
            try
            {
                if (patchDto == null || id == 0)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse);
                }
                var villa = await _dbVilla.GetAsync(i => i.Id == id, tracked: false);
                // EntityFramework dá tracking no modelo e se confunde com outro modelo que está sendo alterado
                // Por isso se adiciona o AsNoTracking para não dar tracking no modelo
                if (villa == null)
                {
                    return BadRequest(villa);
                }

                //villa.Name = "poxa vida";
                //_context.SaveChanges();

                //isso aqui tambén funciona legal

                VillaUpdateDTO villaUpdateDTO = _mapper.Map<VillaUpdateDTO>(villa);



                patchDto.ApplyTo(villaUpdateDTO, ModelState);

                Villa model = _mapper.Map<Villa>(villaUpdateDTO);

                await _dbVilla.UpdateAsync(model);

                if (!ModelState.IsValid)
                {
                    _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse);

                }

                return NoContent(); //Para update também se usa o NoContent de retorno!
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePartialVilla was called");
                return StatusCode(500, "Internal Server Error");

            }
        }
    }
}
