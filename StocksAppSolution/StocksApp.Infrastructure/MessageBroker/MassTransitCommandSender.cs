using MassTransit;
using Microsoft.Extensions.Logging;
using StocksApp.Core.MessageBroker.Publisher;

namespace StocksApp.Infrastructure.MessageBroker
{
    public class MassTransitCommandSender : ICommandSender
    {
        private readonly ILogger<MassTransitCommandSender> _logger;
        private readonly IBus _bus;
        public MassTransitCommandSender(
            ILogger<MassTransitCommandSender> logger,
            IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        public async Task SendAsync<T>(T command, CancellationToken ct = default) where T : class
        {
            try
            {
                await _bus.Send(command, ct);
                _logger.LogInformation("Sent command {CommandType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send command {CommandType}", typeof(T).Name);
                throw;
            }
        }
    }
}
