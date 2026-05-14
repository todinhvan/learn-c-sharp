namespace CoreBanking.API.Models
{
    public class PaginationResponse<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount {
            get
            {
                return (int)Math.Ceiling((double)TotalItems / (double)PageSize);
            }
        }
        public int TotalItems { get; set; }
        public IEnumerable<T> Items { get; set; } = [];
    }
}
