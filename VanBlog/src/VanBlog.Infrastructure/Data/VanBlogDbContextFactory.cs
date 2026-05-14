using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VanBlog.Infrastructure.Data
{
    public class VanBlogDbContextFactory : IDesignTimeDbContextFactory<VanBlogDbContext>
    {
        public VanBlogDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var dbContextBuilder = new DbContextOptionsBuilder<VanBlogDbContext>();
            dbContextBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            return new VanBlogDbContext(dbContextBuilder.Options);
        }
    }
}
