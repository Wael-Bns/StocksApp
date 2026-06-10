namespace StocksApp.Core.Exceptions
{
    public class EmailAlreadyExistsException : ClientException
    {
        public EmailAlreadyExistsException() : base("Email is already in use.", 409) { }
        public EmailAlreadyExistsException(string message) : base(message, 409) { }
    }
}
