namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Handles received events based on their type
    /// </summary>
    public interface IOutboxEventHandler
    {
        string EventType { get; }
        Task HandleAsync(string @event);
    }
}
