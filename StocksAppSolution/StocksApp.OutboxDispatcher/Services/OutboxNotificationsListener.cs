using Microsoft.Extensions.Options;
using StocksApp.OutboxDispatcher.Options;
using StocksApp.Core.ServiceContracts;
using Npgsql;
using StocksApp.Domain.Notifications;
using System.Text.Json;

namespace StocksApp.OutboxDispatcher.Services
{
    public class OutboxNotificationsListener : IOutboxNotificationsListener
    {
        private readonly OutboxOptions _options;
        private readonly ILogger<OutboxNotificationsListener> _logger;
        public event Func<OutboxNotification, Task>? OnNotificationReceived;
        public event Func<Task>? OnPeriodicChecks;
        public OutboxNotificationsListener(IOptions<OutboxOptions> options, ILogger<OutboxNotificationsListener> logger)
        {
            _options = options.Value;
            _logger = logger;
        }
        public async Task ListenAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ListenOnceAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lost Database notification connection.");

                    await Task.Delay(_options.ReconnectDelay, cancellationToken);
                }
            }
        }
        private async Task ListenOnceAsync(CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_options.ConnectionString);

            await connection.OpenAsync(cancellationToken);

            // When the notification is sent from the database, we handle it using registered delegates .
            connection.Notification += async (_, e) =>
            {
                _logger.LogInformation("Received database notification on channel {Channel}", e.Channel);
                if (OnNotificationReceived != null)
                {
                    foreach (Func<OutboxNotification, Task> handler in OnNotificationReceived.GetInvocationList())
                    {
                        var notification = JsonSerializer.Deserialize<OutboxNotification>(e.Payload);
                        if(notification != null)
                        {
                            await handler(notification);
                        }
                    }
                }
            };

            // Quotes the channel name : mychannel => "mychannel"
            var channel = new NpgsqlCommandBuilder()
                .QuoteIdentifier(_options.NotificationChannel);

            await using var cmd = new NpgsqlCommand($"LISTEN {channel}", connection);

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation("Listening on {Channel}", _options.NotificationChannel);

            // A fallback : we periodically poll the database for orders pending with a period of 'FallbackPollInterval'
            while (!cancellationToken.IsCancellationRequested)
            {
                await connection.WaitAsync(
                    _options.FallbackPollInterval,
                    cancellationToken);

                if(OnPeriodicChecks != null)
                {
                    await OnPeriodicChecks.Invoke();
                }
            }
        }
    }
}
