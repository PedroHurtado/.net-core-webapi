using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Fudie.Http;
using Microsoft.AspNetCore.Http;

namespace Fudie.UnitTests.Http;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _handler = new GlobalExceptionHandler();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.TraceIdentifier = "test-trace-id";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonDocument.Parse(body);
    }

    #region KeyNotFoundException Tests

    [Fact]
    public async Task TryHandleAsync_WithKeyNotFoundException_ShouldReturn404()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new KeyNotFoundException("Resource not found");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task TryHandleAsync_WithKeyNotFoundException_ShouldSetJsonContentType()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new KeyNotFoundException("Resource not found");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        // WriteAsJsonAsync sets content type to application/json
        context.Response.ContentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_WithKeyNotFoundException_ShouldReturnCorrectProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var exceptionMessage = "User with id 123 not found";
        var exception = new KeyNotFoundException(exceptionMessage);

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(404);
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("detail").GetString().Should().Be(exceptionMessage);
        root.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        root.GetProperty("instance").GetString().Should().Be("/api/test");
    }

    #endregion

    #region ValidationException Tests

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturn422()
    {
        // Arrange
        var context = CreateHttpContext();
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required")
        };
        var exception = new ValidationException(failures);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturnCorrectProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is invalid")
        };
        var exception = new ValidationException(failures);

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(422);
        root.GetProperty("title").GetString().Should().Be("Validation Error");
        root.GetProperty("detail").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("type").GetString().Should().Be("https://datatracker.ietf.org/doc/html/rfc4918#section-11.2");
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldIncludeValidationErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Name", "Name must be at least 3 characters"),
            new("Email", "Email is invalid")
        };
        var exception = new ValidationException(failures);

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;
        var extensions = root.GetProperty("extensions");
        var errors = extensions.GetProperty("errors");

        errors.GetProperty("Name").GetArrayLength().Should().Be(2);
        errors.GetProperty("Email").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldGroupErrorsByPropertyName()
    {
        // Arrange
        var context = CreateHttpContext();
        var failures = new List<ValidationFailure>
        {
            new("Password", "Password is required"),
            new("Password", "Password must be at least 8 characters"),
            new("Password", "Password must contain a number")
        };
        var exception = new ValidationException(failures);

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;
        var extensions = root.GetProperty("extensions");
        var errors = extensions.GetProperty("errors");
        var passwordErrors = errors.GetProperty("Password");

        passwordErrors.GetArrayLength().Should().Be(3);
    }

    #endregion

    #region ArgumentException Tests

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_ShouldReturn400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ArgumentException("Invalid argument provided");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_ShouldReturnCorrectProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var exceptionMessage = "Parameter 'id' cannot be negative";
        var exception = new ArgumentException(exceptionMessage);

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("detail").GetString().Should().Be(exceptionMessage);
        root.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentNullException_ShouldReturn400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ArgumentNullException("paramName");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    #endregion

    #region Default Exception Tests

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_ShouldReturn500()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_ShouldReturnCorrectProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(500);
        root.GetProperty("title").GetString().Should().Be("Internal Server Error");
        root.GetProperty("detail").GetString().Should().Be("An unexpected error occurred");
        root.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
    }

    [Theory]
    [InlineData(typeof(NullReferenceException))]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(NotImplementedException))]
    [InlineData(typeof(TimeoutException))]
    public async Task TryHandleAsync_WithVariousUnhandledExceptions_ShouldReturn500(Type exceptionType)
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region Common Response Properties Tests

    [Fact]
    public async Task TryHandleAsync_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Any exception");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeTraceId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.TraceIdentifier = "custom-trace-id-12345";
        var exception = new Exception("Any exception");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;
        var extensions = root.GetProperty("extensions");

        extensions.GetProperty("traceId").GetString().Should().Be("custom-trace-id-12345");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeTimestamp()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Any exception");
        var beforeCall = DateTime.UtcNow;

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var afterCall = DateTime.UtcNow;
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;
        var extensions = root.GetProperty("extensions");

        var timestamp = extensions.GetProperty("timestamp").GetDateTime();
        timestamp.Should().BeOnOrAfter(beforeCall);
        timestamp.Should().BeOnOrBefore(afterCall);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeRequestPath()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/users/123";
        var exception = new Exception("Any exception");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var jsonDoc = await GetResponseBody(context);
        var root = jsonDoc.RootElement;

        root.GetProperty("instance").GetString().Should().Be("/api/users/123");
    }

    #endregion
}
