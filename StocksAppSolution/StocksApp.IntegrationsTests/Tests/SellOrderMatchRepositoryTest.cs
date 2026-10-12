using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Infrastructure;
using StocksApp.Infrastructure.Repositories;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.Tests.Common.Builders;

namespace StocksApp.IntegrationsTests.Tests
{
    public class SellOrderMatchRepositoryTest : IClassFixture<TimescaleFixture>, IAsyncLifetime
    {
        private static readonly TimeSpan BucketSize = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan GracePeriod = TimeSpan.FromSeconds(2);
        private const decimal LimitPrice = 100;

        private readonly TimescaleFixture _pg;
        private readonly User _user = new UserBuilder().Build();
        private readonly string _symbol = UniqueSymbol();

        private ApplicationDbContext _ctx = null!;
        private IDbContextTransaction _tx = null!;
        private SellOrderMatchRepository _repo = null!;

        public SellOrderMatchRepositoryTest(TimescaleFixture pg) => _pg = pg;

        #region Lifecycle

        // Transaction opens before seeding and is never committed: disposing rolls everything back.
        public async Task InitializeAsync()
        {
            _ctx = new ApplicationDbContext(_pg.Options);
            _tx = await _ctx.Database.BeginTransactionAsync();
            _repo = new SellOrderMatchRepository(_ctx);
        }

        public async Task DisposeAsync()
        {
            await _tx.DisposeAsync();
            await _ctx.DisposeAsync();
        }

        #endregion

        #region GetMatchableOrderIdsAsync

        [Fact]
        public async Task GetMatchableOrderIds_BarBeforeActivatesAt_DoesNotCount()
        {
            var order = CreatePendingOrder();
            CreateMatchBar(b => b.WithBucketStart(ActivatesAtToDateTimeUtcOffset(order) - BucketSize).WithHigh(LimitPrice + 100));
            await SeedAsync();

            var matchable = await _repo.GetMatchableOrderIdsAsync(GracePeriod, CancellationToken.None);

            matchable.Should().NotContain(order.SellOrderID);   
        }

        [Fact]
        public async Task GetMatchableOrderIds_BarAtActivatesAt_HighAboveLimit_Counts()
        {
            var order = CreatePendingOrder();
            CreateMatchBar(b => b.WithBucketStart(ActivatesAtToDateTimeUtcOffset(order)).WithHigh(LimitPrice + 1));
            await SeedAsync();

            var matchable = await _repo.GetMatchableOrderIdsAsync(GracePeriod, CancellationToken.None);

            matchable.Should().Contain(order.SellOrderID);
        }

        [Fact]
        public async Task GetMatchableOrderIds_HighEqualToLimit_Counts()
        {
            var order = CreatePendingOrder();
            CreateMatchBar(b => b.WithBucketStart(ActivatesAtToDateTimeUtcOffset(order)).WithHigh(LimitPrice));
            await SeedAsync();

            var matchable = await _repo.GetMatchableOrderIdsAsync(GracePeriod, CancellationToken.None);

            matchable.Should().Contain(order.SellOrderID);
        }

        [Fact]
        public async Task GetMatchableOrderIds_CloseAboveLimitButHighBelow_DoesNotCount()
        {
            var order = CreatePendingOrder();
            CreateMatchBar(b => b.WithBucketStart(ActivatesAtToDateTimeUtcOffset(order))
                           .WithOhlc(open: 90, high: 90, low: 85, close: LimitPrice + 1));
            await SeedAsync();

            var matchable = await _repo.GetMatchableOrderIdsAsync(GracePeriod, CancellationToken.None);

            matchable.Should().NotContain(order.SellOrderID);
        }

