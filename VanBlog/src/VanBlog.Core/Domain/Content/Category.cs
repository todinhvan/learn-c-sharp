namespace VanBlog.Core.Domain.Content
{
    public class Category
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public bool IsActive { get; set; }
        public string? SeoDescription { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
