using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using StocksApp.Core.DTO.UsersDTO;
namespace StocksApp.IntegrationTests
{
    public class AuthControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        public AuthControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost");
        }

        [Fact]
        public async Task Register_ToBeSuccessful()
        {
            ApplicationUserRegisterDTO user = new ApplicationUserRegisterDTO
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "test123",
                ConfirmPassword = "test123"
            };
            //Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", user);
            //Assert
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
