using System.Linq.Expressions;
using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Specifications
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
