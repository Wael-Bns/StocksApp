using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO.UsersDTO
{
    public class ApplicationUserResponse
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public double CashBalance { get; set; }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is ApplicationUserResponse other)
            {
                return UserId == other.UserId &&
                       UserName == other.UserName &&
                       Email == other.Email &&
                       CashBalance == other.CashBalance;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, UserName, Email, CashBalance);
        }
    }

    public static class ApplicationUserExtensions
    {
        public static ApplicationUserResponse ToApplicationUserResponse(this ApplicationUser user)
        {
            return new ApplicationUserResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                CashBalance = user.CashBalance
            };
        }
    }
}
