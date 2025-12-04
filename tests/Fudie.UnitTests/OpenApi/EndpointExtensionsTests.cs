using FluentAssertions;
using Fudie.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fudie.UnitTests.OpenApi;

public class EndpointExtensionsTests
{
    #region Test Setup

    private readonly WebApplication _app;

    public EndpointExtensionsTests()
    {
        var builder = WebApplication.CreateSlimBuilder();
        _app = builder.Build();
    }

    #endregion

    #region WithStandardOpenApi<TResponse> Tests

    [Fact]
    public void WithStandardOpenApiGeneric_ShouldReturnRouteHandlerBuilder()
    {
        // Arrange
        var routeBuilder = _app.MapGet("/test", () => "Hello");

        // Act
        var result = routeBuilder.WithStandardOpenApi<string>(
            name: "GetTest",
            summary: "Test summary",
            description: "Test description",
            tag: "TestTag");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RouteHandlerBuilder>();
    }

    [Fact]
    public void WithStandardOpenApiGeneric_WithCustomStatusCode_ShouldReturnRouteHandlerBuilder()
    {
        // Arrange
        var routeBuilder = _app.MapGet("/test-created", () => new TestResponse());

        // Act
        var result = routeBuilder.WithStandardOpenApi<TestResponse>(
            name: "CreateTest",
            summary: "Create test",
            description: "Creates a test resource",
            tag: "TestTag",
            successStatusCode: StatusCodes.Status201Created);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void WithStandardOpenApiGeneric_WithAdditionalErrorCodes_ShouldReturnRouteHandlerBuilder()
    {
        // Arrange
        var routeBuilder = _app.MapGet("/test-errors", () => new TestResponse());

        // Act
        var result = routeBuilder.WithStandardOpenApi<TestResponse>(
            name: "TestWithErrors",
            summary: "Test with errors",
            description: "Test endpoint with additional error codes",
            tag: "TestTag",
            successStatusCode: StatusCodes.Status200OK,
            additionalErrorCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden]);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void WithStandardOpenApiGeneric_WithMultipleAdditionalErrorCodes_ShouldAcceptAll()
    {
        // Arrange
        var routeBuilder = _app.MapGet("/test-many-errors", () => new TestResponse());

        // Act
        var result = routeBuilder.WithStandardOpenApi<TestResponse>(
            name: "TestManyErrors",
            summary: "Test with many errors",
            description: "Test endpoint with many error codes",
            tag: "TestTag",
            successStatusCode: StatusCodes.Status200OK,
            additionalErrorCodes:
            [
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound,
                StatusCodes.Status500InternalServerError
            ]);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void WithStandardOpenApiGeneric_WithEmptyAdditionalErrorCodes_ShouldWork()
    {
        // Arrange
        var routeBuilder = _app.MapGet("/test-no-errors", () => new TestResponse());

        // Act
        var result = routeBuilder.WithStandardOpenApi<TestResponse>(
            name: "TestNoErrors",
            summary: "Test without additional errors",
            description: "Test endpoint without additional error codes",
            tag: "TestTag",
            successStatusCode: StatusCodes.Status200OK);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region WithStandardOpenApi (Non-Generic) Tests

    [Fact]
    public void WithStandardOpenApi_ShouldReturnRouteHandlerBuilder()
    {
        // Arrange
        var routeBuilder = _app.MapDelete("/test-delete/{id}", (int id) => Results.NoContent());

        // Act
        var result = routeBuilder.WithStandardOpenApi(
            name: "DeleteTest",
            summary: "Delete test",
            description: "Deletes a test resource",
            tag: "TestTag",
            successStatusCode: StatusCodes.Status204NoContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RouteHandlerBuilder>();
    }

    [Fact]
    public void WithStandardOpenApi_WithAdditionalErrorCodes_ShouldReturnRouteHandlerBuilder()
    {
        // Arrange
        var routeBuilder = _app.MapPut("/test-update/{id}", (int id) => Results.NoContent());

        // Act
        var result = routeBuilder.WithStandardOpenApi(
            name: "UpdateTest",
            summary: "Update test",
            description: "Updates a test resource",
            tag: "TestTag",
            successStatusCode: StatusCodes.Status204NoContent,
            additionalErrorCodes: [StatusCodes.Status404NotFound, StatusCodes.Status409Conflict]);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void WithStandardOpenApi_ForAcceptedResponse_ShouldWork()
    {
        // Arrange
        var routeBuilder = _app.MapPost("/test-accepted", () => Results.Accepted());

        // Act
        var result = routeBuilder.WithStandardOpenApi(
            name: "AcceptedTest",
            summary: "Accepted test",
            description: "Returns accepted response",
            tag: "TestTag",
            successStatusCode: StatusCodes.Status202Accepted);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region HTTP Methods Coverage Tests

    [Fact]
    public void WithStandardOpenApi_OnGetEndpoint_ShouldWork()
    {
        // Arrange
        var routeBuilder = _app.MapGet("/api/resources", () => new List<TestResponse>());

        // Act
        var result = routeBuilder.WithStandardOpenApi<List<TestResponse>>(
            name: "GetResources",
            summary: "Get all resources",
            description: "Retrieves all resources",
            tag: "Resources");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void WithStandardOpenApi_OnPostEndpoint_ShouldWork()
    {
        // Arrange
        var routeBuilder = _app.MapPost("/api/resources", (TestRequest request) => new TestResponse());

        // Act
        var result = routeBuilder.WithStandardOpenApi<TestResponse>(
            name: "CreateResource",
            summary: "Create resource",
            description: "Creates a new resource",
            tag: "Resources",
            successStatusCode: StatusCodes.Status201Created);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void WithStandardOpenApi_OnPutEndpoint_ShouldWork()
    {
        // Arrange
        var routeBuilder = _app.MapPut("/api/resources/{id}", (int id, TestRequest request) => Results.NoContent());

        // Act
        var result = routeBuilder.WithStandardOpenApi(
            name: "UpdateResource",
            summary: "Update resource",
            description: "Updates an existing resource",
            tag: "Resources",
            successStatusCode: StatusCodes.Status204NoContent);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void WithStandardOpenApi_OnPatchEndpoint_ShouldWork()
    {
        // Arrange
        var routeBuilder = _app.MapPatch("/api/resources/{id}", (int id) => new TestResponse());

        // Act
        var result = routeBuilder.WithStandardOpenApi<TestResponse>(
            name: "PatchResource",
            summary: "Patch resource",
            description: "Partially updates a resource",
            tag: "Resources");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void WithStandardOpenApi_OnDeleteEndpoint_ShouldWork()
    {
        // Arrange
        var routeBuilder = _app.MapDelete("/api/resources/{id}", (int id) => Results.NoContent());

        // Act
        var result = routeBuilder.WithStandardOpenApi(
            name: "DeleteResource",
            summary: "Delete resource",
            description: "Deletes a resource",
            tag: "Resources",
            successStatusCode: StatusCodes.Status204NoContent);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Different Status Codes Tests

    [Theory]
    [InlineData(StatusCodes.Status200OK)]
    [InlineData(StatusCodes.Status201Created)]
    [InlineData(StatusCodes.Status202Accepted)]
    [InlineData(StatusCodes.Status204NoContent)]
    public void WithStandardOpenApi_ShouldAcceptDifferentSuccessStatusCodes(int statusCode)
    {
        // Arrange
        var routeBuilder = _app.MapGet($"/test-status-{statusCode}", () => "result");

        // Act
        var result = routeBuilder.WithStandardOpenApi(
            name: $"TestStatus{statusCode}",
            summary: "Test status code",
            description: $"Test with status {statusCode}",
            tag: "StatusTests",
            successStatusCode: statusCode);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status401Unauthorized)]
    [InlineData(StatusCodes.Status403Forbidden)]
    [InlineData(StatusCodes.Status404NotFound)]
    [InlineData(StatusCodes.Status409Conflict)]
    [InlineData(StatusCodes.Status422UnprocessableEntity)]
    [InlineData(StatusCodes.Status500InternalServerError)]
    public void WithStandardOpenApi_ShouldAcceptDifferentErrorCodes(int errorCode)
    {
        // Arrange
        var routeBuilder = _app.MapGet($"/test-error-{errorCode}", () => new TestResponse());

        // Act
        var result = routeBuilder.WithStandardOpenApi<TestResponse>(
            name: $"TestError{errorCode}",
            summary: "Test error code",
            description: $"Test with error {errorCode}",
            tag: "ErrorTests",
            successStatusCode: StatusCodes.Status200OK,
            additionalErrorCodes: errorCode);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Fluent API Tests

    [Fact]
    public void WithStandardOpenApi_ShouldSupportFluentChaining()
    {
        // Arrange
        var routeBuilder = _app.MapGet("/test-fluent", () => new TestResponse());

        // Act
        var result = routeBuilder
            .WithStandardOpenApi<TestResponse>(
                name: "FluentTest",
                summary: "Fluent test",
                description: "Test fluent API",
                tag: "FluentTests")
            .RequireAuthorization();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Test Models

    private class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
