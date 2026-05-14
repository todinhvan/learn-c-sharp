using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace GHTK.Security
{
    public class GhtkAuthenticationHandler(IOptionsMonitor<GhtkAuthenticationHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : AuthenticationHandler<GhtkAuthenticationHandlerOptions>(options, logger, encoder, clock)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var clientSourceHeader = Context.Request.Headers["X-Client-Source"];
            var tokenHeader = Context.Request.Headers["Token"];

            if (clientSourceHeader.Count == 0)
            {
                return AuthenticateResult.Fail("Missing X-Client-Source from header");
            }

            if (tokenHeader.Count == 0)
            {
                return AuthenticateResult.Fail("Missing Token from header");
            }

            var clientSourceValue = clientSourceHeader.FirstOrDefault();
            var tokenValue = tokenHeader.FirstOrDefault();

            if (!string.IsNullOrEmpty(clientSourceValue)
                && !string.IsNullOrEmpty(tokenValue)
                && ValidateClient(clientSourceValue, tokenValue, out SecurityToken? token, out ClaimsPrincipal? principal))
            {
                if (!await Options.ValidatorAsync(clientSourceValue, token!, principal!))
                {
                    return AuthenticateResult.Fail("Invalid Client Source");
                }

                ((ClaimsIdentity)principal!.Identity!).AddClaim(new Claim("PartnerId", clientSourceValue));
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("Invalid Token");
            }
        }

        private bool ValidateClient(string clientSourceValue, string tokenValue, out SecurityToken? token, out ClaimsPrincipal? principal)
        {
            if (!ValidateToken(tokenValue, out token, out principal))
            {
                return false;
            }

            var sub = ((JwtSecurityToken)token!).Subject;
            if (!string.IsNullOrEmpty(sub) || clientSourceValue != sub)
            {
                return false;
            }
            return true;
        }

        private bool ValidateToken(string tokenValue, out SecurityToken? token, out ClaimsPrincipal? principal)
        {
            IdentityModelEventSource.ShowPII = true;
            IdentityModelEventSource.LogCompleteSecurityArtifact = true;

            var handler = new JwtSecurityTokenHandler();
            var tokenValidationParams = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Options.jwtSecretKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                principal = handler.ValidateToken(tokenValue, tokenValidationParams, out token);
                return true;
            }
            catch(Exception)
            {
                token = null;
                principal = null;
                return false;
            }
        }
    }
}
