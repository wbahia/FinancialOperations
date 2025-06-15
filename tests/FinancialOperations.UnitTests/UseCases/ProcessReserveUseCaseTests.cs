using FinancialOperations.Application.Interfaces;
using FinancialOperations.Application.UseCases;
using FinancialOperations.Domain.Common;
using FinancialOperations.Domain.Entities;
using FinancialOperations.Domain.Enums;
using FluentValidation;
using Moq;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.UseCases;

public class ProcessReserveUseCaseTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ProcessReserveValidator _validator;
    private readonly ProcessReserveUseCase _useCase;

    public ProcessReserveUseCaseTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _validator = new ProcessReserveValidator();
        _useCase = new ProcessReserveUseCase(_accountRepositoryMock.Object, _eventPublisherMock.Object, _validator);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReserveAmount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito"); 

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 200,
            Description = "Teste Reserva"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        account.AvailableBalance.ShouldBe(300); // 500 - 200
        account.ReservedBalance.ShouldBe(200);
        account.GetTotalBalance().ShouldBe(500); // saldo disponivel + reserva
        account.Transactions.Count.ShouldBe(2); // credito + reserva

        var reserveTransaction = account.Transactions.Last();
        reserveTransaction.Type.ShouldBe(TransactionType.Reserve);
        reserveTransaction.Amount.ShouldBe(200);
        reserveTransaction.Description.ShouldBe("Teste Reserva");

        _accountRepositoryMock.Verify(x => x.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        
    }

    [Fact]
    public async Task ExecuteAsync_WithExactAvailableBalance_ShouldReserveSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(300, "Credito");

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 300, 
            Description = "Saldo Reserva"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        account.AvailableBalance.ShouldBe(0);
        account.ReservedBalance.ShouldBe(300);
        account.GetTotalBalance().ShouldBe(300);
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientAvailableBalance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(100, "Credito"); 

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 200, // mais que o saldo disponivel
            Description = "Reserva Invalida"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
        exception.Message.ShouldBe("Insufficient available balance for reservation");

        // Account state should remain unchanged
        account.AvailableBalance.ShouldBe(100);
        account.ReservedBalance.ShouldBe(0);
        account.Transactions.Count.ShouldBe(1); 
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroAvailableBalance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000); // No initial credit, so 0 available balance

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 50,
            Description = "Reservation with zero balance"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public async Task ExecuteAsync_WithInvalidAmount_ShouldThrowValidationException(decimal amount)
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = amount,
            Description = "Reserva Invalida"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(() => _useCase.ExecuteAsync(request));
        exception.Errors.ShouldContain(x => x.PropertyName == nameof(ProcessReserveRequest.Amount));

        _accountRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _accountRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyAccountId_ShouldThrowValidationException()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.Empty,
            Amount = 100,
            Description = "Reserva Invalida"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(() => _useCase.ExecuteAsync(request));
        exception.Errors.ShouldContain(x => x.PropertyName == nameof(ProcessReserveRequest.AccountId));

        _accountRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentAccount_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = "Teste Reserva"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
        exception.Message.ShouldBe("Account not found");

        _accountRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyDescription_ShouldProcessSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 100,
            Description = "" // deve perrmitir descrição vazia
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        account.Transactions.Last().Description.ShouldBe("");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullDescription_ShouldProcessSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 100,
            Description = null! 
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        account.Transactions.Last().Description.ShouldBe("");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldClearDomainEventsAfterPublishing()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 200,
            Description = "Teste Reserva"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        account.DomainEvents.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithLargeAmount_ShouldProcessSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 10000);
        account.Credit(50000, "Creditao");

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 25000.50m, 
            Description = "Creditao"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        account.AvailableBalance.ShouldBe(24999.50m);
        account.ReservedBalance.ShouldBe(25000.50m);
        account.Transactions.Last().Amount.ShouldBe(25000.50m);
    }

    [Fact]
    public async Task ExecuteAsync_WithDecimalPrecision_ShouldHandleCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(100.99m, "Credito");

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 50.55m,
            Description = "Teste de precisao"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        account.AvailableBalance.ShouldBe(50.44m);
        account.ReservedBalance.ShouldBe(50.55m);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 200,
            Description = "Cancelamento"
        };

        var cancellationToken = new CancellationToken();

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, cancellationToken))
            .ReturnsAsync(account);

        // Act
        await _useCase.ExecuteAsync(request, cancellationToken);

        // Assert
        _accountRepositoryMock.Verify(x => x.GetByIdAsync(account.Id, cancellationToken), Times.Once);
        
    }

    [Fact]
    public async Task ExecuteAsync_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = "Teste de Exception"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
        exception.Message.ShouldBe("Repository error");
    }

    [Fact]
    public async Task ExecuteAsync_WithEventPublisherException_ShouldPropagateException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        var request = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 200,
            Description = "Exception"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _eventPublisherMock.Setup(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Event publisher error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
        exception.Message.ShouldBe("Event publisher error");
    }

    [Fact]
    public async Task ExecuteAsync_MultipleReservations_ShouldAccumulateReservedBalance()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(1000, "Credito");

        var request1 = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 200,
            Description = "1a reserva"
        };

        var request2 = new ProcessReserveRequest
        {
            AccountId = account.Id,
            Amount = 300,
            Description = "2a reserva"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        await _useCase.ExecuteAsync(request1);
        await _useCase.ExecuteAsync(request2);

        // Assert
        account.AvailableBalance.ShouldBe(500); // 1000 - 200 - 300
        account.ReservedBalance.ShouldBe(500);  // 200 + 300
        account.GetTotalBalance().ShouldBe(1000);
        account.Transactions.Count.ShouldBe(3); // Credito + 2 reservas
    }
}