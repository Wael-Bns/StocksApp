using StocksApp.Domain.Entities;

namespace StocksApp.Domain.RepositoryContracts
{
    public interface IOutboxRepository : IGenericRepository<Outbox>
    {
        Task MarkAsProcessed(Guid outboxId);
        Task<List<Outbox>> GetUnprocessedEvents();
        Task RecordFailure(Guid outboxId, string error, int maxRetries, TimeSpan backoff);
        Task MarkAsFailed(Guid outboxId, string error);
    }
}
