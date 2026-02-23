using FluentAssertions;
using Fudie.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Fudie.UnitTests.Features;

public class CatalogRegistryTests
{
    #region Register Tests - Basic

    [Fact]
    public void Register_NormalEndpoint_ShouldAddEntry()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "GET", routePattern: "/menus");

        // Act
        registry.Register("GetMenu", endpoint);

        // Assert
        registry.GetAll().Should().HaveCount(1);
        var entry = registry.GetAll()[0];
        entry.ClassName.Should().Be("GetMenu");
        entry.HttpVerb.Should().Be("GET");
        entry.RoutePattern.Should().Be("/menus");
        entry.IsAnonymous.Should().BeFalse();
        entry.IsAuthenticated.Should().BeFalse();
        entry.IsPlatform.Should().BeFalse();
        entry.IsInternal.Should().BeFalse();
        entry.IsExcluded.Should().BeFalse();
        entry.CustomGroup.Should().BeNull();
        entry.Description.Should().BeNull();
    }

    [Fact]
    public void Register_PostEndpoint_ShouldCaptureHttpVerb()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "POST");

        // Act
        registry.Register("CreateMenu", endpoint);

        // Assert
        registry.GetAll()[0].HttpVerb.Should().Be("POST");
    }

    [Fact]
    public void Register_WithoutHttpMethodMetadata_ShouldDefaultToGET()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint();

        // Act
        registry.Register("GetMenu", endpoint);

        // Assert
        registry.GetAll()[0].HttpVerb.Should().Be("GET");
    }

    #endregion

    #region Register Tests - Filtering

    [Fact]
    public void Register_WithExcludeFromDescription_ShouldNotAppearInGetAll()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(excludeFromDescription: true);

        // Act
        registry.Register("SwaggerRedirect", endpoint);

        // Assert
        registry.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Register_WithAllowAnonymous_ShouldSetIsAnonymous()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(allowAnonymous: true);

        // Act
        registry.Register("Login", endpoint);

        // Assert
        var entry = registry.GetAll()[0];
        entry.IsAnonymous.Should().BeTrue();
        entry.IsExcluded.Should().BeFalse();
    }

    [Fact]
    public void Register_WithExcludeFromDescription_ShouldSetIsExcluded()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(excludeFromDescription: true);

        // Act
        registry.Register("SwaggerRedirect", endpoint);

        // Assert
        registry.EndpointMapCount.Should().Be(1);
        registry.GetAll().Should().BeEmpty();
    }

    #endregion

    #region Register Tests - Metadata

    [Fact]
    public void Register_WithPlatformRequirement_ShouldSetIsPlatform()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "POST", isPlatform: true);

        // Act
        registry.Register("CreateAllergen", endpoint);

        // Assert
        var entry = registry.GetAll()[0];
        entry.IsPlatform.Should().BeTrue();
        entry.IsInternal.Should().BeFalse();
    }

    [Fact]
    public void Register_WithInternalRequirement_ShouldSetIsInternal()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "POST", isInternal: true);

        // Act
        registry.Register("SyncCatalog", endpoint);

        // Assert
        var entry = registry.GetAll()[0];
        entry.IsInternal.Should().BeTrue();
        entry.IsPlatform.Should().BeFalse();
    }

    [Fact]
    public void Register_WithAuthenticatedRequirement_ShouldSetIsAuthenticated()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "POST", isAuthenticated: true);

        // Act
        registry.Register("Checkout", endpoint);

        // Assert
        var entry = registry.GetAll()[0];
        entry.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void Register_WithGroupRequirement_ShouldSetCustomGroup()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "PUT", customGroup: "menu:deposit");

        // Act
        registry.Register("UpdateDepositPolicy", endpoint);

        // Assert
        var entry = registry.GetAll()[0];
        entry.CustomGroup.Should().Be("menu:deposit");
    }

    [Fact]
    public void Register_WithCatalogDescription_ShouldSetDescription()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "POST", description: "Create a new menu");

        // Act
        registry.Register("CreateMenu", endpoint);

        // Assert
        var entry = registry.GetAll()[0];
        entry.Description.Should().Be("Create a new menu");
    }

    [Fact]
    public void Register_WithRoutePattern_ShouldCaptureRoutePattern()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "GET", routePattern: "/menus/{id}");

        // Act
        registry.Register("GetMenuById", endpoint);

        // Assert
        var entry = registry.GetAll()[0];
        entry.RoutePattern.Should().Be("/menus/{id}");
    }

    #endregion

    #region Register Tests - Multiple Entries

    [Fact]
    public void Register_MultipleEndpoints_ShouldAddAll()
    {
        // Arrange
        var registry = new CatalogRegistry();

        // Act
        registry.Register("GetMenu", CreateEndpoint(httpMethod: "GET", displayName: "GetMenu"));
        registry.Register("CreateMenu", CreateEndpoint(httpMethod: "POST", displayName: "CreateMenu"));
        registry.Register("UpdateMenu", CreateEndpoint(httpMethod: "PUT", displayName: "UpdateMenu"));

        // Assert
        registry.GetAll().Should().HaveCount(3);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_Empty_ShouldReturnEmptyList()
    {
        // Arrange
        var registry = new CatalogRegistry();

        // Act & Assert
        registry.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void GetAll_ShouldReturnAllEntries()
    {
        // Arrange
        var registry = new CatalogRegistry();
        registry.Register("GetMenu", CreateEndpoint(httpMethod: "GET", displayName: "GetMenu"));
        registry.Register("CreateAllergen", CreateEndpoint(httpMethod: "POST", isPlatform: true, displayName: "CreateAllergen"));
        registry.Register("SyncCatalog", CreateEndpoint(httpMethod: "POST", isInternal: true, displayName: "SyncCatalog"));

        // Act
        var all = registry.GetAll();

        // Assert
        all.Should().HaveCount(3);
    }

    [Fact]
    public void GetAll_ShouldIncludeAnonymousEntries()
    {
        // Arrange
        var registry = new CatalogRegistry();
        registry.Register("Login", CreateEndpoint(httpMethod: "POST", allowAnonymous: true, displayName: "Login"));
        registry.Register("GetMenu", CreateEndpoint(httpMethod: "GET", displayName: "GetMenu"));

        // Act
        var all = registry.GetAll();

        // Assert
        all.Should().HaveCount(2);
        all.Select(e => e.ClassName).Should().Contain("Login");
    }

    #endregion

    #region GetTenant Tests

    [Fact]
    public void GetTenant_ShouldExcludePlatformEntries()
    {
        // Arrange
        var registry = new CatalogRegistry();
        registry.Register("GetMenu", CreateEndpoint(httpMethod: "GET", displayName: "GetMenu"));
        registry.Register("CreateAllergen", CreateEndpoint(httpMethod: "POST", isPlatform: true, displayName: "CreateAllergen"));

        // Act
        var tenant = registry.GetTenant();

        // Assert
        tenant.Should().HaveCount(1);
        tenant[0].ClassName.Should().Be("GetMenu");
    }

    [Fact]
    public void GetTenant_ShouldExcludeInternalEntries()
    {
        // Arrange
        var registry = new CatalogRegistry();
        registry.Register("GetMenu", CreateEndpoint(httpMethod: "GET", displayName: "GetMenu"));
        registry.Register("SyncCatalog", CreateEndpoint(httpMethod: "POST", isInternal: true, displayName: "SyncCatalog"));

        // Act
        var tenant = registry.GetTenant();

        // Assert
        tenant.Should().HaveCount(1);
        tenant[0].ClassName.Should().Be("GetMenu");
    }

    [Fact]
    public void GetTenant_ShouldExcludeBothPlatformAndInternal()
    {
        // Arrange
        var registry = new CatalogRegistry();
        registry.Register("GetMenu", CreateEndpoint(httpMethod: "GET", displayName: "GetMenu"));
        registry.Register("CreateAllergen", CreateEndpoint(httpMethod: "POST", isPlatform: true, displayName: "CreateAllergen"));
        registry.Register("SyncCatalog", CreateEndpoint(httpMethod: "POST", isInternal: true, displayName: "SyncCatalog"));
        registry.Register("UpdateMenu", CreateEndpoint(httpMethod: "PUT", customGroup: "menu:deposit", displayName: "UpdateMenu"));

        // Act
        var tenant = registry.GetTenant();

        // Assert
        tenant.Should().HaveCount(2);
        tenant.Select(e => e.ClassName).Should().Contain("GetMenu");
        tenant.Select(e => e.ClassName).Should().Contain("UpdateMenu");
    }

    [Fact]
    public void GetTenant_Empty_ShouldReturnEmptyList()
    {
        // Arrange
        var registry = new CatalogRegistry();

        // Act & Assert
        registry.GetTenant().Should().BeEmpty();
    }

    #endregion

    #region FindClassName Tests

    [Fact]
    public void FindClassName_RegisteredEndpoint_ShouldReturnClassName()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "GET");
        registry.Register("GetMenu", endpoint);

        // Act
        var result = registry.FindClassName(endpoint);

        // Assert
        result.Should().Be("GetMenu");
    }

    [Fact]
    public void FindClassName_UnregisteredEndpoint_ShouldReturnNull()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "GET");

        // Act
        var result = registry.FindClassName(endpoint);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindClassName_ExcludedEndpoint_ShouldStillReturnClassName()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(excludeFromDescription: true);
        registry.Register("SwaggerRedirect", endpoint);

        // Act
        var result = registry.FindClassName(endpoint);

        // Assert
        result.Should().Be("SwaggerRedirect");
    }

    [Fact]
    public void FindClassName_AllowAnonymousEndpoint_ShouldStillReturnClassName()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(allowAnonymous: true);
        registry.Register("Login", endpoint);

        // Act
        var result = registry.FindClassName(endpoint);

        // Assert
        result.Should().Be("Login");
    }

    #endregion

    #region FindEndpoint Tests

    [Fact]
    public void FindEndpoint_RegisteredEndpoint_ShouldReturnEndpoint()
    {
        // Arrange
        var registry = new CatalogRegistry();
        var endpoint = CreateEndpoint(httpMethod: "GET");
        registry.Register("GetMenu", endpoint);

        // Act
        var result = registry.FindEndpoint("test");

        // Assert
        result.Should().BeSameAs(endpoint);
    }

    [Fact]
    public void FindEndpoint_UnregisteredDisplayName_ShouldReturnNull()
    {
        // Arrange
        var registry = new CatalogRegistry();

        // Act
        var result = registry.FindEndpoint("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private static Endpoint CreateEndpoint(
        string? httpMethod = null,
        bool excludeFromDescription = false,
        bool allowAnonymous = false,
        bool isPlatform = false,
        bool isInternal = false,
        bool isAuthenticated = false,
        string? customGroup = null,
        string? description = null,
        string? routePattern = null,
        string displayName = "test")
    {
        var metadata = new List<object>();

        if (httpMethod is not null)
            metadata.Add(new HttpMethodMetadata([httpMethod]));

        if (excludeFromDescription)
            metadata.Add(new ExcludeFromDescriptionAttribute());

        if (allowAnonymous)
            metadata.Add(new AllowAnonymousAttribute());

        if (isPlatform)
            metadata.Add(new PlatformRequirement());

        if (isInternal)
            metadata.Add(new InternalRequirement());

        if (isAuthenticated)
            metadata.Add(new AuthenticatedRequirement());

        if (customGroup is not null)
            metadata.Add(new GroupRequirement(customGroup));

        if (description is not null)
            metadata.Add(new CatalogDescription(description));

        if (routePattern is not null)
        {
            return new RouteEndpoint(
                requestDelegate: _ => Task.CompletedTask,
                routePattern: RoutePatternFactory.Parse(routePattern),
                order: 0,
                metadata: new EndpointMetadataCollection(metadata),
                displayName: displayName);
        }

        return new Endpoint(
            requestDelegate: null,
            metadata: new EndpointMetadataCollection(metadata),
            displayName: displayName);
    }

    #endregion
}
