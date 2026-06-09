namespace StocksApp.Core.Exceptions
{
    internal class UserNotFoundException : ClientException
    {
        public UserNotFoundException() : base("User not found", 404) { }
        public UserNotFoundException(string message) : base(message, 404) { }
    }
}
