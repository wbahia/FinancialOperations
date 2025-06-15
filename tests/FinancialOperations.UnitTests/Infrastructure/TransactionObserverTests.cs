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

public class TransactionObserverTests
{
    private readonly Mock<ILogger<TransactionObserver>> _loggerMock;
    private readonly TransactionObserver _observer;

    public TransactionObserverTests()
    {
        _loggerMock = new Mock<ILogger<TransactionObserver>>();
        _observer = new TransactionObserver(_loggerMock.Object);
    }

    [Fact]
    public void OnNext_WithTransactionProcessedEvent_ShouldLogTransactionDetails()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transaction = new Transaction(accountId, TransactionType.Credit, 500.50m, "Teste credito");
        var transactionEvent = new TransactionProcessedEvent(transaction);

        // Act
        _observer.OnNext(transactionEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Transacao Processada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Credit")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(accountId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(TransactionType.Credit, "Credit")]
    [InlineData(TransactionType.Debit, "Debit")]
    [InlineData(TransactionType.Reserve, "Reserve")]
    [InlineData(TransactionType.Capture, "Capture")]
    [InlineData(TransactionType.Refund, "Refund")]
    [InlineData(TransactionType.Transfer, "Transfer")]
    public void OnNext_WithDifferentTransactionTypes_ShouldLogCorrectType(TransactionType transactionType, string expectedTypeString)
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transaction = new Transaction(accountId, transactionType, 100, $"Test {transactionType}");
        var transactionEvent = new TransactionProcessedEvent(transaction);

        // Act
        _observer.OnNext(transactionEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedTypeString)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

        
    [Fact]
    public void OnNext_WithTransactionProcessedEvent_ShouldLogDescription()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var description = "Test transaction description with special characters: áéíóú";
        var transaction = new Transaction(accountId, TransactionType.Credit, 100, description);
        var transactionEvent = new TransactionProcessedEvent(transaction);

        // Act
        _observer.OnNext(transactionEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(description)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnNext_WithEmptyDescription_ShouldLogCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transaction = new Transaction(accountId, TransactionType.Credit, 100, "");
        var transactionEvent = new TransactionProcessedEvent(transaction);

        // Act
        _observer.OnNext(transactionEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnNext_WithNonTransactionEvent_ShouldLogGenericEvent()
    {
        // Arrange
        var customEvent = new Mock<IDomainEvent>();
        customEvent.Setup(x => x.Id).Returns(Guid.NewGuid());
        customEvent.Setup(x => x.OccurredAt).Returns(DateTime.UtcNow);

        // Act
        _observer.OnNext(customEvent.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Evento")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnError_WithException_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception message");

        // Act
        _observer.OnError(exception);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro em TransactionObserver")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnCompleted_ShouldLogCompletion()
    {
        // Act
        _observer.OnCompleted();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TransactionObserver concluido")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnNext_WithLargeAmount_ShouldLogCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var largeAmount = 999999999.99m;
        var transaction = new Transaction(accountId, TransactionType.Credit, largeAmount, "Large amount test");
        var transactionEvent = new TransactionProcessedEvent(transaction);

        // Act
        _observer.OnNext(transactionEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnNext_WithZeroAmount_ShouldLogCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transaction = new Transaction(accountId, TransactionType.Credit, 0.01m, "Minimum amount");
        var transactionEvent = new TransactionProcessedEvent(transaction);

        // Act
        _observer.OnNext(transactionEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnNext_WithMultipleEvents_ShouldLogEachEvent()
    {
        // Arrange
        var events = new List<TransactionProcessedEvent>();
        for (int i = 0; i < 5; i++)
        {
            var accountId = Guid.NewGuid();
            var transaction = new Transaction(accountId, TransactionType.Credit, 100 + i, $"Test transaction {i}");
            events.Add(new TransactionProcessedEvent(transaction));
        }

        // Act
        foreach (var eventItem in events)
        {
            _observer.OnNext(eventItem);
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(5));
    }

    [Fact]
    public void OnNext_WithNullEvent_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => _observer.OnNext(null!));
    }

    [Fact]
    public void OnError_WithNullException_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => _observer.OnError(null!));
    }
}
