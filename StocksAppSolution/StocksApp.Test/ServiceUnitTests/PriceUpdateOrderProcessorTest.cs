using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class PriceUpdateOrderProcessorTest
    {
        [Fact]
        public async Task EnqueueMessageAsync_WhenProcessorIsRunning_ShouldExecuteSellOrders()
        {
            // Arrange
            var message = new PriceUpdateMessage
            {
                StockSymbol = "AAPL",
                Price = 150
            };

            var executed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var ordersExecutorMock = new Mock<IOrdersExecutor>();

            ordersExecutorMock
                .Setup(executor => executor.ExecuteSellOrders(message.StockSymbol, message.Price))
                .Callback(() => executed.SetResult())
                .ReturnsAsync(new List<SellOrderResponse>());

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddScoped(_ => ordersExecutorMock.Object)
                .BuildServiceProvider();

            var processor = new PriceUpdateOrderProcessor(
                serviceProvider,
                NullLogger<PriceUpdateOrderProcessor>.Instance);

            using var cancellationTokenSource = new CancellationTokenSource();
            Task processorTask = processor.StartAsync(cancellationTokenSource.Token);

            // Act
            await processor.EnqueueMessageAsync(message);
            Task completedTask = await Task.WhenAny(executed.Task, Task.Delay(TimeSpan.FromSeconds(2)));

            cancellationTokenSource.Cancel();

            try
            {
                await processorTask;
            }
            catch (OperationCanceledException)
            {
                // Expected because the processor reads until cancellation.
            }

            // Assert
            completedTask.Should().Be(executed.Task);
            ordersExecutorMock.Verify(
                executor => executor.ExecuteSellOrders(message.StockSymbol, message.Price),
                Times.Once);
        }
    }
}
