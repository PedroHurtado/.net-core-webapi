using FluentAssertions;
using Fudie.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Fudie.UnitTests.DependencyInjection;

public class InjectionExtensionTests
{
    #region AddInjectables Tests - Basic Functionality

    [Fact]
    public void AddInjectables_WithNoAssemblies_ShouldUseCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInjectables();

        // Assert
        services.Should().NotBeNull();
    }

    [Fact]
    public void AddInjectables_WithSingleInjectableClass_ShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(ITestService));
    }

    [Fact]
    public void AddInjectables_WithScopedService_ShouldRegisterAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ITestService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInjectables_WithTransientService_ShouldRegisterAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceTransient).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ITransientService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient);
    }

    [Fact]
    public void AddInjectables_WithSingletonService_ShouldRegisterAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceSingleton).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ISingletonService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);
    }

    #endregion

    #region AddInjectables Tests - Multiple Interfaces

    [Fact]
    public void AddInjectables_WithMultipleInterfaces_ShouldRegisterTopLevelInterfacesOnly()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(MultiInterfaceService).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(IMultiService));
        services.Should().NotContain(sd => sd.ServiceType == typeof(IBaseService));
    }

    [Fact]
    public void AddInjectables_WithClassWithoutInterfaces_ShouldRegisterClass()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ConcreteService).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(ConcreteService));
    }

    #endregion

    #region AddInjectables Tests - Multiple Assemblies

    [Fact]
    public void AddInjectables_WithMultipleAssemblies_ShouldRegisterFromAllAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = typeof(TestServiceScoped).Assembly;
        var assembly2 = typeof(InjectionExtensionTests).Assembly;

        // Act
        services.AddInjectables(assembly1, assembly2);

        // Assert
        services.Should().NotBeEmpty();
    }

    #endregion

    #region AddInjectables Tests - Duplicate Registration Prevention

    [Fact]
    public void AddInjectables_CalledTwice_ShouldNotRegisterDuplicates()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;

        // Act
        services.AddInjectables(assembly);
        var countAfterFirst = services.Count(sd => sd.ServiceType == typeof(ITestService));
        services.AddInjectables(assembly);
        var countAfterSecond = services.Count(sd => sd.ServiceType == typeof(ITestService));

        // Assert
        countAfterFirst.Should().Be(1);
        countAfterSecond.Should().Be(1);
    }

    #endregion

    #region AddInjectables Tests - Abstract and Non-Class Types

    [Fact]
    public void AddInjectables_WithAbstractClass_ShouldNotRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(AbstractService).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().NotContain(sd => sd.ImplementationType == typeof(AbstractService));
    }

    [Fact]
    public void AddInjectables_WithInterface_ShouldNotRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ITestService).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().NotContain(sd => sd.ServiceType == typeof(ITestService) && sd.ImplementationType == typeof(ITestService));
    }

    #endregion

    #region AddInjectables Tests - Return Value

    [Fact]
    public void AddInjectables_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;

        // Act
        var result = services.AddInjectables(assembly);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInjectables_ShouldAllowChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;

        // Act
        var result = services
            .AddInjectables(assembly)
            .AddInjectables(assembly);

        // Assert
        result.Should().BeSameAs(services);
    }

    #endregion

    #region AddInterfacesFor Tests - Basic Functionality

    [Fact]
    public void AddInterfacesFor_WithRegisteredImplementation_ShouldRegisterInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<MultiInterfaceService>();

        // Act
        services.AddInterfacesFor<MultiInterfaceService>();

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(IMultiService));
    }

    [Fact]
    public void AddInterfacesFor_WithUnregisteredImplementation_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddInterfacesFor<MultiInterfaceService>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not registered in the container*");
    }

    [Fact]
    public void AddInterfacesFor_WithDefaultLifetime_ShouldRegisterAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<MultiInterfaceService>();

        // Act
        services.AddInterfacesFor<MultiInterfaceService>();

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IMultiService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInterfacesFor_WithTransientLifetime_ShouldRegisterAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<MultiInterfaceService>();

        // Act
        services.AddInterfacesFor<MultiInterfaceService>(Fudie.DependencyInjection.ServiceLifetime.Transient);

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IMultiService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient);
    }

    [Fact]
    public void AddInterfacesFor_WithSingletonLifetime_ShouldRegisterAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<MultiInterfaceService>();

        // Act
        services.AddInterfacesFor<MultiInterfaceService>(Fudie.DependencyInjection.ServiceLifetime.Singleton);

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IMultiService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);
    }

    #endregion

    #region AddInterfacesFor Tests - Interface Resolution

    [Fact]
    public void AddInterfacesFor_ShouldResolveInterfaceFromImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<MultiInterfaceService>();
        services.AddInterfacesFor<MultiInterfaceService>();

        // Act
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IMultiService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<MultiInterfaceService>();
    }

    [Fact]
    public void AddInterfacesFor_ShouldReturnSameInstanceForAllInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<MultiInterfaceService>();
        services.AddInterfacesFor<MultiInterfaceService>();

        // Act
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service1 = scope.ServiceProvider.GetService<IMultiService>();
        var service2 = scope.ServiceProvider.GetService<MultiInterfaceService>();

        // Assert
        service1.Should().BeSameAs(service2);
    }

    #endregion

    #region AddInterfacesFor Tests - No Interfaces

    [Fact]
    public void AddInterfacesFor_WithClassWithoutInterfaces_ShouldNotRegisterAnything()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ConcreteService>();
        var countBefore = services.Count;

        // Act
        services.AddInterfacesFor<ConcreteService>();
        var countAfter = services.Count;

        // Assert
        countAfter.Should().Be(countBefore);
    }

    #endregion

    #region AddInterfacesFor Tests - Duplicate Registration Prevention

    [Fact]
    public void AddInterfacesFor_CalledTwice_ShouldNotRegisterDuplicates()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<MultiInterfaceService>();

        // Act
        services.AddInterfacesFor<MultiInterfaceService>();
        var countAfterFirst = services.Count(sd => sd.ServiceType == typeof(IMultiService));
        services.AddInterfacesFor<MultiInterfaceService>();
        var countAfterSecond = services.Count(sd => sd.ServiceType == typeof(IMultiService));

        // Assert
        countAfterFirst.Should().Be(1);
        countAfterSecond.Should().Be(1);
    }

    #endregion

    #region AddInterfacesFor Tests - Return Value

    [Fact]
    public void AddInterfacesFor_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<MultiInterfaceService>();

        // Act
        var result = services.AddInterfacesFor<MultiInterfaceService>();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInterfacesFor_ShouldAllowChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<MultiInterfaceService>();

        // Act
        var result = services
            .AddInterfacesFor<MultiInterfaceService>()
            .AddInterfacesFor<MultiInterfaceService>();

        // Assert
        result.Should().BeSameAs(services);
    }

    #endregion

    #region AddInjectables Tests - Multiple Attributes and ServiceType

    [Fact]
    public void AddInjectables_WithMultipleAttributes_ShouldRegisterAllServiceTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(MultiAttributeService).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(MultiAttributeService));
        services.Should().Contain(sd => sd.ServiceType == typeof(IFirstService));
        services.Should().Contain(sd => sd.ServiceType == typeof(ISecondService));
    }

    [Fact]
    public void AddInjectables_WithExplicitServiceType_ShouldRespectServiceType()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ServiceWithExplicitType).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(IExplicitService));
        services.Should().Contain(sd => sd.ServiceType == typeof(ServiceWithExplicitType));
    }

    [Fact]
    public void AddInjectables_WithExplicitServiceType_ShouldNotRegisterOtherInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ServiceWithExplicitType).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        // INotExplicitService is implemented but not specified in ServiceType, so should NOT be registered
        services.Should().NotContain(sd => sd.ServiceType == typeof(INotExplicitService));
    }

    [Fact]
    public void AddInjectables_WithMultipleAttributes_ShouldUseForwarding()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(MultiAttributeService).Assembly;
        services.AddInjectables(assembly);

        // Act
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service1 = scope.ServiceProvider.GetService<IFirstService>();
        var service2 = scope.ServiceProvider.GetService<ISecondService>();
        var service3 = scope.ServiceProvider.GetService<MultiAttributeService>();

        // Assert - all should be the same instance (forwarding)
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service3.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
        service1.Should().BeSameAs(service3);
    }

    [Fact]
    public void AddInjectables_WithoutExplicitServiceType_ShouldUseForwardingForTopLevelInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(MultiInterfaceService).Assembly;
        services.AddInjectables(assembly);

        // Act
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var fromInterface = scope.ServiceProvider.GetService<IMultiService>();
        var fromClass = scope.ServiceProvider.GetService<MultiInterfaceService>();

        // Assert - should be same instance (forwarding)
        fromInterface.Should().NotBeNull();
        fromClass.Should().NotBeNull();
        fromInterface.Should().BeSameAs(fromClass);
    }

    [Fact]
    public void AddInjectables_AlwaysRegistersConcreteClass()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(TestServiceScoped));
    }

    #endregion

    #region AddInjectables Tests - Classes Inheriting from Abstract Generic Classes

    [Fact]
    public void AddInjectables_WithGenericAbstractClass_ShouldNotRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(AbstractCommand<>).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().NotContain(sd => sd.ImplementationType == typeof(AbstractCommand<>));
        services.Should().NotContain(sd => sd.ImplementationType == typeof(AbstractCommand<,>));
    }

    [Fact]
    public void AddInjectables_WithConcreteClassInheritingFromAbstract_ShouldRegisterConcreteClass()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ConcreteCommandWithoutData).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(ConcreteCommandWithoutData));
    }

    [Fact]
    public void AddInjectables_WithConcreteClassInheritingFromGenericAbstract_ShouldRegisterAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ConcreteCommandWithData).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ConcreteCommandWithData));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInjectables_WithMultipleConcreteClassesInheritingFromSameAbstract_ShouldRegisterAll()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ConcreteCommandWithoutData).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(ConcreteCommandWithoutData));
        services.Should().Contain(sd => sd.ServiceType == typeof(AnotherConcreteCommand));
    }

    [Fact]
    public void AddInjectables_WithConcreteClassInheritingFromAbstract_ShouldNotRegisterAbstractBaseType()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ConcreteCommandWithoutData).Assembly;

        // Act
        services.AddInjectables(assembly);

        // Assert
        // Should not register the closed generic abstract type
        var abstractType = typeof(AbstractCommand<TestEntityClass>);
        services.Should().NotContain(sd => sd.ServiceType == abstractType);
    }

    [Fact]
    public void Integration_ConcreteCommandInheritingFromAbstract_ShouldResolve()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ConcreteCommandWithoutData).Assembly;
        services.AddInjectables(assembly);
        var provider = services.BuildServiceProvider();

        // Act
        var command = provider.GetService<ConcreteCommandWithoutData>();

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<ConcreteCommandWithoutData>();
    }

    [Fact]
    public void Integration_ConcreteCommandWithDataInheritingFromAbstract_ShouldResolve()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ConcreteCommandWithData).Assembly;
        services.AddInjectables(assembly);
        var provider = services.BuildServiceProvider();

        // Act
        var command = provider.GetService<ConcreteCommandWithData>();

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<ConcreteCommandWithData>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_AddInjectablesAndResolve_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;

        // Act
        services.AddInjectables(assembly);
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestServiceScoped>();
    }

    [Fact]
    public void Integration_ScopedService_ShouldReturnSameInstanceInScope()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;
        services.AddInjectables(assembly);
        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var service1 = scope.ServiceProvider.GetService<ITestService>();
        var service2 = scope.ServiceProvider.GetService<ITestService>();

        // Assert
        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void Integration_ScopedService_ShouldReturnDifferentInstancesInDifferentScopes()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceScoped).Assembly;
        services.AddInjectables(assembly);
        var provider = services.BuildServiceProvider();

        // Act
        object? service1;
        object? service2;

        using (var scope1 = provider.CreateScope())
        {
            service1 = scope1.ServiceProvider.GetService<ITestService>();
        }

        using (var scope2 = provider.CreateScope())
        {
            service2 = scope2.ServiceProvider.GetService<ITestService>();
        }

        // Assert
        service1.Should().NotBeSameAs(service2);
    }

    [Fact]
    public void Integration_TransientService_ShouldReturnDifferentInstances()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceTransient).Assembly;
        services.AddInjectables(assembly);
        var provider = services.BuildServiceProvider();

        // Act
        var service1 = provider.GetService<ITransientService>();
        var service2 = provider.GetService<ITransientService>();

        // Assert
        service1.Should().NotBeSameAs(service2);
    }

    [Fact]
    public void Integration_SingletonService_ShouldReturnSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestServiceSingleton).Assembly;
        services.AddInjectables(assembly);
        var provider = services.BuildServiceProvider();

        // Act
        var service1 = provider.GetService<ISingletonService>();
        var service2 = provider.GetService<ISingletonService>();

        // Assert
        service1.Should().BeSameAs(service2);
    }

    #endregion

    #region Test Helper Classes and Interfaces

    private interface IBaseService { }
    private interface IMultiService : IBaseService { }
    private interface ITestService { }
    private interface ITransientService { }
    private interface ISingletonService { }
    private interface IFirstService { }
    private interface ISecondService { }
    private interface IExplicitService { }
    private interface INotExplicitService { }

    [Injectable]
    private class TestServiceScoped : ITestService { }

    [Injectable(Fudie.DependencyInjection.ServiceLifetime.Transient)]
    private class TestServiceTransient : ITransientService { }

    [Injectable(Fudie.DependencyInjection.ServiceLifetime.Singleton)]
    private class TestServiceSingleton : ISingletonService { }

    [Injectable]
    private class MultiInterfaceService : IMultiService { }

    [Injectable]
    private class ConcreteService { }

    [Injectable]
    private abstract class AbstractService { }

    [Injectable(ServiceType = typeof(IFirstService))]
    [Injectable(ServiceType = typeof(ISecondService))]
    private class MultiAttributeService : IFirstService, ISecondService { }

    [Injectable(ServiceType = typeof(IExplicitService))]
    private class ServiceWithExplicitType : IExplicitService, INotExplicitService { }

    // Abstract base classes for testing inheritance scenarios
    private abstract class AbstractCommand<TEntity> where TEntity : class
    {
        public abstract TEntity Execute(TEntity entity);
    }

    private abstract class AbstractCommand<TCommand, TEntity>
        where TCommand : class
        where TEntity : class
    {
        public abstract TEntity Execute(TEntity entity, TCommand command);
    }

    // Concrete implementations inheriting from abstract classes
    private record TestCommandData(string Value);
    private class TestEntityClass { public string Name { get; set; } = ""; }

    [Injectable(Fudie.DependencyInjection.ServiceLifetime.Singleton)]
    private class ConcreteCommandWithoutData : AbstractCommand<TestEntityClass>
    {
        public override TestEntityClass Execute(TestEntityClass entity) => entity;
    }

    [Injectable(Fudie.DependencyInjection.ServiceLifetime.Singleton)]
    private class ConcreteCommandWithData : AbstractCommand<TestCommandData, TestEntityClass>
    {
        public override TestEntityClass Execute(TestEntityClass entity, TestCommandData command) => entity;
    }

    [Injectable(Fudie.DependencyInjection.ServiceLifetime.Singleton)]
    private class AnotherConcreteCommand : AbstractCommand<TestEntityClass>
    {
        public override TestEntityClass Execute(TestEntityClass entity) => entity;
    }

    #endregion
}
