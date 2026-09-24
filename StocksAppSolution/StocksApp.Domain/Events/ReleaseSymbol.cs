namespace StocksApp.Domain.Events
{
    public sealed record ReleaseSymbol(string Symbol, string RequestedBy) : IReleaseSymbol;
}