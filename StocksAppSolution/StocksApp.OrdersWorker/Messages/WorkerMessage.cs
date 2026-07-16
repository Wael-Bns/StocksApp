using MassTransit;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Domain.Events;

namespace StocksApp.OrdersWorker.Messages
{
    public abstract record WorkerMessage;

    public sealed record PriceUpdateWorkerMessage(
        string StockSymbol,
        double Price
    ) : WorkerMessage;

    public sealed record SellOrderCreatedWorkerMessage(
        SellOrderCreatedCommand SellOrder
    ) : WorkerMessage;

    public static class WorkerMessageExtensions
    {
        public static PriceUpdateWorkerMessage ToPriceUpdateWorkerMessage(this PriceUpdateMessage priceUpdateMessage)
        {
            return new PriceUpdateWorkerMessage(
                priceUpdateMessage.StockSymbol,
                priceUpdateMessage.Price
                );
        }
    }
}
