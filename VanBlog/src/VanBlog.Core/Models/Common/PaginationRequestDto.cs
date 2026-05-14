namespace VanBlog.Core.Models.Common
{
    public class PaginationRequestDto
    {
        public string? Keyword { get; set; }
        public Guid? CategoryId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
