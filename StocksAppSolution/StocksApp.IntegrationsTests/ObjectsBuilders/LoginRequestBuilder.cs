using StocksApp.Core.DTO.AuthenticationDTO;

namespace StocksApp.IntegrationsTests.ObjectsBuilders
{
    public class LoginRequestBuilder
    {
        private string _email = "default@test.com";
        private string _password = "Password123!";

        public LoginRequestBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public LoginRequestBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public LoginRequest Build() => new()
        {
            Email = _email,
            Password = _password
        };
    }
}
