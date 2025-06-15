using FinancialOperations.Application.UseCases;
using Shouldly;
using Xunit;

namespace FinancialOperations.UnitTests.Validators;

public class ProcessReserveValidatorTests
{
    private readonly ProcessReserveValidator _validator;

    public ProcessReserveValidatorTests()
    {
        _validator = new ProcessReserveValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = "Reserva"
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
    [InlineData(-0.01)]
    public void Validate_WithInvalidAmount_ShouldBeInvalid(decimal amount)
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = amount,
            Description = "Reserva Invalida"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(ProcessReserveRequest.Amount));
        result.Errors.ShouldContain(x => x.ErrorMessage.Contains("greater than"));
    }

    [Fact]
    public void Validate_WithEmptyAccountId_ShouldBeInvalid()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.Empty,
            Amount = 100,
            Description = "Reserva Invalida"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(ProcessReserveRequest.AccountId));
        result.Errors.ShouldContain(x => x.ErrorMessage.Contains("empty"));
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessReserveRequest
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

    [Fact]
    public void Validate_WithNullDescription_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = null!
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithValidDecimalAmount_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 99.99m,
            Description = "Testando com casas decimais"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithLargeAmount_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 1000000.00m,
            Description = "Reservao"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithMinimumValidAmount_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 0.01m, 
            Description = "Valor minimo"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithLongDescription_ShouldBeValid()
    {
        // Arrange
        var longDescription = new string('A', 500); //descricao enorme 
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = longDescription
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue(); 
    }

    [Fact]
    public void Validate_WithSpecialCharactersInDescription_ShouldBeValid()
    {
        // Arrange
        var request = new ProcessReserveRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Description = "Reserva com caracteres especiais: áéíóú ñ ç $%&*"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}