using System.Text;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.Services
{
    public sealed class PriceFeedSubscriber : IPriceFeedSubscriber
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _rabbitOptions;
        private readonly string _exchangeName;
        private readonly string _subscriberId;
        private readonly ILogger<PriceFeedSubscriber> _logger;

        private IConnection? _connection;
        private IChannel? _channel;
        private string? _queueName;
        private readonly HashSet<string> _boundSymbols = new();
        private readonly SemaphoreSlim _channelLock = new(1, 1);

        public event Func<IPriceTickPublished, CancellationToken, Task>? OnPriceTick;

        public PriceFeedSubscriber(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> rabbitOptions,
            IOptions<PriceFeedClientOptions> clientOptions,
            ILogger<PriceFeedSubscriber> logger)
        {
            _scopeFactory = scopeFactory;
            _rabbitOptions = rabbitOptions.Value;
            _exchangeName = RabbitMQExchanges.PricesExchange;
            _subscriberId = clientOptions.Value.SubscriberId;
            _logger = logger;
        }

        private async Task EnsureChannelAsync(CancellationToken ct)
        {
            if (_channel is not null) return;
            await _channelLock.WaitAsync(ct);
            try
            {
                if (_channel is not null) return;

                var factory = new ConnectionFactory
                {
                    HostName = _rabbitOptions.HostName,
                    UserName = _rabbitOptions.UserName,
                    Password = _rabbitOptions.Password,
                };

                _connection = await factory.CreateConnectionAsync($"price-feed-subscriber-{_subscriberId}", ct);
                _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

                await _channel.ExchangeDeclareAsync(
                exchange: _exchangeName,
                type: ExchangeType.Topic,   
                durable: true,              
                autoDelete: false,          
                cancellationToken: ct);

                var declareResult = await _channel.QueueDeclareAsync(
                    queue: string.Empty, durable: false, exclusive: true, autoDelete: true,
                    cancellationToken: ct);
                _queueName = declareResult.QueueName;

                if (_queueName == null)
                {
                    throw new InvalidOperationException("An unexpected error occurred in queue declaration");
                }

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var json = Encoding.UTF8.GetString(ea.Body.Span);
                    await DispatchTickAsync(json, ct);
                };

                await _channel.BasicConsumeAsync(_queueName, autoAck: true, consumer, ct);
            }
            finally { _channelLock.Release(); }
        }

        private async Task DispatchTickAsync(string json, CancellationToken ct)
        {
            PriceTickPublished? tick;
            try
            {
                tick = JsonSerializer.Deserialize<PriceTickPublished>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize price tick payload: {Json}", json);
                return;
            }

            if (tick is null) return;

            await RaisePriceTickAsync(tick, ct);
        }

        private async Task RaisePriceTickAsync(IPriceTickPublished tick, CancellationToken ct)
        {
            var handler = OnPriceTick;
            if (handler is null) return;

            var invocations = handler.GetInvocationList().Cast<Func<IPriceTickPublished, CancellationToken, Task>>();
            await Task.WhenAll(invocations.Select(h => h(tick, ct)));
        }

        public async Task SubscribeAsync(string symbol, CancellationToken ct = default)
        {
            symbol = symbol.Trim().ToUpperInvariant();
            await EnsureChannelAsync(ct);

            using (var scope = _scopeFactory.CreateScope())
            {
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                await publishEndpoint.Publish<INeedSymbol>(new NeedSymbol(symbol, _subscriberId), ct);
            }

            if (_boundSymbols.Add(symbol))
                await _channel!.QueueBindAsync(_queueName!, _exchangeName, routingKey: symbol, cancellationToken: ct);
        }

        public async Task UnsubscribeAsync(string symbol, CancellationToken ct = default)
        {
            symbol = symbol.Trim().ToUpperInvariant();

            using (var scope = _scopeFactory.CreateScope())
            {
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                await publishEndpoint.Publish<IReleaseSymbol>(new ReleaseSymbol(symbol, _subscriberId), ct);
            }

            if (_boundSymbols.Remove(symbol) && _channel is not null)
                await _channel.QueueUnbindAsync(_queueName!, _exchangeName, routingKey: symbol, cancellationToken: ct);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null) await _channel.CloseAsync();
            if (_connection is not null) await _connection.CloseAsync();
        }
    }
}
