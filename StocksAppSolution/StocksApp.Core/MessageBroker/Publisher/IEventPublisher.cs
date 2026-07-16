namespace StocksApp.Core.MessageBroker.Publisher
{
    /// <summary>
    /// Broadcasts an event to multiple subscribers
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}
