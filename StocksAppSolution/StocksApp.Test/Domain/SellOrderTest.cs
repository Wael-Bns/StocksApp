using FluentAssertions;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.Domain
{
    public class SellOrderTest
    {
        [Fact]
        public void Create_ValidInput_StampsActivatesAtToNextBucketBoundary()
        {
            var createdAt = new DateTime(2026, 10, 4, 14, 0, 2, DateTimeKind.Utc);   // 2s past a 5s boundary
            var order = SellOrder.Create(Guid.NewGuid(), "AAPL", "Apple", 150, 10, createdAt, TimeSpan.FromSeconds(5));

            order.ActivatesAt.Should().Be(new DateTime(2026, 10, 4, 14, 0, 5, DateTimeKind.Utc));
            order.Status.Should().Be(SellOrderStatus.Pending);
        }

        [Fact]
        public void Create_ExactlyOnBoundary_DoesNotRoundUpAgain()
        {
            var createdAt = new DateTime(2026, 10, 4, 14, 0, 5, DateTimeKind.Utc);
            var order = SellOrder.Create(Guid.NewGuid(), "AAPL", "Apple", 150, 10, createdAt, TimeSpan.FromSeconds(5));

            order.ActivatesAt.Should().Be(createdAt);
        }

        [Fact]
        public void MarkExecuted_PendingOrder_SetsStatusToExecuted()
        {
            var order = SellOrder.Create(Guid.NewGuid(), "AAPL", "Apple", 150, 10, DateTime.UtcNow, TimeSpan.FromSeconds(5));

            order.MarkExecuted();

            order.Status.Should().Be(SellOrderStatus.Executed);
        }

        [Fact]
        public void MarkExecuted_AlreadyExecuted_Throws()
        {
            var order = SellOrder.Create(Guid.NewGuid(), "AAPL", "Apple", 150, 10, DateTime.UtcNow, TimeSpan.FromSeconds(5));
            order.MarkExecuted();

            var act = () => order.MarkExecuted();

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
