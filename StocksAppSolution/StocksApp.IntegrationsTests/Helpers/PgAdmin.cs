using Npgsql;

namespace StocksApp.IntegrationsTests.Helpers
{
    public static class PgAdmin
    {
        public static async Task TerminateAdvisoryLockHolderAsync(string connectionString, long lockKey)
        {
            await using var admin = new NpgsqlConnection(connectionString);
            await admin.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_locks " +
                "WHERE locktype = 'advisory' AND granted AND objid = @k", admin);
            cmd.Parameters.AddWithValue("k", lockKey);
            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task TerminateByApplicationNameAsync(string connectionString, string applicationName)
        {
            await using var admin = new NpgsqlConnection(connectionString);
            await admin.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity " +
                "WHERE application_name = @app AND pid <> pg_backend_pid()", admin);
            cmd.Parameters.AddWithValue("app", applicationName);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}