using Testcontainers.PostgreSql;

namespace StocksApp.IntegrationsTests.Factory
{
    public class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16-alpine").Build();
        public string ConnectionString => _pg.GetConnectionString();
        public Task InitializeAsync() => _pg.StartAsync();
        public Task DisposeAsync() => _pg.DisposeAsync().AsTask();
    }
}
