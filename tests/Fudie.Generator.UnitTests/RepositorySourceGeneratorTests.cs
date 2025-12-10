using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Fudie.Generator;

namespace Fudie.Generator.UnitTests;

public class RepositorySourceGeneratorTests
{
    #region Helper Methods

    private static GeneratorDriverRunResult RunGenerator(string sourceCode)
    {
        // Crear compilación con el código fuente
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Crear y ejecutar el generador
        var generator = new RepositorySourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        return driver.GetRunResult();
    }

    private static string CreateTestCode(
        string interfaceCode,
        string? entityCode = null,
        bool includeFudieInfrastructure = true)
    {
        var code = @"
using System;
using System.Collections.Generic;
using Fudie.Attributes;
using Fudie.Infrastructure;
using Fudie.Domain;

namespace TestNamespace
{
    // Entity
    public class Entity
    {
        public Guid Id { get; protected set; }
    }
";

        if (entityCode != null)
        {
            code += entityCode + "\n";
        }

        code += interfaceCode;
        code += "\n}";

        if (includeFudieInfrastructure)
        {
            code += @"
namespace Fudie.Infrastructure
{
    public interface IGet<T, ID> { }
    public interface IAdd<T> { }
    public interface IUpdate<T, ID> : IGet<T, ID> { }
    public interface IRemove<T, ID> : IGet<T, ID> { }
    public interface IEntityLookup { }
    public interface IChangeTracker { }
}

namespace Fudie.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
    public class IncludeAttribute : System.Attribute
    {
        public IncludeAttribute(params string[] paths) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class AsNoTrackingAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class TrackingAttribute : System.Attribute
    {
        public TrackingAttribute(bool enabled) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class AsSplitQueryAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class IgnoreQueryFiltersAttribute : System.Attribute { }
}

namespace Fudie.DependencyInjection
{
    public enum ServiceLifetime { Scoped }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class InjectableAttribute : System.Attribute
    {
        public InjectableAttribute(ServiceLifetime lifetime) { }
    }
}
";
        }

        return code;
    }

    #endregion

    #region Basic Generation Tests

    [Fact]
    public void Generator_WithSimpleIGetInterface_ShouldGenerateRepository()
    {
        // Arrange - SIN atributos, solo herencia
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository");
        generatedCode.Should().Contain("IGet<Customer, System.Guid>");
        generatedCode.Should().Contain("public async Task<Customer> Get(System.Guid id)");
        generatedCode.Should().NotContain("AsNoTracking()"); // Sin atributo, no hay AsNoTracking
    }

    [Fact]
    public void Generator_WithIUpdateInterface_ShouldGenerateRepositoryWithTracking()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository : IUpdate<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository");
        generatedCode.Should().Contain("IUpdate<Customer, System.Guid>");
        generatedCode.Should().Contain("_entityLookup.Set<Customer>()");
        generatedCode.Should().NotContain("AsNoTracking()");
    }

    [Fact]
    public void Generator_WithIAddInterface_ShouldGenerateAddMethod()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository : IAdd<Customer>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository");
        generatedCode.Should().Contain("IAdd<Customer>");
        generatedCode.Should().Contain("public void Add(Customer entity)");
        generatedCode.Should().Contain("_changeTracker.Entry(entity).State = EntityState.Added");
    }

