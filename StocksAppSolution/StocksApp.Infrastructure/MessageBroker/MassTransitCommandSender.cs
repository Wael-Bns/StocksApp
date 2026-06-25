using MassTransit;
using Microsoft.Extensions.Logging;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Infrastructure.MessageBroker.Profiles;

namespace StocksApp.Infrastructure.MessageBroker
{
    public class MassTransitCommandSender : ICommandSender
    {
        private readonly ILogger<MassTransitCommandSender> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IReadOnlyDictionary<Type, Uri> _endpointMap;

        public MassTransitCommandSender(
            ILogger<MassTransitCommandSender> logger,
            ISendEndpointProvider sendEndpointProvider,
            IEnumerable<ICommandBusProfile> profiles)
        {
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _endpointMap = profiles.ToDictionary(p => p.MessageType, p => p.EndpointUri);
        }

        public async Task SendAsync<T>(T command, CancellationToken ct = default) where T : class
        {
            if (!_endpointMap.TryGetValue(typeof(T), out var uri))
                throw new InvalidOperationException($"No endpoint registered for {typeof(T).Name}");

            try
            {
                var endpoint = await _sendEndpointProvider.GetSendEndpoint(uri);
                await endpoint.Send(command, ct);
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
