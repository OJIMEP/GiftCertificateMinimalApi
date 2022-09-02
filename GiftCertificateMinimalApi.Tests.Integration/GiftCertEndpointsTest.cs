using DateTimeService.Areas.Identity.Models;
using FluentAssertions;
using GiftCertificateMinimalApi.Contracts.V1.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace GiftCertificateMinimalApi.Tests.Integration
{
    public class GiftCertEndpointsTest : IClassFixture<WebApplicationFactory<IApiMarker>>
    {
        private readonly WebApplicationFactory<IApiMarker> _factory;

        public GiftCertEndpointsTest(WebApplicationFactory<IApiMarker> factory)
        {
            _factory = factory;
        }

        protected async Task AuthenticateAsync(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "bearer", await GetJwtAsync(client));
        }

        private async Task<string> GetJwtAsync(HttpClient client)
        {
            var response = await client.PostAsJsonAsync("api/Authenticate/login", new LoginModel
            {
                Username = "admin@test.com",
                Password = "qwert1"
            });

            var loginResponse = await response.Content.ReadAsAsync<LoginResponse>();
            return loginResponse.Token;
        }

        [Fact]
        public async Task GetInfoAsync_WithValidBarcode()
        {
            // Arrange 
            var client = _factory.CreateClient();
            await AuthenticateAsync(client);

            // Act 
            var response = await client.GetAsync("api/GiftCert?barcode=CC13AVC5Yrw");

            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<CertGetResponse>();
            result.Should().BeEquivalentTo(new CertGetResponse
            {
                Barcode = "CC13AVC5Yrw",
                Sum = 0.11m
            });
        }

        [Theory]
        [InlineData("api/GiftCert?barcode=CC13AVC5YRK", "Certs aren't valid")]
        [InlineData("api/GiftCert?barcode=CC13AVC5YRK1", "Cert's barcode should be 11 symbols length")]
        [InlineData("api/GiftCert?barcode=CC13AVC5Kдг", "Cert's barcode is in wrong format - only latin symbols and digits are allowed")]
        public async Task GetInfoAsync_WithInvalidBarcode(string query, string expected)
        {
            // Arrange 
            var client = _factory.CreateClient();
            await AuthenticateAsync(client);

            // Act 
            var response = await client.GetAsync(query);

            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            (await response.Content.ReadAsAsync<ErrorResponse>()).Error.Should().Be(expected);
        }
    }
}
