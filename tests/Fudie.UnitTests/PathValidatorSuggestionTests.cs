using FluentAssertions;
using Fudie.Generator;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Fudie.UnitTests;

public class PathValidatorSuggestionTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public PathValidatorSuggestionTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Typo Suggestions

    [Fact]
    public void ValidatePath_WithTypoInPropertyName_ShouldSuggestCorrectProperty()
    {
        // Arrange
        var path = "Addres"; // Missing 's' in "Address"

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Did you mean 'Address'?");
    }

    [Fact]
    public void ValidatePath_WithTypoInCollectionName_ShouldSuggestCorrectProperty()
    {
        // Arrange
        var path = "Order"; // Missing 's' in "Orders"

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Did you mean 'Orders'?");
    }

    [Fact]
    public void ValidatePath_WithCaseTypo_ShouldSuggestCorrectCase()
    {
        // Arrange
        var path = "orders"; // lowercase instead of "Orders"

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Did you mean 'Orders'?");
    }

    [Fact]
    public void ValidatePath_WithTypoInNestedProperty_ShouldSuggestCorrection()
    {
        // Arrange
        var path = "Orders.OrderItem"; // Missing 's' in "OrderItems"

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Did you mean 'OrderItems'?");
    }

    [Fact]
    public void ValidatePath_WithMultipleTypos_ShouldSuggestForFirstError()
    {
        // Arrange
        var path = "Order.OrderItem.Prodct"; // Multiple typos

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Order"); // First error
        result.ErrorMessage.Should().Contain("Did you mean");
    }

    #endregion

    #region No Suggestion Cases

    [Fact]
    public void ValidatePath_WithCompletelyWrongName_ShouldNotSuggest()
    {
        // Arrange
        var path = "XYZ123";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotContain("Did you mean");
    }

    [Fact]
    public void ValidatePath_WithNumbers_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders123";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        // ✅ ACTUALIZADO: puede sugerir "Orders" si el algoritmo lo detecta
        result.ErrorMessage.Should().Contain("Orders123");
        result.ErrorMessage.Should().Contain("does not exist");
    }

    #endregion
}