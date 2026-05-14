using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OpenIDConnect.Server.Helpers;
using OpenIDConnect.Server.Models;
using OpenIDConnect.Server.Repositories;
using System.Security.Claims;

namespace OpenIDConnect.Server.Controllers
{
    public class AuthorizeController(IUserRepository userRepository, ICodeItemRepository codeItemRepository) : Controller
    {
        [HttpGet]
        public IActionResult Index([FromQuery] AuthenticationRequestModel model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult Authorize(AuthenticationRequestModel model, string user, string[] scopes)
        {
            if (userRepository.FindByUserName(user) == null)
            {
                return View("UserNotFound");
            }
            var code = GeneratedCode();

            codeItemRepository.Add(code, new CodeItem()
            {
                AuthenticationRequest = model,
                Scopes = scopes,
                User = user
            });

            var flowModel = new CodeFlowResponseViewModel()
            {

                Code = code,
                State = model.State,
                RedirectUri = model.RedirectUri
            };

            return View("SubmitForm", flowModel);
        }

        [Route("oauth/token")]
        [HttpPost]
        public IActionResult ReturnTokens(string grant_type, string code, string redirect_uri)
        {
            if (grant_type != "authorization_code")
            {
                return BadRequest();
            }

            var codeItem = codeItemRepository.FindByCode(code);
            if (codeItem == null)
            {
                return BadRequest();
            }

            codeItemRepository.Delete(code);

            if (codeItem.AuthenticationRequest.RedirectUri != redirect_uri)
            {
                return BadRequest();
            }

            var jwk = JwkLoader.LoadFromDefault();

            var model = new AuthenticationResponseModel()
            {
                AccessToken = GenerateAccessToken(codeItem.User, string.Join(' ', codeItem.Scopes), codeItem.AuthenticationRequest.ClientId, codeItem.AuthenticationRequest.Nonce, jwk),
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshToken = GeneratedRefreshToken(),
                IdToken = GenerateIdToken(codeItem.User, codeItem.AuthenticationRequest.ClientId, codeItem.AuthenticationRequest.Nonce, jwk)
            };


            return Json(model);
        }

        private string GeneratedCode()
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 32)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GeneratedRefreshToken()
        {
            return GeneratedCode();
        }

        private string GenerateAccessToken(string userId, string scope, string audience, string nonce, JsonWebKey jsonWebKey)
        {
            // access_token can be the same as id_token, but here we might have different values for expirySeconds so we use 2 different functions

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new("scope", scope)
            };
            var idToken = JwtGenerator.GenerateJWTToken(
                20 * 60,
                "https://localhost:7006",
                audience,
                nonce,
                claims,
                jsonWebKey
                );

            return idToken;
        }

        private string GenerateIdToken(string userId, string audience, string nonce, JsonWebKey jsonWebKey)
        {
            // https://openid.net/specs/openid-connect-core-1_0.html#IDToken
            // we can return some claims defined here: https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId)
            };

            var idToken = JwtGenerator.GenerateJWTToken(
                20 * 60,
                "https://localhost:7006",
                audience,
                nonce,
                claims,
                jsonWebKey
                );

            return idToken;
        }


    }
}
