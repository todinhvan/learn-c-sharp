using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CoreBanking.Infrastructure.Data
{
    public class CoreBankingDbContextFactory : IDesignTimeDbContextFactory<CoreBankingDbContext>
    {
        public CoreBankingDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var optionsBuilder = new DbContextOptionsBuilder<CoreBankingDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("corebanking-db"));
            return new CoreBankingDbContext(optionsBuilder.Options);
        }
    }
}
