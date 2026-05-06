using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO.UsersDTO
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public decimal CashBalance { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    public static class UserExtensions
    {
        public static UserResponse ToUserResponse(this User user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                CashBalance = user.CashBalance,
                RefreshToken = user.RefreshToken,
                PasswordHash = user.PasswordHash,
                RefreshTokenExpiry = user.RefreshTokenExpiry
            };
        }
    }
}