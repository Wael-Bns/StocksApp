using System.Linq.Expressions;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

namespace StocksApp.Domain.Specifications
{
    public class PendingSellOrdersSpecification : ISpecification<SellOrder>
    {
        public Expression<Func<SellOrder, bool>> Criteria { get; }
        public PendingSellOrdersSpecification()
        {
            Criteria = order => 
                            order.Status == SellOrderStatus.Pending;
        }
    }
}
