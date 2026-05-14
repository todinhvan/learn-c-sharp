using CoreBanking.API.Models;
using CoreBanking.API.Models.Account;
using CoreBanking.API.Models.Customer;
using CoreBanking.API.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.API.Apis
{
    public static class CoreBankingApi
    {
        public static IEndpointRouteBuilder MapCoreBankingApi(this IEndpointRouteBuilder builder)
        {
            var versionApi = builder.NewVersionedApi("CoreBanking");
            var v1 = versionApi.MapGroup("api/v{version:apiVersion}/core-banking").HasApiVersion(1, 0);

            v1.MapGet("/customers", GetCustomers);
            v1.MapGet("/customers/{id:guid}", GetCustomerById);
            v1.MapPost("/customers", CreateCustomer);

            v1.MapGet("/accounts", GetAccounts);
            v1.MapGet("/accounts/{id:guid}", GetAccountById);
            v1.MapPost("/accounts", CreateAccount);
            v1.MapPut("/accounts/{id:guid}/deposit", Deposit);
            v1.MapPut("/accounts/{id:guid}/withdraw", Withdraw);
            v1.MapPut("/accounts/{id:guid}/transfer", Transfer);

            return builder;
        }

        // ────────────────────────────────────────────
        // Customer endpoints
        // ────────────────────────────────────────────

        private static async Task<Ok<PaginationResponse<CustomerResponse>>> GetCustomers(
            ICustomerService customerService,
            [AsParameters] PaginationRequest pagination)
        {
            var result = await customerService.GetCustomersAsync(pagination.PageIndex, pagination.PageSize);
            return TypedResults.Ok(result.Value!);
        }

        private static async Task<Results<Ok<CustomerResponse>, NotFound<string>>> GetCustomerById(
            ICustomerService customerService,
            Guid id)
        {
            var result = await customerService.GetCustomerByIdAsync(id);
            return result.IsSuccess
                ? TypedResults.Ok(result.Value!)
                : TypedResults.NotFound(result.Error!);
        }

        private static async Task<Results<Ok<CustomerResponse>, BadRequest<string>>> CreateCustomer(
            ICustomerService customerService,
            [FromBody] CreateCustomerRequest model)
        {
            var result = await customerService.CreateCustomerAsync(model);
            return result.IsSuccess
                ? TypedResults.Ok(result.Value!)
                : TypedResults.BadRequest(result.Error!);
        }

        // ────────────────────────────────────────────
        // Account endpoints
        // ────────────────────────────────────────────

        private static async Task<Ok<PaginationResponse<AccountResponse>>> GetAccounts(
            IAccountService accountService,
            [AsParameters] PaginationRequest pagination,
            Guid? customerId = null)
        {
            var result = await accountService.GetAccountsAsync(pagination.PageIndex, pagination.PageSize, customerId);
            return TypedResults.Ok(result.Value!);
        }

        private static async Task<Results<Ok<AccountResponse>, NotFound<string>>> GetAccountById(
            IAccountService accountService,
            Guid id)
        {
            var result = await accountService.GetAccountByIdAsync(id);
            return result.IsSuccess
                ? TypedResults.Ok(result.Value!)
                : TypedResults.NotFound(result.Error!);
        }

        private static async Task<Results<Ok<AccountResponse>, BadRequest<string>>> CreateAccount(
            IAccountService accountService,
            [FromBody] CreateAccountRequest model)
        {
            var result = await accountService.CreateAccountAsync(model);
            return result.IsSuccess
                ? TypedResults.Ok(result.Value!)
                : TypedResults.BadRequest(result.Error!);
        }

        private static async Task<Results<Ok<TransactionResponse>, BadRequest<string>, NotFound<string>, Conflict<string>>> Deposit(
            IAccountService accountService,
            [FromBody] DepositRequest model,
            Guid id)
        {
            var result = await accountService.DepositAsync(id, model.Amount);
            return MapTransactionResult(result);
        }

        private static async Task<Results<Ok<TransactionResponse>, BadRequest<string>, NotFound<string>, Conflict<string>>> Withdraw(
            IAccountService accountService,
            [FromBody] WithdrawalRequest model,
            Guid id)
        {
            var result = await accountService.WithdrawAsync(id, model.Amount);
            return MapTransactionResult(result);
        }

        private static async Task<Results<Ok<TransactionResponse>, BadRequest<string>, NotFound<string>, Conflict<string>>> Transfer(
            IAccountService accountService,
            [FromBody] TransferRequest model,
            Guid id)
        {
            var result = await accountService.TransferAsync(id, model);
            return MapTransactionResult(result);
        }

        // ────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────

        private static Results<Ok<TransactionResponse>, BadRequest<string>, NotFound<string>, Conflict<string>> MapTransactionResult(
            ServiceResult<TransactionResponse> result)
        {
            if (result.IsSuccess)
                return TypedResults.Ok(result.Value!);

            return result.ErrorType switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error!),
                ErrorType.Conflict => TypedResults.Conflict(result.Error!),
                _ => TypedResults.BadRequest(result.Error!)
            };
        }
    }
}
