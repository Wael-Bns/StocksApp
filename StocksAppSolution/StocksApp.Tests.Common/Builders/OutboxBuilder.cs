using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

namespace StocksApp.Tests.Common.Builders
{
    public class OutboxBuilder
    {
        private Guid _outboxId = Guid.NewGuid();
        private string _eventName = "SellOrderCreated";
        private string _payload = "{}";
        private int _retryCount = 0;
        private OutboxStatus _status = OutboxStatus.Pending;

        public OutboxBuilder WithOutboxId(Guid outboxId)
        {
            _outboxId = outboxId;
            return this;
        }

        public OutboxBuilder WithEventName(string eventName)
        {
            _eventName = eventName;
            return this;
        }

        public OutboxBuilder WithPayload(string payload)
        {
            _payload = payload;
            return this;
        }

        public OutboxBuilder WithRetryCount(int retryCount)
        {
            _retryCount = retryCount;
            return this;
        }

        public OutboxBuilder WithStatus(OutboxStatus status)
        {
            _status = status;
            return this;
        }

        public Outbox Build()
        {
            return new Outbox
            {
                OutboxId = _outboxId,
                EventName = _eventName,
                Payload = _payload,
                RetryCount = _retryCount,
                Status = _status,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}