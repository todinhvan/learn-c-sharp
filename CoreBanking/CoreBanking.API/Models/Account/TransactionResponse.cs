using CoreBanking.Infrastructure.Entities;

namespace CoreBanking.API.Models.Account
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
