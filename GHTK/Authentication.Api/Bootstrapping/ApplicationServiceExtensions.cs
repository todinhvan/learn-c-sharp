using Authentication.Client;

namespace Authentication.Api.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ClientAuthentication")
                                ?? throw new Exception("ClientAuthentication database connection string not found");
            services.AddSingleton<IClientSourceAuthenticationHandler>(
                new SqlServerClientSourceAuthenticationHandler(connectionString)
            );

            services.AddControllers();

            services.AddOpenApi();

            return services;
        }
    }
}
