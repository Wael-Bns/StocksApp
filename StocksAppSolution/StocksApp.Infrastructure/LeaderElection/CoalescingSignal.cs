using System.Threading.Channels;

namespace StocksApp.Infrastructure.LeaderElection
{
    /// <summary>
    /// A "something changed" signal. Any number of Raise() calls before the reader
    /// consumes collapse into a single pending signal.
    /// </summary>
    public sealed class CoalescingSignal
    {
        private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true
            });

        public ChannelReader<bool> Reader => _channel.Reader;

        public void Raise() => _channel.Writer.TryWrite(true);

        public void Complete() => _channel.Writer.TryComplete();
    }
}
