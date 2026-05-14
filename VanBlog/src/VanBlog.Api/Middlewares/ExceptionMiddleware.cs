using System.Net;
using VanBlog.Core.Exceptions;
using VanBlog.Core.Models.Common;

namespace VanBlog.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.ContentType = "application/json";

                HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
                string message = "Internal Server Error";
                string error = ex.GetType().Name;

                switch (ex)
                {
                    case NotFoundException:
                        context.Response.StatusCode = 404;
                        statusCode = HttpStatusCode.NotFound;
                        message = ex.Message;
                        break;

                    case BadRequestException:
                        context.Response.StatusCode = 400;
                        statusCode = HttpStatusCode.BadRequest;
                        message = ex.Message;
                        break;

                    case ConflictException:
                        context.Response.StatusCode = 409;
                        statusCode = HttpStatusCode.Conflict;
                        message = ex.Message;
                        break;
                }

                var response = new ApiErrorResponseDto<string>
                {
                    StatusCode = statusCode,
                    Message = message,
                    Error = error
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
