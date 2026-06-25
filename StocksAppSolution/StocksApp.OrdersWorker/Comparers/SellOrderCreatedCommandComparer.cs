using StocksApp.Domain.Events;

namespace StocksApp.OrdersWorker.Comparers
{
    public class SellOrderCreatedCommandComparer : IComparer<SellOrderCreatedCommand>
    {
        public int Compare(SellOrderCreatedCommand? x, SellOrderCreatedCommand? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            int priceComparison = x.Price.CompareTo(y.Price);
            if (priceComparison != 0)
            {
                return priceComparison;
            }

            int timeComparison = x.CreatedAt.CompareTo(y.CreatedAt);
            if (timeComparison != 0)
            {
                return timeComparison;
            }

            return x.SellOrderId.CompareTo(y.SellOrderId);
        }
    }
}
