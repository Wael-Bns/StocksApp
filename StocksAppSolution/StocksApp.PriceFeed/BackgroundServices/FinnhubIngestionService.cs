using System.Threading.Channels;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.WebSocketClientAbstractions;

namespace StocksApp.PriceFeed.BackgroundServices
{
    public sealed class FinnhubIngestionService : BackgroundService
    {
        private readonly IFinnhubWebSocketClient _client;
        private readonly ChannelWriter<PriceUpdateMessage> _writer;
        private readonly ILogger<FinnhubIngestionService> _logger;

        public FinnhubIngestionService(
            IFinnhubWebSocketClient client,
            ChannelWriter<PriceUpdateMessage> writer,
            ILogger<FinnhubIngestionService> logger)
        {
            _client = client;
            _writer = writer;
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
                if (!_writer.TryWrite(update))
                    _logger.LogWarning("Tick channel full — dropped an update for {Symbol}", update.StockSymbol);
            }
            return Task.CompletedTask;
        }
    }
}
