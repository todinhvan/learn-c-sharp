namespace CoreConcept.MiddlewareDemo.Middlewares
{
    public static class ApiKeyMiddlewareExtensions
    {
        public static WebApplication UseApiKeyMiddleware(this WebApplication app)
        {
            app.UseMiddleware<ApiKeyMiddleware>();
            return app;
        }
    }
}
