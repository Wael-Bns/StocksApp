using System.Threading.Channels;
using MassTransit.Transports.Fabric;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.PriceFeed.Diagnostics;

namespace StocksApp.PriceFeed.BackgroundServices
{
    public sealed class FinnhubIngestionService : BackgroundService
    {
        private readonly IFinnhubWebSocketClient _client;
        private readonly ChannelWriter<PriceUpdateMessage> _writer;
        private readonly PriceFeedMetrics _metrics;
        private readonly ILogger<FinnhubIngestionService> _logger;

        public FinnhubIngestionService(
            IFinnhubWebSocketClient client,
            ChannelWriter<PriceUpdateMessage> writer,
            PriceFeedMetrics metrics,
            ILogger<FinnhubIngestionService> logger)
        {
            _client = client;
            _writer = writer;
            _metrics = metrics;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.OnPriceUpdatesReceived += OnUpdatesAsync;
            try
            {
                await _client.ConnectAsync(stoppingToken);
                await _client.ReceiveLoopAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Finnhub ingestion stopping.");
            }
            finally
            {
                _client.OnPriceUpdatesReceived -= OnUpdatesAsync;
                await _client.DisconnectAsync(CancellationToken.None);
            }
        }

        private Task OnUpdatesAsync(IReadOnlyCollection<PriceUpdateMessage> updates)
        {
            foreach (var update in updates)
            {
                _metrics.TickReceived();
                _writer.TryWrite(update);
            }
            return Task.CompletedTask;
        }
    }
}
