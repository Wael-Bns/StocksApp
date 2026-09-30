using Npgsql;

namespace StocksApp.IntegrationsTests.Helpers
{
    internal static class PgNotify
    {
        public static async Task SendAsync(string connectionString, string channel, int times = 1)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            for (var i = 0; i < times; i++)
            {
                await using var cmd = new NpgsqlCommand($"NOTIFY {channel}", conn);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
