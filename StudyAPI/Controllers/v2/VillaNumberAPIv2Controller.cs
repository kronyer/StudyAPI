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
[ApiVersion("2.0")] //isso nao é muito clean
public class VillaNumberAPIv2Controller : ControllerBase
{
    protected APIResponse _apiResponse;
    private readonly IMapper _mapper;
    private readonly IVillaNumberRepository _dbVillaNo;
    private readonly IVillaRepository _dbVilla;

    public VillaNumberAPIv2Controller(IVillaNumberRepository dbVillaNo, IMapper mapper, IVillaRepository villa)
    {
        _dbVillaNo = dbVillaNo;
        _mapper = mapper;
        _dbVilla = villa;
        _apiResponse = new APIResponse(); // Pode ser nos moldes de DI
    }

    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }


}
