using StocksApp.Core.DTO.UsersDTO;

namespace StocksApp.IntegrationsTests.ObjectsBuilders
{
    public class UserAddRequestBuilder
    {
        private string _username = "DefaultUser";
        private string _email = "default@test.com";
        private string _password = "Password123!";

        public UserAddRequestBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public UserAddRequestBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserAddRequestBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public UserAddRequest Build() => new()
        {
            UserName = _username,
            Email = _email,
            Password = _password
        };
    }
}
