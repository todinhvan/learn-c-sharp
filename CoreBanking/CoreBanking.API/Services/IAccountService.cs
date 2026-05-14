using CoreBanking.API.Models;
using CoreBanking.API.Models.Account;

namespace CoreBanking.API.Services
{
    public interface IAccountService
    {
        Task<ServiceResult<PaginationResponse<AccountResponse>>> GetAccountsAsync(int pageIndex, int pageSize, Guid? customerId = null);
        Task<ServiceResult<AccountResponse>> GetAccountByIdAsync(Guid id);
        Task<ServiceResult<AccountResponse>> CreateAccountAsync(CreateAccountRequest request);
        Task<ServiceResult<TransactionResponse>> DepositAsync(Guid accountId, decimal amount);
        Task<ServiceResult<TransactionResponse>> WithdrawAsync(Guid accountId, decimal amount);
        Task<ServiceResult<TransactionResponse>> TransferAsync(Guid fromAccountId, TransferRequest request);
    }
}
