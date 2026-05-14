using System.Net;

namespace VanBlog.Core.Models.Common
{
    public class ReponseApiBase
    {
        public HttpStatusCode StatusCode { get; set; }
        public required string Message { get; set; }
    }
}
