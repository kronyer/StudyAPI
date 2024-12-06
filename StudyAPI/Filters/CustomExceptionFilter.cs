using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudyAPI.Filters
{
    public class CustomExceptionFilter : IActionFilter //Inherits from IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context) // In this case I can filter for the exeptions i want to personalize
        {
            if (context.Exception is FileNotFoundException filenotFound)
            {
                context.Result = new ObjectResult("File not found")
                {
                    StatusCode = 503
                };
                context.ExceptionHandled = true; // This prevents this method to be overriden by the default exception handler
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
