using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace StudyAPI.Extensions
{
    public class CustomExceptionExtensions
    {
        public static void HandleError(IApplicationBuilder app)
        {
            app.UseExceptionHandler(err =>
            {
                err.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        if (app.Environment.IsDevelopment())
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                            {
                                StatusCodeContext = context.Response.StatusCode,
                                ErrorMessage = contextFeature.Error.Message,
                                StackTrace = contextFeature.Error.StackTrace,
                            }));
                        }
                        else
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                            {
                                StatusCodeContext = context.Response.StatusCode,
                                ErrorMessage = "An error occurred. Try again later.",
                            }));
                        }
                    }
                });
            });
        }
    }
}
