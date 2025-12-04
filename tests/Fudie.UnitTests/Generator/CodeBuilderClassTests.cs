using FluentAssertions;
using Fudie.Generator;
using Microsoft.CodeAnalysis;

namespace Fudie.UnitTests.Generator;

public class CodeBuilderClassTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public CodeBuilderClassTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Basic Class Generation

    [Fact]
    public void GenerateRepositoryClass_WithIGetOnly_ShouldGenerateCorrectClass()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = true,
            ImplementIAdd = false,
            ImplementIUpdate = false,
            ImplementIRemove = false,
            IncludePaths = Array.Empty<PathValidator.IncludePathInfo>(),
            AsNoTracking = false,
            AsSplitQuery = false,
            IgnoreQueryFilters = false
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CustomerRepository",
            "MyApp.Repositories",
            "Customer",
            "Guid",
            config);

        // Assert
        result.Should().Contain("namespace MyApp.Repositories;");
        result.Should().Contain("[Injectable(ServiceLifetime.Scoped)]");
        result.Should().Contain("public class CustomerRepository : IGet<Customer, Guid>");
        result.Should().Contain("private readonly IEntityLookup _entityLookup;");
        result.Should().NotContain("IChangeTracker");
        result.Should().Contain("public CustomerRepository(IEntityLookup entityLookup)");
        result.Should().Contain("public async Task<Customer> Get(Guid id)");
        result.Should().NotContain("public void Add");
        result.Should().NotContain("public void Remove");
    }

    [Fact]
    public void GenerateRepositoryClass_WithIAddOnly_ShouldGenerateCorrectClass()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = false,
            ImplementIAdd = true,
            ImplementIUpdate = false,
            ImplementIRemove = false
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CustomerRepository",
            "MyApp.Repositories",
            "Customer",
            "Guid",
            config);

        // Assert
        result.Should().Contain("public class CustomerRepository : IAdd<Customer>");
        result.Should().Contain("private readonly IChangeTracker _changeTracker;");
        result.Should().NotContain("IEntityLookup");
        result.Should().Contain("public CustomerRepository(IChangeTracker changeTracker)");
        result.Should().Contain("public void Add(Customer entity)");
        result.Should().NotContain("public async Task<Customer> Get");
    }

    [Fact]
    public void GenerateRepositoryClass_WithMultipleInterfaces_ShouldImplementAll()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = true,
            ImplementIAdd = true,
            ImplementIUpdate = false,
            ImplementIRemove = true
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CustomerRepository",
            "MyApp.Repositories",
            "Customer",
            "Guid",
            config);

        // Assert
        result.Should().Contain("public class CustomerRepository : IGet<Customer, Guid>, IAdd<Customer>, IRemove<Customer, Guid>");
        result.Should().Contain("private readonly IEntityLookup _entityLookup;");
        result.Should().Contain("private readonly IChangeTracker _changeTracker;");
        result.Should().Contain("public CustomerRepository(IEntityLookup entityLookup, IChangeTracker changeTracker)");
        result.Should().Contain("public async Task<Customer> Get(Guid id)");
        result.Should().Contain("public void Add(Customer entity)");
        result.Should().Contain("public void Remove(Customer entity)");
    }

    #endregion

    #region Usings

    [Fact]
    public void GenerateRepositoryClass_ShouldIncludeAllRequiredUsings()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = true
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CustomerRepository",
            "MyApp.Repositories",
            "Customer",
            "Guid",
            config);

        // Assert
        result.Should().Contain("using System;");
        result.Should().Contain("using System.Collections.Generic;");
        result.Should().Contain("using System.Linq;");
        result.Should().Contain("using System.Threading.Tasks;");
        result.Should().Contain("using Microsoft.EntityFrameworkCore;");
        result.Should().Contain("using Fudie.Infrastructure;");
        result.Should().Contain("using Fudie.DependencyInjection;");
    }

    #endregion

    #region Complete Scenario

    [Fact]
    public void GenerateRepositoryClass_WithCompleteScenario_ShouldGenerateFullClass()
    {
        // Arrange
        var path1 = PathValidator.ValidatePath("Orders.OrderItems.Product", _testData.customerSymbol, _testData.compilation);
        var path2 = PathValidator.ValidatePath("Address", _testData.customerSymbol, _testData.compilation);

        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = true,
            ImplementIAdd = true,
            ImplementIUpdate = false,
            ImplementIRemove = false,
            IncludePaths = new[] { path1, path2 },
            AsNoTracking = true,
            AsSplitQuery = true,
            IgnoreQueryFilters = false
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CustomerRepository",
            "MyApp.Repositories",
            "Customer",
            "Guid",
            config);

        // Assert - Estructura básica
        result.Should().Contain("namespace MyApp.Repositories;");
        result.Should().Contain("[Injectable(ServiceLifetime.Scoped)]");
        result.Should().Contain("public class CustomerRepository : IGet<Customer, Guid>, IAdd<Customer>");
        
        // Assert - Fields y constructor
        result.Should().Contain("private readonly IEntityLookup _entityLookup;");
        result.Should().Contain("private readonly IChangeTracker _changeTracker;");
        result.Should().Contain("public CustomerRepository(IEntityLookup entityLookup, IChangeTracker changeTracker)");
        
        // Assert - Método Get con includes
        result.Should().Contain("public async Task<Customer> Get(Guid id)");
        result.Should().Contain("query = query.Include(c => c.Orders)");
        result.Should().Contain(".ThenInclude(o => o.OrderItems)");
        result.Should().Contain(".ThenInclude(oi => oi.Product);");
        result.Should().Contain("query = query.Include(c => c.Address);");
        
        // Assert - Query modifiers
        result.Should().Contain("query = query.AsSplitQuery();");
        result.Should().Contain("query = query.AsNoTracking();");
        
        // Assert - Método Add
        result.Should().Contain("public void Add(Customer entity)");
        result.Should().Contain("_changeTracker.Entry(entity).State = EntityState.Added;");
    }

    #endregion
}