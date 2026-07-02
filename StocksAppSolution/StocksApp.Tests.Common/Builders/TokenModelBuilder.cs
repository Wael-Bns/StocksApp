using StocksApp.Core.DTO.AuthenticationDTO;

namespace StocksApp.Tests.Common.Builders
{
    public class TokenModelBuilder
    {
        private string _token = "access_token";
        private string _refreshToken = "refresh_token";

        public TokenModelBuilder WithToken(string token)
        {
            _token = token;
            return this;
        }

        public TokenModelBuilder WithRefreshToken(string refreshToken)
        {
            _refreshToken = refreshToken;
            return this;
        }

        public TokenModel Build() => new()
        {
            Token = _token,
            RefreshToken = _refreshToken
        };
    }
}