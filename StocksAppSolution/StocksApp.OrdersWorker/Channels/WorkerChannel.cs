using System.Threading.Channels;
using StocksApp.OrdersWorker.Messages;

namespace StocksApp.OrdersWorker.Channels
{
    public sealed class WorkerChannel : IWorkerChannel
    {
        private readonly Channel<WorkerMessage> _channel;
        public WorkerChannel()
        {
            _channel = Channel.CreateBounded<WorkerMessage>(
            new BoundedChannelOptions(200)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
            });
        }

        public ValueTask EnqueueAsync(WorkerMessage message, CancellationToken cancellationToken = default)
            => _channel.Writer.WriteAsync(message, cancellationToken);

        public IAsyncEnumerable<WorkerMessage> ReadAllAsync(CancellationToken cancellationToken)
            => _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
