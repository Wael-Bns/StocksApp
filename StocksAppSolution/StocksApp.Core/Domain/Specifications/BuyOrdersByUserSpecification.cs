using System.Linq.Expressions;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.Domain.Specifications
{
    public class BuyOrdersByUserSpecification : ISpecification<BuyOrder>
    {
        public Expression<Func<BuyOrder, bool>> Criteria { get; }
        public BuyOrdersByUserSpecification(Guid userId)
        {
            Criteria = order => order.UserId == userId;
        }
    }
}
