using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace StudyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // It doesn't need to be authenticated
    [ApiVersionNeutral]
    [ApiExplorerSettings(IgnoreApi = true)] // It won't be shown in the Swagger
    public class ErrorHandlingController : ControllerBase
    {
        [Route("ProcessError")]
        public IActionResult ProcessError([FromServices] IHostEnvironment hostEnvironment)
        {

            if (hostEnvironment.IsDevelopment())
            {
                var feat = HttpContext.Features.Get<IExceptionHandlerFeature>();
                return Problem(
            detail: feat.Error.InnerException != null ? feat.Error.InnerException.StackTrace : feat.Error.StackTrace,
                    title: feat.Error.Message,
                    instance: hostEnvironment.EnvironmentName
                    );
            }
            else
            {
                return Problem();

            }
        }
    }
}
