using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.IntegrationsTests.Collection;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.IntegrationsTests.Helpers;

namespace StocksApp.IntegrationsTests.Tests
{
    [Collection(ExpiredTokenCollection.Name)]
    public class AuthTokenExpiryTests : ExpiredTokenTestBase
    {
        private readonly AuthHelper _auth;
        public AuthTokenExpiryTests(ExpiredTokenWebApplicationFactory factory) : base(factory) 
        { 
            _auth = new AuthHelper(Client);
        }

        [Fact]
        public async Task RefreshToken_ExpiredAccessToken_WithValidRefreshToken_ReturnsNewToken()
        {
            var initial = await _auth.RegisterAsync("TestUser", "expiry@test.com");

            var response = await _auth.RefreshRawAsync(new TokenModel
            {
                Token = initial.Token!,
                RefreshToken = initial.RefreshToken!
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var newTokens = await response.Content
                .ReadFromJsonAsync<AuthenticationResponse>();

            newTokens!.Token.Should().NotBeNullOrWhiteSpace();
            newTokens.Token.Should().NotBe(initial.Token);
        }

        [Fact]
        public async Task RefreshToken_ExpiredAccessToken_WithInvalidRefreshToken_ReturnsError()
        {
            var initial = await _auth.RegisterAsync("TestUser", "expiry_invalid@test.com");

            var response = await _auth.RefreshRawAsync(new TokenModel
            {
                Token = initial.Token!,
                RefreshToken = "invalid-refresh-token"
            });

            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }
    }
}
