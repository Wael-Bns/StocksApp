using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO.UsersDTO
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public decimal CashBalance { get; set; }
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
                CashBalance = user.CashBalance
            };
        }
    }
}