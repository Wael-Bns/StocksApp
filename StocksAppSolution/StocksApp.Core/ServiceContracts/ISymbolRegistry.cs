namespace StocksApp.Core.ServiceContracts
{
    public interface ISymbolRegistry
    {
        Task AddInterestAsync(string symbol, CancellationToken ct = default);
        Task RemoveInterestAsync(string symbol, CancellationToken ct = default);
        IReadOnlyCollection<string> ActiveSymbols { get; }
    }
}
