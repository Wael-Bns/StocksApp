namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISubscriptionReconciler
    {
        /// <summary>Call after a fresh socket connect: nothing is subscribed yet.</summary>
        void Reset();

        Task ReconcileAsync(CancellationToken ct);
    }
}