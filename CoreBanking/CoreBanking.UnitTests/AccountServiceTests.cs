using CoreBanking.API.Models;
using CoreBanking.API.Models.Account;
using CoreBanking.API.Services;
using CoreBanking.Infrastructure.Entities;

namespace CoreBanking.UnitTests
{
    public class AccountServiceTests : TestBase
    {
        private async Task<Guid> SeedCustomerAsync()
        {
            using var db = CreateDbContext();
            var c = new Customer { Id = Guid.NewGuid(), Name = "Test Customer" };
            db.Customers.Add(c);
            await db.SaveChangesAsync();
            return c.Id;
        }

        private async Task<(Guid CustomerId, Guid AccountId)> SeedAccountAsync(decimal balance = 1000m)
        {
            var db = CreateDbContext();
            var c = new Customer { Id = Guid.NewGuid(), Name = "Test Customer" };
            var a = new Account { Id = Guid.NewGuid(), AccountNumber = "CB-TEST-000001", Balance = balance, CustomerId = c.Id };
            db.Customers.Add(c); db.Accounts.Add(a);
            await db.SaveChangesAsync();
            return (c.Id, a.Id);
        }

        private async Task<(Guid FromId, Guid ToId)> SeedTwoAccountsAsync(decimal fromBal = 1000m, decimal toBal = 500m)
        {
            var db = CreateDbContext();
            var c = new Customer { Id = Guid.NewGuid(), Name = "Owner" };
            var from = new Account { Id = Guid.NewGuid(), AccountNumber = "CB-FROM-001", Balance = fromBal, CustomerId = c.Id };
            var to = new Account { Id = Guid.NewGuid(), AccountNumber = "CB-TO-002", Balance = toBal, CustomerId = c.Id };
            db.Customers.Add(c); db.Accounts.AddRange(from, to);
            await db.SaveChangesAsync();
            return (from.Id, to.Id);
        }

        // ═══ CreateAccount ═══════════════════════════

        [Fact]
        public async Task CreateAccount_Valid_ReturnsSuccessWithZeroBalance()
        {
            var cid = await SeedCustomerAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.CreateAccountAsync(new CreateAccountRequest { CustomerId = cid });
            Assert.True(r.IsSuccess);
            Assert.Equal(0, r.Value!.Balance);
            Assert.StartsWith("CB-", r.Value.AccountNumber);
            Assert.Equal(cid, r.Value.CustomerId);
        }

        [Fact]
        public async Task CreateAccount_GeneratesMeaningfulFormat()
        {
            var cid = await SeedCustomerAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.CreateAccountAsync(new CreateAccountRequest { CustomerId = cid });
            var parts = r.Value!.AccountNumber.Split('-');
            Assert.Equal(3, parts.Length);
            Assert.Equal("CB", parts[0]);
            Assert.Equal(8, parts[1].Length);
            Assert.Equal(6, parts[2].Length);
        }

        [Fact]
        public async Task CreateAccount_TwoAccounts_DifferentNumbers()
        {
            var cid = await SeedCustomerAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r1 = await svc.CreateAccountAsync(new CreateAccountRequest { CustomerId = cid });
            var r2 = await svc.CreateAccountAsync(new CreateAccountRequest { CustomerId = cid });
            Assert.NotEqual(r1.Value!.AccountNumber, r2.Value!.AccountNumber);
            Assert.NotEqual(r1.Value.Id, r2.Value.Id);
        }

        [Fact]
        public async Task CreateAccount_EmptyCustomerId_BadRequest()
        {
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.CreateAccountAsync(new CreateAccountRequest { CustomerId = Guid.Empty });
            Assert.False(r.IsSuccess);
            Assert.Equal(ErrorType.BadRequest, r.ErrorType);
        }

        [Fact]
        public async Task CreateAccount_NonExistentCustomer_BadRequest()
        {
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.CreateAccountAsync(new CreateAccountRequest { CustomerId = Guid.NewGuid() });
            Assert.False(r.IsSuccess);
            Assert.Contains("Customer not found", r.Error!);
        }

        // ═══ GetAccountById ══════════════════════════

