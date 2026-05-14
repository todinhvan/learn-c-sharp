namespace VanBlog.Core.Domain.Content
{
    public class PostSeries
    {
        public Guid PostId { get; set; }
        public Guid SeriesId { get; set; }
        public int SortOrder { get; set; }
    }
}
