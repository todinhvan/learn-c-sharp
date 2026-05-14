using CoreBanking.API.Models;
using CoreBanking.API.Models.Customer;

namespace CoreBanking.API.Services
{
    public interface ICustomerService
    {
        Task<ServiceResult<PaginationResponse<CustomerResponse>>> GetCustomersAsync(int pageIndex, int pageSize);
        Task<ServiceResult<CustomerResponse>> GetCustomerByIdAsync(Guid id);
        Task<ServiceResult<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request);
    }
}
