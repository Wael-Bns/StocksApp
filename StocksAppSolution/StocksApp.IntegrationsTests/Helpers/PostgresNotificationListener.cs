using Npgsql;

namespace StocksApp.IntegrationsTests.Helpers
{
    public sealed class PostgresNotificationListener : IAsyncDisposable
    {
        private readonly NpgsqlConnection _connection;
        private readonly SemaphoreSlim _received = new(0);
        private readonly CancellationTokenSource _cts = new();
        private Task _pump = Task.CompletedTask;

        private PostgresNotificationListener(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public static async Task<PostgresNotificationListener> StartAsync(
            string connectionString, string channel)
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var listener = new PostgresNotificationListener(connection);
            connection.Notification += (_, e) =>
            {
                if (e.Channel == channel) listener._received.Release();
            };

            await using (var cmd = new NpgsqlCommand($"LISTEN {channel}", connection))
                await cmd.ExecuteNonQueryAsync();

            listener._pump = Task.Run(listener.PumpAsync);
            return listener;
        }

        private async Task PumpAsync()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                    await _connection.WaitAsync(_cts.Token);
            }
            catch (OperationCanceledException) { }
        }

        public Task<bool> WaitForNotificationAsync(TimeSpan? timeout = null)
            => _received.WaitAsync(timeout ?? TimeSpan.FromSeconds(5));

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            await _pump;
            await _connection.DisposeAsync();
            _cts.Dispose();
            _received.Dispose();
        }
    }
}