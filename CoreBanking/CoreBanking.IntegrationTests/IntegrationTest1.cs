using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoreBanking.IntegrationTests
{
    /// <summary>
    /// End-to-end integration tests using Aspire DistributedApplicationTestingBuilder.
    /// Requires Docker to be running for PostgreSQL container.
    /// </summary>
    public class CoreBankingApiTests : IAsyncLifetime
    {
        private DistributedApplication? _app;
        private HttpClient? _client;
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task InitializeAsync()
        {
            var cts = new CancellationTokenSource(Timeout);
            var appHost = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.CoreBanking_Aspire_AppHost>(cts.Token);

            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddFilter("Aspire.", LogLevel.Warning);
            });

            appHost.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
            });

            _app = await appHost.BuildAsync(cts.Token).WaitAsync(Timeout, cts.Token);
            await _app.StartAsync(cts.Token).WaitAsync(Timeout, cts.Token);

            _client = _app.CreateHttpClient("corebanking-api");
            await _app.ResourceNotifications
                .WaitForResourceHealthyAsync("corebanking-api", cts.Token)
                .WaitAsync(Timeout, cts.Token);
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            if (_app is not null) await _app.DisposeAsync();
        }

        private string Api(string path) => $"/api/v1/core-banking{path}";

        // ═══════════════════════════════════════════
        // Customer Endpoints — Happy Cases
        // ═══════════════════════════════════════════

        [Fact]
        public async Task CreateCustomer_Valid_Returns200()
        {
            var response = await _client!.PostAsJsonAsync(Api("/customers"),
                new { Name = "Integration Test Customer", Address = "HCMC" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOpts);
            Assert.NotNull(body);
            Assert.Equal("Integration Test Customer", body!.Name);
            Assert.NotEqual(Guid.Empty, body.Id);
        }

        [Fact]
        public async Task GetCustomerById_Existing_Returns200()
        {
            // Create
            var create = await _client!.PostAsJsonAsync(Api("/customers"),
                new { Name = "Get By Id Test" });
            var created = await create.Content.ReadFromJsonAsync<CustomerDto>(JsonOpts);

            // Get
            var response = await _client.GetAsync(Api($"/customers/{created!.Id}"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOpts);
            Assert.Equal(created.Id, body!.Id);
            Assert.Equal("Get By Id Test", body.Name);
        }

        [Fact]
        public async Task GetCustomers_ReturnsPaginatedList()
        {
            await _client!.PostAsJsonAsync(Api("/customers"), new { Name = "Paginate Test A" });
            await _client.PostAsJsonAsync(Api("/customers"), new { Name = "Paginate Test B" });

            var response = await _client.GetAsync(Api("/customers?pageIndex=1&pageSize=100"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<PaginatedDto<CustomerDto>>(JsonOpts);
            Assert.NotNull(body);
            Assert.True(body!.TotalItems >= 2);
        }

        // ═══════════════════════════════════════════
        // Customer Endpoints — Edge Cases
        // ═══════════════════════════════════════════

        [Fact]
        public async Task GetCustomerById_NonExistent_Returns404()
        {
            var response = await _client!.GetAsync(Api($"/customers/{Guid.NewGuid()}"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomer_ShortName_Returns400()
        {
            var response = await _client!.PostAsJsonAsync(Api("/customers"), new { Name = "AB" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCustomer_EmptyName_Returns400()
        {
            var response = await _client!.PostAsJsonAsync(Api("/customers"), new { Name = "" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ═══════════════════════════════════════════
        // Account Endpoints — Happy Cases
        // ═══════════════════════════════════════════

        private async Task<CustomerDto> CreateTestCustomerAsync()
        {
            var r = await _client!.PostAsJsonAsync(Api("/customers"),
                new { Name = $"AccTest-{Guid.NewGuid():N}" });
            return (await r.Content.ReadFromJsonAsync<CustomerDto>(JsonOpts))!;
        }

        private async Task<AccountDto> CreateTestAccountAsync(Guid customerId)
        {
            var r = await _client!.PostAsJsonAsync(Api("/accounts"),
                new { CustomerId = customerId });
            return (await r.Content.ReadFromJsonAsync<AccountDto>(JsonOpts))!;
        }

        [Fact]
        public async Task CreateAccount_Valid_Returns200WithZeroBalance()
        {
            var customer = await CreateTestCustomerAsync();
            var response = await _client!.PostAsJsonAsync(Api("/accounts"),
                new { CustomerId = customer.Id });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<AccountDto>(JsonOpts);
            Assert.Equal(0, body!.Balance);
            Assert.StartsWith("CB-", body.AccountNumber);
        }

        [Fact]
        public async Task GetAccountById_Existing_Returns200()
        {
            var customer = await CreateTestCustomerAsync();
            var account = await CreateTestAccountAsync(customer.Id);

            var response = await _client!.GetAsync(Api($"/accounts/{account.Id}"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<AccountDto>(JsonOpts);
            Assert.Equal(account.Id, body!.Id);
        }

        [Fact]
        public async Task GetAccounts_FilterByCustomer_ReturnsFiltered()
        {
            var customer = await CreateTestCustomerAsync();
            await CreateTestAccountAsync(customer.Id);
            await CreateTestAccountAsync(customer.Id);

            var response = await _client!.GetAsync(Api($"/accounts?customerId={customer.Id}&pageSize=100"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<PaginatedDto<AccountDto>>(JsonOpts);
            Assert.Equal(2, body!.TotalItems);
        }

        // ═══════════════════════════════════════════
        // Account Endpoints — Edge Cases
        // ═══════════════════════════════════════════

        [Fact]
        public async Task GetAccountById_NonExistent_Returns404()
        {
            var response = await _client!.GetAsync(Api($"/accounts/{Guid.NewGuid()}"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateAccount_NonExistentCustomer_Returns400()
        {
            var response = await _client!.PostAsJsonAsync(Api("/accounts"),
                new { CustomerId = Guid.NewGuid() });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ═══════════════════════════════════════════
        // Deposit — Happy & Edge
        // ═══════════════════════════════════════════

        [Fact]
        public async Task Deposit_Valid_Returns200AndUpdatesBalance()
        {
            var customer = await CreateTestCustomerAsync();
            var account = await CreateTestAccountAsync(customer.Id);

            var response = await _client!.PutAsJsonAsync(
                Api($"/accounts/{account.Id}/deposit"), new { Amount = 500m });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var tx = await response.Content.ReadFromJsonAsync<TransactionDto>(JsonOpts);
            Assert.Equal(500m, tx!.Amount);
            Assert.Equal(0, tx.Type); // TransactionType.Deposit = 0

            // Verify balance via GET
            var get = await _client.GetFromJsonAsync<AccountDto>(Api($"/accounts/{account.Id}"), JsonOpts);
            Assert.Equal(500m, get!.Balance);
        }

        [Fact]
        public async Task Deposit_ZeroAmount_Returns400()
        {
            var customer = await CreateTestCustomerAsync();
            var account = await CreateTestAccountAsync(customer.Id);
            var response = await _client!.PutAsJsonAsync(
                Api($"/accounts/{account.Id}/deposit"), new { Amount = 0m });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Deposit_NonExistentAccount_Returns404()
        {
            var response = await _client!.PutAsJsonAsync(
                Api($"/accounts/{Guid.NewGuid()}/deposit"), new { Amount = 100m });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ═══════════════════════════════════════════
        // Withdraw — Happy & Edge
        // ═══════════════════════════════════════════

        [Fact]
        public async Task Withdraw_Valid_Returns200AndUpdatesBalance()
        {
            var customer = await CreateTestCustomerAsync();
            var account = await CreateTestAccountAsync(customer.Id);
            await _client!.PutAsJsonAsync(Api($"/accounts/{account.Id}/deposit"), new { Amount = 1000m });

            var response = await _client.PutAsJsonAsync(
                Api($"/accounts/{account.Id}/withdraw"), new { Amount = 300m });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var get = await _client.GetFromJsonAsync<AccountDto>(Api($"/accounts/{account.Id}"), JsonOpts);
            Assert.Equal(700m, get!.Balance);
        }

        [Fact]
        public async Task Withdraw_InsufficientBalance_Returns400()
        {
            var customer = await CreateTestCustomerAsync();
            var account = await CreateTestAccountAsync(customer.Id);
            // Balance is 0, try to withdraw
            var response = await _client!.PutAsJsonAsync(
                Api($"/accounts/{account.Id}/withdraw"), new { Amount = 100m });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ═══════════════════════════════════════════
        // Transfer — Happy & Edge
        // ═══════════════════════════════════════════

        [Fact]
        public async Task Transfer_Valid_Returns200AndUpdatesBalances()
        {
            var customer = await CreateTestCustomerAsync();
            var from = await CreateTestAccountAsync(customer.Id);
            var to = await CreateTestAccountAsync(customer.Id);
            await _client!.PutAsJsonAsync(Api($"/accounts/{from.Id}/deposit"), new { Amount = 1000m });

            var response = await _client.PutAsJsonAsync(
                Api($"/accounts/{from.Id}/transfer"),
                new { ReceivedAccountId = to.Id, Amount = 400m });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var tx = await response.Content.ReadFromJsonAsync<TransactionDto>(JsonOpts);
            Assert.Equal(2, tx!.Type); // TransactionType.TransferOut = 2
            Assert.NotNull(tx.ReferenceId);

            var fromGet = await _client.GetFromJsonAsync<AccountDto>(Api($"/accounts/{from.Id}"), JsonOpts);
            var toGet = await _client.GetFromJsonAsync<AccountDto>(Api($"/accounts/{to.Id}"), JsonOpts);
            Assert.Equal(600m, fromGet!.Balance);
            Assert.Equal(400m, toGet!.Balance);
        }

        [Fact]
        public async Task Transfer_SameAccount_Returns400()
        {
            var customer = await CreateTestCustomerAsync();
            var account = await CreateTestAccountAsync(customer.Id);
            await _client!.PutAsJsonAsync(Api($"/accounts/{account.Id}/deposit"), new { Amount = 1000m });

            var response = await _client.PutAsJsonAsync(
                Api($"/accounts/{account.Id}/transfer"),
                new { ReceivedAccountId = account.Id, Amount = 100m });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_InsufficientBalance_Returns400()
        {
            var customer = await CreateTestCustomerAsync();
            var from = await CreateTestAccountAsync(customer.Id);
            var to = await CreateTestAccountAsync(customer.Id);
            // from has 0 balance

            var response = await _client!.PutAsJsonAsync(
                Api($"/accounts/{from.Id}/transfer"),
                new { ReceivedAccountId = to.Id, Amount = 100m });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_NonExistentDestination_Returns404()
        {
            var customer = await CreateTestCustomerAsync();
            var from = await CreateTestAccountAsync(customer.Id);
            await _client!.PutAsJsonAsync(Api($"/accounts/{from.Id}/deposit"), new { Amount = 1000m });

            var response = await _client.PutAsJsonAsync(
                Api($"/accounts/{from.Id}/transfer"),
                new { ReceivedAccountId = Guid.NewGuid(), Amount = 100m });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ═══════════════════════════════════════════
        // Full Workflow E2E
        // ═══════════════════════════════════════════

        [Fact]
        public async Task FullWorkflow_CreateDepositWithdrawTransfer_AllCorrect()
        {
            // 1. Create customer
            var customer = await CreateTestCustomerAsync();

            // 2. Create two accounts
            var accountA = await CreateTestAccountAsync(customer.Id);
            var accountB = await CreateTestAccountAsync(customer.Id);

            // 3. Deposit to A
            await _client!.PutAsJsonAsync(Api($"/accounts/{accountA.Id}/deposit"), new { Amount = 2000m });

            // 4. Withdraw from A
            await _client.PutAsJsonAsync(Api($"/accounts/{accountA.Id}/withdraw"), new { Amount = 500m });

            // 5. Transfer A → B
            await _client.PutAsJsonAsync(
                Api($"/accounts/{accountA.Id}/transfer"),
                new { ReceivedAccountId = accountB.Id, Amount = 700m });

            // 6. Verify final balances
            var a = await _client.GetFromJsonAsync<AccountDto>(Api($"/accounts/{accountA.Id}"), JsonOpts);
            var b = await _client.GetFromJsonAsync<AccountDto>(Api($"/accounts/{accountB.Id}"), JsonOpts);
            Assert.Equal(800m, a!.Balance);   // 0 + 2000 - 500 - 700
            Assert.Equal(700m, b!.Balance);   // 0 + 700
        }

        // ═══════════════════════════════════════════
        // Health Check
        // ═══════════════════════════════════════════

        [Fact]
        public async Task HealthEndpoint_Returns200()
        {
            var response = await _client!.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ═══════════════════════════════════════════
        // DTOs for deserialization
        // ═══════════════════════════════════════════

        private record CustomerDto(Guid Id, string Name, string? Address);
        private record AccountDto(Guid Id, string AccountNumber, decimal Balance, Guid CustomerId);
        private record TransactionDto(Guid Id, Guid AccountId, decimal Amount, DateTime Date, int Type, Guid? ReferenceId);
        private record PaginatedDto<T>(int PageIndex, int PageSize, int TotalItems, int PageCount, List<T> Items);
    }
}
