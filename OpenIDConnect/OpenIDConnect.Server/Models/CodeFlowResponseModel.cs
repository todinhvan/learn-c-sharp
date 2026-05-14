using System.Text.Json.Serialization;

namespace OpenIDConnect.Server.Models
{
    public class CodeFlowResponseModel
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("state")]
        public required string State { get; set; }
    }
}
