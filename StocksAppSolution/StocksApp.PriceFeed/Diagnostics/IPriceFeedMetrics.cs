namespace StocksApp.PriceFeed.Diagnostics
{
    public interface IPriceFeedMetrics
    {
        void ReconnectAttempted();
        void SymbolCounts(int desired, int actual);
        void TickReceived();
        void TickPublished();
        void TickDropped();
        void LeaderAcquired();
        void LeaderLost();
        void SocketConnected();
        void SocketDisconnected();
    }
}