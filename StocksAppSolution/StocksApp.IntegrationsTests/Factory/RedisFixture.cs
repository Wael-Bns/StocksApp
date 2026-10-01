using Testcontainers.Redis;

namespace StocksApp.IntegrationsTests.Factory
{
    public class RedisFixture : IAsyncLifetime
    {
        private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();
        public string ConnectionString => _redis.GetConnectionString();
        public Task InitializeAsync() => _redis.StartAsync();
        public Task DisposeAsync() => _redis.DisposeAsync().AsTask();
    }
}