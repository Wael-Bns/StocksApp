using System.Threading.Channels;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    ///     Wakes up on any change to tracked_symbols. The channel is a hint only —
    ///     it is never read for data, and a missed signal is always healed by the
    ///     reconciler's periodic pass.
    /// </summary>
    public interface ITrackedSymbolsNotifier
    {
        ChannelReader<bool> ChangedChannelReader { get; }

        /// <summary>Runs until ct is cancelled; retries its own connection internally.</summary>
        Task RunAsync(CancellationToken ct);
    }
}