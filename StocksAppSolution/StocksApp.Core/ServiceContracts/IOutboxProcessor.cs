using StocksApp.Domain.Entities;
using StocksApp.Domain.Notifications;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Processes messages from the outbox and publishes them to the message broker.
    /// </summary>
    public interface IOutboxProcessor
    {
        /// <summary>
        /// Publishes received database notification event payload to message broker .
        /// </summary>
        /// <param name="notification">Database Notification containing the event payload</param>
        /// <returns></returns>
        Task PublishNotificationAsync(OutboxNotification notification);
        /// <summary>
        /// Publishes unprocessed events.
        /// </summary>
        /// <returns></returns>
        Task PublishUnprocessedEvents(List<Outbox> unprocessedEvents);
    }
}
