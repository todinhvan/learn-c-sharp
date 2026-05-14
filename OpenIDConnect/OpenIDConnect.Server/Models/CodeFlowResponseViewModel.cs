using System.Text.Json.Serialization;

namespace OpenIDConnect.Server.Models
{
    public class CodeFlowResponseViewModel : CodeFlowResponseModel
    {
        [JsonPropertyName("redirect_uri")]
        public required string RedirectUri { get; set; }
    }
}
