using Asp.Versioning;
using CoreBanking.API.Middleware;
using CoreBanking.API.Services;
using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.API.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.Services.AddOpenApi();
            builder.Services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Version")
                    );
            });
            builder.AddNpgsqlDbContext<CoreBankingDbContext>("corebanking-db", configureDbContextOptions: options =>
            {
                options.UseNpgsql(optionBuilder =>
                {
                    optionBuilder.MigrationsAssembly(typeof(CoreBankingDbContext).Assembly.FullName);
                });
            });

            // Register application services
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IAccountService, AccountService>();

            // Register global exception handler
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            return builder;
        }
    }
}
