using System.ComponentModel.DataAnnotations;

namespace StocksApp.Core.Domain.Entities
{
    public class ApplicationUser
    {
        [Key]
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public double CashBalance { get; set; } = 100000;
        public ICollection<BuyOrder>? BuyOrders { get; set; }
        public ICollection<SellOrder>? SellOrders { get; set; }
    }
}
