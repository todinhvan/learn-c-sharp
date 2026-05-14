using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CoreBanking.UnitTests
{
    /// <summary>
    /// Provides shared test helpers: in-memory DbContext factory and logger stubs.
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        private readonly string _dbName;

        protected TestBase()
        {
            _dbName = Guid.NewGuid().ToString();
        }

        protected CoreBankingDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new CoreBankingDbContext(options);
        }

        protected static ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.Create(b => b.AddDebug()).CreateLogger<T>();
        }

        public void Dispose()
        {
            // In-memory DB is disposed when last context is disposed
            GC.SuppressFinalize(this);
        }
    }
}
