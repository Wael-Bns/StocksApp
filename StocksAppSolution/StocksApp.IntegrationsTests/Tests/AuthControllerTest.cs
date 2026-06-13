using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.IntegrationsTests.Collection;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.IntegrationsTests.Helpers;
using StocksApp.IntegrationsTests.ObjectsBuilders;

namespace StocksApp.IntegrationsTests.Tests
{
    [Collection(IntegrationTestsCollection.Name)]
    public class AuthControllerTest : IntegrationTestBase
    {
        private readonly AuthHelper _auth;

        public AuthControllerTest(CustomWebApplicationFactory factory) : base(factory)
        {
            _auth = new AuthHelper(Client);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkAndToken()
        {
            var result = await _auth.RegisterAsync("TestUser", "valid@test.com");

            result.Should().NotBeNull();
            result.Email.Should().Be("valid@test.com");
            result.Token.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            var request = new UserAddRequestBuilder()
                .WithEmail("duplicate@test.com")
                .WithUsername("User1")
                .Build();

            await _auth.RegisterRawAsync(request);

            request.UserName = "User2";
            var response = await _auth.RegisterRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequest()
        {
            var request = new UserAddRequestBuilder()
                .WithEmail("")
                .WithPassword("")
                .Build();

            var response = await _auth.RegisterRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            await _auth.RegisterAsync("TestUser", "login@test.com");

            var result = await _auth.LoginAsync("login@test.com", "Password123!");

            result.Token.Should().NotBeNullOrWhiteSpace();
        }
        [Fact]
        public async Task Login_InvalidEmail_ReturnsBadRequest()
        {
            var request = new LoginRequestBuilder()
                .WithEmail("doesnotexist@test.com")
                .Build();

            var response = await _auth.LoginRawAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        [Fact]
        public async Task Login_InvalidPassword_ReturnsBadRequest()
        {
            await _auth.RegisterAsync("TestUser", "user@test.com");

            var request = new LoginRequestBuilder()
                .WithEmail("user@test.com")
                .WithPassword("WrongPassword")
                .Build();

            var response = await _auth.LoginRawAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        [Fact]
        public async Task RefreshToken_ValidTokens_ProducesUsableToken()
        {
            var initial = await _auth.RegisterAsync("TestUser", "refresh@test.com");

            var refreshResponse = await _auth.RefreshRawAsync(new TokenModel
            {
                Token = initial.Token!,
                RefreshToken = initial.RefreshToken!
            });

            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var newTokens = await refreshResponse.Content
                .ReadFromJsonAsync<AuthenticationResponse>();

            newTokens!.Token.Should().NotBe(initial.Token);

            _auth.AuthorizeClient(newTokens.Token!);
            var protectedResponse = await Client.GetAsync("/api/trade/allbuyorders");
            protectedResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }
    }
}

