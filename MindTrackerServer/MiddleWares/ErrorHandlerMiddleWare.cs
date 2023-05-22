using System.Net.Mime;
using Domain.Exceptions;

namespace MindTrackerServer.MiddleWares
{
    public class ErrorHandlerMiddleWare
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                ILogger logger = context.RequestServices.GetService<ILogger<ErrorHandlerMiddleWare>>()!;

                response.ContentType = MediaTypeNames.Text.Plain;

                context.Response.StatusCode = error switch
                {
                    AccountNotFoundException => StatusCodes.Status404NotFound,
                    AccountAlreadyExistsException => StatusCodes.Status409Conflict,
                    AccountRefreshTokenException => StatusCodes.Status400BadRequest,
                    InvalidAccountException => StatusCodes.Status400BadRequest,
                    DeleteMoodMarkException => StatusCodes.Status500InternalServerError,
                    UpdateMoodMarkException => StatusCodes.Status500InternalServerError,
                    MoodMarkNotFoundException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError,
                };

                logger.LogError(error.Message);
                await response.WriteAsync(error.Message);
            }
        }
    }
}
