namespace CoreBanking.API.Models.Account
{
    public class TransferRequest
    {
        public Guid ReceivedAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
