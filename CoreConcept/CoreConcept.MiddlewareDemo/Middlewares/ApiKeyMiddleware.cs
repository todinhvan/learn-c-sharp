using CoreConcept.MiddlewareDemo.Models;
using CoreConcept.MiddlewareDemo.Repositories;
using Microsoft.Extensions.Options;

namespace CoreConcept.MiddlewareDemo.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiSettings _apiSettings;

        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiSettings> options)
        {
            _next = next;
            _apiSettings = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? apiKey = context.Request.Headers["X-API-KEY"];
            if (string.IsNullOrEmpty(apiKey) || apiKey != _apiSettings.ApiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            context.Features.Set(_apiSettings);

            await _next(context);
            return;
        }
    }
}
