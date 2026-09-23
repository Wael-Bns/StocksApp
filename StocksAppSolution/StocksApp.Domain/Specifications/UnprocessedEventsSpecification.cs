using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

namespace StocksApp.Domain.Specifications
{
    public class UnprocessedEventsSpecification : BaseSpecification<Outbox>
    {
        public UnprocessedEventsSpecification() : base(o =>
        o.Status == OutboxStatus.Pending
        && (o.NextRetryAt == null || o.NextRetryAt <= DateTime.UtcNow)) 
        {
            ApplyOrderBy(o => o.CreatedAt);
        }
    }
}
