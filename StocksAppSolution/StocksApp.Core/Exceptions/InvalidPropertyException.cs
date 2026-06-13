namespace StocksApp.Core.Exceptions
{
    public class InvalidPropertyException : ClientException
    {
        public InvalidPropertyException(string message) : base(message, 400) { }
    }
}
