namespace VanBlog.Core.Domain.Content
{
    public class Series
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsActive { get; set; }
        public Guid AuthorId { get; set; }
        public string? SeoDescription { get; set; }
        public string? Content { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
