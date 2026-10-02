using Microsoft.EntityFrameworkCore;
using StocksApp.Infrastructure;
using Testcontainers.PostgreSql;

namespace StocksApp.IntegrationsTests.Factory
{
    public class TimescaleFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("timescale/timescaledb:latest-pg16")
            .Build();

        public string ConnectionString => _pg.GetConnectionString();
        public DbContextOptions<ApplicationDbContext> Options { get; private set; } = default!;

        public async Task InitializeAsync()
        {
            await _pg.StartAsync();

            Options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            await using var ctx = new ApplicationDbContext(Options);
            await ctx.Database.MigrateAsync();   // runs once per test class, not per test method
        }

        public Task DisposeAsync() => _pg.DisposeAsync().AsTask();
    }
}