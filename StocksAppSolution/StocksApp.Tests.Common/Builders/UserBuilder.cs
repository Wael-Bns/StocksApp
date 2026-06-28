using StocksApp.Domain.Entities;

namespace StocksApp.Tests.Common.Builders
{
    public class UserBuilder
    {
        private Guid _userId = Guid.NewGuid();
        private string _userName = "DefaultUser";
        private string _email = "default@test.com";
        private string _passwordHash = "password_hash";
        private double _cashBalance = 100000;
        private string? _refreshToken;
        private DateTime? _refreshTokenExpiry;

        public UserBuilder WithUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public UserBuilder WithUserName(string userName)
        {
            _userName = userName;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserBuilder WithPasswordHash(string passwordHash)
        {
            _passwordHash = passwordHash;
            return this;
        }

        public UserBuilder WithCashBalance(double cashBalance)
        {
            _cashBalance = cashBalance;
            return this;
        }

        public UserBuilder WithRefreshToken(string? refreshToken)
        {
            _refreshToken = refreshToken;
            return this;
        }

        public UserBuilder WithRefreshTokenExpiry(DateTime? refreshTokenExpiry)
        {
            _refreshTokenExpiry = refreshTokenExpiry;
            return this;
        }

        public User Build() => new()
        {
            UserId = _userId,
            UserName = _userName,
            Email = _email,
            PasswordHash = _passwordHash,
            CashBalance = _cashBalance,
            RefreshToken = _refreshToken,
            RefreshTokenExpiry = _refreshTokenExpiry
        };
    }
}
