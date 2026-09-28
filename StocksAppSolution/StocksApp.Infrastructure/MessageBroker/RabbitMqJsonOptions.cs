using System.Text.Json;

namespace StocksApp.Infrastructure.MessageBroker
{
    public static class RabbitMqJsonOptions
    {
        public static readonly JsonSerializerOptions Default = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
