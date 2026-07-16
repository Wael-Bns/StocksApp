using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Specifications
{
    public class SellOrdersByIdsSpecification : BaseSpecification<SellOrder>
    {
        public SellOrdersByIdsSpecification(List<Guid> sellOrderIds)
            : base(o => sellOrderIds.Contains(o.SellOrderID))
        {
            AddInclude(o => o.User);
        }
    }
}