namespace StocksApp.Core.Exceptions
{
    public class InvalidOutboxEventTypeException : Exception
    {

        public InvalidOutboxEventTypeException() : base("Invalid event type in the outbox table .") { }
        public InvalidOutboxEventTypeException(string message) : base( message ) { }
    }
}
