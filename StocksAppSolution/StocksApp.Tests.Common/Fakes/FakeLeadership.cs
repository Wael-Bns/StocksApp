using StocksApp.Core.ServiceContracts;

namespace StocksApp.IntegrationsTests.Fakes
{
    public sealed class FakeLeadership : ILeadership
    {
        private readonly CancellationTokenSource _lost = new();
        public bool Disposed { get; private set; }

        public CancellationToken LostToken => _lost.Token;

        public void Lose() => _lost.Cancel();

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            _lost.Cancel();
            return ValueTask.CompletedTask;
        }
    }
}