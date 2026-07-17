using System.Text.Json;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;

namespace StocksApp.OutboxDispatcher.EventHandlers
{
    public class SellOrderCreatedCommandOutboxHandler : IOutboxEventHandler
    {
        public string EventType => nameof(SellOrderCreatedCommand);
        private readonly ICommandSender _commandSender;
        public SellOrderCreatedCommandOutboxHandler(ICommandSender commandSender)
        {
            _commandSender = commandSender;
        }

        public async Task HandleAsync(string @event)
        {
            SellOrderCreatedCommand? command = JsonSerializer.Deserialize<SellOrderCreatedCommand>(@event);
            if(command != null)
            {
                await _commandSender.SendAsync(command);
            }
        }
    }
}
