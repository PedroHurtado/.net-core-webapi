using System.Reflection;
using FluentAssertions;
using Fudie.OpenApi;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fudie.UnitTests.OpenApi;

public class GlobalErrorResponsesOperationFilterTests
{
    #region Test Setup

    private readonly Mock<ISchemaGenerator> _schemaGeneratorMock;
    private readonly SchemaRepository _schemaRepository;
    private readonly GlobalErrorResponsesOperationFilter _filter;

    public GlobalErrorResponsesOperationFilterTests()
    {
        _schemaGeneratorMock = new Mock<ISchemaGenerator>();
        _schemaRepository = new SchemaRepository();
        _filter = new GlobalErrorResponsesOperationFilter();

        // Setup schema generator to return a simple schema
        _schemaGeneratorMock
            .Setup(x => x.GenerateSchema(typeof(CustomProblemDetails), _schemaRepository, null, null, null))
            .Returns(new OpenApiSchema { Type = "object" });
    }

    private OperationFilterContext CreateOperationFilterContext(string? httpMethod)
    {
        var apiDescription = new ApiDescription
        {
            HttpMethod = httpMethod
        };

        return new OperationFilterContext(
            apiDescription,
            _schemaGeneratorMock.Object,
            _schemaRepository,
            typeof(GlobalErrorResponsesOperationFilterTests).GetMethod(nameof(DummyMethod), BindingFlags.NonPublic | BindingFlags.Instance)!);
    }

    private void DummyMethod() { }

    #endregion

    #region 400 Bad Request Tests

