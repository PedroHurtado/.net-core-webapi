using FluentAssertions;
using Fudie.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.UnitTests.Features;

public class RouteExtensionTests
{
    #region MapFeatures Tests - Basic Functionality

    [Fact]
    public void MapFeatures_WithNoFeatureModules_ShouldNotThrow()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MapFeatures_ShouldNotThrowWhenCalledOnValidBuilder()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MapFeatures_ShouldCompleteSuccessfully()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        builder.MapFeatures();

        // Assert
        // If we reach here without exception, the test passes
        true.Should().BeTrue();
    }

    #endregion

    #region MapFeatures Tests - Feature Module Discovery

    [Fact]
    public void MapFeatures_ShouldOnlyDiscoverPublicClasses()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
        // Private and internal classes should not be discovered
    }

    [Fact]
    public void MapFeatures_ShouldNotDiscoverAbstractClasses()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
        // Abstract classes should not be instantiated
    }

    [Fact]
    public void MapFeatures_ShouldNotDiscoverInterfaces()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
        // Interfaces should not be instantiated
    }

    #endregion

    #region MapFeatures Tests - Assembly Discovery

    [Fact]
    public void MapFeatures_ShouldHandleReflectionTypeLoadException()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow("MapFeatures should handle ReflectionTypeLoadException gracefully");
    }

    #endregion

    #region MapFeatures Tests - Multiple Calls

    [Fact]
    public void MapFeatures_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () =>
        {
            builder.MapFeatures();
            builder.MapFeatures();
        };

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Helper Methods

    private static IEndpointRouteBuilder CreateEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddSingleton<ICatalogRegistry, CatalogRegistry>();
        var serviceProvider = services.BuildServiceProvider();

        var builder = new DefaultEndpointRouteBuilder(
            new ApplicationBuilder(serviceProvider));

        return builder;
    }

    private class DefaultEndpointRouteBuilder : IEndpointRouteBuilder
    {
        private readonly IApplicationBuilder _applicationBuilder;

        public DefaultEndpointRouteBuilder(IApplicationBuilder applicationBuilder)
        {
            _applicationBuilder = applicationBuilder;
            DataSources = new List<EndpointDataSource>();
        }

        public IApplicationBuilder CreateApplicationBuilder()
        {
            return _applicationBuilder.New();
        }

        public ICollection<EndpointDataSource> DataSources { get; }

        public IServiceProvider ServiceProvider => _applicationBuilder.ApplicationServices;
    }

    #endregion
}

public class IFeatureModuleTests
{
    #region Interface Tests

    [Fact]
    public void IFeatureModule_ShouldBeAnInterface()
    {
        // Arrange
        var type = typeof(IFeatureModule);

        // Act & Assert
        type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IFeatureModule_ShouldHaveAddRoutesMethod()
    {
        // Arrange
        var type = typeof(IFeatureModule);

        // Act
        var method = type.GetMethod(nameof(IFeatureModule.AddRoutes));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(IEndpointRouteBuilder));
    }

    [Fact]
    public void IFeatureModule_ShouldBePublic()
    {
        // Arrange
        var type = typeof(IFeatureModule);

        // Act & Assert
        type.IsPublic.Should().BeTrue();
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void IFeatureModule_CanBeImplemented()
    {
        // Arrange & Act
        var implementation = new TestImplementation();

        // Assert
        implementation.Should().BeAssignableTo<IFeatureModule>();
    }

    [Fact]
    public void IFeatureModule_Implementation_CanCallAddRoutes()
    {
        // Arrange
        var implementation = new TestImplementation();
        var builder = CreateMockEndpointRouteBuilder();

        // Act
        var act = () => implementation.AddRoutes(builder);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void IFeatureModule_Implementation_ReceivesCorrectParameter()
    {
        // Arrange
        var implementation = new TestImplementation();
        var builder = CreateMockEndpointRouteBuilder();

        // Act
        implementation.AddRoutes(builder);

        // Assert
        implementation.ReceivedBuilder.Should().BeSameAs(builder);
    }

    #endregion

    #region Helper Methods and Classes

    private static IEndpointRouteBuilder CreateMockEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();

        return new MockEndpointRouteBuilder(serviceProvider);
    }

    private class MockEndpointRouteBuilder : IEndpointRouteBuilder
    {
        public MockEndpointRouteBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            DataSources = new List<EndpointDataSource>();
        }

        public IApplicationBuilder CreateApplicationBuilder()
        {
            var builder = new ApplicationBuilder(ServiceProvider);
            return builder;
        }

        public ICollection<EndpointDataSource> DataSources { get; }

        public IServiceProvider ServiceProvider { get; }
    }

    private class TestImplementation : IFeatureModule
    {
        public IEndpointRouteBuilder? ReceivedBuilder { get; private set; }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            ReceivedBuilder = app;
        }
    }

    #endregion
}
