using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

namespace StocksApp.Domain.RepositoryContracts
{
    public interface IOutboxRepository : IGenericRepository<Outbox>
    {
        Task MarkAsProcessed(Guid outboxId);
        Task<List<Outbox>> GetUnprocessedEvents();
        Task RecordTransientFailure(Guid outboxId, string error, int retryCount, OutboxStatus status, DateTime? nextRetryAt);
        Task MarkAsFailed(Guid outboxId, string error);
    }
}
