using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Specifications
{
    public class UnprocessedEventsSpecification : BaseSpecification<Outbox>
    {
        public UnprocessedEventsSpecification() : base(o => o.ProcessedAt == null) { }
    }
}
