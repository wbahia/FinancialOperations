using FinancialOperations.Application.UseCases;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.Validators;

public class ProcessCreditValidatorTests
{
    private readonly ProcessCreditValidator _validator;

    public ProcessCreditValidatorTests()
    {
        _validator = new ProcessCreditValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessCreditRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = "Credito"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_WithInvalidAmount_ShouldBeInvalid(decimal amount)
    {
        // Arrange
        var request = new ProcessCreditRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = amount,
            Description = "Credito Invalido"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(ProcessCreditRequest.Amount));
    }

    [Fact]
    public void Validate_WithEmptyAccountId_ShouldBeInvalid()
    {
        // Arrange
        var request = new ProcessCreditRequest
        {
            AccountId = Guid.Empty,
            Amount = 100,
            Description = "Credito Invalido"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(ProcessCreditRequest.AccountId));
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessCreditRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = ""
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}