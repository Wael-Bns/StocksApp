using System.Linq.Expressions;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.Domain.Specifications
{
    public class SellOrderByUserSpecification : ISpecification<SellOrder>
    {
        public Expression<Func<SellOrder, bool>> Criteria { get;  }
        public SellOrderByUserSpecification(Guid userId)
        {
            Criteria = order => order.UserId == userId;
        }
    }
}
