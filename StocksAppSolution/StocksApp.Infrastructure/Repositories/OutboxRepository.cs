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
        public async Task RecordFailure(Guid outboxId, string error, int maxRetries, TimeSpan backoff)
        {
            var outbox = await _context.Set<Outbox>().FindAsync(outboxId);
            outbox!.RetryCount++;
            outbox.LastError = error;

            outbox.Status = outbox.RetryCount >= maxRetries
                ? OutboxStatus.Failed
                : OutboxStatus.Pending;

            outbox.NextRetryAt = outbox.Status == OutboxStatus.Pending
                ? DateTime.UtcNow.Add(backoff)
                : null;

            await _context.SaveChangesAsync();
        }
        public async Task MarkAsFailed(Guid outboxId, string error)
        {
            var outbox = await _context.Set<Outbox>().FindAsync(outboxId);
            if (outbox is null) return;

            outbox.Status = OutboxStatus.Failed;
            outbox.LastError = error;
            outbox.NextRetryAt = null;

            await _context.SaveChangesAsync();
        }
    }
}
