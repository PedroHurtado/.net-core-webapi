using FluentAssertions;
using Fudie.Generator;
using Microsoft.CodeAnalysis;

namespace Fudie.UnitTests.Generator;

public class CodeBuilderParameterNameTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public CodeBuilderParameterNameTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Parameter Name Generation

    [Fact]
    public void GenerateIncludeChain_WithCustomer_ShouldUseParameterC()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "Customer", "query");

        // Assert
        result.Should().Contain("(c => c.Orders)");
    }

    [Fact]
    public void GenerateIncludeChain_WithOrder_ShouldUseParameterO()
    {
        // Arrange
        var path = PathValidator.ValidatePath("OrderItems", _testData.orderSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "Order", "query");

        // Assert
        result.Should().Contain("(o => o.OrderItems)");
    }

    [Fact]
    public void GenerateIncludeChain_WithProduct_ShouldUseParameterP()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Category", _testData.productSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "Product", "query");

        // Assert
        result.Should().Contain("(p => p.Category)");
    }

    [Fact]
    public void GenerateIncludeChain_WithNestedPath_ShouldUseCorrectParametersForEachLevel()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Orders.OrderItems.Product", _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "Customer", "query");

        // Assert
        result.Should().Contain("(c => c.Orders)");
        result.Should().Contain("(o => o.OrderItems)");
        result.Should().Contain("(oi => oi.Product)");
    }

    [Fact]
    public void GenerateIncludeChain_WithOrderItem_ShouldUseTwoLetterParameter()
    {
        // Arrange - OrderItem debería generar 'oi' porque 'o' podría confundirse
        var path = PathValidator.ValidatePath("Product", _testData.orderItemSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "OrderItem", "query");

        // Assert
        result.Should().Contain("(oi => oi.Product)");
    }

    #endregion
}