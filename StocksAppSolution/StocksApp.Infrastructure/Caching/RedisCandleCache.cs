using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StocksApp.Core.DTO.CandleDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Infrastructure.Caching
{
    public sealed class RedisCandleCache : ICandleCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCandleCache> _logger;

        public RedisCandleCache(IConnectionMultiplexer redis, ILogger<RedisCandleCache> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        private IDatabase Db => _redis.GetDatabase();

        public async Task SetLatestPriceAsync(LatestPriceSnapshot snapshot, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(snapshot);
            await Db.StringSetAsync(RedisKeyNames.LatestPrice(snapshot.Symbol), json);
        }

        public async Task SetActiveBarAsync(ActiveBarSnapshot bar, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(bar);
            await Db.StringSetAsync(RedisKeyNames.ActiveBar(bar.Symbol), json);
        }

        public async Task<ActiveBarSnapshot?> GetActiveBarAsync(string symbol, CancellationToken ct)
        {
            var value = await Db.StringGetAsync(RedisKeyNames.ActiveBar(symbol));
            if (value.IsNullOrEmpty) return null;

            try
            {
                return JsonSerializer.Deserialize<ActiveBarSnapshot>(value!);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Corrupt active-bar cache entry for {Symbol}; treating as absent.", symbol);
                return null;
            }
        }

        public async Task DeleteActiveBarAsync(string symbol, CancellationToken ct)
        {
            await Db.KeyDeleteAsync(RedisKeyNames.ActiveBar(symbol));
        }

        public async IAsyncEnumerable<ActiveBarSnapshot> GetAllActiveBarsAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);

            await foreach (var key in server.KeysAsync(pattern: RedisKeyNames.ActiveBarScanPattern)
                .WithCancellation(ct))
            {
                var value = await Db.StringGetAsync(key);
                if (value.IsNullOrEmpty) continue;   // deleted between SCAN and GET — benign race

                ActiveBarSnapshot? bar;
                try
                {
                    bar = JsonSerializer.Deserialize<ActiveBarSnapshot>(value!);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Corrupt active-bar cache entry at {Key}; skipping.", key.ToString());
                    continue;
                }

                if (bar is not null) yield return bar;
            }
        }

        public async Task DeleteLatestPriceAsync(string symbol, CancellationToken ct)
        {
            await Db.KeyDeleteAsync(RedisKeyNames.LatestPrice(symbol));
        }
    }
}