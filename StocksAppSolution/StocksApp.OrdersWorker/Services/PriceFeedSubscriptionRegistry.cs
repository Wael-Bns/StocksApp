using Polly;
using Polly.Registry;
using StocksApp.Core.ServiceContracts;
using StocksApp.OrdersWorker.Resilience;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Services
{
    public class PriceFeedSubscriptionRegistry : IPriceFeedSubscriptionRegistry
    {
        private readonly IPriceFeedSubscriber _priceFeedSubscriber;
        private readonly HashSet<string> _subscribedStockSymbols;
        private readonly ResiliencePipeline _pipeline;
        public PriceFeedSubscriptionRegistry(
            IPriceFeedSubscriber priceFeedSubscriber,
            ResiliencePipelineProvider<string> pipelineProvider)
        {
            _priceFeedSubscriber = priceFeedSubscriber;
            _subscribedStockSymbols = new HashSet<string>();
            _pipeline = pipelineProvider.GetPipeline(ResilienceOptions.PriceFeedSubscriptionPipeline);
        }

        public async Task EnsureSubscribedAsync(string stockSymbol, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(stockSymbol) || _subscribedStockSymbols.Contains(stockSymbol))
                return;

            await _pipeline.ExecuteAsync(
            async token => await _priceFeedSubscriber.SubscribeAsync(stockSymbol, token), cancellationToken);

            _subscribedStockSymbols.Add(stockSymbol);
        }
        public async Task UnsubscribeAsync(string stockSymbol, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(stockSymbol) || !_subscribedStockSymbols.Contains(stockSymbol))
                return;

            await _pipeline.ExecuteAsync(
                async token => await _priceFeedSubscriber.UnsubscribeAsync(stockSymbol, token), cancellationToken);
            _subscribedStockSymbols.Remove(stockSymbol);
        }
    }
}
