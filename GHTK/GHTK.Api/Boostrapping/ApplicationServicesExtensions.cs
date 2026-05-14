using Authentication.Client;
using GHTK.Api.AutoMapperProfiles;
using GHTK.Infrastructure.Repositories;
using GHTK.Security;
using MongoDB.Driver;

namespace GHTK.Api.Boostrapping
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            IClientSourceAuthenticationHandler handler
                = ClientSourceAuthenticationHandlerFactory.CreateClientSourceAuthenticationHandler(configuration);

            services.AddControllers();

            services.AddAuthentication("X-Client-Source")
                .AddScheme<GhtkAuthenticationHandlerOptions, GhtkAuthenticationHandler>("X-Client-Source", options =>
                {
                    options.jwtSecretKey = configuration["JwtSecretKey"] ?? "";
                    options.ValidatorAsync = async (clientSource, token, princical) => await handler.AuthenticateAsync(clientSource);
                });

            var mongoClient = new MongoClient(configuration.GetConnectionString("MongoDbConnection"));
            services.AddSingleton(mongoClient);

            services.AddScoped<IOrderRepository, MongoOrderRepository>();

            services.AddAutoMapper(config => config.AddProfile<OrderProfile>());

            services.AddOpenApi();

            return services;
        }
    }
}
