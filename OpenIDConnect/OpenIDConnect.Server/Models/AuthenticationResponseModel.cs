using System.Text.Json.Serialization;

namespace OpenIDConnect.Server.Models
{
    public class AuthenticationResponseModel : RefreshResponseModel
    {
        [JsonPropertyName("id_token")]
        public required string IdToken { get; set; }
    }
}
