using CoreBanking.API.Models;
using CoreBanking.API.Models.Customer;
using CoreBanking.API.Services;

namespace CoreBanking.UnitTests
{
    public class CustomerServiceTests : TestBase
    {
        // ═══════════════════════════════════════════════
        // CreateCustomer — Happy Cases
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task CreateCustomer_ValidNameAndAddress_ReturnsSuccess()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.CreateCustomerAsync(
                new CreateCustomerRequest { Name = "Nguyen Van A", Address = "Hanoi" });

            Assert.True(result.IsSuccess);
            Assert.Equal("Nguyen Van A", result.Value!.Name);
            Assert.Equal("Hanoi", result.Value.Address);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
        }

        [Fact]
        public async Task CreateCustomer_NullAddress_ReturnsSuccess()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.CreateCustomerAsync(
                new CreateCustomerRequest { Name = "No Address Customer", Address = null });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value!.Address);
        }

        [Fact]
        public async Task CreateCustomer_TrimsWhitespace_ReturnsCleanedData()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.CreateCustomerAsync(
                new CreateCustomerRequest { Name = "  John Doe  ", Address = "  123 Main St  " });

            Assert.True(result.IsSuccess);
            Assert.Equal("John Doe", result.Value!.Name);
            Assert.Equal("123 Main St", result.Value.Address);
        }

        [Fact]
        public async Task CreateCustomer_MultipleCustomers_EachGetsUniqueId()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result1 = await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "Customer A" });
            var result2 = await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "Customer B" });

            Assert.NotEqual(result1.Value!.Id, result2.Value!.Id);
        }

        // ═══════════════════════════════════════════════
        // CreateCustomer — Edge Cases (Name Validation)
        // ═══════════════════════════════════════════════

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null!)]
        public async Task CreateCustomer_EmptyOrWhitespaceName_ReturnsBadRequest(string? name)
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.CreateCustomerAsync(new CreateCustomerRequest { Name = name! });

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.BadRequest, result.ErrorType);
            Assert.Contains("required", result.Error!, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AB")]
        public async Task CreateCustomer_NameTooShort_ReturnsBadRequest(string name)
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.CreateCustomerAsync(new CreateCustomerRequest { Name = name });

            Assert.False(result.IsSuccess);
            Assert.Contains("at least 3 characters", result.Error!);
        }

        [Fact]
        public async Task CreateCustomer_NameExactly3Chars_Succeeds()
        {
            // Boundary: minimum valid length
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "ABC" });

            Assert.True(result.IsSuccess);
            Assert.Equal("ABC", result.Value!.Name);
        }

        [Fact]
        public async Task CreateCustomer_NameExactly200Chars_Succeeds()
        {
            // Boundary: maximum valid length
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());
            var name = new string('A', 200);

            var result = await service.CreateCustomerAsync(new CreateCustomerRequest { Name = name });

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.Value!.Name.Length);
        }

        [Fact]
        public async Task CreateCustomer_NameExceeds200Chars_ReturnsBadRequest()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());
            var name = new string('A', 201);

            var result = await service.CreateCustomerAsync(new CreateCustomerRequest { Name = name });

            Assert.False(result.IsSuccess);
            Assert.Contains("200 characters", result.Error!);
        }

        [Fact]
        public async Task CreateCustomer_WhitespaceNameTrimsToTooShort_ReturnsBadRequest()
        {
            // "  AB  " trims to "AB" which is 2 chars < 3
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "  AB  " });

            Assert.False(result.IsSuccess);
            Assert.Contains("at least 3 characters", result.Error!);
        }

        // ═══════════════════════════════════════════════
        // CreateCustomer — Edge Cases (Address Validation)
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task CreateCustomer_AddressExactly500Chars_Succeeds()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());
            var address = new string('X', 500);

            var result = await service.CreateCustomerAsync(
                new CreateCustomerRequest { Name = "Test User", Address = address });

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateCustomer_AddressExceeds500Chars_ReturnsBadRequest()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());
            var address = new string('X', 501);

            var result = await service.CreateCustomerAsync(
                new CreateCustomerRequest { Name = "Test User", Address = address });

            Assert.False(result.IsSuccess);
            Assert.Contains("500 characters", result.Error!);
        }

        [Fact]
        public async Task CreateCustomer_EmptyStringAddress_TrimsToEmpty()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.CreateCustomerAsync(
                new CreateCustomerRequest { Name = "Test User", Address = "   " });

            Assert.True(result.IsSuccess);
            // Trimmed whitespace-only address should be empty string
            Assert.Equal("", result.Value!.Address);
        }

        // ═══════════════════════════════════════════════
        // GetCustomerById — Happy & Edge Cases
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task GetCustomerById_ExistingCustomer_ReturnsAllFields()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());
            var created = await service.CreateCustomerAsync(
                new CreateCustomerRequest { Name = "Full Customer", Address = "123 Main St" });

            var result = await service.GetCustomerByIdAsync(created.Value!.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(created.Value.Id, result.Value!.Id);
            Assert.Equal("Full Customer", result.Value.Name);
            Assert.Equal("123 Main St", result.Value.Address);
        }

        [Fact]
        public async Task GetCustomerById_NonExistent_ReturnsNotFound()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.GetCustomerByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Contains("not found", result.Error!, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetCustomerById_EmptyGuid_ReturnsNotFound()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.GetCustomerByIdAsync(Guid.Empty);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }

        // ═══════════════════════════════════════════════
        // GetCustomers — Happy & Edge Cases
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task GetCustomers_FirstPage_ReturnsCorrectData()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            for (int i = 0; i < 5; i++)
                await service.CreateCustomerAsync(new CreateCustomerRequest { Name = $"Customer {i:D2}" });

            var result = await service.GetCustomersAsync(pageIndex: 1, pageSize: 10);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value!.PageIndex);
            Assert.Equal(5, result.Value.TotalItems);
            Assert.Equal(5, result.Value.Items.Count());
        }

        [Fact]
        public async Task GetCustomers_Pagination_ReturnsCorrectMiddlePage()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            for (int i = 0; i < 15; i++)
                await service.CreateCustomerAsync(new CreateCustomerRequest { Name = $"Customer {i:D2}" });

            var result = await service.GetCustomersAsync(pageIndex: 2, pageSize: 5);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.PageIndex);
            Assert.Equal(5, result.Value.PageSize);
            Assert.Equal(15, result.Value.TotalItems);
            Assert.Equal(3, result.Value.PageCount);
            Assert.Equal(5, result.Value.Items.Count());
        }

        [Fact]
        public async Task GetCustomers_LastPage_ReturnsRemainingItems()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            for (int i = 0; i < 7; i++)
                await service.CreateCustomerAsync(new CreateCustomerRequest { Name = $"Customer {i:D2}" });

            // Page 2 of 3-per-page = last page with 1 item
            var result = await service.GetCustomersAsync(pageIndex: 3, pageSize: 3);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value!.Items);
            Assert.Equal(7, result.Value.TotalItems);
        }

        [Fact]
        public async Task GetCustomers_EmptyDatabase_ReturnsEmptyList()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            var result = await service.GetCustomersAsync(pageIndex: 1, pageSize: 10);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalItems);
        }

        [Fact]
        public async Task GetCustomers_PageBeyondTotal_ReturnsEmptyItems()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "Only Customer" });

            var result = await service.GetCustomersAsync(pageIndex: 99, pageSize: 10);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value!.Items);
            Assert.Equal(1, result.Value.TotalItems); // TotalItems still correct
        }

        [Fact]
        public async Task GetCustomers_PageSize1_ReturnsSingleItem()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "AAA" });
            await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "BBB" });
            await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "CCC" });

            var result = await service.GetCustomersAsync(pageIndex: 1, pageSize: 1);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value!.Items);
            Assert.Equal(3, result.Value.TotalItems);
            Assert.Equal(3, result.Value.PageCount);
        }

        [Fact]
        public async Task GetCustomers_OrderedByName_ReturnsAlphabetical()
        {
            using var dbContext = CreateDbContext();
            var service = new CustomerService(dbContext, CreateLogger<CustomerService>());

            await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "Zara" });
            await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "Alpha" });
            await service.CreateCustomerAsync(new CreateCustomerRequest { Name = "Mike" });

            var result = await service.GetCustomersAsync(pageIndex: 1, pageSize: 10);

            var names = result.Value!.Items.Select(c => c.Name).ToList();
            Assert.Equal("Alpha", names[0]);
            Assert.Equal("Mike", names[1]);
            Assert.Equal("Zara", names[2]);
        }
    }
}