        [Fact]
        public async Task GetMatchableOrderIds_BarStillWithinGracePeriod_NotYetEligible()
        {
            // Bucket ended ~1s ago, grace period is 2s.
            var order = CreatePendingOrder();
            var justClosed = DateTimeOffset.UtcNow - BucketSize - TimeSpan.FromSeconds(1);
            CreateMatchBar(b => b.WithBucketStart(justClosed).WithHigh(LimitPrice + 100));
            await SeedAsync();

            var matchable = await _repo.GetMatchableOrderIdsAsync(GracePeriod, CancellationToken.None);

            matchable.Should().NotContain(order.SellOrderID);
        }

        [Fact]
        public async Task GetMatchableOrderIds_BarForOtherSymbol_DoesNotCount()
        {
            var order = CreatePendingOrder();
            CreateMatchBar(b => b.WithSymbol(UniqueSymbol())
                           .WithBucketStart(ActivatesAtToDateTimeUtcOffset(order))
                           .WithHigh(LimitPrice + 100));
            await SeedAsync();

            var matchable = await _repo.GetMatchableOrderIdsAsync(GracePeriod, CancellationToken.None);

            matchable.Should().NotContain(order.SellOrderID);
        }

        [Fact]
        public async Task GetMatchableOrderIds_ExecutedOrder_NeverReturned()
        {
            var order = CreatePendingOrder();
            order.MarkExecuted();
            CreateMatchBar(b => b.WithBucketStart(ActivatesAtToDateTimeUtcOffset(order)).WithHigh(LimitPrice + 100));
            await SeedAsync();

            var matchable = await _repo.GetMatchableOrderIdsAsync(GracePeriod, CancellationToken.None);

            matchable.Should().NotContain(order.SellOrderID);
        }

        #endregion

        #region TryCancelAsync

        [Fact]
        public async Task TryCancel_NoQualifyingBarYet_Cancels()
        {
            var order = CreatePendingOrder();
            await SeedAsync();

            var cancelled = await _repo.TryCancelAsync(order.SellOrderID, GracePeriod, CancellationToken.None);

            cancelled.Should().BeTrue();
            (await StatusOf(order)).Should().Be(SellOrderStatus.Cancelled);
        }

        [Fact]
        public async Task TryCancel_QualifyingBarAlreadyExists_RefusesToCancel()
        {
            var order = CreatePendingOrder();
            CreateMatchBar(b => b.WithBucketStart(ActivatesAtToDateTimeUtcOffset(order)).WithHigh(LimitPrice + 100));
            await SeedAsync();

            var cancelled = await _repo.TryCancelAsync(order.SellOrderID, GracePeriod, CancellationToken.None);

            cancelled.Should().BeFalse();
            (await StatusOf(order)).Should().Be(SellOrderStatus.Pending);
        }

        #endregion

        #region Helpers

        // Other test classes may commit rows to the same database.
        private static string UniqueSymbol() =>
            "T" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        // SellOrder.Create snaps ActivatesAt up to the bucket grid, so bars must use the stored value.
        private static DateTimeOffset ActivatesAtToDateTimeUtcOffset(SellOrder order) =>
            new(DateTime.SpecifyKind(order.ActivatesAt, DateTimeKind.Utc));

        private SellOrder CreatePendingOrder()
        {
            var order = SellOrder.Create(
                _user.UserId, _symbol, _symbol, (double)LimitPrice, 10,
                createdAt: DateTime.UtcNow.AddMinutes(-1), BucketSize);
            order.User = _user;
            _ctx.SellOrders.Add(order);
            return order;
        }

        private void CreateMatchBar(Func<CandleMatch5sBuilder, CandleMatch5sBuilder> configure) =>
            _ctx.CandleMatches5s.Add(configure(new CandleMatch5sBuilder().WithSymbol(_symbol)).Build());

        private async Task SeedAsync()
        {
            _ctx.Users.Add(_user);
            await _ctx.SaveChangesAsync();
        }

        private Task<SellOrderStatus> StatusOf(SellOrder order) =>
            _ctx.SellOrders.AsNoTracking()
                .Where(o => o.SellOrderID == order.SellOrderID)
                .Select(o => o.Status)
                .SingleAsync();

        #endregion
    }
}