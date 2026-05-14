namespace CoreBanking.API.Models.Customer
{
    public class CustomerResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Address { get; set; }
    }
}
