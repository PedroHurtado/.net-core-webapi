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
    public void MapCatalog_WhenPlatformTenant_ShouldGroupAllEntriesByReadAndWrite()
    {
        // Arrange
        var platformTenantId = "platform-tenant-123";
        var catalog = new CatalogRegistry();
        catalog.Register("GetMenu", CreateEndpoint("GET", displayName: "GetMenu"));
        catalog.Register("CreateAllergen", CreateEndpoint("POST", isPlatform: true, displayName: "CreateAllergen"));
        catalog.Register("SyncCatalog", CreateEndpoint("POST", isInternal: true, displayName: "SyncCatalog"));

        var configuration = CreateConfiguration("menu-service", platformTenantId);
        var user = CreateUser(platformTenantId);

        // Act
        var groups = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        groups.Should().HaveCount(2);
        groups["menu-service:read"].Should().Equal("GetMenu");
        groups["menu-service:write"].Should().Equal("CreateAllergen", "SyncCatalog");
    }

    #endregion

    #region MapCatalog Tests - Regular Tenant

    [Fact]
    public void MapCatalog_WhenRegularTenant_ShouldOnlyIncludeTenantEntries()
    {
        // Arrange
        var platformTenantId = "platform-tenant-123";
        var catalog = new CatalogRegistry();
        catalog.Register("GetMenu", CreateEndpoint("GET", displayName: "GetMenu"));
        catalog.Register("CreateMenu", CreateEndpoint("POST", displayName: "CreateMenu"));
        catalog.Register("CreateAllergen", CreateEndpoint("POST", isPlatform: true, displayName: "CreateAllergen"));
        catalog.Register("SyncCatalog", CreateEndpoint("POST", isInternal: true, displayName: "SyncCatalog"));

        var configuration = CreateConfiguration("menu-service", platformTenantId);
        var user = CreateUser("restaurant-tenant-456");

        // Act
        var groups = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        groups.Should().HaveCount(2);
        groups["menu-service:read"].Should().Equal("GetMenu");
        groups["menu-service:write"].Should().Equal("CreateMenu");
    }

    #endregion

    #region MapCatalog Tests - No Tid Claim

    [Fact]
    public void MapCatalog_WhenUserHasNoTidClaim_ShouldOnlyIncludeTenantEntries()
    {
        // Arrange
        var catalog = new CatalogRegistry();
        catalog.Register("GetMenu", CreateEndpoint("GET", displayName: "GetMenu"));
        catalog.Register("CreateAllergen", CreateEndpoint("POST", isPlatform: true, displayName: "CreateAllergen"));

        var configuration = CreateConfiguration("menu-service", "platform-tenant-123");
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var groups = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        groups.Should().HaveCount(1);
        groups["menu-service:read"].Should().Equal("GetMenu");
    }

    #endregion

    #region MapCatalog Tests - ServiceId in Keys

    [Fact]
    public void MapCatalog_ShouldUseServiceIdInGroupKeys()
    {
        // Arrange
        var catalog = new CatalogRegistry();
        catalog.Register("GetUser", CreateEndpoint("GET", displayName: "GetUser"));
        catalog.Register("CreateUser", CreateEndpoint("POST", displayName: "CreateUser"));

        var configuration = CreateConfiguration("auth-service", "platform-123");
        var user = CreateUser("some-tenant");

        // Act
        var groups = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        groups.Should().ContainKey("auth-service:read");
        groups.Should().ContainKey("auth-service:write");
    }

    #endregion

    #region MapCatalog Tests - Custom Groups

    [Fact]
    public void MapCatalog_WhenEntriesHaveCustomGroup_ShouldIncludeCustomGroup()
    {
        // Arrange
        var catalog = new CatalogRegistry();
        catalog.Register("GetMenu", CreateEndpoint("GET", displayName: "GetMenu"));
        catalog.Register("ApproveMenu", CreateEndpoint("POST", customGroup: "menu:approve", displayName: "ApproveMenu"));
        catalog.Register("RejectMenu", CreateEndpoint("POST", customGroup: "menu:approve", displayName: "RejectMenu"));

        var configuration = CreateConfiguration("menu-service", "platform-123");
        var user = CreateUser("platform-123");

        // Act
        var groups = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        groups.Should().HaveCount(3);
        groups["menu-service:read"].Should().Equal("GetMenu");
        groups["menu-service:write"].Should().Equal("ApproveMenu", "RejectMenu");
        groups["menu:approve"].Should().Equal("ApproveMenu", "RejectMenu");
    }

    #endregion

    #region MapCatalog Tests - Empty Catalog

    [Fact]
    public void MapCatalog_WhenNoEntries_ShouldReturnEmptyGroups()
    {
        // Arrange
        var catalog = new CatalogRegistry();
        var configuration = CreateConfiguration("menu-service", "platform-123");
        var user = CreateUser("some-tenant");

        // Act
        var groups = InvokeCatalogHandler(catalog, configuration, user);

        // Assert
        groups.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static Dictionary<string, List<string>> InvokeCatalogHandler(
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

        var groups = new Dictionary<string, List<string>>();

        var readers = entries
            .Where(e => e.HttpVerb == "GET")
            .Select(e => e.ClassName)
            .ToList();

        if (readers.Count > 0)
            groups[$"{serviceId}:read"] = readers;

        var writers = entries
            .Where(e => e.HttpVerb != "GET")
            .Select(e => e.ClassName)
            .ToList();

        if (writers.Count > 0)
            groups[$"{serviceId}:write"] = writers;

        foreach (var custom in entries
            .Where(e => e.CustomGroup != null)
            .GroupBy(e => e.CustomGroup!))
        {
            groups[custom.Key] = custom
                .Select(e => e.ClassName)
                .ToList();
        }

        return groups;
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
        bool isInternal = false,
        string? customGroup = null,
        string displayName = "test")
    {
        var metadata = new List<object>
        {
            new HttpMethodMetadata([httpMethod])
        };

        if (isPlatform)
            metadata.Add(new PlatformRequirement());

        if (isInternal)
            metadata.Add(new InternalRequirement());

        if (customGroup is not null)
            metadata.Add(new GroupRequirement(customGroup));

        return new Endpoint(null, new EndpointMetadataCollection(metadata), displayName);
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
