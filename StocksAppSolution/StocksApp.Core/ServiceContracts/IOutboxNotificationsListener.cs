using StocksApp.Domain.Notifications;

namespace StocksApp.Core.ServiceContracts
{
    public interface IOutboxNotificationsListener
    {
        event Func<OutboxNotification, Task>? OnNotificationReceived;
        event Func<Task>? OnPeriodicChecks;
        Task ListenAsync(CancellationToken cancellationToken = default);
    }
}
