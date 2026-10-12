using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Specifications
{
    public class SellOrderByIdAndUserSpecification : BaseSpecification<SellOrder>
    {
        public SellOrderByIdAndUserSpecification(Guid orderId, Guid userId) : 
            base(o => o.SellOrderID == orderId && o.UserId == userId) 
        {
            ApplyNoTracking();
        }
    }
}
