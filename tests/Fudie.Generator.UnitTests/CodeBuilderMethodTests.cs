using FluentAssertions;
using Fudie.Generator;
using Microsoft.CodeAnalysis;

namespace Fudie.Generator.UnitTests;

public class CodeBuilderMethodTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public CodeBuilderMethodTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Get Method

    [Fact]
    public void GenerateGetMethod_WithNoIncludes_ShouldGenerateBasicMethod()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateGetMethod(
            "Customer",
            "Guid",
            paths,
            asNoTracking: false,
            asSplitQuery: false,
            ignoreQueryFilters: false);

        // Assert
        result.Should().Contain("public async Task<Customer> Get(Guid id)");
        result.Should().Contain("IQueryable<Customer> query = _entityLookup.Set<Customer>();");
        result.Should().Contain("var entity = await query.FirstOrDefaultAsync(c => c.Id == id);");
        result.Should().Contain("if (entity == null)");
        result.Should().Contain("throw new KeyNotFoundException");
        result.Should().Contain("return entity;");
        result.Should().NotContain("Include");
        result.Should().NotContain("AsNoTracking");
    }

    [Fact]
    public void GenerateGetMethod_WithSingleInclude_ShouldIncludeIncludeCode()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path };

        // Act
        var result = CodeBuilder.GenerateGetMethod(
            "Customer",
            "Guid",
            paths,
            asNoTracking: false,
            asSplitQuery: false,
            ignoreQueryFilters: false);

        // Assert
        result.Should().Contain("// Apply includes");
        result.Should().Contain("query = query.Include(c => c.Orders);");
    }

    [Fact]
    public void GenerateGetMethod_WithMultipleIncludes_ShouldIncludeAllIncludes()
    {
        // Arrange
        var path1 = PathValidator.ValidatePath("Orders.OrderItems", _testData.customerSymbol, _testData.compilation);
        var path2 = PathValidator.ValidatePath("Address", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path1, path2 };

        // Act
        var result = CodeBuilder.GenerateGetMethod(
            "Customer",
            "Guid",
            paths,
            asNoTracking: false,
            asSplitQuery: false,
            ignoreQueryFilters: false);

        // Assert
        result.Should().Contain("query = query.Include(c => c.Orders)");
        result.Should().Contain(".ThenInclude(o => o.OrderItems);");
        result.Should().Contain("query = query.Include(c => c.Address);");
    }

    [Fact]
    public void GenerateGetMethod_WithAsNoTracking_ShouldIncludeAsNoTracking()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateGetMethod(
            "Customer",
            "Guid",
            paths,
            asNoTracking: true,
            asSplitQuery: false,
            ignoreQueryFilters: false);

        // Assert
        result.Should().Contain("// Apply query modifiers");
        result.Should().Contain("query = query.AsNoTracking();");
    }

    [Fact]
    public void GenerateGetMethod_WithAsSplitQuery_ShouldIncludeAsSplitQuery()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateGetMethod(
            "Customer",
            "Guid",
            paths,
            asNoTracking: false,
            asSplitQuery: true,
            ignoreQueryFilters: false);

        // Assert
        result.Should().Contain("// Apply query modifiers");
        result.Should().Contain("query = query.AsSplitQuery();");
    }

    [Fact]
    public void GenerateGetMethod_WithIgnoreQueryFilters_ShouldIncludeIgnoreQueryFilters()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateGetMethod(
            "Customer",
            "Guid",
            paths,
            asNoTracking: false,
            asSplitQuery: false,
            ignoreQueryFilters: true);

        // Assert
        result.Should().Contain("// Apply query modifiers");
        result.Should().Contain("query = query.IgnoreQueryFilters();");
    }

    [Fact]
    public void GenerateGetMethod_WithAllModifiers_ShouldIncludeAllInCorrectOrder()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateGetMethod(
            "Customer",
            "Guid",
            paths,
            asNoTracking: true,
            asSplitQuery: true,
            ignoreQueryFilters: true);

        // Assert
        result.Should().Contain("// Apply query modifiers");

        // Verificar orden: AsSplitQuery -> AsNoTracking -> IgnoreQueryFilters
        var splitIndex = result.IndexOf("AsSplitQuery");
        var trackingIndex = result.IndexOf("AsNoTracking");
        var filtersIndex = result.IndexOf("IgnoreQueryFilters");

        splitIndex.Should().BeLessThan(trackingIndex);
        trackingIndex.Should().BeLessThan(filtersIndex);
    }

    [Fact]
    public void GenerateGetMethod_WithComplexScenario_ShouldGenerateCompleteMethod()
    {
        // Arrange
        var path1 = PathValidator.ValidatePath("Orders.OrderItems.Product", _testData.customerSymbol, _testData.compilation);
        var path2 = PathValidator.ValidatePath("Address", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path1, path2 };

        // Act
        var result = CodeBuilder.GenerateGetMethod(
            "Customer",
            "Guid",
            paths,
            asNoTracking: true,
            asSplitQuery: true,
            ignoreQueryFilters: false);

        // Assert
        result.Should().Contain("public async Task<Customer> Get(Guid id)");
        result.Should().Contain("IQueryable<Customer> query = _entityLookup.Set<Customer>();");
        result.Should().Contain("// Apply includes");
        result.Should().Contain("query = query.Include(c => c.Orders)");
        result.Should().Contain(".ThenInclude(o => o.OrderItems)");
        result.Should().Contain(".ThenInclude(oi => oi.Product);");
        result.Should().Contain("query = query.Include(c => c.Address);");
        result.Should().Contain("// Apply query modifiers");
        result.Should().Contain("query = query.AsSplitQuery();");
        result.Should().Contain("query = query.AsNoTracking();");
        result.Should().Contain("var entity = await query.FirstOrDefaultAsync(c => c.Id == id);");
        result.Should().Contain("return entity;");
    }

    #endregion

    #region Add Method

    [Fact]
    public void GenerateAddMethod_ShouldGenerateCorrectMethod()
    {
        // Act
        var result = CodeBuilder.GenerateAddMethod("Customer");

        // Assert
        result.Should().Contain("public void Add(Customer entity)");
        result.Should().Contain("_changeTracker.Entry(entity).State = EntityState.Added;");
    }

    #endregion

    #region Remove Method

    [Fact]
    public void GenerateRemoveMethod_ShouldGenerateCorrectMethod()
    {
        // Act
        var result = CodeBuilder.GenerateRemoveMethod("Customer");

        // Assert
        result.Should().Contain("public void Remove(Customer entity)");
        result.Should().Contain("_changeTracker.Entry(entity).State = EntityState.Deleted;");
    }

    #endregion

    #region Update Method

    [Fact]
    public void GenerateUpdateGetMethod_WithNoIncludes_ShouldGenerateBasicMethodWithTracking()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateUpdateGetMethod(
            "Customer",
            "Guid",
            paths,
            asSplitQuery: false);

        // Assert
        result.Should().Contain("public async Task<Customer> Get(Guid id)");
        result.Should().Contain("IQueryable<Customer> query = _entityLookup.Set<Customer>();  // Tracking habilitado para updates");
        result.Should().Contain("var entity = await query.FirstOrDefaultAsync(c => c.Id == id);");
        result.Should().Contain("if (entity == null)");
        result.Should().Contain("throw new KeyNotFoundException");
        result.Should().Contain("return entity;");
        result.Should().NotContain("Include");
        result.Should().NotContain("AsNoTracking");  // KEY: NO debe tener AsNoTracking
    }

    [Fact]
    public void GenerateUpdateGetMethod_ShouldUseSetInsteadOfQuery()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateUpdateGetMethod(
            "Customer",
            "Guid",
            paths,
            asSplitQuery: false);

        // Assert
        result.Should().Contain("_entityLookup.Set<Customer>()");
        result.Should().NotContain("_entityLookup.Query<Customer>()");
    }

    [Fact]
    public void GenerateUpdateGetMethod_WithSingleInclude_ShouldIncludeIncludeCode()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path };

        // Act
        var result = CodeBuilder.GenerateUpdateGetMethod(
            "Customer",
            "Guid",
            paths,
            asSplitQuery: false);

        // Assert
        result.Should().Contain("// Apply includes");
        result.Should().Contain("query = query.Include(c => c.Orders);");
    }

    [Fact]
    public void GenerateUpdateGetMethod_WithMultipleIncludes_ShouldIncludeAllIncludes()
    {
        // Arrange
        var path1 = PathValidator.ValidatePath("Orders.OrderItems", _testData.customerSymbol, _testData.compilation);
        var path2 = PathValidator.ValidatePath("Address", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path1, path2 };

        // Act
        var result = CodeBuilder.GenerateUpdateGetMethod(
            "Customer",
            "Guid",
            paths,
            asSplitQuery: false);

        // Assert
        result.Should().Contain("query = query.Include(c => c.Orders)");
        result.Should().Contain(".ThenInclude(o => o.OrderItems);");
        result.Should().Contain("query = query.Include(c => c.Address);");
    }

    [Fact]
    public void GenerateUpdateGetMethod_WithAsSplitQuery_ShouldIncludeAsSplitQuery()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateUpdateGetMethod(
            "Customer",
            "Guid",
            paths,
            asSplitQuery: true);

        // Assert
        result.Should().Contain("// Apply query modifiers");
        result.Should().Contain("query = query.AsSplitQuery();");
    }

    [Fact]
    public void GenerateUpdateGetMethod_WithComplexScenario_ShouldGenerateCompleteMethodWithTracking()
    {
        // Arrange
        var path1 = PathValidator.ValidatePath("Orders.OrderItems.Product", _testData.customerSymbol, _testData.compilation);
        var path2 = PathValidator.ValidatePath("Address", _testData.customerSymbol, _testData.compilation);
        var paths = new[] { path1, path2 };

        // Act
        var result = CodeBuilder.GenerateUpdateGetMethod(
            "Customer",
            "Guid",
            paths,
            asSplitQuery: true);

        // Assert
        result.Should().Contain("public async Task<Customer> Get(Guid id)");
        result.Should().Contain("IQueryable<Customer> query = _entityLookup.Set<Customer>();  // Tracking habilitado para updates");
        result.Should().Contain("// Apply includes");
        result.Should().Contain("query = query.Include(c => c.Orders)");
        result.Should().Contain(".ThenInclude(o => o.OrderItems)");
        result.Should().Contain(".ThenInclude(oi => oi.Product);");
        result.Should().Contain("query = query.Include(c => c.Address);");
        result.Should().Contain("// Apply query modifiers");
        result.Should().Contain("query = query.AsSplitQuery();");
        result.Should().NotContain("AsNoTracking");  // KEY: NO debe tener AsNoTracking
        result.Should().NotContain("IgnoreQueryFilters");  // KEY: NO debe tener IgnoreQueryFilters
        result.Should().Contain("var entity = await query.FirstOrDefaultAsync(c => c.Id == id);");
        result.Should().Contain("return entity;");
    }

    [Fact]
    public void GenerateUpdateGetMethod_ShouldNotIncludeAsNoTracking()
    {
        // Arrange - Escenario que tendría AsNoTracking en Get normal
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateUpdateGetMethod(
            "Customer",
            "Guid",
            paths,
            asSplitQuery: false);

        // Assert - Verificar explícitamente que NO tiene AsNoTracking
        result.Should().NotContain("AsNoTracking");
    }

    [Fact]
    public void GenerateUpdateGetMethod_ShouldNotIncludeIgnoreQueryFilters()
    {
        // Arrange
        var paths = Array.Empty<PathValidator.IncludePathInfo>();

        // Act
        var result = CodeBuilder.GenerateUpdateGetMethod(
            "Customer",
            "Guid",
            paths,
            asSplitQuery: false);

        // Assert - Verificar explícitamente que NO tiene IgnoreQueryFilters
        result.Should().NotContain("IgnoreQueryFilters");
    }

    #endregion
}
