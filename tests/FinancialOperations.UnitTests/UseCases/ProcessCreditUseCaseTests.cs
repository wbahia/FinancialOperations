using FinancialOperations.Application.Interfaces;
using FinancialOperations.Application.UseCases;
using FinancialOperations.Domain.Common;
using FinancialOperations.Domain.Entities;
using FluentValidation;
using Moq;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.UseCases;

public class ProcessCreditUseCaseTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ProcessCreditValidator _validator;
    private readonly ProcessCreditUseCase _useCase;

    public ProcessCreditUseCaseTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _validator = new ProcessCreditValidator();
        _useCase = new ProcessCreditUseCase(_accountRepositoryMock.Object, _eventPublisherMock.Object, _validator);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldCreditAccount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        var request = new ProcessCreditRequest
        {
            AccountId = account.Id,
            Amount = 500,
            Description = "Credito"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        account.AvailableBalance.ShouldBe(500);
        account.Transactions.Count.ShouldBe(1);
        account.Transactions.First().Amount.ShouldBe(500);
        account.Transactions.First().Description.ShouldBe("Credito");

        _accountRepositoryMock.Verify(x => x.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidAmount_ShouldThrowValidationException()
    {
        // Arrange
        var request = new ProcessCreditRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = -100,
            Description = "Credito invalido"
        };

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => _useCase.ExecuteAsync(request));
        _accountRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyAccountId_ShouldThrowValidationException()
    {
        // Arrange
        var request = new ProcessCreditRequest
        {
            AccountId = Guid.Empty,
            Amount = 100,
            Description = "Credito invalido"
        };

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentAccount_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new ProcessCreditRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = "Credito"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldClearDomainEventsAfterPublishing()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        var request = new ProcessCreditRequest
        {
            AccountId = account.Id,
            Amount = 500,
            Description = "Credito"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        account.DomainEvents.Count.ShouldBe(0);
    }
}