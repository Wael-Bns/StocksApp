namespace StocksApp.Domain.Events
{
    /// <summary>
    /// Published by the consumer when a symbol is no longer needed
    /// </summary>
    public interface IReleaseSymbol
    {
        string Symbol { get; }

        /// <summary>Logical origin of the request, for observability only</summary>
        string RequestedBy { get; }
    }
}
