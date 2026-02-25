using FluentAssertions;
using Fudie.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.UnitTests.Features;

public class EndpointAuthExtensionsTests
{
    #region RequireAuthenticated Tests

    [Fact]
    public void RequireAuthenticated_ShouldAddAuthenticatedRequirementMetadata()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        endpoint.RequireAuthenticated();

        // Assert
        var dataSource = builder.DataSources.First();
        var endpointMetadata = dataSource.Endpoints.First().Metadata;
        endpointMetadata.GetMetadata<AuthenticatedRequirement>().Should().NotBeNull();
    }

    [Fact]
    public void RequireAuthenticated_ShouldReturnBuilder()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        var result = endpoint.RequireAuthenticated();

        // Assert
        result.Should().BeSameAs(endpoint);
    }

    #endregion

    #region RequirePlatform Tests

    [Fact]
    public void RequirePlatform_ShouldAddPlatformRequirementMetadata()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        endpoint.RequirePlatform();

        // Assert
        var dataSource = builder.DataSources.First();
        var endpointMetadata = dataSource.Endpoints.First().Metadata;
        endpointMetadata.GetMetadata<PlatformRequirement>().Should().NotBeNull();
    }

    [Fact]
    public void RequirePlatform_ShouldReturnBuilder()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        var result = endpoint.RequirePlatform();

        // Assert
        result.Should().BeSameAs(endpoint);
    }

    #endregion

    #region RequireInternal Tests

    [Fact]
    public void RequireInternal_ShouldAddInternalRequirementMetadata()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        endpoint.RequireInternal();

        // Assert
        var dataSource = builder.DataSources.First();
        var endpointMetadata = dataSource.Endpoints.First().Metadata;
        endpointMetadata.GetMetadata<InternalRequirement>().Should().NotBeNull();
    }

    [Fact]
    public void RequireInternal_ShouldReturnBuilder()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        var result = endpoint.RequireInternal();

        // Assert
        result.Should().BeSameAs(endpoint);
    }

    #endregion

    #region RequireGroup Tests

    [Fact]
    public void RequireGroup_ShouldAddGroupRequirementMetadata()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        endpoint.RequireGroup("menu:deposit", "Menu deposits");

        // Assert
        var dataSource = builder.DataSources.First();
        var endpointMetadata = dataSource.Endpoints.First().Metadata;
        var groupRequirement = endpointMetadata.GetMetadata<GroupRequirement>();
        groupRequirement.Should().NotBeNull();
        groupRequirement!.Group.Should().Be("menu:deposit");
        groupRequirement.Description.Should().Be("Menu deposits");
    }

    [Fact]
    public void RequireGroup_ShouldReturnBuilder()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        var result = endpoint.RequireGroup("menu:deposit", "Menu deposits");

        // Assert
        result.Should().BeSameAs(endpoint);
    }

    #endregion

    #region WithDescriptionCatalog Tests

    [Fact]
    public void WithDescriptionCatalog_ShouldAddCatalogDescriptionMetadata()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        endpoint.WithDescriptionCatalog("Create menu");

        // Assert
        var dataSource = builder.DataSources.First();
        var endpointMetadata = dataSource.Endpoints.First().Metadata;
        var catalogDescription = endpointMetadata.GetMetadata<CatalogDescription>();
        catalogDescription.Should().NotBeNull();
        catalogDescription!.Description.Should().Be("Create menu");
    }

    [Fact]
    public void WithDescriptionCatalog_ShouldReturnBuilder()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        var result = endpoint.WithDescriptionCatalog("Create menu");

        // Assert
        result.Should().BeSameAs(endpoint);
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public void RequirePlatform_AndRequireGroup_ShouldAddBothMetadata()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        endpoint.RequirePlatform().RequireGroup("admin:manage", "Manage administration");

        // Assert
        var dataSource = builder.DataSources.First();
        var endpointMetadata = dataSource.Endpoints.First().Metadata;
        endpointMetadata.GetMetadata<PlatformRequirement>().Should().NotBeNull();
        endpointMetadata.GetMetadata<GroupRequirement>().Should().NotBeNull();
        endpointMetadata.GetMetadata<GroupRequirement>()!.Group.Should().Be("admin:manage");
    }

    [Fact]
    public void WithDescriptionCatalog_AndRequireGroup_ShouldAddBothMetadata()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();
        var endpoint = builder.MapGet("/test", () => Results.Ok());

        // Act
        endpoint.RequireGroup("menus:write", "Manage menus").WithDescriptionCatalog("Create menu");

        // Assert
        var dataSource = builder.DataSources.First();
        var endpointMetadata = dataSource.Endpoints.First().Metadata;
        endpointMetadata.GetMetadata<GroupRequirement>().Should().NotBeNull();
        endpointMetadata.GetMetadata<CatalogDescription>()!.Description.Should().Be("Create menu");
    }

    #endregion

    #region Helper Methods

    private static IEndpointRouteBuilder CreateEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
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
