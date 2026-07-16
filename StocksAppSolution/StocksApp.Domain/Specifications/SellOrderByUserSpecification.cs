using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Specifications
{
    public class SellOrderByUserSpecification : BaseSpecification<SellOrder>
    {
        public SellOrderByUserSpecification(Guid userId) : base(o => o.UserId == userId)
        {
            ApplyNoTracking();
        }
    }
}