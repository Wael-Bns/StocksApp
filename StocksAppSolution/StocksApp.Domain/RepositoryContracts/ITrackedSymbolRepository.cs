namespace StocksApp.Domain.RepositoryContracts
{
    public interface ITrackedSymbolRepository
    {
        Task<IReadOnlyList<string>> GetActiveSymbolsAsync(CancellationToken ct);
    }
}