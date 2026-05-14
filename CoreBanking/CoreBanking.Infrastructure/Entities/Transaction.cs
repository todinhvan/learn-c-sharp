using System.Text.Json.Serialization;

namespace CoreBanking.Infrastructure.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }

        /// <summary>
        /// Links paired transfer transactions together.
        /// Both the TransferOut and TransferIn records share the same ReferenceId.
        /// </summary>
        public Guid? ReferenceId { get; set; }

        [JsonIgnore]
        public Account Account { get; set; } = default!;
    }

    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1,
        TransferOut = 2,
        TransferIn = 3
    }
}
