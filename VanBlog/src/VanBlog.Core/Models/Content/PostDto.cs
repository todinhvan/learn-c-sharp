using AutoMapper;
using VanBlog.Core.Domain.Content;

namespace VanBlog.Core.Models.Content
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public string? Thumbnail { get; set; }
        public string? Content { get; set; }
        public Guid AuthorId { get; set; }
        public string? Source { get; set; }
        public string? Tags { get; set; }
        public string? SeoDescription { get; set; }
        public PostStatus Status { get; set; }
        public double RoyaltyAmount { get; set; }
        public bool IsPaid { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
