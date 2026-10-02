namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="DesiredCount">Desired number of subscriptions.</param>
    /// <param name="ActualCount">Actual number of subscriptions.</param>
    public readonly record struct ReconciliationResult(int DesiredCount, int ActualCount)
    {
        /// <summary>Returned when the desired-set read failed; ActualCount still reflects reality.</summary>
        public static ReconciliationResult Unknown(int actualCount) => new(-1, actualCount);
    }
    /// <summary>
    /// Updates the subscriptions to match the current tracked symbols. This is called after a fresh socket connect, and also when the tracked symbols change.
    /// </summary>
    public interface ISubscriptionReconciler
    {
        /// <summary>Call after a fresh socket connect: nothing is subscribed yet.</summary>
        void Reset();

        Task<ReconciliationResult> ReconcileAsync(CancellationToken ct);
        /// <summary>Fired once per symbol removed from the desired set, after it has been
        /// unsubscribed from Finnhub. Used by the candle pipeline to flush and
        /// clean up that symbol's Redis entries.</summary>
        event Func<string, CancellationToken, Task>? SymbolUnsubscribed;
    }
}