using FluentAssertions;
using Fudie.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

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

    #endregion

    #region MapCatalog Tests - Platform Tenant

    [Fact]
    public void MapCatalog_WhenTidMatchesPlatformTenantId_ShouldReturnAllEntries()
    {
        // Arrange
        var platformTenantId = "platform-tenant-123";
        var catalog = new CatalogRegistry();
        catalog.Register("GetMenu", CreateEndpoint("GET"));
        catalog.Register("CreateAllergen", CreateEndpoint("POST", isPlatform: true));
        catalog.Register("SyncCatalog", CreateEndpoint("POST", isInternal: true));

        var configuration = CreateConfiguration("menu-service", platformTenantId);
        var user = CreateUser(platformTenantId);

        // Act
        var response = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        response.ServiceId.Should().Be("menu-service");
        response.Scopes.Should().HaveCount(3);
    }

    #endregion

    #region MapCatalog Tests - Regular Tenant

    [Fact]
    public void MapCatalog_WhenTidDoesNotMatchPlatformTenantId_ShouldReturnTenantEntries()
    {
        // Arrange
        var platformTenantId = "platform-tenant-123";
        var catalog = new CatalogRegistry();
        catalog.Register("GetMenu", CreateEndpoint("GET"));
        catalog.Register("CreateAllergen", CreateEndpoint("POST", isPlatform: true));
        catalog.Register("SyncCatalog", CreateEndpoint("POST", isInternal: true));

        var configuration = CreateConfiguration("menu-service", platformTenantId);
        var user = CreateUser("restaurant-tenant-456");

        // Act
        var response = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        response.ServiceId.Should().Be("menu-service");
        response.Scopes.Should().HaveCount(1);
        response.Scopes[0].ClassName.Should().Be("GetMenu");
    }

    #endregion

    #region MapCatalog Tests - No Tid Claim

    [Fact]
    public void MapCatalog_WhenUserHasNoTidClaim_ShouldReturnTenantEntries()
    {
        // Arrange
        var catalog = new CatalogRegistry();
        catalog.Register("GetMenu", CreateEndpoint("GET"));
        catalog.Register("CreateAllergen", CreateEndpoint("POST", isPlatform: true));

        var configuration = CreateConfiguration("menu-service", "platform-tenant-123");
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var response = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        response.Scopes.Should().HaveCount(1);
        response.Scopes[0].ClassName.Should().Be("GetMenu");
    }

    #endregion

    #region MapCatalog Tests - ServiceId

    [Fact]
    public void MapCatalog_ShouldReturnCorrectServiceId()
    {
        // Arrange
        var catalog = new CatalogRegistry();
        var configuration = CreateConfiguration("auth-service", "platform-123");
        var user = CreateUser("some-tenant");

        // Act
        var response = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        response.ServiceId.Should().Be("auth-service");
    }

    #endregion

    #region Helper Methods

    private static CatalogResponse InvokeCatalogHandler(
        ICatalogRegistry catalog,
        IConfiguration configuration,
        ClaimsPrincipal user)
    {
        var serviceId = configuration["Fudie:ServiceId"];
        var platformTenantId = configuration["Fudie:PlatformTenantId"];
        var tid = user.FindFirst("tid")?.Value;

        var entries = tid == platformTenantId
            ? catalog.GetAll()
            : catalog.GetTenant();

        return new CatalogResponse(serviceId!, entries);
    }

    private static IEndpointRouteBuilder CreateEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddSingleton<ICatalogRegistry, CatalogRegistry>();
        services.AddSingleton<IConfiguration>(CreateConfiguration("test-service", "platform-123"));
        var serviceProvider = services.BuildServiceProvider();

        return new DefaultEndpointRouteBuilder(
            new ApplicationBuilder(serviceProvider));
    }

    private static IConfiguration CreateConfiguration(string serviceId, string platformTenantId)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fudie:ServiceId"] = serviceId,
                ["Fudie:PlatformTenantId"] = platformTenantId
            })
            .Build();
    }

    private static ClaimsPrincipal CreateUser(string tid)
    {
        var claims = new[] { new Claim("tid", tid) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static Endpoint CreateEndpoint(
        string httpMethod,
        bool isPlatform = false,
        bool isInternal = false)
    {
        var metadata = new List<object>
        {
            new HttpMethodMetadata([httpMethod])
        };

        if (isPlatform)
            metadata.Add(new PlatformRequirement());

        if (isInternal)
            metadata.Add(new InternalRequirement());

        return new Endpoint(null, new EndpointMetadataCollection(metadata), "test");
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
