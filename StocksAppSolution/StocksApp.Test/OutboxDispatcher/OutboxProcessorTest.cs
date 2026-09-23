using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Exceptions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.OutboxDispatcher.Options;
using StocksApp.OutboxDispatcher.Services;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.OutboxDispatcher
{
    public class OutboxProcessorTest
    {
        private const string RegisteredEventType = "SellOrderCreated";

        private readonly Mock<IOutboxEventHandler> _outboxEventHandlerMock;
        private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
        private readonly OutboxOptions _outboxOptions;
        private readonly OutboxProcessor _outboxProcessor;

        public OutboxProcessorTest()
        {
            _outboxEventHandlerMock = new Mock<IOutboxEventHandler>();
            _outboxEventHandlerMock.SetupGet(h => h.EventType).Returns(RegisteredEventType);

            _outboxRepositoryMock = new Mock<IOutboxRepository>();
            _outboxOptions = new OutboxOptions { MaxRetries = 3 };

            _outboxProcessor = new OutboxProcessor(
                new[] { _outboxEventHandlerMock.Object },
                Mock.Of<ILogger<OutboxProcessor>>(),
                _outboxRepositoryMock.Object,
                Options.Create(_outboxOptions));
        }

        [Fact]
        public async Task ProcessNotificationAsync_HandlerSucceeds_MarksAsProcessed()
        {
            // Arrange
            var notification = new OutboxNotificationBuilder()
                .WithEventName(RegisteredEventType)
                .Build();
            _outboxEventHandlerMock
                .Setup(h => h.HandleAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _outboxProcessor.ProcessNotificationAsync(notification);

            // Assert
            _outboxRepositoryMock.Verify(r => r.MarkAsProcessed(notification.OutboxId), Times.Once);
            _outboxRepositoryMock.Verify(r => r.MarkAsFailed(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _outboxRepositoryMock.Verify(r => r.RecordTransientFailure(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<OutboxStatus>(), It.IsAny<DateTime?>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessNotificationAsync_NoHandlerRegisteredForEventType_DoesNotTouchRepository()
        {
            // Arrange
            var notification = new OutboxNotificationBuilder()
                .WithEventName("SomeUnregisteredEvent")
                .Build();

            // Act
            await _outboxProcessor.ProcessNotificationAsync(notification);

            // Assert
            _outboxEventHandlerMock.Verify(h => h.HandleAsync(It.IsAny<string>()), Times.Never);
            _outboxRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ProcessNotificationAsync_DeserializationFails_MarksAsFailedImmediately_NoRetryRecorded()
        {
            // Arrange
            var notification = new OutboxNotificationBuilder()
                .WithEventName(RegisteredEventType)
                .Build();
            _outboxEventHandlerMock
                .Setup(h => h.HandleAsync(It.IsAny<string>()))
                .ThrowsAsync(new JsonException("bad payload"));

            // Act
            await _outboxProcessor.ProcessNotificationAsync(notification);

            // Assert
            _outboxRepositoryMock.Verify(r => r.MarkAsFailed(notification.OutboxId, It.IsAny<string>()), Times.Once);
            _outboxRepositoryMock.Verify(r => r.MarkAsProcessed(It.IsAny<Guid>()), Times.Never);
            _outboxRepositoryMock.Verify(r => r.RecordTransientFailure(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<OutboxStatus>(), It.IsAny<DateTime?>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessNotificationAsync_TransientFailure_RecordsFirstAttemptAsPendingWithFutureRetry()
        {
            // Arrange — a notification always represents a fresh row, so it always starts at attempt 0
            var notification = new OutboxNotificationBuilder()
                .WithEventName(RegisteredEventType)
                .Build();
            _outboxEventHandlerMock
                .Setup(h => h.HandleAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("broker unreachable"));

            // Act
            await _outboxProcessor.ProcessNotificationAsync(notification);

            // Assert
            _outboxRepositoryMock.Verify(r => r.RecordTransientFailure(
                notification.OutboxId,
                It.IsAny<string>(),
                1,
                OutboxStatus.Pending,
                It.Is<DateTime?>(nextRetryAt => nextRetryAt.HasValue && nextRetryAt.Value > DateTime.UtcNow)),
                Times.Once);
        }

        [Theory]
        [InlineData(0, 1, OutboxStatus.Pending)]  // 1st failure of 3 allowed — retry
        [InlineData(1, 2, OutboxStatus.Pending)]  // 2nd failure — still under MaxRetries — retry
        [InlineData(2, 3, OutboxStatus.Failed)]   // 3rd failure — hits MaxRetries — dead-letter
        public async Task ProcessUnprocessedEvents_RetryDecision_MatchesMaxRetriesBoundary(
            int existingRetryCount, int expectedNewRetryCount, OutboxStatus expectedStatus)
        {
            // Arrange
            var outboxEvent = new OutboxBuilder()
                .WithEventName(RegisteredEventType)
                .WithRetryCount(existingRetryCount)
                .Build();
            _outboxEventHandlerMock
                .Setup(h => h.HandleAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            await _outboxProcessor.ProcessUnprocessedEvents(new List<Outbox> { outboxEvent });

            // Assert
            _outboxRepositoryMock.Verify(r => r.RecordTransientFailure(
                outboxEvent.OutboxId,
                It.IsAny<string>(),
                expectedNewRetryCount,
                expectedStatus,
                It.Is<DateTime?>(nextRetryAt =>
                    expectedStatus == OutboxStatus.Failed
                        ? nextRetryAt == null
                        : nextRetryAt.HasValue && nextRetryAt.Value > DateTime.UtcNow)),
                Times.Once);
        }

        [Fact]
        public async Task ProcessUnprocessedEvents_PassesExistingRetryCountUnmodified_DoesNotDoubleIncrement()
        {
            // Arrange — regression guard for the earlier bug where the caller pre-incremented
            // retryCount AND HandleTransientFailureAsync incremented again, skipping a count.
            var outboxEvent = new OutboxBuilder()
                .WithEventName(RegisteredEventType)
                .WithRetryCount(1) // this row has already failed once
                .Build();
            _outboxEventHandlerMock
                .Setup(h => h.HandleAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            await _outboxProcessor.ProcessUnprocessedEvents(new List<Outbox> { outboxEvent });

            // Assert — should become 2, never 3
            _outboxRepositoryMock.Verify(r => r.RecordTransientFailure(
                outboxEvent.OutboxId, It.IsAny<string>(), 2, It.IsAny<OutboxStatus>(), It.IsAny<DateTime?>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessUnprocessedEvents_OneEventFailsAmongMany_OthersAreStillProcessed()
        {
            // Arrange
            var failingEvent = new OutboxBuilder().WithEventName(RegisteredEventType).Build();
            var succeedingEvent = new OutboxBuilder().WithEventName(RegisteredEventType).Build();

            _outboxEventHandlerMock
                .SetupSequence(h => h.HandleAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("boom"))
                .Returns(Task.CompletedTask);

            // Act
            await _outboxProcessor.ProcessUnprocessedEvents(new List<Outbox> { failingEvent, succeedingEvent });

            // Assert
            _outboxRepositoryMock.Verify(r => r.RecordTransientFailure(
                failingEvent.OutboxId, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<OutboxStatus>(), It.IsAny<DateTime?>()),
                Times.Once);
            _outboxRepositoryMock.Verify(r => r.MarkAsProcessed(succeedingEvent.OutboxId), Times.Once);
        }
    }
}