        [Fact]
        public async Task GetAccountById_Existing_ReturnsCorrectData()
        {
            var (_, aid) = await SeedAccountAsync(500m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.GetAccountByIdAsync(aid);
            Assert.True(r.IsSuccess);
            Assert.Equal(aid, r.Value!.Id);
            Assert.Equal(500m, r.Value.Balance);
        }

        [Fact]
        public async Task GetAccountById_NonExistent_NotFound()
        {
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.GetAccountByIdAsync(Guid.NewGuid());
            Assert.False(r.IsSuccess);
            Assert.Equal(ErrorType.NotFound, r.ErrorType);
        }

        [Fact]
        public async Task GetAccountById_EmptyGuid_NotFound()
        {
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.GetAccountByIdAsync(Guid.Empty);
            Assert.False(r.IsSuccess);
            Assert.Equal(ErrorType.NotFound, r.ErrorType);
        }

        // ═══ GetAccounts ═════════════════════════════

        [Fact]
        public async Task GetAccounts_NoFilter_ReturnsAll()
        {
            var cid = await SeedCustomerAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            await svc.CreateAccountAsync(new CreateAccountRequest { CustomerId = cid });
            await svc.CreateAccountAsync(new CreateAccountRequest { CustomerId = cid });

            var r = await svc.GetAccountsAsync(1, 10);
            Assert.True(r.IsSuccess);
            Assert.Equal(2, r.Value!.TotalItems);
        }

        [Fact]
        public async Task GetAccounts_FilterByCustomerId_ReturnsOnlyMatching()
        {
            var db = CreateDbContext();
            var c1 = new Customer { Id = Guid.NewGuid(), Name = "C1" };
            var c2 = new Customer { Id = Guid.NewGuid(), Name = "C2" };
            db.Customers.AddRange(c1, c2);
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "CB-A-001", Balance = 0, CustomerId = c1.Id });
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "CB-A-002", Balance = 0, CustomerId = c1.Id });
            db.Accounts.Add(new Account { Id = Guid.NewGuid(), AccountNumber = "CB-B-001", Balance = 0, CustomerId = c2.Id });
            await db.SaveChangesAsync();

            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.GetAccountsAsync(1, 10, c1.Id);
            Assert.Equal(2, r.Value!.TotalItems);
            Assert.All(r.Value.Items, a => Assert.Equal(c1.Id, a.CustomerId));
        }

        [Fact]
        public async Task GetAccounts_FilterNonExistentCustomer_ReturnsEmpty()
        {
            var (_, _) = await SeedAccountAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.GetAccountsAsync(1, 10, Guid.NewGuid());
            Assert.Empty(r.Value!.Items);
            Assert.Equal(0, r.Value.TotalItems);
        }

        [Fact]
        public async Task GetAccounts_EmptyDb_ReturnsEmpty()
        {
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.GetAccountsAsync(1, 10);
            Assert.True(r.IsSuccess);
            Assert.Empty(r.Value!.Items);
        }

        [Fact]
        public async Task GetAccounts_PageBeyondTotal_ReturnsEmpty()
        {
            var (_, _) = await SeedAccountAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.GetAccountsAsync(99, 10);
            Assert.Empty(r.Value!.Items);
            Assert.Equal(1, r.Value.TotalItems);
        }

        // ═══ Deposit ═════════════════════════════════

        [Fact]
        public async Task Deposit_Valid_IncreasesBalance()
        {
            var (_, aid) = await SeedAccountAsync(500m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.DepositAsync(aid, 200m);
            Assert.True(r.IsSuccess);
            Assert.Equal(TransactionType.Deposit, r.Value!.Type);
            Assert.Equal(200m, r.Value.Amount);
            Assert.Equal(700m, (await db.Accounts.FindAsync(aid))!.Balance);
        }

        [Fact]
        public async Task Deposit_MultipleSequential_AccumulatesBalance()
        {
            var (_, aid) = await SeedAccountAsync(0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            await svc.DepositAsync(aid, 100m);
            await svc.DepositAsync(aid, 250m);
            await svc.DepositAsync(aid, 50m);
            Assert.Equal(400m, (await db.Accounts.FindAsync(aid))!.Balance);
        }

        [Fact]
        public async Task Deposit_LargeAmount_Succeeds()
        {
            var (_, aid) = await SeedAccountAsync(0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.DepositAsync(aid, 999_999_999.99m);
            Assert.True(r.IsSuccess);
            Assert.Equal(999_999_999.99m, (await db.Accounts.FindAsync(aid))!.Balance);
        }

        [Fact]
        public async Task Deposit_SmallestValid_Succeeds()
        {
            var (_, aid) = await SeedAccountAsync(0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.DepositAsync(aid, 0.01m);
            Assert.True(r.IsSuccess);
            Assert.Equal(0.01m, (await db.Accounts.FindAsync(aid))!.Balance);
        }

        [Fact]
        public async Task Deposit_CreatesTransactionRecord()
        {
            var (_, aid) = await SeedAccountAsync(0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.DepositAsync(aid, 100m);
            var tx = await db.Transactions.FindAsync(r.Value!.Id);
            Assert.NotNull(tx);
            Assert.Equal(aid, tx!.AccountId);
            Assert.Equal(100m, tx.Amount);
            Assert.Null(tx.ReferenceId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task Deposit_InvalidAmount_BadRequest(decimal amount)
        {
            var (_, aid) = await SeedAccountAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.DepositAsync(aid, amount);
            Assert.False(r.IsSuccess);
            Assert.Equal(ErrorType.BadRequest, r.ErrorType);
            Assert.Contains("positive", r.Error!);
        }

        [Fact]
        public async Task Deposit_NonExistentAccount_NotFound()
        {
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.DepositAsync(Guid.NewGuid(), 100m);
            Assert.False(r.IsSuccess);
            Assert.Equal(ErrorType.NotFound, r.ErrorType);
        }

        // ═══ Withdraw ════════════════════════════════

        [Fact]
        public async Task Withdraw_Valid_DecreasesBalance()
        {
            var (_, aid) = await SeedAccountAsync(1000m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.WithdrawAsync(aid, 300m);
            Assert.True(r.IsSuccess);
            Assert.Equal(TransactionType.Withdrawal, r.Value!.Type);
            Assert.Equal(700m, (await db.Accounts.FindAsync(aid))!.Balance);
        }

        [Fact]
        public async Task Withdraw_ExactBalance_SucceedsWithZero()
        {
            // Boundary: withdraw all money
            var (_, aid) = await SeedAccountAsync(500m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.WithdrawAsync(aid, 500m);
            Assert.True(r.IsSuccess);
            Assert.Equal(0m, (await db.Accounts.FindAsync(aid))!.Balance);
        }

        [Fact]
        public async Task Withdraw_MultipleSequential_CorrectBalance()
        {
            var (_, aid) = await SeedAccountAsync(1000m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            await svc.WithdrawAsync(aid, 200m);
            await svc.WithdrawAsync(aid, 300m);
            Assert.Equal(500m, (await db.Accounts.FindAsync(aid))!.Balance);
        }

        [Fact]
        public async Task Withdraw_SmallestValid_Succeeds()
        {
            var (_, aid) = await SeedAccountAsync(100m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.WithdrawAsync(aid, 0.01m);
            Assert.True(r.IsSuccess);
            Assert.Equal(99.99m, (await db.Accounts.FindAsync(aid))!.Balance);
        }

        [Fact]
        public async Task Withdraw_ExceedsBalance_BadRequest()
        {
            var (_, aid) = await SeedAccountAsync(100m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.WithdrawAsync(aid, 500m);
            Assert.False(r.IsSuccess);
            Assert.Contains("Insufficient", r.Error!);
        }

        [Fact]
        public async Task Withdraw_ExceedsByOneCent_BadRequest()
        {
            // Boundary: balance=100.00, withdraw=100.01
            var (_, aid) = await SeedAccountAsync(100.00m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.WithdrawAsync(aid, 100.01m);
            Assert.False(r.IsSuccess);
            Assert.Contains("Insufficient", r.Error!);
        }

        [Fact]
        public async Task Withdraw_FromZeroBalance_BadRequest()
        {
            var (_, aid) = await SeedAccountAsync(0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.WithdrawAsync(aid, 1m);
            Assert.False(r.IsSuccess);
            Assert.Contains("Insufficient", r.Error!);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task Withdraw_InvalidAmount_BadRequest(decimal amount)
        {
            var (_, aid) = await SeedAccountAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.WithdrawAsync(aid, amount);
            Assert.False(r.IsSuccess);
            Assert.Contains("positive", r.Error!);
        }

        [Fact]
        public async Task Withdraw_NonExistentAccount_NotFound()
        {
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.WithdrawAsync(Guid.NewGuid(), 100m);
            Assert.False(r.IsSuccess);
            Assert.Equal(ErrorType.NotFound, r.ErrorType);
        }

        // ═══ Transfer ════════════════════════════════

        [Fact]
        public async Task Transfer_Valid_CorrectBalancesAndTwoRecords()
        {
            var (fromId, toId) = await SeedTwoAccountsAsync(1000m, 500m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = 300m });

            Assert.True(r.IsSuccess);
            Assert.Equal(TransactionType.TransferOut, r.Value!.Type);
            Assert.NotNull(r.Value.ReferenceId);
            Assert.Equal(700m, (await db.Accounts.FindAsync(fromId))!.Balance);
            Assert.Equal(800m, (await db.Accounts.FindAsync(toId))!.Balance);

            var txs = db.Transactions.Where(t => t.ReferenceId == r.Value.ReferenceId).ToList();
            Assert.Equal(2, txs.Count);
            Assert.Contains(txs, t => t.Type == TransactionType.TransferOut && t.AccountId == fromId);
            Assert.Contains(txs, t => t.Type == TransactionType.TransferIn && t.AccountId == toId);
        }

        [Fact]
        public async Task Transfer_ExactBalance_SucceedsWithZero()
        {
            var (fromId, toId) = await SeedTwoAccountsAsync(500m, 0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = 500m });
            Assert.True(r.IsSuccess);
            Assert.Equal(0m, (await db.Accounts.FindAsync(fromId))!.Balance);
            Assert.Equal(500m, (await db.Accounts.FindAsync(toId))!.Balance);
        }

        [Fact]
        public async Task Transfer_SmallestAmount_Succeeds()
        {
            var (fromId, toId) = await SeedTwoAccountsAsync(100m, 100m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = 0.01m });
            Assert.True(r.IsSuccess);
            Assert.Equal(99.99m, (await db.Accounts.FindAsync(fromId))!.Balance);
            Assert.Equal(100.01m, (await db.Accounts.FindAsync(toId))!.Balance);
        }

        [Fact]
        public async Task Transfer_MultipleSequential_CorrectBalances()
        {
            var (fromId, toId) = await SeedTwoAccountsAsync(1000m, 0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = 100m });
            await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = 200m });
            Assert.Equal(700m, (await db.Accounts.FindAsync(fromId))!.Balance);
            Assert.Equal(300m, (await db.Accounts.FindAsync(toId))!.Balance);
        }

        [Fact]
        public async Task Transfer_SameAccount_BadRequest()
        {
            var (_, aid) = await SeedAccountAsync(1000m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(aid, new TransferRequest { ReceivedAccountId = aid, Amount = 100m });
            Assert.False(r.IsSuccess);
            Assert.Contains("same account", r.Error!);
        }

        [Fact]
        public async Task Transfer_InsufficientBalance_BadRequest()
        {
            var (fromId, toId) = await SeedTwoAccountsAsync(50m, 500m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = 100m });
            Assert.False(r.IsSuccess);
            Assert.Contains("Insufficient", r.Error!);
        }

        [Fact]
        public async Task Transfer_ExceedsByOneCent_BadRequest()
        {
            var (fromId, toId) = await SeedTwoAccountsAsync(100m, 0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = 100.01m });
            Assert.False(r.IsSuccess);
            Assert.Contains("Insufficient", r.Error!);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task Transfer_InvalidAmount_BadRequest(decimal amount)
        {
            var (fromId, toId) = await SeedTwoAccountsAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = amount });
            Assert.False(r.IsSuccess);
            Assert.Contains("positive", r.Error!);
        }

        [Fact]
        public async Task Transfer_EmptyReceivedAccountId_BadRequest()
        {
            var (_, aid) = await SeedAccountAsync(1000m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(aid, new TransferRequest { ReceivedAccountId = Guid.Empty, Amount = 100m });
            Assert.False(r.IsSuccess);
            Assert.Contains("ReceivedAccountId", r.Error!);
        }

        [Fact]
        public async Task Transfer_NonExistentSource_NotFound()
        {
            var (_, aid) = await SeedAccountAsync();
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(Guid.NewGuid(), new TransferRequest { ReceivedAccountId = aid, Amount = 100m });
            Assert.False(r.IsSuccess);
            Assert.Equal(ErrorType.NotFound, r.ErrorType);
            Assert.Contains("Source", r.Error!);
        }

        [Fact]
        public async Task Transfer_NonExistentDestination_NotFound()
        {
            var (_, aid) = await SeedAccountAsync(1000m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());
            var r = await svc.TransferAsync(aid, new TransferRequest { ReceivedAccountId = Guid.NewGuid(), Amount = 100m });
            Assert.False(r.IsSuccess);
            Assert.Contains("Destination", r.Error!);
        }

        // ═══ Mixed Workflow ══════════════════════════

        [Fact]
        public async Task FullWorkflow_DepositWithdrawTransfer_CorrectFinalBalances()
        {
            var (fromId, toId) = await SeedTwoAccountsAsync(0m, 0m);
            using var db = CreateDbContext();
            var svc = new AccountService(db, CreateLogger<AccountService>());

            await svc.DepositAsync(fromId, 1000m);
            await svc.DepositAsync(toId, 500m);
            await svc.WithdrawAsync(fromId, 200m);
            await svc.TransferAsync(fromId, new TransferRequest { ReceivedAccountId = toId, Amount = 300m });

            Assert.Equal(500m, (await db.Accounts.FindAsync(fromId))!.Balance);  // 0+1000-200-300
            Assert.Equal(800m, (await db.Accounts.FindAsync(toId))!.Balance);    // 0+500+300
        }
    }
}
