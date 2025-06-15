using FinancialOperations.Domain.Common;
using FinancialOperations.Domain.Entities;
using FinancialOperations.Domain.Enums;
using FinancialOperations.Domain.Events;
using FinancialOperations.Infrastructure.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.Infrastructure;

public class ConsoleEventPublisherTests
{
    private readonly Mock<ILogger<ConsoleEventPublisher>> _loggerMock;
    private readonly ConsoleEventPublisher _eventPublisher;

    public ConsoleEventPublisherTests()
    {
        _loggerMock = new Mock<ILogger<ConsoleEventPublisher>>();
        _eventPublisher = new ConsoleEventPublisher(_loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_WithValidEvent_ShouldNotifyObservers()
    {
        // Arrange
        var observerMock = new Mock<IObserver<IDomainEvent>>();
        _eventPublisher.Subscribe(observerMock.Object);

        var transaction = new Transaction(Guid.NewGuid(), TransactionType.Credit, 100, "Test");
        var creditEvent = new TransactionProcessedEvent(transaction); 

        // Act
        await _eventPublisher.PublishAsync(creditEvent);

        // Assert
        observerMock.Verify(x => x.OnNext(creditEvent), Times.Once);
        observerMock.Verify(x => x.OnNext(It.IsAny<IDomainEvent>()), Times.Exactly(1));

    }
}
