namespace StocksApp.Core.Domain.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public double CashBalance { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public ICollection<BuyOrder>? BuyOrders { get; set; }
        public ICollection<SellOrder>? SellOrders { get; set; }
    }
}
