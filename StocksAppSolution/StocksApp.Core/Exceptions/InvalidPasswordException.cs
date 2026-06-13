namespace StocksApp.Core.Exceptions
{
    public class InvalidPasswordException : ClientException
    {
        public InvalidPasswordException() : base("Invalid password.", 400) { }
        public InvalidPasswordException(string message) : base(message, 400) { }
    }
}
