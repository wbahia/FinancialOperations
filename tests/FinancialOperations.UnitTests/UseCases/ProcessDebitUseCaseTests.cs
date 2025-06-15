using FinancialOperations.Application.Interfaces;
using FinancialOperations.Application.UseCases;
using FinancialOperations.Domain.Entities;
using FluentValidation;
using Moq;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.UseCases;

public class ProcessDebitUseCaseTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ProcessDebitValidator _validator;
    private readonly ProcessDebitUseCase _useCase;

    public ProcessDebitUseCaseTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _validator = new ProcessDebitValidator();
        _useCase = new ProcessDebitUseCase(_accountRepositoryMock.Object, _eventPublisherMock.Object, _validator);
    }

    [Fact]
    public async Task ExecuteAsync_WithSufficientBalance_ShouldDebitAccount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito"); 

        var request = new ProcessDebitRequest
        {
            AccountId = account.Id,
            Amount = 200,
            Description = "Debito"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        account.AvailableBalance.ShouldBe(300); // 500 - 200
        account.Transactions.Count.ShouldBe(2); // credito & debito
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientBalance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 100); 

        var request = new ProcessDebitRequest
        {
            AccountId = account.Id,
            Amount = 200,
            Description = "Debito"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task ExecuteAsync_WithInvalidAmount_ShouldThrowValidationException(decimal amount)
    {
        // Arrange
        var request = new ProcessDebitRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = amount,
            Description = "Debito invalido"
        };

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => _useCase.ExecuteAsync(request));
    }
}