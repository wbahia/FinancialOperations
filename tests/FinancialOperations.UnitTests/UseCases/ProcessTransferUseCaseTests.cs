using FinancialOperations.Application.Interfaces;
using FinancialOperations.Application.UseCases;
using FinancialOperations.Domain.Entities;
using FluentValidation;
using Moq;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.UseCases;

public class ProcessTransferUseCaseTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ProcessTransferValidator _validator;
    private readonly ProcessTransferUseCase _useCase;

    public ProcessTransferUseCaseTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _validator = new ProcessTransferValidator();
        _useCase = new ProcessTransferUseCase(_accountRepositoryMock.Object, _eventPublisherMock.Object, _validator);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidTransfer_ShouldTransferFunds()
    {
        // Arrange
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();
        var fromAccount = new Account(customer1Id, 1000);
        var toAccount = new Account(customer2Id, 1000);

        fromAccount.Credit(500, "Credit");

        var request = new ProcessTransferRequest
        {
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            Amount = 200,
            Description = "Teste Transferencia"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(fromAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromAccount);
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(toAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(toAccount);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.ShouldBeTrue();
        fromAccount.AvailableBalance.ShouldBe(300); // 500 - 200
        toAccount.AvailableBalance.ShouldBe(200);   // 0 + 200

        _accountRepositoryMock.Verify(x => x.UpdateAsync(fromAccount, It.IsAny<CancellationToken>()), Times.Once);
        _accountRepositoryMock.Verify(x => x.UpdateAsync(toAccount, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithSameAccount_ShouldThrowValidationException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var request = new ProcessTransferRequest
        {
            FromAccountId = accountId,
            ToAccountId = accountId,
            Amount = 100,
            Description = "Transferencia Invalida"
        };

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentAccounts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new ProcessTransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 100,
            Description = "Teste Transferencia"
        };

        _accountRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
    }
}