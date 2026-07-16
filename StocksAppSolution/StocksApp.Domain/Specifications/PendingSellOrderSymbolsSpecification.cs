using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

namespace StocksApp.Domain.Specifications
{
    public class PendingSellOrderSymbolsSpecification : BaseSpecification<SellOrder, string>
    {
        public PendingSellOrderSymbolsSpecification() : base(o => o.Status == SellOrderStatus.Pending)
        {
            Selector = o => o.StockSymbol!;
            ApplyNoTracking();
        }
    }
}