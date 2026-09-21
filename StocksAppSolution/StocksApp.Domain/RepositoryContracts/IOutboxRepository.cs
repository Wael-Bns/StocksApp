using StocksApp.Domain.Entities;

namespace StocksApp.Domain.RepositoryContracts
{
    public interface IOutboxRepository : IGenericRepository<Outbox>
    {
        Task MarkAsProcessed(Guid outboxId);
        Task<List<Outbox>> GetUnprocessedEvents();
    }
}
