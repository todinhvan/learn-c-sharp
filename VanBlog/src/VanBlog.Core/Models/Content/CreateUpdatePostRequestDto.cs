using System.ComponentModel.DataAnnotations;

namespace VanBlog.Core.Models.Content
{
    public class CreateUpdatePostRequestDto
    {
        [Required]
        [MaxLength(250)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(250)]
        public required string Slug { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Thumbnail { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public string? Content { get; set; }

        [MaxLength(128)]
        public string? Source { get; set; }

        [MaxLength(250)]
        public string? Tags { get; set; }
    }
}
