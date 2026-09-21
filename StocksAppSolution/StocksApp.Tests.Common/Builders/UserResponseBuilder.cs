using StocksApp.Core.DTO.UsersDTO;

namespace StocksApp.Tests.Common.Builders
{
    public class UserResponseBuilder
    {
        private Guid _userId = Guid.NewGuid();
        private string _userName = "TestUser";
        private string _email = "test@test.com";
        private string _passwordHash = "password_hash";
        private string? _refreshToken;
        private DateTime? _refreshTokenExpiry;

        public UserResponseBuilder WithUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public UserResponseBuilder WithUserName(string userName)
        {
            _userName = userName;
            return this;
        }

        public UserResponseBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserResponseBuilder WithPasswordHash(string passwordHash)
        {
            _passwordHash = passwordHash;
            return this;
        }

        public UserResponseBuilder WithRefreshToken(string refreshToken)
        {
            _refreshToken = refreshToken;
            return this;
        }

        public UserResponseBuilder WithRefreshTokenExpiry(DateTime refreshTokenExpiry)
        {
            _refreshTokenExpiry = refreshTokenExpiry;
            return this;
        }

        public UserResponse Build() => new()
        {
            UserId = _userId,
            UserName = _userName,
            Email = _email,
            PasswordHash = _passwordHash,
            RefreshToken = _refreshToken,
            RefreshTokenExpiry = _refreshTokenExpiry
        };
    }
}