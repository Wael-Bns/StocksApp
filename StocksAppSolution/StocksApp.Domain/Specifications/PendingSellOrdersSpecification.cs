using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

namespace StocksApp.Domain.Specifications
{
    public class PendingSellOrdersSpecification : BaseSpecification<SellOrder>
    {
        public PendingSellOrdersSpecification() : base(o => o.Status == SellOrderStatus.Pending)
        {
            ApplyNoTracking();
        }
    }
}