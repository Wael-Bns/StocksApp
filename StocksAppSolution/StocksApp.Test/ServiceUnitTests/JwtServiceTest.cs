using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class JwtServiceTest
    {
        private readonly JwtOptions _jwtOptions;
        private readonly ITokenService _jwtService;

        public JwtServiceTest()
        {
            _jwtOptions = new JwtOptions
            {
                Issuer = "issuer",
                Audience = "audience",
                Key = "super_secret_key_12345678901234567890",
                EXPIRATION_MINUTES = 15
            };

            _jwtService = new JwtService(Options.Create(_jwtOptions));
        }

        [Fact]
        public void CreateAccessToken_ValidInput_ReturnsTokenWithExpectedClaims()
        {
            Guid userId = Guid.NewGuid();
            string userName = "TestUser";
            string email = "test@test.com";

            string token = _jwtService.CreateAccessToken(userId, userName, email);
            ClaimsPrincipal? principal = _jwtService.GetPrincipalFromAccessToken(token);

            token.Should().NotBeNullOrWhiteSpace();
            principal.Should().NotBeNull();
            principal!.FindFirstValue(ClaimTypes.Email).Should().Be(email);
            principal.FindFirstValue(ClaimTypes.Name).Should().Be(userName);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsValidBase64String()
        {
            string refreshToken = _jwtService.GenerateRefreshToken();

            refreshToken.Should().NotBeNullOrWhiteSpace();
            Action action = () => Convert.FromBase64String(refreshToken);
            action.Should().NotThrow();
        }

        [Fact]
        public void GetPrincipalFromAccessToken_InvalidToken_ThrowsSecurityTokenException()
        {
            var otherOptions = new JwtOptions
            {
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                Key = "different_secret_key_12345678901234567890",
                EXPIRATION_MINUTES = _jwtOptions.EXPIRATION_MINUTES
            };
            var otherService = new JwtService(Options.Create(otherOptions));

            string token = otherService.CreateAccessToken(Guid.NewGuid(), "User", "user@test.com");
            Action action = () => _jwtService.GetPrincipalFromAccessToken(token);

            action.Should().Throw<SecurityTokenException>();
        }
    }
}