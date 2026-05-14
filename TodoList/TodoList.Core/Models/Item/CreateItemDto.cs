using System.ComponentModel.DataAnnotations;

namespace TodoList.Core.Models.Item
{
    public class CreateItemDto
    {
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
