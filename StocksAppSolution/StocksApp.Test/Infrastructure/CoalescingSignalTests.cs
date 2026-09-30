using FluentAssertions;
using StocksApp.Infrastructure.LeaderElection;
using Xunit;

namespace StocksApp.Test.Infrastructure
{
    public class CoalescingSignalTests
    {
        [Fact]
        public void MultipleRaises_CollapseToOnePendingSignal()
        {
            var sut = new CoalescingSignal();

            sut.Raise();
            sut.Raise();
            sut.Raise();

            sut.Reader.TryRead(out _).Should().BeTrue("one signal is pending");
            sut.Reader.TryRead(out _).Should().BeFalse("the other two were coalesced");
        }

        [Fact]
        public void Raise_AfterSignalConsumed_SignalsAgain()
        {
            var sut = new CoalescingSignal();
            sut.Raise();
            sut.Reader.TryRead(out _);

            sut.Raise();

            sut.Reader.TryRead(out _).Should().BeTrue();
        }

        [Fact]
        public void NoRaise_NothingPending()
        {
            var sut = new CoalescingSignal();

            sut.Reader.TryRead(out _).Should().BeFalse();
        }
    }
}