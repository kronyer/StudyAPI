using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using StudyAPI.DTOs;

namespace StudyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<VillaDTO>> GetAllVillas()
        {
            return Ok(new List<VillaDTO>
            {
                new VillaDTO { Id = 1, Name = "Villa 1" },
                new VillaDTO { Id = 2, Name = "Villa 2" }
            });
        }

        [HttpGet("{id:int}", Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //Cadastrar os tipos de respostas que a API pode retornar
        public ActionResult<VillaDTO> GetVilla(int id)
        {

            if (id == 0)
            {
                return BadRequest();
            }

            return Ok(new VillaDTO { Id = id, Name = $"Villa {id}" });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villa)
        {
            if (villa.Name.ToLower() == "Pedro".ToLower()) //Custom validation
            {
                ModelState.AddModelError("PedroName", "Name cannot be Pedro");
            }

            if (!ModelState.IsValid) //Isso é para custom validations ou controle, o [ApiController] já busca as data annotations
            {
                return BadRequest(ModelState);
            }
            if (villa == null)
            {
                return BadRequest(villa);
            }
            return CreatedAtRoute("GetVilla", new { id = villa.Id }, villa); //Retorna uma location de onde acessar o recurso criado, no caso um get
            //return CreatedAtAction(nameof(GetVilla), new { id = villa.Id }, villa);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult DeleteVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            return NoContent(); //Nao se retorna nada no delete, geralmente
        }

        [HttpPut("{id:int}", Name ="UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult UpdateVilla(int id, [FromBody] VillaDTO villa)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            if (villa == null)
            {
                return BadRequest(villa);
            }
            return NoContent(); //Para update também se usa o NoContent de retorno!
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> villa)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            if (villa == null)
            {
                return BadRequest(villa);
            }


            return NoContent(); //Para update também se usa o NoContent de retorno!
        }
    }
}
