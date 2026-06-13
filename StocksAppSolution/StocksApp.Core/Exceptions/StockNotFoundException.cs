namespace StocksApp.Core.Exceptions
{
    public class StockNotFoundException : ClientException
    {
        public StockNotFoundException() : base("Stock was not found.", 404)
        {
        }
        public StockNotFoundException(string message) : base(message, 404)
        {
        }
    }
}
