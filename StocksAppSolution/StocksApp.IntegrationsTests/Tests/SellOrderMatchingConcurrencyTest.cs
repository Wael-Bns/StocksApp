using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StocksApp.Core.Options;
using StocksApp.Core.Services;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Infrastructure;
using StocksApp.Infrastructure.Repositories;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.Tests.Common.Builders;

namespace StocksApp.IntegrationsTests.Tests
{
    public class SellOrderMatchingConcurrencyTest : IClassFixture<TimescaleFixture>, IAsyncLifetime
    {
        private static readonly TimeSpan BucketSize = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan GracePeriod = TimeSpan.FromSeconds(2);

        private readonly TimescaleFixture _pg;
        private readonly string _symbol = UniqueSymbol();
        private User _user = null!;

        public SellOrderMatchingConcurrencyTest(TimescaleFixture pg) => _pg = pg;

        #region Lifecycle

        public Task InitializeAsync() => Task.CompletedTask;

        // Concurrency tests need real commits (separate connections must see the data),
        // so rollback isolation is impossible and the rows are deleted explicitly.
        public async Task DisposeAsync()
        {
            await using var ctx = new ApplicationDbContext(_pg.Options);
            await ctx.Database.ExecuteSqlInterpolatedAsync(
                $@"DELETE FROM ""SellOrder"" WHERE ""StockSymbol"" = {_symbol}");
            await ctx.Database.ExecuteSqlInterpolatedAsync(
                $@"DELETE FROM candle_matches_5s WHERE symbol = {_symbol}");
            if (_user is not null)
                await ctx.Users.Where(u => u.UserId == _user.UserId).ExecuteDeleteAsync();
        }

        #endregion

        #region Matcher vs matcher

        [Fact]
        public async Task TwoConcurrentMatcherRuns_SameQualifyingOrder_ExecutesExactlyOnce()
        {
            _user = new UserBuilder().WithCashBalance(1000).Build();
            var order = CreatePendingOrderWithPrivateUser(price: 100, quantity: 10);
            await SeedAsync(order, CreateCandleMatch5sBar(order, high: 150));

            await RunConcurrentlyAsync(RunMatcherOnceAsync, RunMatcherOnceAsync);

            (await StatusOf(order)).Should().Be(SellOrderStatus.Executed);
            (await CashBalanceOf(_user)).Should().Be(_user.CashBalance + order.Price * order.Quantity, "crediting twice would mean the lock failed");
        }
        [Fact]
        public async Task TwoOrdersSameUser_ConcurrentMatcherRuns_BothCreditsApplyNoLostUpdate()
        {
            _user = new UserBuilder().WithCashBalance(0).Build();
            var orderA = CreatePendingOrderWithPrivateUser(price: 100, quantity: 5, createdAt: DateTime.UtcNow.AddMinutes(-2));
            var orderB = CreatePendingOrderWithPrivateUser(price: 200, quantity: 3, createdAt: DateTime.UtcNow.AddMinutes(-1));
            await SeedAsync(
                new[] { orderA, orderB },
                new[] { CreateCandleMatch5sBar(orderA, high: 150), CreateCandleMatch5sBar(orderB, high: 250) });

            await RunConcurrentlyAsync(RunMatcherOnceAsync, RunMatcherOnceAsync);

            (await StatusOf(orderA)).Should().Be(SellOrderStatus.Executed);
            (await StatusOf(orderB)).Should().Be(SellOrderStatus.Executed);
            (await CashBalanceOf(_user)).Should().Be(_user.CashBalance + orderA.Price * orderA.Quantity + orderB.Price * orderB.Quantity, "both credits must land — neither may be lost");
        }

        #endregion

        #region Cancel vs matcher

        [Fact]
        public async Task CancelRacingMatcher_OnAlreadyQualifyingOrder_CancelAlwaysLoses()
        {
            _user = new UserBuilder().WithCashBalance(500).Build();
            var order = CreatePendingOrderWithPrivateUser(price: 50, quantity: 4);
            await SeedAsync(order, CreateCandleMatch5sBar(order, high: 60));

            var cancelled = false;
            await RunConcurrentlyAsync(
                async () => cancelled = await TryCancelAsync(order),
                RunMatcherOnceAsync);

            cancelled.Should().BeFalse("a qualifying bar already existed before the race began");
            (await StatusOf(order)).Should().Be(SellOrderStatus.Executed);
        }

