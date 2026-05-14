using System.Text.Json.Serialization;

namespace CoreBanking.Infrastructure.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Address { get; set; }

        [JsonIgnore]
        public ICollection<Account> Accounts { get; set; } = [];
    }
}
