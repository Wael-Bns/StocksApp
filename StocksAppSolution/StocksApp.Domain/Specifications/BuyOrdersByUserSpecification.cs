using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Specifications
{
    public class BuyOrdersByUserSpecification : BaseSpecification<BuyOrder>
    {
        public BuyOrdersByUserSpecification(Guid userId) : base(o => o.UserId == userId)
        {
            ApplyNoTracking();
        }
    }
}