using FinancialOperations.Domain.Entities;
using FinancialOperations.Domain.Enums;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.Domain;

public class AccountTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAccount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var creditLimit = 1000m;

        // Act
        var account = new Account(customerId, creditLimit);

        // Assert
        account.CustomerId.ShouldBe(customerId);
        account.CreditLimit.ShouldBe(creditLimit);
        account.AvailableBalance.ShouldBe(0);
        account.ReservedBalance.ShouldBe(0);
        account.Status.ShouldBe(AccountStatus.Active);
    }

    [Fact]
    public void Credit_WithValidAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);

        // Act
        account.Credit(500, "PIX");

        // Assert
        account.AvailableBalance.ShouldBe(500);
        account.Transactions.Count.ShouldBe(1);
        account.Transactions.First().Type.ShouldBe(TransactionType.Credit);
        account.Transactions.First().Amount.ShouldBe(500);
        account.Transactions.First().Description.ShouldBe("PIX");
        account.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void Debit_WithSufficientBalance_ShouldDecreaseBalance()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        // Act
        account.Debit(200, "Debito");

        // Assert
        account.AvailableBalance.ShouldBe(300);
        account.Transactions.Count.ShouldBe(2);
        account.Transactions.Last().Type.ShouldBe(TransactionType.Debit);
        account.Transactions.Last().Amount.ShouldBe(200);
    }

    [Fact]
    public void Debit_WithCreditLimit_ShouldAllowNegativeBalance()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        // Act
        account.Debit(800, "Teste Debito Saldo Negativo");

        // Assert
        account.AvailableBalance.ShouldBe(-300);
    }

    [Fact]
    public void Debit_WithInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 100);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => account.Debit(200, "Debito Invalido"));
    }

    [Fact]
    public void Reserve_WithSufficientBalance_ShouldMoveToReserved()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        // Act
        account.Reserve(200, "Teste Reserva");

        // Assert
        account.AvailableBalance.ShouldBe(300);
        account.ReservedBalance.ShouldBe(200);
        account.GetTotalBalance().ShouldBe(500);
        account.Transactions.Count.ShouldBe(2);
        account.Transactions.Last().Type.ShouldBe(TransactionType.Reserve);
    }

    [Fact]
    public void Reserve_WithInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(100, "Credito");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => account.Reserve(200, "Erro Reserva"));
    }

    [Fact]
    public void Capture_WithSufficientReserved_ShouldRemoveFromReserved()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");
        account.Reserve(200, "Teste reserva");

        // Act
        account.Capture(150, "Teste captura");

        // Assert
        account.AvailableBalance.ShouldBe(300);
        account.ReservedBalance.ShouldBe(50);
        account.GetTotalBalance().ShouldBe(350);
        account.Transactions.Count.ShouldBe(3);
        account.Transactions.Last().Type.ShouldBe(TransactionType.Capture);
    }

    [Fact]
    public void Capture_WithInsufficientReserved_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");
        account.Reserve(100, "Teste Reserva");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => account.Capture(200, "Erro Captura"));
    }

    [Fact]
    public void Refund_WithSufficientReserved_ShouldMoveBackToAvailable()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");
        account.Reserve(200, "Teste reserva");

        // Act
        account.Refund(100, "Teste refund");

        // Assert
        account.AvailableBalance.ShouldBe(400);
        account.ReservedBalance.ShouldBe(100);
        account.GetTotalBalance().ShouldBe(500);
        account.Transactions.Count.ShouldBe(3);
        account.Transactions.Last().Type.ShouldBe(TransactionType.Refund);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Credit_WithInvalidAmount_ShouldThrowException(decimal amount)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);

        // Act & Assert
        Should.Throw<ArgumentException>(() => account.Credit(amount, "Credito invalido"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Debit_WithInvalidAmount_ShouldThrowException(decimal amount)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);

        // Act & Assert
        Should.Throw<ArgumentException>(() => account.Debit(amount, "Debito invalido"));
    }

    [Fact]
    public void GetTotalLimit_ShouldReturnAvailableBalancePlusCreditLimit()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = new Account(customerId, 1000);
        account.Credit(500, "Credito");

        // Act
        var totalLimit = account.GetTotalLimit();

        // Assert
        totalLimit.ShouldBe(1500); // 500 disponivel + 1000 limite
    }
}