using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.IntegrationsTests.Fakes;
using StocksApp.PriceFeed.BackgroundServices;
using StocksApp.PriceFeed.Options;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.PriceFeed
{
    public class FinnhubIngestionServiceTest : IAsyncLifetime
    {
        private const string AaplSymbol = "AAPL";
        private const string ConnectionResetMessage = "connection reset";
        private const string ReconcileFailureMessage = "reconcile failed";

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan BackoffTimeout = TimeSpan.FromSeconds(3);

        private readonly FinnhubIngestionService _service;
        private readonly RecordingFinnhubWebSocketClient _client;
        private readonly Channel<PriceUpdateMessage> _priceChannel;
        private readonly Channel<bool> _trackedSymbolsChannel;
        private readonly Mock<IOhlcBarAggregator> _aggregatorMock;
        private readonly Mock<ILeaderElection> _electionMock;

        private readonly Mock<ITrackedSymbolsNotifier> _notifierMock;
        private readonly Mock<ISubscriptionReconciler> _reconcilerMock;
        private readonly PriceFeedIngestionOptions _options;
        private readonly Mock<IPriceFeedMetrics> _priceFeedMetricsMock;

        public FinnhubIngestionServiceTest()
        {
            _client = new RecordingFinnhubWebSocketClient();
            _priceChannel = Channel.CreateUnbounded<PriceUpdateMessage>();
            _trackedSymbolsChannel = Channel.CreateBounded<bool>(1);
            _aggregatorMock = new Mock<IOhlcBarAggregator>();
            _electionMock = new Mock<ILeaderElection>();
            _notifierMock = new Mock<ITrackedSymbolsNotifier>();
            _reconcilerMock = new Mock<ISubscriptionReconciler>();
            _priceFeedMetricsMock = new Mock<IPriceFeedMetrics>();

            _options = new PriceFeedIngestionOptions
            {
                InitialBackoff = TimeSpan.FromMilliseconds(20),
                MaxBackoff = TimeSpan.FromMilliseconds(200),
                StableConnectionThreshold = TimeSpan.FromMilliseconds(300),
                ReconcileInterval = TimeSpan.FromSeconds(30), // long enough to not fire during a test
                ReconcileDebounce = TimeSpan.FromMilliseconds(10)
            };

            _notifierMock.Setup(n => n.TrackedSymbolsChannelReader)
                .Returns(_trackedSymbolsChannel.Reader);
            _notifierMock.Setup(n => n.RunAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => Task.Delay(Timeout.Infinite, ct));

            _service = new FinnhubIngestionService(
                _client,
                _aggregatorMock.Object,
                _priceChannel.Writer,
                _electionMock.Object,
                _notifierMock.Object,
                _reconcilerMock.Object,
                _priceFeedMetricsMock.Object,
                Options.Create(_options),
                Mock.Of<ILogger<FinnhubIngestionService>>());
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _service.StopAsync(CancellationToken.None);

            // StopAsync suppresses exceptions from ExecuteAsync, so surface a crashed loop here
            if (_service.ExecuteTask is not null)
                _service.ExecuteTask.IsFaulted.Should().BeFalse("the ingestion loop must not crash");

            _service.Dispose();
        }

        #region Leadership Tests

        [Fact]
        public async Task StartAsync_LeadershipAcquired_ConnectsAndResetsReconciler()
        {
            ArrangeLeadership();
            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => _client.ConnectCalls.Count >= 1);
            await WaitUntilAsync(() => CountInvocations(_reconcilerMock, nameof(ISubscriptionReconciler.Reset)) >= 1);

            _reconcilerMock.Verify(r => r.Reset(), Times.Once);
        }

        [Fact]
        public async Task StartAsync_LeadershipLost_DisconnectsAndReacquires()
        {
            var firstLeadership = new FakeLeadership();
            _electionMock.SetupSequence(e => e.AcquireAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(firstLeadership)
                .ReturnsAsync(new FakeLeadership());

            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());
            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => _client.ConnectCalls.Count >= 1);

            firstLeadership.Lose();

            await WaitUntilAsync(() => _client.DisconnectCalls.Count >= 1);
            await WaitUntilAsync(() =>
                CountInvocations(_electionMock, nameof(ILeaderElection.AcquireAsync)) >= 2);

            _client.DisconnectCalls.Should().HaveCountGreaterThanOrEqualTo(1);
        }

        #endregion

        #region Reconnect Backoff Tests

        [Fact]
        public async Task StartAsync_SocketFails_DisconnectsBeforeReconnecting()
        {
            ArrangeLeadership();
            _client.ReceiveLoopBehaviors.Enqueue(
                RecordingFinnhubWebSocketClient.ThrowsImmediately(new Exception(ConnectionResetMessage)));
            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => _client.ConnectCalls.Count >= 2, BackoffTimeout);

            _client.DisconnectCalls.Should().HaveCountGreaterThanOrEqualTo(1);
            _client.DisconnectCalls.First().Should().BeOnOrBefore(_client.ConnectCalls.ElementAt(1));
        }

        [Fact]
        public async Task StartAsync_RepeatedFailures_BackoffGrows()
        {
            ArrangeLeadership();
            EnqueueFailures(3);
            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => _client.ConnectCalls.Count >= 4, BackoffTimeout);

            // expected delays: 20ms, 40ms, 80ms
            TimeSpan[] delays = GetReconnectDelays();

            delays[2].Should().BeGreaterThan(delays[0]);
        }

        [Fact]
        public async Task StartAsync_StableConnectionThenFailure_BackoffResetsToInitial()
        {
            ArrangeLeadership();
            EnqueueFailures(3);
            _client.ReceiveLoopBehaviors.Enqueue(
                RecordingFinnhubWebSocketClient.ReturnsAfter(TimeSpan.FromMilliseconds(350))); // > StableConnectionThreshold
            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => _client.ConnectCalls.Count >= 5, BackoffTimeout);

            // delays: 20ms, 40ms, 80ms, then back to 20ms after the stable connection
            TimeSpan[] delays = GetReconnectDelays();

            delays[3].Should().BeLessThan(delays[2]);
            delays[3].Should().BeCloseTo(_options.InitialBackoff, TimeSpan.FromMilliseconds(50));
        }

        #endregion

        #region Reconcile Tests

        [Fact]
        public async Task StartAsync_TrackedSymbolsChanged_TriggersReconcile()
        {
            ArrangeLeadership();
            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => ReconcileCount() >= 1); // the reconcile that happens right after connect
            int reconcilesBeforeSignal = ReconcileCount();

            _trackedSymbolsChannel.Writer.TryWrite(true);

            await WaitUntilAsync(() => ReconcileCount() > reconcilesBeforeSignal);
        }

        [Fact]
        public async Task StartAsync_ReconcileThrows_KeepsSessionAliveAndRetries()
        {
            ArrangeLeadership();
            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());
            _reconcilerMock.SetupSequence(r => r.ReconcileAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(ReconcileFailureMessage))
                .Returns(Task.FromResult(new ReconciliationResult()));

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => ReconcileCount() >= 1);

            _trackedSymbolsChannel.Writer.TryWrite(true);

            await WaitUntilAsync(() => ReconcileCount() >= 2);
            _client.ConnectCalls.Should().HaveCount(1); // the socket session was not torn down
        }

        #endregion

        #region Price Update Tests

        [Fact]
        public async Task StartAsync_PriceUpdatesReceived_WritesToChannel()
        {
            ArrangeLeadership();
            _client.ReceiveLoopBehaviors.Enqueue(RecordingFinnhubWebSocketClient.RunsUntilCancelled());
            var update = new PriceUpdateMessageBuilder().WithSymbol(AaplSymbol).Build();

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => _client.ConnectCalls.Count >= 1);

            await _client.RaiseUpdatesAsync(new[] { update });

            using var readCts = new CancellationTokenSource(DefaultTimeout);
            PriceUpdateMessage received = await _priceChannel.Reader.ReadAsync(readCts.Token);

            received.StockSymbol.Should().Be(AaplSymbol);
        }

        #endregion

        #region Helpers

        private FakeLeadership ArrangeLeadership()
        {
            var leadership = new FakeLeadership();
            _electionMock.Setup(e => e.AcquireAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(leadership);
            return leadership;
        }

        private void EnqueueFailures(int count)
        {
            for (var i = 0; i < count; i++)
                _client.ReceiveLoopBehaviors.Enqueue(
                    RecordingFinnhubWebSocketClient.ThrowsImmediately(new Exception(ConnectionResetMessage)));
        }

        // time between each disconnect and the next connect, i.e. the backoff delay actually applied
        private TimeSpan[] GetReconnectDelays()
        {
            var connects = _client.ConnectCalls.ToArray();
            var disconnects = _client.DisconnectCalls.ToArray();

            return connects.Skip(1)
                .Zip(disconnects, (connect, disconnect) => connect - disconnect)
                .ToArray();
        }

        private int ReconcileCount() =>
            CountInvocations(_reconcilerMock, nameof(ISubscriptionReconciler.ReconcileAsync));

        private static int CountInvocations<T>(Mock<T> mock, string methodName) where T : class =>
            mock.Invocations.Count(i => i.Method.Name == methodName);

        private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);
            while (!condition())
            {
                if (DateTime.UtcNow > deadline)
                    throw new TimeoutException("Condition was not met in time.");
                await Task.Delay(10);
            }
        }

        #endregion
    }
}