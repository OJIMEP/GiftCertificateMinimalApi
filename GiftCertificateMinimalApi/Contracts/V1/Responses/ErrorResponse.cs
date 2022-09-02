using System.Text.Json.Serialization;

namespace GiftCertificateMinimalApi.Contracts.V1.Responses
{
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