    [Fact]
    public void Generator_WithIRemoveInterface_ShouldGenerateRemoveMethod()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository : IRemove<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository");
        generatedCode.Should().Contain("IRemove<Customer, System.Guid>");
        generatedCode.Should().Contain("public void Remove(Customer entity)");
        generatedCode.Should().Contain("_changeTracker.Entry(entity).State = EntityState.Deleted");
    }

    [Fact]
    public void Generator_WithMultipleInterfaces_ShouldImplementAll()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    public interface ICustomerRepository :
        IGet<Customer, Guid>,
        IAdd<Customer>,
        IRemove<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();

        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("IGet<Customer, System.Guid>");
        generatedCode.Should().Contain("IAdd<Customer>");
        generatedCode.Should().Contain("IRemove<Customer, System.Guid>");
        generatedCode.Should().Contain("public async Task<Customer> Get(System.Guid id)");
        generatedCode.Should().Contain("public void Add(Customer entity)");
        generatedCode.Should().Contain("public void Remove(Customer entity)");
    }

    #endregion

    #region Attribute Tests

    [Fact]
    public void Generator_WithAsNoTrackingAttribute_ShouldGenerateAsNoTracking()
    {
        // Arrange - CON atributo [AsNoTracking]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    [AsNoTracking]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("query = query.AsNoTracking()");
    }

    [Fact]
    public void Generator_WithIncludeAttribute_ShouldGenerateIncludes()
    {
        // Arrange - CON atributo [Include]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Order : Entity
    {
        public Guid CustomerId { get; set; }
    }

    public class Customer : Entity
    {
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }

    [Include(""Orders"")]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("query = query.Include(c => c.Orders)");
    }

    [Fact]
    public void Generator_WithAsSplitQueryAttribute_ShouldGenerateAsSplitQuery()
    {
        // Arrange - CON atributo [AsSplitQuery]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    [AsSplitQuery]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("query = query.AsSplitQuery()");
    }

    [Fact]
    public void Generator_WithIgnoreQueryFiltersAttribute_ShouldGenerateIgnoreQueryFilters()
    {
        // Arrange - CON atributo [IgnoreQueryFilters]
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    [IgnoreQueryFilters]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("query = query.IgnoreQueryFilters()");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Generator_WithInvalidIncludePath_ShouldReportError()
    {
        // Arrange
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity
    {
        public string Name { get; set; }
    }

    [Include(""NonExistentProperty"")]
    public interface ICustomerRepository : IGet<Customer, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().Contain(d => d.Id == "FUDIE004");
        var diagnostic = result.Diagnostics.First(d => d.Id == "FUDIE004");
        diagnostic.GetMessage().Should().Contain("Property 'NonExistentProperty' does not exist on type 'Customer'");
    }

    [Fact]
    public void Generator_WithNonExistentEntityType_ShouldReportError()
    {
        // Arrange - Interfaz referencia entidad que no existe
        var source = CreateTestCode(
            interfaceCode: @"
    public interface ICustomerRepository : IGet<NonExistentEntity, Guid>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().Contain(d => d.Id == "FUDIE003");
        var diagnostic = result.Diagnostics.First(d => d.Id == "FUDIE003");
        diagnostic.GetMessage().Should().Contain("Could not find entity type");
    }

    #endregion

    #region Naming Tests

    [Fact]
    public void Generator_WithIPrefix_ShouldRemoveIPrefixFromClassName()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IGet<Customer, Guid> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepository");
        generatedCode.Should().NotContain("public class ICustomerRepository");
    }

    [Fact]
    public void Generator_WithoutIPrefix_ShouldAppendImpl()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface CustomerRepository : IGet<Customer, Guid> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("public class CustomerRepositoryImpl");
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Generator_WithComplexScenario_ShouldGenerateCorrectCode()
    {
        // Arrange - CON atributos (escenario complejo)
        var source = CreateTestCode(
            interfaceCode: @"
    public class OrderItem : Entity
    {
        public Guid OrderId { get; set; }
        public string ProductName { get; set; }
    }

    public class Order : Entity
    {
        public Guid CustomerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    public class Customer : Entity
    {
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }

    [Include(""Orders"", ""Orders.OrderItems"")]
    [AsSplitQuery]
    [AsNoTracking]
    public interface ICustomerRepository : IGet<Customer, Guid>, IAdd<Customer>
    {
    }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();

        // Verificar interfaces
        generatedCode.Should().Contain("IGet<Customer, System.Guid>");
        generatedCode.Should().Contain("IAdd<Customer>");

        // Verificar includes
        generatedCode.Should().Contain("query = query.Include(c => c.Orders);");
        generatedCode.Should().Contain("query = query.Include(c => c.Orders)");
        generatedCode.Should().Contain(".ThenInclude(o => o.OrderItems);");

        // Verificar modificadores
        generatedCode.Should().Contain("query = query.AsSplitQuery();");
        generatedCode.Should().Contain("query = query.AsNoTracking();");

        // Verificar métodos
        generatedCode.Should().Contain("public async Task<Customer> Get(System.Guid id)");
        generatedCode.Should().Contain("public void Add(Customer entity)");
    }

    #endregion

    #region Constructor and Dependencies Tests

    [Fact]
    public void Generator_WithIGet_ShouldInjectIEntityLookup()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IGet<Customer, Guid> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("private readonly IEntityLookup _entityLookup;");
        generatedCode.Should().Contain("public CustomerRepository(IEntityLookup entityLookup)");
        generatedCode.Should().Contain("_entityLookup = entityLookup;");
    }

    [Fact]
    public void Generator_WithIAdd_ShouldInjectIChangeTracker()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IAdd<Customer> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("private readonly IChangeTracker _changeTracker;");
        generatedCode.Should().Contain("public CustomerRepository(IChangeTracker changeTracker)");
        generatedCode.Should().Contain("_changeTracker = changeTracker;");
    }

    [Fact]
    public void Generator_WithMultipleInterfaces_ShouldInjectBothDependencies()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IGet<Customer, Guid>, IAdd<Customer> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("private readonly IEntityLookup _entityLookup;");
        generatedCode.Should().Contain("private readonly IChangeTracker _changeTracker;");
        generatedCode.Should().Contain("IEntityLookup entityLookup, IChangeTracker changeTracker");
    }

    #endregion

    #region Injectable Attribute Tests

    [Fact]
    public void Generator_ShouldAddInjectableAttribute()
    {
        // Arrange - SIN atributos
        var source = CreateTestCode(
            interfaceCode: @"
    public class Customer : Entity { }

    public interface ICustomerRepository : IGet<Customer, Guid> { }");

        // Act
        var result = RunGenerator(source);

        // Assert
        result.GeneratedTrees.Should().ContainSingle();
        var generatedCode = result.GeneratedTrees[0].ToString();
        generatedCode.Should().Contain("[Injectable(ServiceLifetime.Scoped)]");
    }

    #endregion
}
