using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StocksApp.Core.DTO.CandleDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.Core
{
    public class LatestPriceCacheWriterTest
    {
        private readonly Mock<ICandleCache> _cacheMock = new();
        private readonly LatestPriceCacheWriter _sut;

        public LatestPriceCacheWriterTest()
        {
            _sut = new LatestPriceCacheWriter(_cacheMock.Object, Mock.Of<ILogger<LatestPriceCacheWriter>>());
        }

        [Fact]
        public async Task TrackAsync_WritesSnapshotMatchingTheTick()
        {
            // Arrange
            var tick = new PriceUpdateMessageBuilder().WithSymbol("AAPL").WithPrice(150).WithVolume(25).Build();

            // Act
            await _sut.WriteAsync(tick, CancellationToken.None);

            // Assert
            _cacheMock.Verify(c => c.SetLatestPriceAsync(
                It.Is<LatestPriceSnapshot>(s => s.Symbol == "AAPL" && s.Price == 150m && s.Volume == 25),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TrackAsync_CacheThrows_LogsAndDoesNotThrow()
        {
            // Arrange
            _cacheMock.Setup(c => c.SetLatestPriceAsync(It.IsAny<LatestPriceSnapshot>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("redis unreachable"));
            var tick = new PriceUpdateMessageBuilder().Build();

            // Act
            var act = () => _sut.WriteAsync(tick, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeleteAsync_DeletesTheSymbolsCacheEntry()
        {
            // Act
            await _sut.DeleteAsync("AAPL", CancellationToken.None);

            // Assert
            _cacheMock.Verify(c => c.DeleteLatestPriceAsync("AAPL", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CacheThrows_LogsAndDoesNotThrow()
        {
            // Arrange
            _cacheMock.Setup(c => c.DeleteLatestPriceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("redis unreachable"));

            // Act
            var act = () => _sut.DeleteAsync("AAPL", CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}