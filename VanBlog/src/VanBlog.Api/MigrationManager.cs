using Microsoft.EntityFrameworkCore;
using VanBlog.Infrastructure.Data;

namespace VanBlog.Api
{
    public static class MigrationManager
    {
        public static async Task MigrateDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var mainLog = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                using (var context = scope.ServiceProvider.GetRequiredService<VanBlogDbContext>())
                {
                    try
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();


                        context.Database.Migrate();
                        await DataSeeder.SeedAsync(context, logger);
                    }
                    catch (Exception ex)
                    {
                        mainLog.LogError(ex, "An error occurred while migrating or seeding the database.");
                    }
                }
            }
        }
    }
}
