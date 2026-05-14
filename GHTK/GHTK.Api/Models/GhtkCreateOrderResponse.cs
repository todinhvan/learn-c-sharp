using System.Text.Json.Serialization;

namespace GHTK.Api.Models
{
    public partial class GhtkCreateOrderResponse : ApiResponse
    {
        [JsonPropertyName("order")]
        public required OrderResponse Order { get; set; }
    }

    public class OrderResponse
    {
        [JsonPropertyName("partner_id")]
        public required string PartnerId { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = default!;

        [JsonPropertyName("area")]
        public int Area { get; set; }

        [JsonPropertyName("fee")]
        public double Fee { get; set; }

        [JsonPropertyName("insurance_fee")]
        public double InsuranceFee { get; set; }

        [JsonPropertyName("tracking_id")]
        public string TrackingId { get; set; } = default!;

        [JsonPropertyName("estimated_pick_time")]
        public string EstimatedPickTime { get; set; } = default!;

        [JsonPropertyName("estimated_deliver_time")]
        public string EstimatedDeliverTime { get; set; } = default!;

        [JsonPropertyName("products")]
        public GhtkProduct[] Products { get; set; } = [];

        [JsonPropertyName("status_id")]
        public int StatusId { get; set; }
    }
}
