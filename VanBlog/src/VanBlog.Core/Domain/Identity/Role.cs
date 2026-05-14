using Microsoft.AspNetCore.Identity;

namespace VanBlog.Core.Domain.Identity
{
    public class Role : IdentityRole<Guid>
    {
        public required string DisplayName { get; set; }
    }
}
