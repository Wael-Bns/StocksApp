using StocksApp.Domain.Enums;

namespace StocksApp.Domain.Entities
{
    public class SellOrder
    {
        public Guid SellOrderID { get; private set; }
        public string StockSymbol { get; private set; } = null!;
        public string? StockName { get; private set; }
        public DateTime DateAndTimeOfOrder { get; private set; }
        public DateTime ActivatesAt { get; private set; }
        public uint Quantity { get; private set; }
        public double Price { get; private set; }
        public SellOrderStatus Status { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; set; } = null!;

        private SellOrder() { }

        public static SellOrder Create(
            Guid userId, string stockSymbol, string? stockName,
            double price, uint quantity, DateTime createdAt, TimeSpan matchBucketSize)
        {
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price));
            if (quantity == 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            return new SellOrder
            {
                SellOrderID = Guid.NewGuid(),
                UserId = userId,
                StockSymbol = stockSymbol,
                StockName = stockName,
                Price = price,
                Quantity = quantity,
                DateAndTimeOfOrder = createdAt,
                ActivatesAt = CeilToBucket(createdAt, matchBucketSize),
                Status = SellOrderStatus.Pending
            };
        }

        public void MarkExecuted(User user)
        {
            if (Status != SellOrderStatus.Pending)
                throw new InvalidOperationException($"Cannot execute a {Status} order.");
            if (user.UserId != UserId)
                throw new ArgumentException("User does not own this order.", nameof(user));

            Status = SellOrderStatus.Executed;
            user.CashBalance += Price * Quantity;
        }

        private static DateTime CeilToBucket(DateTime t, TimeSpan bucket)
        {
            var ticks = (t.Ticks + bucket.Ticks - 1) / bucket.Ticks * bucket.Ticks;
            return new DateTime(ticks, t.Kind);
        }
    }
}