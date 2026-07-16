using System.Linq.Expressions;

namespace StocksApp.Domain.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
    }
}
