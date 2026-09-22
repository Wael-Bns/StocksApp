using System.Text.Json;
using StocksApp.Domain.Notifications;

namespace StocksApp.Tests.Common.Builders
{
    public class OutboxNotificationBuilder
    {
        private Guid _outboxId = Guid.NewGuid();
        private string _eventName = "SellOrderCreated";
        private JsonElement _payload = JsonDocument.Parse("{}").RootElement;

        public OutboxNotificationBuilder WithOutboxId(Guid outboxId)
        {
            _outboxId = outboxId;
            return this;
        }

        public OutboxNotificationBuilder WithEventName(string eventName)
        {
            _eventName = eventName;
            return this;
        }

        public OutboxNotificationBuilder WithPayload(string json)
        {
            _payload = JsonDocument.Parse(json).RootElement;
            return this;
        }

        public OutboxNotification Build()
        {
            return new OutboxNotification
            {
                OutboxId = _outboxId,
                EventName = _eventName,
                Payload = _payload
            };
        }
    }
}