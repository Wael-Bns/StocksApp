using System.Text.Json;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Domain.Events;

namespace StocksApp.OutboxDispatcher.EventHandlers
{
    public class SellOrderCreatedCommandOutboxHandler : OutboxEventHandlerBase<SellOrderCreatedCommand>
    {
        private readonly ICommandSender _commandSender;
        public SellOrderCreatedCommandOutboxHandler(ICommandSender commandSender)
        {
            _commandSender = commandSender;
        }

        protected override async Task HandleEventAsync(SellOrderCreatedCommand @event)
        {
            await _commandSender.SendAsync(@event);
        }
    }
}
