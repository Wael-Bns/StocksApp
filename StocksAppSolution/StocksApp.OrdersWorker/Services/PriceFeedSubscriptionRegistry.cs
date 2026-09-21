using Polly;
using Polly.Registry;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.Resilience;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Services
{
    public class PriceFeedSubscriptionRegistry : IPriceFeedSubscriptionRegistry
    {
        private readonly IFinnhubWebSocketClient _finnhubWebSocketClient;
        private readonly HashSet<string> _subscribedStockSymbols;
        private readonly ResiliencePipeline _pipeline;
        public PriceFeedSubscriptionRegistry(IFinnhubWebSocketClient finnhubWebSocketClient,
            ResiliencePipelineProvider<string> pipelineProvider)
        {
            _finnhubWebSocketClient = finnhubWebSocketClient;
            _subscribedStockSymbols = new HashSet<string>();
            _pipeline = pipelineProvider.GetPipeline(ResilienceOptions.PriceFeedSubscriptionPipeline);
        }

        public async Task EnsureSubscribedAsync(string stockSymbol, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(stockSymbol) || _subscribedStockSymbols.Contains(stockSymbol))
                return;

            await _pipeline.ExecuteAsync(
            async token => await _finnhubWebSocketClient.SubscribeAsync(stockSymbol, token), cancellationToken);

            _subscribedStockSymbols.Add(stockSymbol);
        }
        public async Task UnsubscribeAsync(string stockSymbol, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(stockSymbol) && _subscribedStockSymbols.Remove(stockSymbol))
            {
                await _pipeline.ExecuteAsync(
                    async token => await _finnhubWebSocketClient.UnsubscribeAsync(stockSymbol, token), cancellationToken);
            }
        }
    }
}
