using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace StudyAPI.Extensions
{
    public static class CustomExceptionExtensions// must be static class
    {
        public static void HandleError(this IApplicationBuilder app, bool isDev)
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
                        if (isDev)
                        {
                            if (contextFeature.Error is BadImageFormatException badImageExp)
                            {
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                                {
                                    StatusCodeContext = 404,
                                    ErrorMessage = "This is a custom logic",
                                    StackTrace = contextFeature.Error.StackTrace,
                                }));
                            }
                            else
                            {
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                                {
                                    StatusCodeContext = context.Response.StatusCode,
                                    ErrorMessage = contextFeature.Error.Message,
                                    StackTrace = contextFeature.Error.StackTrace,
                                }));
                            }
                           
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
