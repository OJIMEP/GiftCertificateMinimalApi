using System.Text.Json.Serialization;

namespace GiftCertificateMinimalApi.Contracts.V1.Responses
{
    public class CertGetResponse
    {
        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }
        [JsonPropertyName("sum")]
        public decimal Sum { get; set; }
    }
}
