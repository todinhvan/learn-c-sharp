using System.ComponentModel.DataAnnotations;

namespace CoreBanking.API.Models.Customer
{
    public class CreateCustomerRequest
    {
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
    }
}
