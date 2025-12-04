using FluentAssertions;
using Fudie.Generator;
using Microsoft.CodeAnalysis;

namespace Fudie.UnitTests.Generator;

public class CodeBuilderIncludeTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public CodeBuilderIncludeTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Simple Include Chains

    [Fact]
    public void GenerateIncludeChain_WithSimplePath_ShouldGenerateCorrectCode()
    {
        // Arrange
        var path = "Address";
        var pathInfo = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(pathInfo, "Customer", "query");

        // Assert
        result.Should().Be("query = query.Include(c => c.Address);");
    }

    [Fact]
    public void GenerateIncludeChain_WithCollectionPath_ShouldGenerateCorrectCode()
    {
        // Arrange
        var path = "Orders";
        var pathInfo = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(pathInfo, "Customer", "query");

        // Assert
        result.Should().Be("query = query.Include(c => c.Orders);");
    }

    #endregion

    #region Nested Include Chains (ThenInclude)

    [Fact]
    public void GenerateIncludeChain_WithTwoLevelPath_ShouldGenerateThenInclude()
    {
        // Arrange
        var path = "Orders.OrderItems";
        var pathInfo = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(pathInfo, "Customer", "query");

        // Assert
        var expected = @"query = query.Include(c => c.Orders)
    .ThenInclude(o => o.OrderItems);";
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateIncludeChain_WithThreeLevelPath_ShouldGenerateMultipleThenInclude()
    {
        // Arrange
        var path = "Orders.OrderItems.Product";
        var pathInfo = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(pathInfo, "Customer", "query");

        // Assert
        var expected = @"query = query.Include(c => c.Orders)
    .ThenInclude(o => o.OrderItems)
    .ThenInclude(oi => oi.Product);";
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateIncludeChain_WithFourLevelPath_ShouldGenerateAllLevels()
    {
        // Arrange
        var path = "Orders.OrderItems.Product.Category";
        var pathInfo = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(pathInfo, "Customer", "query");

        // Assert
        var expected = @"query = query.Include(c => c.Orders)
    .ThenInclude(o => o.OrderItems)
    .ThenInclude(oi => oi.Product)
    .ThenInclude(p => p.Category);";
        result.Should().Be(expected);
    }

    #endregion

    #region Multiple Includes

    [Fact]
    public void GenerateMultipleIncludes_WithTwoPaths_ShouldGenerateBothIncludes()
    {
        // Arrange
        var path1 = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);
        var path2 = PathValidator.ValidatePath("Address", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path1, path2 };

        // Act
        var result = CodeBuilder.GenerateMultipleIncludes(paths, "Customer", "query");

        // Assert
        var expected = @"query = query.Include(c => c.Orders);
query = query.Include(c => c.Address);";
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateMultipleIncludes_WithMixedPaths_ShouldGenerateAllCorrectly()
    {
        // Arrange
        var path1 = PathValidator.ValidatePath("Orders.OrderItems", _testData.customerSymbol, _testData.compilation);
        var path2 = PathValidator.ValidatePath("Address", _testData.customerSymbol, _testData.compilation);
        var path3 = PathValidator.ValidatePath("Payments", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path1, path2, path3 };

        // Act
        var result = CodeBuilder.GenerateMultipleIncludes(paths, "Customer", "query");

        // Assert
        result.Should().Contain("query = query.Include(c => c.Orders)");
        result.Should().Contain(".ThenInclude(o => o.OrderItems);");
        result.Should().Contain("query = query.Include(c => c.Address);");
        result.Should().Contain("query = query.Include(c => c.Payments);");
    }

    [Fact]
    public void GenerateMultipleIncludes_WithInvalidPaths_ShouldSkipInvalidOnes()
    {
        // Arrange
        var path1 = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);
        var path2 = PathValidator.ValidatePath("InvalidProperty", _testData.customerSymbol, _testData.compilation);
        var path3 = PathValidator.ValidatePath("Address", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path1, path2, path3 };

        // Act
        var result = CodeBuilder.GenerateMultipleIncludes(paths, "Customer", "query");

        // Assert
        result.Should().Contain("query = query.Include(c => c.Orders);");
        result.Should().Contain("query = query.Include(c => c.Address);");
        result.Should().NotContain("InvalidProperty");
    }

    #endregion

    #region Empty/Invalid Cases

    [Fact]
    public void GenerateIncludeChain_WithInvalidPath_ShouldReturnEmpty()
    {
        // Arrange
        var pathInfo = PathValidator.ValidatePath("InvalidProperty", _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(pathInfo, "Customer", "query");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateIncludeChain_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var pathInfo = PathValidator.ValidatePath("", _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(pathInfo, "Customer", "query");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateMultipleIncludes_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateMultipleIncludes(paths, "Customer", "query");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Custom Query Variable Names

    [Fact]
    public void GenerateIncludeChain_WithCustomVariableName_ShouldUseCustomName()
    {
        // Arrange
        var path = "Orders";
        var pathInfo = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(pathInfo, "Customer", "myQuery");

        // Assert
        result.Should().Be("myQuery = myQuery.Include(c => c.Orders);");
    }

    #endregion
}