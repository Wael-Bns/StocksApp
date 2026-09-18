using System.Text.Json;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;

namespace StocksApp.OutboxDispatcher.EventHandlers
{
    public class SellOrderCreatedCommandOutboxHandler : IOutboxEventHandler
    {
        public string EventType => typeof(SellOrderCreatedCommand).AssemblyQualifiedName!;
        private readonly ICommandSender _commandSender;
        public SellOrderCreatedCommandOutboxHandler(ICommandSender commandSender)
        {
            _commandSender = commandSender;
        }

        public async Task HandleAsync(string @event)
        {
            SellOrderCreatedCommand? command = JsonSerializer.Deserialize<SellOrderCreatedCommand>(@event);
            if(command == null)
            {
                throw new JsonException("Error in command deserialization .");
            }
            await _commandSender.SendAsync(command);
        }
    }
}
