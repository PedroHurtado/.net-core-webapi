using FluentAssertions;
using Fudie.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.UnitTests.Features;

public class CatalogEndpointExtensionsTests
{
    #region MapCatalog Tests - Endpoint Registration

    [Fact]
    public void MapCatalog_ShouldRegisterGetCatalogEndpoint()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        builder.MapCatalog();

        // Assert
        var endpoints = builder.DataSources
            .SelectMany(ds => ds.Endpoints)
            .ToList();
        endpoints.Should().HaveCount(1);
    }

    [Fact]
    public void MapCatalog_ShouldMarkEndpointAsExcludedFromDescription()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        builder.MapCatalog();

        // Assert
        var endpoint = builder.DataSources
            .SelectMany(ds => ds.Endpoints).First();
        endpoint.Metadata
            .GetMetadata<ExcludeFromDescriptionAttribute>()
            .Should().NotBeNull();
    }

    [Fact]
    public void MapCatalog_ShouldMarkEndpointAsInternal()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        builder.MapCatalog();

        // Assert
        var endpoint = builder.DataSources
            .SelectMany(ds => ds.Endpoints).First();
        endpoint.Metadata
            .GetMetadata<InternalRequirement>()
            .Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static IEndpointRouteBuilder CreateEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddSingleton<ICatalogRegistry, CatalogRegistry>();
        services.AddSingleton<IConfiguration>(CreateConfiguration("test-service", "Test Service"));
        var serviceProvider = services.BuildServiceProvider();

        return new DefaultEndpointRouteBuilder(
            new ApplicationBuilder(serviceProvider));
    }

    private static IConfiguration CreateConfiguration(string serviceId, string serviceName)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fudie:ServiceId"] = serviceId,
                ["Fudie:ServiceName"] = serviceName
            })
            .Build();
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
            => _applicationBuilder.New();

        public ICollection<EndpointDataSource> DataSources { get; }
        public IServiceProvider ServiceProvider => _applicationBuilder.ApplicationServices;
    }

    #endregion
}
