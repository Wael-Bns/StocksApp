using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

public class SellOrderResponse
{
    public Guid SellOrderID { get; set; }
    public string? StockSymbol { get; set; }
    public string? StockName { get; set; }
    public DateTime DateAndTimeOfOrder { get; set; }
    public DateTime ActivatesAt { get; set; }
    public uint Quantity { get; set; }
    public double Price { get; set; }
    public double TradeAmount { get; set; }
    public SellOrderStatus Status { get; set; }
}

public static class SellOrderResponseExtension
{
    public static SellOrderResponse ToSellOrderResponse(this SellOrder sellOrder) => new()
    {
        SellOrderID = sellOrder.SellOrderID,
        StockName = sellOrder.StockName,
        StockSymbol = sellOrder.StockSymbol,
        DateAndTimeOfOrder = sellOrder.DateAndTimeOfOrder,
        ActivatesAt = sellOrder.ActivatesAt,
        Price = sellOrder.Price,
        Status = sellOrder.Status,
        Quantity = sellOrder.Quantity,
        TradeAmount = sellOrder.Quantity * sellOrder.Price
    };
}