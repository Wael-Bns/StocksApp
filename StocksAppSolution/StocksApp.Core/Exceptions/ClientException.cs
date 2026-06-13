namespace StocksApp.Core.Exceptions
{
    public abstract class ClientException : Exception
    {
        public int StatusCode { get; }
        protected ClientException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
