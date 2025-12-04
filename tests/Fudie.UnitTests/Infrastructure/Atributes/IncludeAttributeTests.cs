using FluentAssertions;
using Fudie.Infrastructure.Atributes;

namespace Fudie.UnitTests.Infrastructure.Atributes;

public class IncludeAttributeTests
{
    #region Test Entities

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithSinglePath_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var attribute = new IncludeAttribute<TestEntity>("Orders");

        // Assert
        attribute.Paths.Should().NotBeNull();
        attribute.Paths.Should().HaveCount(1);
        attribute.Paths[0].Should().Be("Orders");
        attribute.AsSplitQuery.Should().BeFalse(); // Default value
    }

    [Fact]
    public void Constructor_WithMultiplePaths_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var attribute = new IncludeAttribute<TestEntity>("Orders", "Address", "ContactMethods");

        // Assert
        attribute.Paths.Should().NotBeNull();
        attribute.Paths.Should().HaveCount(3);
        attribute.Paths[0].Should().Be("Orders");
        attribute.Paths[1].Should().Be("Address");
        attribute.Paths[2].Should().Be("ContactMethods");
    }

    [Fact]
    public void Constructor_WithNestedPath_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var attribute = new IncludeAttribute<TestEntity>("Orders.OrderItems.Product");

        // Assert
        attribute.Paths.Should().NotBeNull();
        attribute.Paths.Should().HaveCount(1);
        attribute.Paths[0].Should().Be("Orders.OrderItems.Product");
    }

    [Fact]
    public void Constructor_WithMultipleNestedPaths_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var attribute = new IncludeAttribute<TestEntity>(
            "Orders.OrderItems.Product",
            "Orders.OrderItems.Discount",
            "Customer.Address");

        // Assert
        attribute.Paths.Should().NotBeNull();
        attribute.Paths.Should().HaveCount(3);
        attribute.Paths[0].Should().Be("Orders.OrderItems.Product");
        attribute.Paths[1].Should().Be("Orders.OrderItems.Discount");
        attribute.Paths[2].Should().Be("Customer.Address");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Constructor_WithNullPaths_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new IncludeAttribute<TestEntity>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("paths");
    }

    [Fact]
    public void Constructor_WithEmptyPathsArray_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new IncludeAttribute<TestEntity>(Array.Empty<string>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("paths")
            .WithMessage("At least one path is required.*");
    }

    [Fact]
    public void Constructor_WithNullPathInArray_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new IncludeAttribute<TestEntity>("Orders", null!, "Address");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("paths")
            .WithMessage("Path at index 1 cannot be null or whitespace.*");
    }

    [Fact]
    public void Constructor_WithEmptyStringPath_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new IncludeAttribute<TestEntity>("Orders", "", "Address");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("paths")
            .WithMessage("Path at index 1 cannot be null or whitespace.*");
    }

    [Fact]
    public void Constructor_WithWhitespaceOnlyPath_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new IncludeAttribute<TestEntity>("Orders", "   ", "Address");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("paths")
            .WithMessage("Path at index 1 cannot be null or whitespace.*");
    }

    [Fact]
    public void Constructor_WithMultipleInvalidPaths_ShouldThrowForFirstInvalid()
    {
        // Arrange & Act
        var act = () => new IncludeAttribute<TestEntity>("Orders", "", null!, "Address");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("paths")
            .WithMessage("Path at index 1 cannot be null or whitespace.*");
    }

    #endregion

    #region AsSplitQuery Property Tests

    [Fact]
    public void AsSplitQuery_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var attribute = new IncludeAttribute<TestEntity>("Orders");

        // Assert
        attribute.AsSplitQuery.Should().BeFalse();
    }

    [Fact]
    public void AsSplitQuery_WhenSetToTrue_ShouldReturnTrue()
    {
        // Arrange
        var attribute = new IncludeAttribute<TestEntity>("Orders", "Address")
        {
            AsSplitQuery = true
        };

        // Act & Assert
        attribute.AsSplitQuery.Should().BeTrue();
    }

    [Fact]
    public void AsSplitQuery_WhenSetToFalse_ShouldReturnFalse()
    {
        // Arrange
        var attribute = new IncludeAttribute<TestEntity>("Orders")
        {
            AsSplitQuery = false
        };

        // Act & Assert
        attribute.AsSplitQuery.Should().BeFalse();
    }

    #endregion

    #region Attribute Usage Tests

    [Fact]
    public void IncludeAttribute_ShouldHaveCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(IncludeAttribute<TestEntity>);

        // Act
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.Should().Be(AttributeTargets.Interface);
        attributeUsage.AllowMultiple.Should().BeTrue();
        attributeUsage.Inherited.Should().BeFalse();
    }

    #endregion

    #region Generic Type Constraint Tests

    [Fact]
    public void IncludeAttribute_ShouldAcceptClassTypes()
    {
        // Arrange & Act
        var attribute = new IncludeAttribute<TestEntity>("Orders");

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void IncludeAttribute_ShouldWorkWithDifferentEntityTypes()
    {
        // Arrange & Act
        var attribute1 = new IncludeAttribute<TestEntity>("Orders");
        var attribute2 = new IncludeAttribute<string>("Length"); // string is a class

        // Assert
        attribute1.Should().NotBeNull();
        attribute2.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithVeryLongPath_ShouldInitializeCorrectly()
    {
        // Arrange
        var longPath = "Level1.Level2.Level3.Level4.Level5.Level6.Level7.Level8";

        // Act
        var attribute = new IncludeAttribute<TestEntity>(longPath);

        // Assert
        attribute.Paths.Should().HaveCount(1);
        attribute.Paths[0].Should().Be(longPath);
    }

    [Fact]
    public void Constructor_WithPathContainingSpecialCharacters_ShouldInitializeCorrectly()
    {
        // Arrange
        var pathWithUnderscore = "Order_Items.Product_Category";

        // Act
        var attribute = new IncludeAttribute<TestEntity>(pathWithUnderscore);

        // Assert
        attribute.Paths.Should().HaveCount(1);
        attribute.Paths[0].Should().Be(pathWithUnderscore);
    }

    [Fact]
    public void Paths_ShouldBeReadOnly()
    {
        // Arrange
        var attribute = new IncludeAttribute<TestEntity>("Orders", "Address");

        // Act
        var paths = attribute.Paths;

        // Assert
        paths.Should().NotBeNull();
        // Verify it's the same reference (not creating a new array each time)
        attribute.Paths.Should().BeSameAs(paths);
    }

    #endregion

    #region Multiple Attributes Scenario

    [Fact]
    public void MultipleAttributes_ShouldBeIndependent()
    {
        // Arrange & Act
        var attribute1 = new IncludeAttribute<TestEntity>("Orders") { AsSplitQuery = true };
        var attribute2 = new IncludeAttribute<TestEntity>("Address") { AsSplitQuery = false };

        // Assert
        attribute1.Paths[0].Should().Be("Orders");
        attribute1.AsSplitQuery.Should().BeTrue();

        attribute2.Paths[0].Should().Be("Address");
        attribute2.AsSplitQuery.Should().BeFalse();
    }

    #endregion
}
