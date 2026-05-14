namespace VanBlog.Core.Models.Common
{
    public class PaginationBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount
        {
            get
            {
                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }
        public int TotalItems { get; set; }
    }
}
