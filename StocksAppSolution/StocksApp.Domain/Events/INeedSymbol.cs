namespace StocksApp.Domain.Events
{
    /// <summary>
    /// Published by the consumer when a symbol becomes needed
    /// </summary>
    public interface INeedSymbol
    {
        string Symbol { get; }
        /// <summary>Logical origin of the request, for observability only</summary>
        string RequestedBy { get; }
    }
}
