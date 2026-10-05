namespace StocksApp.Core.Diagnostics
{
    public interface ICandleMetrics
    {
        void TickLateForClosedBucket();
        void CandleFlushed();
        void CandleFlushFailed();
        void MatchBarFlushFailed();
        void MatchBarBackupRecovered();
        void SetMatchBarBackupPending(int count);
    }
}
