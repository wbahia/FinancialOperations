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

    [Fact]
    public void Unsubscribe_WithSubscribedObserver_ShouldRemoveObserver()
    {
        // Arrange
        var observerMock = new Mock<IObserver<IDomainEvent>>();
        _eventPublisher.Subscribe(observerMock.Object);

        // Verify observer is subscribed (by publishing an event)
        var transaction = new Transaction(Guid.NewGuid(), TransactionType.Credit, 100, "Test");
        var domainEvent = new TransactionProcessedEvent(transaction);
        _eventPublisher.PublishAsync(domainEvent).Wait();

        observerMock.Verify(x => x.OnNext(domainEvent), Times.Once);
        observerMock.Reset();

        // Act
        _eventPublisher.Unsubscribe(observerMock.Object);

        // Assert
        // Publish another event to verify observer is no longer called
        var newTransaction = new Transaction(Guid.NewGuid(), TransactionType.Debit, 50, "Test2");
        var newDomainEvent = new TransactionProcessedEvent(newTransaction);
        _eventPublisher.PublishAsync(newDomainEvent).Wait();

        observerMock.Verify(x => x.OnNext(It.IsAny<IDomainEvent>()), Times.Never);
    }


}
