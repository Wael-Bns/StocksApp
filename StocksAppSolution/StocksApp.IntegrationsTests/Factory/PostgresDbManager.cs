
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using StocksApp.Infrastructure;
using Testcontainers.PostgreSql;

namespace StocksApp.IntegrationsTests.Factory
{
    public class PostgresDbManager : IAsyncDisposable
    {
        private readonly PostgreSqlContainer _postgreSqlContainer;
        private IServiceScopeFactory _scopeFactory = default!;
        private Respawner _respawner = default!;
        private string _connectionString = default!;

        public string ConnectionString => _connectionString;
        public PostgresDbManager()
        {
            _postgreSqlContainer = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("testdb")
                .WithUsername("test")
                .WithPassword("test")
                .Build();
        }
        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();
            _connectionString = _postgreSqlContainer.GetConnectionString();
        }
        public async Task ApplyMigrationsAsync(IServiceProvider services)
        {
            _scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres
            });

            await connection.CloseAsync();
        }
        public async Task ResetAsync()
        {

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();

            await _respawner.ResetAsync(connection);

            await connection.CloseAsync();
        }
        public async ValueTask DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }
    }
}
