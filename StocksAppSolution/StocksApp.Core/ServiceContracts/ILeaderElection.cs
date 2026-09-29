namespace StocksApp.Core.ServiceContracts
{
    public interface ILeaderElection
    {
        /// <summary>Blocks until this instance is the leader.</summary>
        Task<ILeadership> AcquireAsync(CancellationToken ct);
    }
    public interface ILeadership : IAsyncDisposable
    {
        /// <summary>Cancelled when leadership is lost or disposed.</summary>
        CancellationToken LostToken { get; }
    }
}
