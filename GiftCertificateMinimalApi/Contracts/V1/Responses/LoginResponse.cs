using DateTimeService.Areas.Identity.Models;
using System.Text.Json.Serialization;

namespace GiftCertificateMinimalApi.Contracts.V1.Responses
{
    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        [JsonPropertyName("expiration")]
        public DateTime Expiration { get; set; }
        [JsonPropertyName("refresh")]
        public string Refresh { get; set; } = string.Empty;
        [JsonPropertyName("expiration_refresh")]
        public DateTime ExpirationRefresh { get; set; }

        public LoginResponse()
        {
        }

        public LoginResponse(AuthenticateResponse response)
        {
            Token = response.JwtToken;
            Expiration = response.JwtValidTo;
            Refresh = response.RefreshToken;
            ExpirationRefresh = response.RefreshValidTo;
        }
    }
}
