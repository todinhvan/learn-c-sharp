using System;
using System.Collections.Generic;
using System.Text;

namespace VanBlog.Core.Models.Common
{
    public class ApiErrorResponseDto<T> : ReponseApiBase
    {
        public T? Error { get; set; }
    }
}