    [Fact]
    public void Apply_ShouldAdd400Response_WhenNotExists()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().ContainKey("400");
        operation.Responses["400"].Description.Should().Be("Bad Request");
    }

    [Fact]
    public void Apply_ShouldNotOverwrite400Response_WhenAlreadyExists()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["400"] = new OpenApiResponse { Description = "Custom Bad Request" }
            }
        };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["400"].Description.Should().Be("Custom Bad Request");
    }

    [Fact]
    public void Apply_400Response_ShouldHaveCorrectContentType()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["400"].Content.Should().ContainKey("application/problem+json");
    }

    #endregion

    #region 422 Unprocessed Entity Tests

    [Fact]
    public void Apply_ShouldAdd422Response_WhenNotExists()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().ContainKey("422");
        operation.Responses["422"].Description.Should().Be("Unprocesed entity");
    }

    [Fact]
    public void Apply_ShouldNotOverwrite422Response_WhenAlreadyExists()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["422"] = new OpenApiResponse { Description = "Custom Validation Error" }
            }
        };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["422"].Description.Should().Be("Custom Validation Error");
    }

    [Fact]
    public void Apply_422Response_ShouldHaveCorrectContentType()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["422"].Content.Should().ContainKey("application/problem+json");
    }

    #endregion

    #region 404 Not Found Tests

    [Fact]
    public void Apply_ShouldAdd404Response_WhenNotExistsAndNotPost()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().ContainKey("404");
        operation.Responses["404"].Description.Should().Be("Not Found");
    }

    [Fact]
    public void Apply_ShouldNotAdd404Response_ForPostMethod()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("POST");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().NotContainKey("404");
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public void Apply_ShouldAdd404Response_ForNonPostMethods(string httpMethod)
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext(httpMethod);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().ContainKey("404");
    }

    [Fact]
    public void Apply_ShouldNotOverwrite404Response_WhenAlreadyExists()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["404"] = new OpenApiResponse { Description = "Custom Not Found" }
            }
        };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["404"].Description.Should().Be("Custom Not Found");
    }

    [Fact]
    public void Apply_404Response_ShouldHaveCorrectContentType()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["404"].Content.Should().ContainKey("application/problem+json");
    }

    [Theory]
    [InlineData("post")]
    [InlineData("Post")]
    [InlineData("POST")]
    public void Apply_ShouldNotAdd404Response_ForPostMethodCaseInsensitive(string httpMethod)
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext(httpMethod);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().NotContainKey("404");
    }

    #endregion

    #region 500 Internal Server Error Tests

    [Fact]
    public void Apply_ShouldAdd500Response_WhenNotExists()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().ContainKey("500");
        operation.Responses["500"].Description.Should().Be("Internal Server Error");
    }

    [Fact]
    public void Apply_ShouldNotOverwrite500Response_WhenAlreadyExists()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["500"] = new OpenApiResponse { Description = "Custom Server Error" }
            }
        };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["500"].Description.Should().Be("Custom Server Error");
    }

    [Fact]
    public void Apply_500Response_ShouldHaveCorrectContentType()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["500"].Content.Should().ContainKey("application/problem+json");
    }

    #endregion

    #region All Responses Combined Tests

    [Fact]
    public void Apply_ShouldAddAllExpectedResponses_ForGetMethod()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().HaveCount(4);
        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("422");
        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("500");
    }

    [Fact]
    public void Apply_ShouldAddThreeResponses_ForPostMethod()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("POST");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().HaveCount(3);
        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("422");
        operation.Responses.Should().ContainKey("500");
        operation.Responses.Should().NotContainKey("404");
    }

    [Fact]
    public void Apply_ShouldPreserveExistingResponses()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Description = "Success" },
                ["201"] = new OpenApiResponse { Description = "Created" }
            }
        };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().ContainKey("200");
        operation.Responses.Should().ContainKey("201");
        operation.Responses["200"].Description.Should().Be("Success");
        operation.Responses["201"].Description.Should().Be("Created");
    }

    [Fact]
    public void Apply_WithAllResponsesAlreadyDefined_ShouldNotAddAny()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["400"] = new OpenApiResponse { Description = "Custom 400" },
                ["404"] = new OpenApiResponse { Description = "Custom 404" },
                ["422"] = new OpenApiResponse { Description = "Custom 422" },
                ["500"] = new OpenApiResponse { Description = "Custom 500" }
            }
        };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["400"].Description.Should().Be("Custom 400");
        operation.Responses["404"].Description.Should().Be("Custom 404");
        operation.Responses["422"].Description.Should().Be("Custom 422");
        operation.Responses["500"].Description.Should().Be("Custom 500");
    }

    #endregion

    #region Schema Generation Tests

    [Fact]
    public void Apply_ShouldCallSchemaGenerator_ForEachAddedResponse()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert - Should call for 400, 422, 404, and 500
        _schemaGeneratorMock.Verify(
            x => x.GenerateSchema(typeof(CustomProblemDetails), _schemaRepository, null, null, null),
            Times.Exactly(4));
    }

    [Fact]
    public void Apply_ShouldNotCallSchemaGenerator_WhenAllResponsesExist()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["400"] = new OpenApiResponse { Description = "Existing" },
                ["404"] = new OpenApiResponse { Description = "Existing" },
                ["422"] = new OpenApiResponse { Description = "Existing" },
                ["500"] = new OpenApiResponse { Description = "Existing" }
            }
        };
        var context = CreateOperationFilterContext("GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        _schemaGeneratorMock.Verify(
            x => x.GenerateSchema(typeof(CustomProblemDetails), _schemaRepository, null, null, null),
            Times.Never);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Apply_WithNullHttpMethod_ShouldAdd404Response()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext(null);

        // Act
        _filter.Apply(operation, context);

        // Assert - null != "POST" so 404 should be added
        operation.Responses.Should().ContainKey("404");
    }

    [Fact]
    public void Apply_ShouldBeIdempotent()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = new OpenApiResponses() };
        var context = CreateOperationFilterContext("GET");

        // Act - Apply twice
        _filter.Apply(operation, context);
        var countAfterFirst = operation.Responses.Count;
        _filter.Apply(operation, context);
        var countAfterSecond = operation.Responses.Count;

        // Assert
        countAfterFirst.Should().Be(countAfterSecond);
    }

    #endregion
}
