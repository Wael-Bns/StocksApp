namespace StocksApp.Core.MessageBroker.Publisher
{
    /// <summary>
    /// Sends command to specific subscribers
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        /// Sends a command to the message broker.
        /// </summary>
        /// <typeparam name="T">The type of the command.</typeparam>
        /// <param name="command">The command to send.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class;
    }
}
