using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyAPI.Data;
using StudyAPI.DTOs;
using StudyAPI.Models;
using StudyAPI.Repository.IRepository;

namespace StudyAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
public class VillaNumberAPIController : ControllerBase
{
    protected APIResponse _apiResponse;
    private readonly IMapper _mapper;
    private readonly IVillaNumberRepository _dbVillaNo;
    private readonly IVillaRepository _dbVilla;

    public VillaNumberAPIController(IVillaNumberRepository dbVillaNo, IMapper mapper, IVillaRepository villa)
    {
        _dbVillaNo = dbVillaNo;
        _mapper = mapper;
        _dbVilla = villa;
        _apiResponse = new APIResponse(); // Pode ser nos moldes de DI
    }


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<APIResponse>> GetAllVillasNo()
    {
        try
        {
            IEnumerable<VillaNumber> villasNo = await _dbVillaNo.GetAllAsync();
            _apiResponse.Response = _mapper.Map<IEnumerable<VillaNumberDTO>>(villasNo);
            _apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
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
            return _apiResponse;
        }

    }

    [HttpGet("{id:int}", Name = "GetVillaNo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            VillaNumber villa = await _dbVillaNo.GetAsync(i => i.VillaNo == id);
            if (villa == null)
            {
                _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;

                return BadRequest(_apiResponse);
            }

            _apiResponse.Response = _mapper.Map<VillaNumberDTO>(villa);
            _apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
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
            return _apiResponse;

        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> CreateVillaNo([FromBody] VillaNumberCreateDTO villaDto)
    {
        try
        {
            if (await _dbVillaNo.GetAsync(i => i.VillaNo == villaDto.VillaNo) != null)
            {
                ModelState.AddModelError("", "Villa Number already exists");
                return BadRequest(ModelState);
            }

            if (await _dbVilla.GetAsync(i => i.Id == villaDto.VillaId) == null)
            {
                ModelState.AddModelError("", "Villa does not exist");
                return BadRequest(ModelState);
            }

            if (villaDto == null)
            {
                _apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                return BadRequest(_apiResponse);
            }

            VillaNumber model = _mapper.Map<VillaNumber>(villaDto);

            await _dbVillaNo.CreateAsync(model); // precisa ser mapeado para o modelo
                                                 //O id aqui vai estar disponivel graças ao tracking do EF no model
            _apiResponse.Response = _mapper.Map<VillaNumberDTO>(model);
            _apiResponse.StatusCode = System.Net.HttpStatusCode.Created;
            return CreatedAtRoute("GetVillaNo", new { id = model.VillaNo }, _apiResponse);
            //return CreatedAtAction(nameof(GetVilla), new { id = villa.Id }, villa);
        }
        catch (Exception ex)
        {
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
    public async Task<ActionResult<APIResponse>> DeleteVillaNo(int id)
    {

        try
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var villaNo = await _dbVillaNo.GetAsync(i => i.VillaNo == id);
            if (villaNo == null)
            {
                return BadRequest();
            }
            await _dbVillaNo.DeleteAsync(villaNo); //Nao existe removeAsync
            _apiResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
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
            return _apiResponse;
        }
    }

    [HttpPut("{id:int}", Name = "UpdateVillaNo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaNumberUpdateDTO villaDto)
    {
        try
        {
            if (id == 0)
            {
                return BadRequest();
            }
            if (villaDto == null || villaDto.VillaNo != id)
            {
                return BadRequest(villaDto);
            }
            if (await _dbVilla.GetAsync(i => i.Id == villaDto.VillaId) == null)
            {
                ModelState.AddModelError("", "Villa does not exist");
                return BadRequest(ModelState);
            }

            VillaNumber model = _mapper.Map<VillaNumber>(villaDto);


            await _dbVillaNo.UpdateAsync(model); //Update nao tem async
            _apiResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
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
            return _apiResponse;
        }
    }

    [HttpPatch("{id:int}", Name = "UpdatePartialVillaNo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdatePartialVillaNo(int id, JsonPatchDocument<VillaNumberUpdateDTO> patchDto)
    {
        try
        {
            if (patchDto == null || id == 0)
            {
                return BadRequest();
            }
            var villa = await _dbVillaNo.GetAsync(i => i.VillaNo == id, tracked: false);
            // EntityFramework dá tracking no modelo e se confunde com outro modelo que está sendo alterado
            // Por isso se adiciona o AsNoTracking para não dar tracking no modelo
            if (villa == null)
            {
                return BadRequest(villa);
            }

            //villa.Name = "poxa vida";
            //_context.SaveChanges();

            //isso aqui tambén funciona legal

            VillaNumberUpdateDTO villaUpdateDTO = _mapper.Map<VillaNumberUpdateDTO>(villa);



            patchDto.ApplyTo(villaUpdateDTO, ModelState);

            VillaNumber model = _mapper.Map<VillaNumber>(villaUpdateDTO);

            await _dbVillaNo.UpdateAsync(model);

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
            return StatusCode(500, "Internal Server Error");

        }
    }
}
