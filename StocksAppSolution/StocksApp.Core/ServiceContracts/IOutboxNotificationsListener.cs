using StocksApp.Domain.Notifications;

namespace StocksApp.Core.ServiceContracts
{
    public interface IOutboxNotificationsListener
    {
        event Func<OutboxNotification, Task>? OnNotificationReceived;
        Task ListenAsync(CancellationToken cancellationToken = default);
    }
}
