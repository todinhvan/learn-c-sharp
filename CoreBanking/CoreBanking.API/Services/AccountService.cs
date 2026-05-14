using CoreBanking.API.Models;
using CoreBanking.API.Models.Account;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.API.Services
{
    public class AccountService(CoreBankingDbContext dbContext, ILogger<AccountService> logger) : IAccountService
    {
        /// <summary>
        /// Generates a meaningful account number in the format CB-YYYYMMDD-XXXXXX.
        /// </summary>
        private static string GenerateAccountNumber()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Random.Shared.Next(100000, 999999).ToString();
            return $"CB-{datePart}-{randomPart}";
        }

        public async Task<ServiceResult<PaginationResponse<AccountResponse>>> GetAccountsAsync(
            int pageIndex, int pageSize, Guid? customerId = null)
        {
            logger.LogInformation("Fetching accounts — page {PageIndex}, size {PageSize}, customerId {CustomerId}",
                pageIndex, pageSize, customerId);

            IQueryable<Account> query = dbContext.Accounts;
            if (customerId.HasValue)
            {
                query = query.Where(a => a.CustomerId == customerId.Value);
            }

            int totalItems = await query.CountAsync();
            int skip = (pageIndex - 1) * pageSize;
            List<Account> accounts = await query
                .OrderBy(a => a.AccountNumber)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var response = new PaginationResponse<AccountResponse>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = accounts.Select(a => new AccountResponse
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    CustomerId = a.CustomerId
                })
            };

            logger.LogInformation("Returned {Count}/{Total} accounts", accounts.Count, totalItems);
            return ServiceResult<PaginationResponse<AccountResponse>>.Success(response);
        }

        public async Task<ServiceResult<AccountResponse>> GetAccountByIdAsync(Guid id)
        {
            logger.LogInformation("Fetching account {AccountId}", id);

            var account = await dbContext.Accounts.FindAsync(id);
            if (account is null)
            {
                logger.LogWarning("Account {AccountId} not found", id);
                return ServiceResult<AccountResponse>.NotFound("Account not found.");
            }

            return ServiceResult<AccountResponse>.Success(new AccountResponse
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                CustomerId = account.CustomerId
            });
        }

        public async Task<ServiceResult<AccountResponse>> CreateAccountAsync(CreateAccountRequest request)
        {
            // Validation
            if (request.CustomerId == Guid.Empty)
            {
                return ServiceResult<AccountResponse>.BadRequest("CustomerId is required.");
            }

            var customer = await dbContext.Customers.FindAsync(request.CustomerId);
            if (customer is null)
            {
                return ServiceResult<AccountResponse>.BadRequest("Customer not found.");
            }

            logger.LogInformation("Creating account for customer {CustomerId}", request.CustomerId);

            Account account = new()
            {
                Id = Guid.NewGuid(),
                AccountNumber = GenerateAccountNumber(),
                Balance = 0,
                CustomerId = customer.Id,
            };

            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Created account {AccountId} with number {AccountNumber}",
                account.Id, account.AccountNumber);

            return ServiceResult<AccountResponse>.Success(new AccountResponse
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                CustomerId = account.CustomerId
            });
        }

        public async Task<ServiceResult<TransactionResponse>> DepositAsync(Guid accountId, decimal amount)
        {
            // Validation
            if (amount <= 0)
            {
                return ServiceResult<TransactionResponse>.BadRequest("Amount must be positive.");
            }

            logger.LogInformation("Depositing {Amount} to account {AccountId}", amount, accountId);

            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var dbTransaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    var account = await dbContext.Accounts.FindAsync(accountId);
                    if (account is null)
                    {
                        return ServiceResult<TransactionResponse>.NotFound("Account not found.");
                    }

                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = account.Id,
                        Amount = amount,
                        Date = DateTime.UtcNow,
                        Type = TransactionType.Deposit
                    };

                    account.Balance += amount;

                    dbContext.Accounts.Update(account);
                    dbContext.Transactions.Add(transaction);
                    await dbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    logger.LogInformation("Deposited {Amount} to account {AccountId}. New balance: {Balance}",
                        amount, accountId, account.Balance);

                    return ServiceResult<TransactionResponse>.Success(new TransactionResponse
                    {
                        Id = transaction.Id,
                        AccountId = account.Id,
                        Amount = transaction.Amount,
                        Date = transaction.Date,
                        Type = transaction.Type
                    });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogWarning(ex, "Concurrency conflict during deposit to account {AccountId}", accountId);
                    return ServiceResult<TransactionResponse>.Conflict(
                        "The account was modified by another operation. Please retry.");
                }
            });
        }

        public async Task<ServiceResult<TransactionResponse>> WithdrawAsync(Guid accountId, decimal amount)
        {
            // Validation
            if (amount <= 0)
            {
                return ServiceResult<TransactionResponse>.BadRequest("Amount must be positive.");
            }

            logger.LogInformation("Withdrawing {Amount} from account {AccountId}", amount, accountId);

            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var dbTransaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    var account = await dbContext.Accounts.FindAsync(accountId);
                    if (account is null)
                    {
                        return ServiceResult<TransactionResponse>.NotFound("Account not found.");
                    }

                    if (amount > account.Balance)
                    {
                        return ServiceResult<TransactionResponse>.BadRequest("Insufficient balance.");
                    }

                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = account.Id,
                        Amount = amount,
                        Date = DateTime.UtcNow,
                        Type = TransactionType.Withdrawal
                    };

                    account.Balance -= amount;

                    dbContext.Accounts.Update(account);
                    dbContext.Transactions.Add(transaction);
                    await dbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    logger.LogInformation("Withdrew {Amount} from account {AccountId}. New balance: {Balance}",
                        amount, accountId, account.Balance);

                    return ServiceResult<TransactionResponse>.Success(new TransactionResponse
                    {
                        Id = transaction.Id,
                        AccountId = account.Id,
                        Amount = transaction.Amount,
                        Date = transaction.Date,
                        Type = transaction.Type
                    });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogWarning(ex, "Concurrency conflict during withdrawal from account {AccountId}", accountId);
                    return ServiceResult<TransactionResponse>.Conflict(
                        "The account was modified by another operation. Please retry.");
                }
            });
        }

        public async Task<ServiceResult<TransactionResponse>> TransferAsync(Guid fromAccountId, TransferRequest request)
        {
            // Validation
            if (request.Amount <= 0)
            {
                return ServiceResult<TransactionResponse>.BadRequest("Amount must be positive.");
            }

            if (request.ReceivedAccountId == Guid.Empty)
            {
                return ServiceResult<TransactionResponse>.BadRequest("ReceivedAccountId is required.");
            }

            if (fromAccountId == request.ReceivedAccountId)
            {
                return ServiceResult<TransactionResponse>.BadRequest("Cannot transfer to the same account.");
            }

            logger.LogInformation("Transferring {Amount} from account {FromAccountId} to {ToAccountId}",
                request.Amount, fromAccountId, request.ReceivedAccountId);

            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var dbTransaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    var fromAccount = await dbContext.Accounts.FindAsync(fromAccountId);
                    if (fromAccount is null)
                    {
                        return ServiceResult<TransactionResponse>.NotFound("Source account not found.");
                    }

                    if (request.Amount > fromAccount.Balance)
                    {
                        return ServiceResult<TransactionResponse>.BadRequest("Insufficient balance.");
                    }

                    var toAccount = await dbContext.Accounts.FindAsync(request.ReceivedAccountId);
                    if (toAccount is null)
                    {
                        return ServiceResult<TransactionResponse>.NotFound("Destination account not found.");
                    }

                    var now = DateTime.UtcNow;
                    var referenceId = Guid.NewGuid();

                    // Debit the sender (TransferOut)
                    var transferOut = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = fromAccount.Id,
                        Amount = request.Amount,
                        Date = now,
                        Type = TransactionType.TransferOut,
                        ReferenceId = referenceId
                    };

                    // Credit the receiver (TransferIn)
                    var transferIn = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = toAccount.Id,
                        Amount = request.Amount,
                        Date = now,
                        Type = TransactionType.TransferIn,
                        ReferenceId = referenceId
                    };

                    fromAccount.Balance -= request.Amount;
                    toAccount.Balance += request.Amount;

                    dbContext.Accounts.UpdateRange(fromAccount, toAccount);
                    dbContext.Transactions.AddRange(transferOut, transferIn);
                    await dbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    logger.LogInformation(
                        "Transfer completed. ReferenceId={ReferenceId}, From={FromAccountId} (balance={FromBalance}), To={ToAccountId} (balance={ToBalance})",
                        referenceId, fromAccountId, fromAccount.Balance, request.ReceivedAccountId, toAccount.Balance);

                    return ServiceResult<TransactionResponse>.Success(new TransactionResponse
                    {
                        Id = transferOut.Id,
                        AccountId = fromAccount.Id,
                        Amount = transferOut.Amount,
                        Date = transferOut.Date,
                        Type = transferOut.Type,
                        ReferenceId = referenceId
                    });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogWarning(ex, "Concurrency conflict during transfer from account {FromAccountId}", fromAccountId);
                    return ServiceResult<TransactionResponse>.Conflict(
                        "One of the accounts was modified by another operation. Please retry.");
                }
            });
        }
    }
}
