namespace StocksApp.Core.Exceptions
{
    public class InvalidOutboxEventTypeException : Exception
    {

        public InvalidOutboxEventTypeException() : base("Invalid event type or not registered handler .") { }
        public InvalidOutboxEventTypeException(string message) : base( message ) { }
    }
}
