namespace VanBlog.Core.Models.Common
{
    public class ApiSuccessResponseDto<T> : ReponseApiBase
    {
        public T? Data { get; set; }
    }
}
