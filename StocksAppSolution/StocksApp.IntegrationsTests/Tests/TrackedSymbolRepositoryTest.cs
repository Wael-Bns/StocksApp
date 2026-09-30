using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Domain.Entities;
using StocksApp.Infrastructure;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Infrastructure.Services;
using StocksApp.IntegrationsTests.Collection;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.IntegrationsTests.Helpers;
using StocksApp.Tests.Common.Builders;

namespace StocksApp.IntegrationsTests.Tests
{
    [Collection(IntegrationTestsCollection.Name)]
    public class TrackedSymbolRepositoryTest : IntegrationTestBase
    {
        private const string NotifyChannel = "tracked_symbols_changed";

        public TrackedSymbolRepositoryTest(CustomWebApplicationFactory factory) : base(factory) { }

        // ---------- repository ----------

        [Fact]
        public async Task GetActiveSymbols_ActiveAndInactiveRows_ReturnsOnlyActive()
        {
            await SeedAsync(
                new TrackedSymbolBuilder().WithSymbol("AAPL").Build(),
                new TrackedSymbolBuilder().WithSymbol("MSFT").Build(),
                new TrackedSymbolBuilder().WithSymbol("OLD").Inactive().Build());

            var result = await QueryActiveAsync();

            result.Should().BeEquivalentTo(new[] { "AAPL", "MSFT" });
        }

        [Fact]
        public async Task GetActiveSymbols_EmptyTable_ReturnsEmpty()
        {
            var result = await QueryActiveAsync();

            result.Should().BeEmpty();
        }

        // ---------- schema ----------

        [Fact]
        public async Task Insert_LowercaseSymbol_ThrowsDbUpdateException()
        {
            Func<Task> act = () => SeedAsync(
                new TrackedSymbolBuilder().WithSymbol("aapl").Build());

            await act.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task Insert_DuplicateSymbol_ThrowsDbUpdateException()
        {
            await SeedAsync(new TrackedSymbolBuilder().WithSymbol("AAPL").Build());

            Func<Task> act = () => SeedAsync(
                new TrackedSymbolBuilder().WithSymbol("AAPL").Build());

            await act.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task Insert_WithoutTimestamps_DatabaseAppliesDefaults()
        {
            await SeedAsync(new TrackedSymbolBuilder().Build());

            var row = await ReadSingleAsync();

            row.IsActive.Should().BeTrue();
            row.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            row.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task Insert_InactiveSymbol_PersistsIsActiveFalse()
        {
            await SeedAsync(new TrackedSymbolBuilder().Inactive().Build());

            var row = await ReadSingleAsync();

            row.IsActive.Should().BeFalse();
        }

        // ---------- NOTIFY trigger ----------

        [Fact]
        public async Task Insert_NewRow_RaisesChangeNotification()
        {
            await using var listener = await StartListenerAsync();

            await SeedAsync(new TrackedSymbolBuilder().Build());

            (await listener.WaitForNotificationAsync()).Should().BeTrue();
        }

        [Fact]
        public async Task Update_DeactivateRow_RaisesChangeNotification()
        {
            await SeedAsync(new TrackedSymbolBuilder().Build());
            await using var listener = await StartListenerAsync();   // after the seed, so the insert doesn't count

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var row = await db.TrackedSymbols.SingleAsync();
                row.IsActive = false;
                row.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            (await listener.WaitForNotificationAsync()).Should().BeTrue();
        }

        [Fact]
        public async Task Delete_Row_RaisesChangeNotification()
        {
            await SeedAsync(new TrackedSymbolBuilder().Build());
            await using var listener = await StartListenerAsync();

            using (var scope = Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.TrackedSymbols.ExecuteDeleteAsync();
            }

            (await listener.WaitForNotificationAsync()).Should().BeTrue();
        }

        // ---------- scoped store ----------

        [Fact]
        public async Task Store_RowAddedBetweenCalls_SecondCallSeesIt()
        {
            // proves the adapter opens a fresh scope per read instead of holding a stale DbContext
            var store = new TrackedSymbolStore(
                Factory.Services.GetRequiredService<IServiceScopeFactory>());

            await SeedAsync(new TrackedSymbolBuilder().WithSymbol("AAPL").Build());
            var first = await store.GetActiveSymbolsAsync(CancellationToken.None);

            await SeedAsync(new TrackedSymbolBuilder().WithSymbol("MSFT").Build());
            var second = await store.GetActiveSymbolsAsync(CancellationToken.None);

            first.Should().BeEquivalentTo(new[] { "AAPL" });
            second.Should().BeEquivalentTo(new[] { "AAPL", "MSFT" });
        }

        // ---------- helpers ----------

        private async Task SeedAsync(params TrackedSymbol[] symbols)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.TrackedSymbols.AddRange(symbols);
            await db.SaveChangesAsync();
        }

        private async Task<IReadOnlyList<string>> QueryActiveAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await new TrackedSymbolRepository(db).GetActiveSymbolsAsync(CancellationToken.None);
        }

        private async Task<TrackedSymbol> ReadSingleAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.TrackedSymbols.AsNoTracking().SingleAsync();
        }

        private async Task<PostgresNotificationListener> StartListenerAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await PostgresNotificationListener.StartAsync(
                db.Database.GetConnectionString()!, NotifyChannel);
        }
    }
}