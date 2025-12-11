using FluentAssertions;
using Fudie.Generator;
using Microsoft.CodeAnalysis;

namespace Fudie.Generator.UnitTests;

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
        result.Should().Contain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped)]");
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

    [Fact]
    public void GenerateRepositoryClass_WithAdditionalUsings_ShouldIncludeEntityNamespace()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIAdd = true,
            AdditionalUsings = new[] { "MyApp.Domain.Models" }
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "Repository",
            "MyApp.Features.Products.Commands",
            "Product",
            "Guid",
            config);

        // Assert
        result.Should().Contain("using MyApp.Domain.Models;");
        result.Should().Contain("public class Repository : IAdd<Product>");
    }

    [Fact]
    public void GenerateRepositoryClass_WithMultipleAdditionalUsings_ShouldIncludeAll()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIAdd = true,
            AdditionalUsings = new[] { "MyApp.Domain.Models", "MyApp.Shared.Entities" }
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "Repository",
            "MyApp.Features.Products.Commands",
            "Product",
            "Guid",
            config);

        // Assert
        result.Should().Contain("using MyApp.Domain.Models;");
        result.Should().Contain("using MyApp.Shared.Entities;");
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
        result.Should().Contain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped)]");
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

    #region Container Interface Tests

    [Fact]
    public void GenerateRepositoryClass_WithContainerInterface_ShouldImplementContainerInterface()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIAdd = true,
            ContainerInterfaceName = "IRepository",
            ContainerInterfaceFullName = "CreateIngredient.IRepository",
            BaseInterfaceNames = new List<string> { "IAdd<Ingredient>" }
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CreateIngredient_Repository",
            "MyApp.Features.Ingredients.Commands",
            "Ingredient",
            "Guid",
            config);

        // Assert - uses ContainerInterfaceFullName for implementation
        result.Should().Contain("public class CreateIngredient_Repository : CreateIngredient.IRepository");
        result.Should().NotContain("public class CreateIngredient_Repository : IAdd<Ingredient>");
    }

    [Fact]
    public void GenerateRepositoryClass_WithContainerInterface_ShouldGenerateMultipleInjectableAttributes()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIAdd = true,
            ContainerInterfaceName = "IRepository",
            ContainerInterfaceFullName = "CreateIngredient.IRepository",
            BaseInterfaceNames = new List<string> { "IAdd<Ingredient>" }
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CreateIngredient_Repository",
            "MyApp.Features.Ingredients.Commands",
            "Ingredient",
            "Guid",
            config);

        // Assert
        result.Should().Contain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(CreateIngredient.IRepository))]");
        result.Should().Contain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IAdd<Ingredient>))]");
    }

    [Fact]
    public void GenerateRepositoryClass_WithMultipleBaseInterfaces_ShouldGenerateInjectableForEach()
    {
        // Arrange
        // Nota: IRemove hereda de IGet, así que en el uso real NO se incluye IGet en BaseInterfaceNames
        // cuando hay IRemove (eso lo filtra RepositorySourceGenerator). Este test refleja ese comportamiento.
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = true,
            ImplementIAdd = true,
            ImplementIRemove = true,
            ContainerInterfaceName = "ICustomerRepository",
            ContainerInterfaceFullName = "ICustomerRepository",
            BaseInterfaceNames = new List<string>
            {
                // IGet NO se incluye porque IRemove ya hereda de IGet
                "IAdd<Customer>",
                "IRemove<Customer, Guid>"
            }
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CustomerRepository",
            "MyApp.Repositories",
            "Customer",
            "Guid",
            config);

        // Assert - should have 3 Injectable attributes (1 container + 2 base, sin IGet duplicado)
        result.Should().Contain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(ICustomerRepository))]");
        result.Should().NotContain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IGet<Customer, Guid>))]");
        result.Should().Contain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IAdd<Customer>))]");
        result.Should().Contain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IRemove<Customer, Guid>))]");
    }

    [Fact]
    public void GenerateRepositoryClass_WithoutContainerInterface_ShouldUseLegacyBehavior()
    {
        // Arrange - without ContainerInterfaceName (legacy behavior)
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIAdd = true,
            ImplementIGet = true,
            // No ContainerInterfaceName set
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CustomerRepository",
            "MyApp.Repositories",
            "Customer",
            "Guid",
            config);

        // Assert - should use legacy behavior (single Injectable without ServiceType)
        result.Should().Contain("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped)]");
        result.Should().NotContain("ServiceType = typeof");
        result.Should().Contain("public class CustomerRepository : IGet<Customer, Guid>, IAdd<Customer>");
    }

    #endregion

    #region Code Coverage and Debugger Attributes

    [Fact]
    public void GenerateRepositoryClass_ShouldIncludeExcludeFromCodeCoverageAttribute()
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
        result.Should().Contain("[ExcludeFromCodeCoverage]");
        result.Should().Contain("using System.Diagnostics.CodeAnalysis;");
    }

    [Fact]
    public void GenerateRepositoryClass_ShouldIncludeDebuggerNonUserCodeAttribute()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIAdd = true
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "ProductRepository",
            "MyApp.Repositories",
            "Product",
            "int",
            config);

        // Assert
        result.Should().Contain("[DebuggerNonUserCode]");
        result.Should().Contain("using System.Diagnostics;");
    }

    [Fact]
    public void GenerateRepositoryClass_ShouldIncludeEditorBrowsableAttribute()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIRemove = true
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "OrderRepository",
            "MyApp.Repositories",
            "Order",
            "Guid",
            config);

        // Assert
        result.Should().Contain("[EditorBrowsable(EditorBrowsableState.Never)]");
        result.Should().Contain("using System.ComponentModel;");
    }

    [Fact]
    public void GenerateRepositoryClass_ShouldIncludeAllCodeGenerationAttributes()
    {
        // Arrange
        var config = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = true,
            ImplementIAdd = true
        };

        // Act
        var result = CodeBuilder.GenerateRepositoryClass(
            "CustomerRepository",
            "MyApp.Repositories",
            "Customer",
            "Guid",
            config);

        // Assert - All three attributes should be present
        result.Should().Contain("[ExcludeFromCodeCoverage]");
        result.Should().Contain("[DebuggerNonUserCode]");
        result.Should().Contain("[EditorBrowsable(EditorBrowsableState.Never)]");

        // Assert - Required usings for the attributes
        result.Should().Contain("using System.Diagnostics.CodeAnalysis;");
        result.Should().Contain("using System.Diagnostics;");
        result.Should().Contain("using System.ComponentModel;");
    }

    [Fact]
    public void GenerateRepositoryClass_AttributesShouldAppearBeforeInjectableAttribute()
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

        // Assert - Attributes should appear before [Injectable]
        var excludeFromCodeCoverageIndex = result.IndexOf("[ExcludeFromCodeCoverage]");
        var debuggerNonUserCodeIndex = result.IndexOf("[DebuggerNonUserCode]");
        var editorBrowsableIndex = result.IndexOf("[EditorBrowsable(EditorBrowsableState.Never)]");
        var injectableIndex = result.IndexOf("[Injectable(");

        excludeFromCodeCoverageIndex.Should().BeLessThan(injectableIndex);
        debuggerNonUserCodeIndex.Should().BeLessThan(injectableIndex);
        editorBrowsableIndex.Should().BeLessThan(injectableIndex);
    }

    #endregion
}
