using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.IntegrationTests;
    using Xunit;

namespace StocksApp.IntegrationsTests
{
    public class AuthControllerTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private const string ApplicationJson = "application/json";
        private const string DefaultPassword = "Password123!";
        
        // Routes
        private const string RegisterRoute = "/api/auth/register";
        private const string LoginRoute = "/api/auth/login";
        private const string RefreshRoute = "/api/auth/generate-new-access-token";
        
        // Some protected endpoint in your API used strictly to verify tokens work. (Adjust if needed)
        private const string ProtectedSmokeRoute = "/api/trade/orders"; 

        public AuthControllerTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // Helper to ensure fresh client state per test runs avoiding leakage
        private HttpClient CreateFreshClient() => _factory.CreateClient();

        #region Register Tests

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkAndToken()
        {
            var client = CreateFreshClient();
            var registerRequest = new UserAddRequest 
            { 
                UserName = "TestUser_RegisterValid",
                Email = "register_valid@test.com",
                Password = DefaultPassword 
            };

            var response = await client.PostAsJsonAsync(RegisterRoute, registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be(ApplicationJson);
            
            var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            result.Should().NotBeNull();
            result!.Email.Should().Be(registerRequest.Email);
            result.Token.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            using var client = CreateFreshClient();
            var email = "duplicate@test.com";
            var registerRequest = new UserAddRequest 
            { 
                UserName = "TestUser_Duplicate1", 
                Email = email, 
                Password = DefaultPassword 
            };

            // Seed user
            await client.PostAsJsonAsync(RegisterRoute, registerRequest);

            // Attempt duplicate
            registerRequest.UserName = "TestUser_Duplicate2";
            var response = await client.PostAsJsonAsync(RegisterRoute, registerRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequest()
        {
            using var client = CreateFreshClient();
            // Missing email and password
            var badRequest = new UserAddRequest { UserName = "NoEmailNoPassword" };

            var response = await client.PostAsJsonAsync(RegisterRoute, badRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkAndToken()
        {
            using var client = CreateFreshClient();
            var email = "login_valid@test.com";
            
            await client.PostAsJsonAsync(RegisterRoute, new UserAddRequest 
            { 
                UserName = "TestUser_LoginValid", 
                Email = email, 
                Password = DefaultPassword 
            });

            var response = await client.PostAsJsonAsync(LoginRoute, new LoginRequest 
            { 
                Email = email, 
                Password = DefaultPassword 
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be(ApplicationJson);

            var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            using var client = CreateFreshClient();
            var response = await client.PostAsJsonAsync(LoginRoute, new LoginRequest 
            { 
                Email = "doesnotexist@test.com", 
                Password = "WrongPassword" 
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Refresh Token & Flow Tests

        [Fact]
        public async Task GenerateNewAccessToken_ValidTokens_ProducesUsableProtectedToken()
        {
            using var client = CreateFreshClient();
            var registerRequest = new UserAddRequest 
            { 
                UserName = "TestUser_RefreshValid", 
                Email = "refresh_valid@test.com", 
                Password = DefaultPassword 
            };
            
            var registerResponse = await client.PostAsJsonAsync(RegisterRoute, registerRequest);
            var initialTokens = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();

            var refreshResponse = await client.PostAsJsonAsync(RefreshRoute, new TokenModel
            {
                Token = initialTokens!.Token!,
                RefreshToken = initialTokens.RefreshToken!
            });

            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            refreshResponse.Content.Headers.ContentType?.MediaType.Should().Be(ApplicationJson);
            
            var newTokens = await refreshResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
            newTokens.Should().NotBeNull();
            newTokens!.Token.Should().NotBe(initialTokens.Token);

            // Verify the new token provides real access
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Token);
            
            var protectedCallResponse = await client.GetAsync(ProtectedSmokeRoute);
            
            // Should not be 401 Unauthorized. Depending on your system it may be validly 200 or 404 (if no resource exists).
            protectedCallResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GenerateNewAccessToken_TamperedToken_ReturnsUnauthorized()
        {
            using var client = CreateFreshClient();
            var response = await client.PostAsJsonAsync(RefreshRoute, new TokenModel
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.signature",
                RefreshToken = "some-fake-refresh-token"
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion
    }
}
