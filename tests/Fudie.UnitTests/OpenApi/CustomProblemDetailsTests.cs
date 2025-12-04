using FluentAssertions;
using Fudie.OpenApi;

namespace Fudie.UnitTests.OpenApi;

public class CustomProblemDetailsTests
{
    #region Default Values Tests

    [Fact]
    public void Constructor_ShouldSetDefaultTypeToAboutBlank()
    {
        // Arrange & Act
        var problemDetails = new CustomProblemDetails();

        // Assert
        problemDetails.Type.Should().Be("about:blank");
    }

    [Fact]
    public void Constructor_ShouldSetDefaultTitleToEmptyString()
    {
        // Arrange & Act
        var problemDetails = new CustomProblemDetails();

        // Assert
        problemDetails.Title.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetDefaultStatusToZero()
    {
        // Arrange & Act
        var problemDetails = new CustomProblemDetails();

        // Assert
        problemDetails.Status.Should().Be(0);
    }

    [Fact]
    public void Constructor_ShouldSetDefaultDetailToEmptyString()
    {
        // Arrange & Act
        var problemDetails = new CustomProblemDetails();

        // Assert
        problemDetails.Detail.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetDefaultInstanceToEmptyString()
    {
        // Arrange & Act
        var problemDetails = new CustomProblemDetails();

        // Assert
        problemDetails.Instance.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetDefaultExtensionsToNull()
    {
        // Arrange & Act
        var problemDetails = new CustomProblemDetails();

        // Assert
        problemDetails.Extensions.Should().BeNull();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Type_ShouldBeSettable()
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();
        var expectedType = "https://example.com/probs/out-of-credit";

        // Act
        problemDetails.Type = expectedType;

        // Assert
        problemDetails.Type.Should().Be(expectedType);
    }

    [Fact]
    public void Title_ShouldBeSettable()
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();
        var expectedTitle = "You do not have enough credit.";

        // Act
        problemDetails.Title = expectedTitle;

        // Assert
        problemDetails.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void Status_ShouldBeSettable()
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();
        var expectedStatus = 400;

        // Act
        problemDetails.Status = expectedStatus;

        // Assert
        problemDetails.Status.Should().Be(expectedStatus);
    }

    [Fact]
    public void Detail_ShouldBeSettable()
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();
        var expectedDetail = "Your current balance is 30, but that costs 50.";

        // Act
        problemDetails.Detail = expectedDetail;

        // Assert
        problemDetails.Detail.Should().Be(expectedDetail);
    }

    [Fact]
    public void Instance_ShouldBeSettable()
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();
        var expectedInstance = "/account/12345/transactions/67890";

        // Act
        problemDetails.Instance = expectedInstance;

        // Assert
        problemDetails.Instance.Should().Be(expectedInstance);
    }

    [Fact]
    public void Extensions_ShouldBeSettable()
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();
        var expectedExtensions = new Dictionary<string, object>
        {
            { "balance", 30 },
            { "currency", "USD" }
        };

        // Act
        problemDetails.Extensions = expectedExtensions;

        // Assert
        problemDetails.Extensions.Should().BeEquivalentTo(expectedExtensions);
    }

    #endregion

    #region Full Object Initialization Tests

    [Fact]
    public void ObjectInitializer_ShouldSetAllProperties()
    {
        // Arrange
        var expectedType = "https://example.com/probs/validation-error";
        var expectedTitle = "Validation Error";
        var expectedStatus = 422;
        var expectedDetail = "One or more validation errors occurred.";
        var expectedInstance = "/api/users/create";
        var expectedExtensions = new Dictionary<string, object>
        {
            { "errors", new[] { "Name is required", "Email is invalid" } }
        };

        // Act
        var problemDetails = new CustomProblemDetails
        {
            Type = expectedType,
            Title = expectedTitle,
            Status = expectedStatus,
            Detail = expectedDetail,
            Instance = expectedInstance,
            Extensions = expectedExtensions
        };

        // Assert
        problemDetails.Type.Should().Be(expectedType);
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Status.Should().Be(expectedStatus);
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Instance.Should().Be(expectedInstance);
        problemDetails.Extensions.Should().BeEquivalentTo(expectedExtensions);
    }

    #endregion

    #region HTTP Status Code Tests

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(422)]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    public void Status_ShouldAcceptCommonHttpStatusCodes(int statusCode)
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();

        // Act
        problemDetails.Status = statusCode;

        // Assert
        problemDetails.Status.Should().Be(statusCode);
    }

    #endregion

    #region Extensions Dictionary Tests

    [Fact]
    public void Extensions_ShouldSupportDifferentValueTypes()
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();
        var extensions = new Dictionary<string, object>
        {
            { "stringValue", "test" },
            { "intValue", 42 },
            { "boolValue", true },
            { "doubleValue", 3.14 },
            { "arrayValue", new[] { 1, 2, 3 } },
            { "objectValue", new { Name = "Test", Value = 123 } }
        };

        // Act
        problemDetails.Extensions = extensions;

        // Assert
        problemDetails.Extensions.Should().HaveCount(6);
        problemDetails.Extensions!["stringValue"].Should().Be("test");
        problemDetails.Extensions["intValue"].Should().Be(42);
        problemDetails.Extensions["boolValue"].Should().Be(true);
        problemDetails.Extensions["doubleValue"].Should().Be(3.14);
    }

    [Fact]
    public void Extensions_ShouldAllowEmptyDictionary()
    {
        // Arrange
        var problemDetails = new CustomProblemDetails();
        var emptyExtensions = new Dictionary<string, object>();

        // Act
        problemDetails.Extensions = emptyExtensions;

        // Assert
        problemDetails.Extensions.Should().NotBeNull();
        problemDetails.Extensions.Should().BeEmpty();
    }

    #endregion
}
