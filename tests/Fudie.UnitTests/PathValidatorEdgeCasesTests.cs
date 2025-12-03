using FluentAssertions;
using Fudie.Generator;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Fudie.UnitTests;

public class PathValidatorEdgeCasesTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public PathValidatorEdgeCasesTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Whitespace Handling

    [Fact]
    public void ValidatePath_WithLeadingWhitespace_ShouldTrimAndSucceed()
    {
        // Arrange
        var path = "  Address";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        // ✅ ACTUALIZADO: PathValidator hace trim automático
        result.IsValid.Should().BeTrue();
        result.SegmentDetails[0].PropertyName.Should().Be("Address");
    }

    [Fact]
    public void ValidatePath_WithTrailingWhitespace_ShouldTrimAndSucceed()
    {
        // Arrange
        var path = "Address  ";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        // ✅ ACTUALIZADO: PathValidator hace trim automático
        result.IsValid.Should().BeTrue();
        result.SegmentDetails[0].PropertyName.Should().Be("Address");
    }

    [Fact]
    public void ValidatePath_WithWhitespaceAroundDot_ShouldTrimAndSucceed()
    {
        // Arrange
        var path = "Orders . OrderItems";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        // ✅ ACTUALIZADO: PathValidator hace trim de cada segmento
        result.IsValid.Should().BeTrue();
        result.Segments.Should().HaveCount(2);
        result.SegmentDetails[0].PropertyName.Should().Be("Orders");
        result.SegmentDetails[1].PropertyName.Should().Be("OrderItems");
    }

    #endregion

    #region Multiple Dots

    [Fact]
    public void ValidatePath_WithConsecutiveDots_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders..OrderItems";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidatePath_WithTrailingDot_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders.";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidatePath_WithLeadingDot_ShouldReturnInvalid()
    {
        // Arrange
        var path = ".Orders";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Special Characters

    [Fact]
    public void ValidatePath_WithSpecialCharacters_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders@OrderItems";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Location Handling

    [Fact]
    public void ValidatePath_WithLocation_ShouldPreserveLocation()
    {
        // Arrange
        var path = "Orders";
        var location = Location.None;

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation, location);

        // Assert
        result.Location.Should().Be(location);
    }

    [Fact]
    public void ValidatePath_WithoutLocation_ShouldHaveNullLocation()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.Location.Should().BeNull();
    }

    #endregion

    #region Very Long Paths

    [Fact]
    public void ValidatePath_WithVeryLongValidPath_ShouldReturnValid()
    {
        // Arrange
        var path = "Orders.OrderItems.Product.Category"; // 4 levels

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Segments.Should().HaveCount(4);
        result.SegmentDetails.Should().HaveCount(4);
    }

    #endregion

    
    #region Null Compilation/Symbol

    [Fact]
    public void ValidatePath_WithNullCompilation_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, null!);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Compilation cannot be null.");
    }

    [Fact]
    public void ValidatePath_WithNullRootEntity_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, null!, _testData.compilation);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Root entity type cannot be null.");
    }

    [Fact]
    public void ValidatePath_WithBothNull_ShouldReturnInvalidForRootEntity()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, null!, null!);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        // Valida root entity primero (orden de validación)
        result.ErrorMessage.Should().Be("Root entity type cannot be null.");
    }

    #endregion
}