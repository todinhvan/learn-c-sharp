using System;
using System.Collections.Generic;
using System.Text;

namespace VanBlog.Core.Models.Common
{
    public class PaginationResponseDto<T> : PaginationBase
    {
        public List<T> Items { get; set; } = [];
    }
}
