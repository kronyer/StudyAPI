using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace StudyAPI.Middlewares
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public CustomExceptionMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _requestDelegate(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            
                    if (ex is BadImageFormatException badImageExp)
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            StatusCodeContext = 404,
                            ErrorMessage = "This is a custom logic",
                            StackTrace = ex.StackTrace,
                        }));
                    }
                    else
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            StatusCodeContext = context.Response.StatusCode,
                            ErrorMessage = ex.Message,
                            StackTrace = ex.StackTrace,
                        }));
                    }

        }
    }
}
