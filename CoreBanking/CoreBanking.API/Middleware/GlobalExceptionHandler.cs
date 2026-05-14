using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.API.Middleware
{
    /// <summary>
    /// Global exception handler that catches unhandled exceptions
    /// and returns a consistent ProblemDetails JSON response.
    /// </summary>
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = httpContext.RequestServices
                    .GetRequiredService<IHostEnvironment>()
                    .IsDevelopment()
                        ? exception.Message
                        : "An internal error occurred. Please try again later.",
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
