namespace StocksApp.Domain.Events
{
    public sealed record NeedSymbol(string Symbol, string RequestedBy) : INeedSymbol;
}