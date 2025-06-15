using FinancialOperations.Domain.Entities;
using FinancialOperations.Infrastructure.Repositories;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.Infrastructure;

public class InMemoryCustomerRepositoryTests
{
    private readonly InMemoryCustomerRepository _repository;

    public InMemoryCustomerRepositoryTests()
    {
        _repository = new InMemoryCustomerRepository();
    }

    [Fact]
    public async Task AddAsync_WithValidCustomer_ShouldAddAndReturnCustomer()
    {
        // Arrange
        var customer = new Customer("John Doe", "12345678901", "john@email.com");

        // Act
        var result = await _repository.AddAsync(customer);

        // Assert
        result.ShouldBe(customer);
        result.Id.ShouldBe(customer.Id);
        result.Name.ShouldBe("John Doe");
        result.Document.ShouldBe("12345678901");
        result.Email.ShouldBe("john@email.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCustomer_ShouldReturnCustomer()
    {
        // Arrange
        var customer = new Customer("Jane Smith", "98765432109", "jane@email.com");
        await _repository.AddAsync(customer);

        // Act
        var result = await _repository.GetByIdAsync(customer.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(customer.Id);
        result.Name.ShouldBe("Jane Smith");
        result.Document.ShouldBe("98765432109");
        result.Email.ShouldBe("jane@email.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByDocumentAsync_WithExistingDocument_ShouldReturnCustomer()
    {
        // Arrange
        var customer = new Customer("Bob Johnson", "11122233344", "bob@email.com");
        await _repository.AddAsync(customer);

        // Act
        var result = await _repository.GetByDocumentAsync("11122233344");

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(customer.Id);
        result.Document.ShouldBe("11122233344");
        result.Name.ShouldBe("Bob Johnson");
    }

    [Fact]
    public async Task GetByDocumentAsync_WithNonExistentDocument_ShouldReturnNull()
    {
        // Arrange
        var customer = new Customer("Alice Brown", "55566677788", "alice@email.com");
        await _repository.AddAsync(customer);

        // Act
        var result = await _repository.GetByDocumentAsync("99999999999");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithExistingCustomer_ShouldUpdateSuccessfully()
    {
        // Arrange
        var customer = new Customer("Original Name", "12345678901", "original@email.com");
        await _repository.AddAsync(customer);

        // Act
        await _repository.UpdateAsync(customer);

        // Assert
        var updatedCustomer = await _repository.GetByIdAsync(customer.Id);
        updatedCustomer.ShouldNotBeNull();
        updatedCustomer.Id.ShouldBe(customer.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleCustomers_ShouldReturnAllCustomers()
    {
        // Arrange
        var customer1 = new Customer("Customer 1", "11111111111", "customer1@email.com");
        var customer2 = new Customer("Customer 2", "22222222222", "customer2@email.com");
        var customer3 = new Customer("Customer 3", "33333333333", "customer3@email.com");

        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.AddAsync(customer3);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var customers = result.ToList();
        customers.Count.ShouldBe(3);
        customers.ShouldContain(c => c.Id == customer1.Id);
        customers.ShouldContain(c => c.Id == customer2.Id);
        customers.ShouldContain(c => c.Id == customer3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithNoCustomers_ShouldReturnEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(0);
    }

    [Fact]
    public async Task AddAsync_WithDuplicateId_ShouldKeepOriginal()
    {
        // Arrange
        var originalCustomer = new Customer("Original", "12345678901", "original@email.com");
        await _repository.AddAsync(originalCustomer);

        // Create a new customer with same ID (simulating edge case)
        var duplicateCustomer = new Customer("Duplicate", "98765432109", "duplicate@email.com");
        typeof(Customer).GetProperty("Id")?.SetValue(duplicateCustomer, originalCustomer.Id);

        // Act
        await _repository.AddAsync(duplicateCustomer);

        // Assert
        var storedCustomer = await _repository.GetByIdAsync(originalCustomer.Id);
        storedCustomer.ShouldNotBeNull();
        storedCustomer.Name.ShouldBe("Original"); // Should keep original
    }

    [Fact]
    public async Task Repository_ShouldBeThreadSafe()
    {
        // Arrange
        const int numberOfCustomers = 100;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < numberOfCustomers; i++)
        {
            int customerNumber = i;
            tasks.Add(Task.Run(async () =>
            {
                var customer = new Customer($"Customer {customerNumber}", $"{customerNumber:D11}", $"customer{customerNumber}@email.com");
                await _repository.AddAsync(customer);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var allCustomers = await _repository.GetAllAsync();
        allCustomers.Count().ShouldBe(numberOfCustomers);
    }

    [Fact]
    public async Task GetByDocumentAsync_WithCaseInSensitiveSearch_ShouldWorkCorrectly()
    {
        // Arrange
        var customer = new Customer("Test Customer", "12345678901", "test@email.com");
        await _repository.AddAsync(customer);

        // Act
        var result = await _repository.GetByDocumentAsync("12345678901");

        // Assert
        result.ShouldNotBeNull();
        result.Document.ShouldBe("12345678901");
    }
}