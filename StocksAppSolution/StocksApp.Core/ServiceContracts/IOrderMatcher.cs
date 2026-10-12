namespace StocksApp.Core.ServiceContracts
{
    public interface IOrderMatcher
    {
        Task RunOnceAsync(CancellationToken ct);
    }
}