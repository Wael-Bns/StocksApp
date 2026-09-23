using Microsoft.EntityFrameworkCore;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;

namespace StocksApp.Infrastructure.Repositories
{
    public class OutboxRepository : GenericRepository<Outbox>, IOutboxRepository
    {
        private readonly ApplicationDbContext _context;
        public OutboxRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Outbox>> GetUnprocessedEvents()
        {
            var spec = new UnprocessedEventsSpecification();
            var unprocessedEvents = await ListAsync(spec);
            return unprocessedEvents;
        }
        public async Task MarkAsProcessed(Guid outboxId)
        {
            await _context.Set<Outbox>()
                .Where(o => o.OutboxId == outboxId)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.ProcessedAt, DateTime.UtcNow)
                                          .SetProperty(o => o.Status, OutboxStatus.Processed));
        }
        public async Task RecordTransientFailure(Guid outboxId, string error, int retryCount, OutboxStatus status, DateTime? nextRetryAt)
        {
            await _context.Set<Outbox>()
                .Where(o => o.OutboxId == outboxId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.RetryCount, retryCount)
                    .SetProperty(o => o.LastError, error)
                    .SetProperty(o => o.Status, status)
                    .SetProperty(o => o.NextRetryAt, nextRetryAt));
        }
        public async Task MarkAsFailed(Guid outboxId, string error)
        {
            var outbox = await _context.Set<Outbox>()
                .Where(o => o.OutboxId == outboxId)
                .ExecuteUpdateAsync(o => 
                             o.SetProperty(o => o.Status, OutboxStatus.Failed)
                              .SetProperty(o => o.LastError, error)
                              .SetProperty(o => o.NextRetryAt, null as DateTime?));
        }
    }
}
