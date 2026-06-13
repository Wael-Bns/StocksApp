using System.Net.Http.Headers;
using System.Net.Http.Json;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;

namespace StocksApp.IntegrationsTests.Helpers
{
    public class AuthHelper
    {
        private readonly HttpClient _client;
        private const string RegisterRoute = "/api/auth/register";
        private const string LoginRoute = "/api/auth/login";
        private const string RefreshRoute = "/api/auth/generate-new-access-token";

        public AuthHelper(HttpClient client)
        {
            _client = client;
        }

        public async Task<AuthenticationResponse> RegisterAsync(
            string username,
            string email,
            string password = "Password123!")
        {
            var response = await _client.PostAsJsonAsync(RegisterRoute, new UserAddRequest
            {
                UserName = username,
                Email = email,
                Password = password
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthenticationResponse>()
                   ?? throw new InvalidOperationException("Empty registration response");
        }

        public async Task<HttpResponseMessage> RegisterRawAsync(UserAddRequest request)
            => await _client.PostAsJsonAsync(RegisterRoute, request);

        public async Task<AuthenticationResponse> LoginAsync(string email, string password)
        {
            var response = await _client.PostAsJsonAsync(LoginRoute, new LoginRequest
            {
                Email = email,
                Password = password
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthenticationResponse>()
                   ?? throw new InvalidOperationException("Empty login response");
        }

        public async Task<HttpResponseMessage> LoginRawAsync(LoginRequest request)
            => await _client.PostAsJsonAsync(LoginRoute, request);

        public async Task<HttpResponseMessage> RefreshRawAsync(TokenModel tokenModel)
            => await _client.PostAsJsonAsync(RefreshRoute, tokenModel);

        public void AuthorizeClient(string token)
            => _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }
}
