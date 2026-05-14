using CoreBanking.API.Models;
using CoreBanking.API.Models.Customer;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.API.Services
{
    public class CustomerService(CoreBankingDbContext dbContext, ILogger<CustomerService> logger) : ICustomerService
    {
        public async Task<ServiceResult<PaginationResponse<CustomerResponse>>> GetCustomersAsync(int pageIndex, int pageSize)
        {
            logger.LogInformation("Fetching customers — page {PageIndex}, size {PageSize}", pageIndex, pageSize);

            int skip = (pageIndex - 1) * pageSize;
            int totalItems = await dbContext.Customers.CountAsync();
            List<Customer> customers = await dbContext.Customers
                .OrderBy(c => c.Name)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var response = new PaginationResponse<CustomerResponse>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = customers.Select(c => new CustomerResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address
                })
            };

            logger.LogInformation("Returned {Count}/{Total} customers", customers.Count, totalItems);
            return ServiceResult<PaginationResponse<CustomerResponse>>.Success(response);
        }

        public async Task<ServiceResult<CustomerResponse>> GetCustomerByIdAsync(Guid id)
        {
            logger.LogInformation("Fetching customer {CustomerId}", id);

            var customer = await dbContext.Customers.FindAsync(id);
            if (customer is null)
            {
                logger.LogWarning("Customer {CustomerId} not found", id);
                return ServiceResult<CustomerResponse>.NotFound("Customer not found.");
            }

            return ServiceResult<CustomerResponse>.Success(new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Address = customer.Address
            });
        }

        public async Task<ServiceResult<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ServiceResult<CustomerResponse>.BadRequest("Customer name is required.");
            }

            if (request.Name.Trim().Length < 3)
            {
                return ServiceResult<CustomerResponse>.BadRequest("Customer name must be at least 3 characters long.");
            }

            if (request.Name.Trim().Length > 200)
            {
                return ServiceResult<CustomerResponse>.BadRequest("Customer name must not exceed 200 characters.");
            }

            if (request.Address is not null && request.Address.Length > 500)
            {
                return ServiceResult<CustomerResponse>.BadRequest("Address must not exceed 500 characters.");
            }

            logger.LogInformation("Creating customer with name '{Name}'", request.Name);

            Customer customer = new()
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Address = request.Address?.Trim()
            };

            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Created customer {CustomerId}", customer.Id);

            return ServiceResult<CustomerResponse>.Success(new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Address = customer.Address
            });
        }
    }
}
