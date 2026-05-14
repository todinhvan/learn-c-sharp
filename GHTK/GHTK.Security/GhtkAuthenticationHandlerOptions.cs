using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace GHTK.Security
{
    public class GhtkAuthenticationHandlerOptions : AuthenticationSchemeOptions
    {
        public Func<string, SecurityToken, ClaimsPrincipal, Task<bool>> ValidatorAsync { get; set; }
                = (xClientSource, jwtToken, pricipal) => Task.FromResult(false);
        public string jwtSecretKey { get; set; } = default!;
    }
}
