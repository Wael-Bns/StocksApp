namespace StocksApp.Core.Exceptions
{
    public class InvalidEmailException : ClientException
    {
        public InvalidEmailException() : base("Invalid email format.", 400) { }
        public InvalidEmailException(string message) : base(message, 400) { }   
    }
}
