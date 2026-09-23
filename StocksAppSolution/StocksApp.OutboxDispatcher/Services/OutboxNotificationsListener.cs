using System.Text.Json;
using Microsoft.Extensions.Options;
using Npgsql;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Notifications;
using StocksApp.OutboxDispatcher.Options;

namespace StocksApp.OutboxDispatcher.Services
{
    public class OutboxNotificationsListener : IOutboxNotificationsListener
    {
        private readonly OutboxOptions _options;
        private readonly ILogger<OutboxNotificationsListener> _logger;

        public event Func<OutboxNotification, Task>? OnNotificationReceived;

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
                    _logger.LogError(ex, "Lost database notification connection.");
                    await Task.Delay(_options.ReconnectDelay, cancellationToken);
                }
            }
        }

        private async Task ListenOnceAsync(CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            connection.Notification += async (_, e) =>
            {
                try
                {
                    _logger.LogInformation("Received database notification on channel {Channel}", e.Channel);

                    var notification = JsonSerializer.Deserialize<OutboxNotification>(e.Payload);
                    if (notification is null || OnNotificationReceived is null)
                        return;

                    foreach (Func<OutboxNotification, Task> handler in OnNotificationReceived.GetInvocationList())
                    {
                        try
                        {
                            await handler(notification);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Outbox notification handler failed for {OutboxId}", notification.OutboxId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling database notification.");
                }
            };

            var channel = new NpgsqlCommandBuilder().QuoteIdentifier(_options.NotificationChannel);
            await using var cmd = new NpgsqlCommand($"LISTEN {channel}", connection);
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation("Listening on {Channel}", _options.NotificationChannel);

            while (!cancellationToken.IsCancellationRequested)
            {
                await connection.WaitAsync(cancellationToken);
            }
        }
    }
}