        [Fact]
        public async Task CancelBeforeAnyQualifyingBarExists_AlwaysWins()
        {
            _user = new UserBuilder().Build();
            var order = CreatePendingOrderWithPrivateUser(price: 100, quantity: 5, createdAt: DateTime.UtcNow);
            await SeedAsync(order);

            var cancelled = false;
            await RunConcurrentlyAsync(
                async () => cancelled = await TryCancelAsync(order),
                RunMatcherOnceAsync);

            cancelled.Should().BeTrue();
            (await StatusOf(order)).Should().Be(SellOrderStatus.Cancelled);
        }

        #endregion

        #region Helpers

        private static string UniqueSymbol() =>
            "T" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        // SellOrder.Create snaps ActivatesAt up to the bucket grid, so bars must use the stored value.
        private static DateTimeOffset ConvertActivatesAtToUtcDateTimeOffset(SellOrder order) =>
            new(DateTime.SpecifyKind(order.ActivatesAt, DateTimeKind.Utc));

        private SellOrder CreatePendingOrderWithPrivateUser(double price, uint quantity, DateTime? createdAt = null)
        {
            var order = SellOrder.Create(
                _user.UserId, _symbol, _symbol, price, quantity,
                createdAt ?? DateTime.UtcNow.AddMinutes(-1), BucketSize);
            order.User = _user;
            return order;
        }

        private CandleMatch5s CreateCandleMatch5sBar(SellOrder order, decimal high) =>
            new CandleMatch5sBuilder()
                .WithSymbol(_symbol)
                .WithBucketStart(ConvertActivatesAtToUtcDateTimeOffset(order))
                .WithHigh(high)
                .Build();

        private Task SeedAsync(SellOrder order, CandleMatch5s? bar = null) =>
             SeedAsync(new[] { order }, bar is null ? null : new[] { bar });

        private async Task SeedAsync(IReadOnlyCollection<SellOrder> orders, IReadOnlyCollection<CandleMatch5s>? bars = null)
        {
            await using var ctx = new ApplicationDbContext(_pg.Options);
            ctx.Users.Add(_user);
            ctx.SellOrders.AddRange(orders);
            if (bars is not null) ctx.CandleMatches5s.AddRange(bars);
            await ctx.SaveChangesAsync();
        }

        // Releases all actions from a shared gate so they genuinely start together,
        // instead of the first one running ahead until its first await.
        private static async Task RunConcurrentlyAsync(params Func<Task>[] actions)
        {
            var gate = new TaskCompletionSource();
            var tasks = actions
                .Select(action => Task.Run(async () =>
                {
                    await gate.Task;
                    await action();
                }))
                .ToArray();

            gate.SetResult();
            await Task.WhenAll(tasks);
        }

        private async Task RunMatcherOnceAsync()
        {
            await using var ctx = new ApplicationDbContext(_pg.Options);
            var matcher = new OrderMatcher(
                new SellOrderMatchRepository(ctx),
                new GenericRepository<Outbox>(ctx),
                new UnitOfWork(ctx),
                Options.Create(new OrderMatchingOptions { MatcherGracePeriod = GracePeriod }),
                NullLogger<OrderMatcher>.Instance);

            await matcher.RunOnceAsync(CancellationToken.None);
        }

        private async Task<bool> TryCancelAsync(SellOrder order)
        {
            await using var ctx = new ApplicationDbContext(_pg.Options);
            return await new SellOrderMatchRepository(ctx)
                .TryCancelAsync(order.SellOrderID, GracePeriod, CancellationToken.None);
        }

        private async Task<SellOrderStatus> StatusOf(SellOrder order)
        {
            await using var ctx = new ApplicationDbContext(_pg.Options);
            return await ctx.SellOrders.AsNoTracking()
                .Where(o => o.SellOrderID == order.SellOrderID)
                .Select(o => o.Status)
                .SingleAsync();
        }

        private async Task<double> CashBalanceOf(User user)
        {
            await using var ctx = new ApplicationDbContext(_pg.Options);
            return await ctx.Users.AsNoTracking()
                .Where(u => u.UserId == user.UserId)
                .Select(u => u.CashBalance)
                .SingleAsync();
        }

        #endregion
    }
}