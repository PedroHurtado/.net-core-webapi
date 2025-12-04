using FluentAssertions;
using Fudie.Generator;
using Microsoft.CodeAnalysis;

namespace Fudie.UnitTests.Generator;

public class PathValidatorTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public PathValidatorTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Null/Empty Validation

    [Fact]
    public void ValidatePath_WithNullPath_ShouldReturnInvalid()
    {
        // Arrange
        string path = null!;

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be null or whitespace");
    }

    [Fact]
    public void ValidatePath_WithEmptyPath_ShouldReturnInvalid()
    {
        // Arrange
        var path = "";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be null or whitespace");
    }

    [Fact]
    public void ValidatePath_WithWhitespacePath_ShouldReturnInvalid()
    {
        // Arrange
        var path = "   ";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be null or whitespace");
    }

    #endregion

    #region Simple Valid Paths

    [Fact]
    public void ValidatePath_WithSimpleNavigationProperty_ShouldReturnValid()
    {
        // Arrange
        var path = "Address";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.Segments.Should().HaveCount(1);
        result.Segments[0].Should().Be("Address");
        result.SegmentDetails.Should().HaveCount(1);
        result.SegmentDetails[0].PropertyName.Should().Be("Address");
        result.SegmentDetails[0].IsCollection.Should().BeFalse();
    }

    [Fact]
    public void ValidatePath_WithCollectionNavigationProperty_ShouldReturnValid()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Segments.Should().HaveCount(1);
        result.SegmentDetails[0].PropertyName.Should().Be("Orders");
        result.SegmentDetails[0].IsCollection.Should().BeTrue();
        result.SegmentDetails[0].ElementType.Should().NotBeNull();
        result.SegmentDetails[0].ElementType!.Name.Should().Be("Order");
    }

    #endregion

    #region Nested Valid Paths

    [Fact]
    public void ValidatePath_WithTwoLevelPath_ShouldReturnValid()
    {
        // Arrange
        var path = "Orders.OrderItems";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Segments.Should().HaveCount(2);
        result.Segments.Should().Equal("Orders", "OrderItems");
        result.SegmentDetails.Should().HaveCount(2);
        
        // First level
        result.SegmentDetails[0].PropertyName.Should().Be("Orders");
        result.SegmentDetails[0].IsCollection.Should().BeTrue();
        result.SegmentDetails[0].ElementType!.Name.Should().Be("Order");
        
        // Second level
        result.SegmentDetails[1].PropertyName.Should().Be("OrderItems");
        result.SegmentDetails[1].IsCollection.Should().BeTrue();
        result.SegmentDetails[1].ElementType!.Name.Should().Be("OrderItem");
    }

    [Fact]
    public void ValidatePath_WithThreeLevelPath_ShouldReturnValid()
    {
        // Arrange
        var path = "Orders.OrderItems.Product";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Segments.Should().HaveCount(3);
        result.Segments.Should().Equal("Orders", "OrderItems", "Product");
        result.SegmentDetails.Should().HaveCount(3);
        
        // Third level
        result.SegmentDetails[2].PropertyName.Should().Be("Product");
        result.SegmentDetails[2].IsCollection.Should().BeFalse();
        result.SegmentDetails[2].PropertyType.Name.Should().Be("Product");
    }

    [Fact]
    public void ValidatePath_WithFourLevelPath_ShouldReturnValid()
    {
        // Arrange
        var path = "Orders.OrderItems.Product.Category";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Segments.Should().HaveCount(4);
        result.SegmentDetails.Should().HaveCount(4);
        result.SegmentDetails[3].PropertyName.Should().Be("Category");
        result.SegmentDetails[3].IsCollection.Should().BeFalse();
    }

    #endregion

    #region Invalid Paths - Property Not Found

    [Fact]
    public void ValidatePath_WithNonExistentProperty_ShouldReturnInvalid()
    {
        // Arrange
        var path = "NonExistentProperty";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        // ✅ ACTUALIZADO: mensaje más específico
        result.ErrorMessage.Should().Contain("NonExistentProperty");
        result.ErrorMessage.Should().Contain("does not exist");
        result.ErrorMessage.Should().Contain("Customer");
    }

    [Fact]
    public void ValidatePath_WithNonExistentNestedProperty_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders.NonExistentProperty";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        // ✅ ACTUALIZADO: mensaje más específico
        result.ErrorMessage.Should().Contain("NonExistentProperty");
        result.ErrorMessage.Should().Contain("does not exist");
        result.ErrorMessage.Should().Contain("Order");
    }

    [Fact]
    public void ValidatePath_WithInvalidSecondSegment_ShouldProvideCorrectContext()
    {
        // Arrange
        var path = "Orders.InvalidProperty.Product";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("InvalidProperty");
        result.ErrorMessage.Should().Contain("Order");
    }

    #endregion

    #region Invalid Paths - Scalar Properties

    [Fact]
    public void ValidatePath_WithScalarProperty_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Name"; // string property

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        // ✅ ACTUALIZADO: mensaje más descriptivo
        result.ErrorMessage.Should().Contain("Name");
        result.ErrorMessage.Should().Contain("not a navigation property");
        result.ErrorMessage.Should().Contain("scalar");
    }

    [Fact]
    public void ValidatePath_WithScalarIntProperty_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Age"; // int property

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        // ✅ ACTUALIZADO: mensaje más descriptivo
        result.ErrorMessage.Should().Contain("Age");
        result.ErrorMessage.Should().Contain("not a navigation property");
        result.ErrorMessage.Should().Contain("scalar");
    }

    [Fact]
    public void ValidatePath_WithScalarInNestedPath_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders.TotalAmount"; // decimal property in Order

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        // ✅ ACTUALIZADO: mensaje más descriptivo
        result.ErrorMessage.Should().Contain("TotalAmount");
        result.ErrorMessage.Should().Contain("not a navigation property");
        result.ErrorMessage.Should().Contain("scalar");
    }

    [Fact]
    public void ValidatePath_WithScalarAtEndOfLongPath_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders.OrderItems.Quantity"; // int property

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        // ✅ ACTUALIZADO: mensaje más descriptivo
        result.ErrorMessage.Should().Contain("Quantity");
        result.ErrorMessage.Should().Contain("not a navigation property");
        result.ErrorMessage.Should().Contain("scalar");
    }

    #endregion

    #region Collection Types Detection

    [Fact]
    public void ValidatePath_WithICollectionProperty_ShouldDetectAsCollection()
    {
        // Arrange
        var path = "Orders"; // ICollection<Order>

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SegmentDetails[0].IsCollection.Should().BeTrue();
        result.SegmentDetails[0].ElementType!.Name.Should().Be("Order");
    }

    [Fact]
    public void ValidatePath_WithListProperty_ShouldDetectAsCollection()
    {
        // Arrange
        var path = "Payments"; // List<Payment>

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SegmentDetails[0].IsCollection.Should().BeTrue();
        result.SegmentDetails[0].ElementType!.Name.Should().Be("Payment");
    }

    [Fact]
    public void ValidatePath_WithHashSetProperty_ShouldDetectAsCollection()
    {
        // Arrange
        var path = "Notifications"; // HashSet<Notification>

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SegmentDetails[0].IsCollection.Should().BeTrue();
        result.SegmentDetails[0].ElementType!.Name.Should().Be("Notification");
    }

    #endregion

    #region OriginalPath Preservation

    [Fact]
    public void ValidatePath_ShouldPreserveOriginalPath()
    {
        // Arrange
        var path = "Orders.OrderItems.Product";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.OriginalPath.Should().Be(path);
    }

    #endregion
}