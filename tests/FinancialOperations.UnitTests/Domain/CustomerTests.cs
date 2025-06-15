using FinancialOperations.Domain.Entities;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.Domain;

public class CustomerTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCustomer()
    {
        // Arrange
        var name = "Welisson Arley";
        var document = "12345678901";
        var email = "welisson@email.com";

        // Act
        var customer = new Customer(name, document, email);

        // Assert
        customer.Name.ShouldBe(name);
        customer.Document.ShouldBe(document);
        customer.Email.ShouldBe(email);
        customer.Accounts.Count.ShouldBe(0);
        customer.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(null, "12345678901", "welisson@email.com")]
    [InlineData("Welisson", null, "welisson@email.com")]
    [InlineData("Welisson", "12345678901", null)]
    public void Constructor_WithNullParameters_ShouldThrowArgumentNullException(string name, string document, string email)
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Customer(name, document, email));
    }

    [Fact]
    public void CreateAccount_WithCreditLimit_ShouldCreateAndAddAccount()
    {
        // Arrange
        var customer = new Customer("Welisson Arley", "12345678901", "welisson@email.com");
        var creditLimit = 1000m;

        // Act
        var account = customer.CreateAccount(creditLimit);

        // Assert
        customer.Accounts.Count.ShouldBe(1);
        customer.Accounts.First().ShouldBe(account);
        account.CustomerId.ShouldBe(customer.Id);
        account.CreditLimit.ShouldBe(creditLimit);
    }

    [Fact]
    public void CreateAccount_WithoutCreditLimit_ShouldCreateAccountWithZeroLimit()
    {
        // Arrange
        var customer = new Customer("Welisson Arley", "12345678901", "welisson@email.com");

        // Act
        var account = customer.CreateAccount();

        // Assert
        customer.Accounts.Count.ShouldBe(1);
        account.CreditLimit.ShouldBe(0);
    }

    [Fact]
    public void CreateAccount_MultipleCalls_ShouldCreateMultipleAccounts()
    {
        // Arrange
        var customer = new Customer("Welisson Arley", "12345678901", "welisson@email.com");

        // Act
        var account1 = customer.CreateAccount(1000);
        var account2 = customer.CreateAccount(500);

        // Assert
        customer.Accounts.Count.ShouldBe(2);
        customer.Accounts.ShouldContain(account1);
        customer.Accounts.ShouldContain(account2);
        account1.Id.ShouldNotBe(account2.Id);
    }
}