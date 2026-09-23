namespace StocksApp.Domain.Events
{
    public interface IOutboxEvent
    {
        static abstract string EventName { get; }
    }
}
