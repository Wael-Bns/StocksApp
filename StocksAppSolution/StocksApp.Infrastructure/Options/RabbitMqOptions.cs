namespace StocksApp.Infrastructure.Options
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";
        public string HostName { get; init; } = default!;
        public string UserName { get; init; } = default!;
        public string Password { get; init; } = default!;

        public string ToAmqpUri() => $"amqp://{UserName}:{Password}@{HostName}";
    }
}
