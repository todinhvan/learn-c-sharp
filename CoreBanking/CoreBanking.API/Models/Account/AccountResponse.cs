namespace CoreBanking.API.Models.Account
{
    public class AccountResponse
    {
        public Guid Id { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public Guid CustomerId { get; set; }
    }
}
