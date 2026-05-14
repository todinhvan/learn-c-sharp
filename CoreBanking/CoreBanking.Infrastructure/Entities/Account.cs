using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoreBanking.Infrastructure.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Concurrency token for optimistic concurrency control.
        /// PostgreSQL uses xmin system column mapped via EF Core.
        /// </summary>
        [Timestamp]
        public uint RowVersion { get; set; }

        [JsonIgnore]
        public Customer Customer { get; set; } = default!;

        [JsonIgnore]
        public ICollection<Transaction> Transactions { get; set; } = [];
    }
}
