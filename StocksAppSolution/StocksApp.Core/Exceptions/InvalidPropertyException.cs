namespace StocksApp.Core.Exceptions
{
    internal class InvalidPropertyException : ClientException
    {
        public InvalidPropertyException(string message) : base(message, 400) { }
    }
}
