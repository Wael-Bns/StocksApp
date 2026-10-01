namespace StocksApp.Core.Diagnostics
{
    public interface ICandleMetrics
    {
        void TickLateForClosedBucket();
        void CandleFlushed();
        void CandleFlushFailed();
    }
}
