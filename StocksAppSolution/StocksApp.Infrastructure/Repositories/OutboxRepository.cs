using Microsoft.EntityFrameworkCore;
using StocksApp.Domain.Entities;
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
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.ProcessedAt, DateTime.UtcNow));
        }
    }
}
