using FluentAssertions;
using Fudie.Domain;

namespace Fudie.UnitTests.Domain;

public class ValidationErrorTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithErrorMessageAndPropertyName_ShouldSetProperties()
    {
        // Arrange
        var errorMessage = "Field is required";
        var propertyName = "Email";

        // Act
        var validationError = new ValidationError(errorMessage, propertyName);

        // Assert
        validationError.ErrorMessage.Should().Be(errorMessage);
        validationError.PropertyName.Should().Be(propertyName);
    }

    [Fact]
    public void Constructor_WithOnlyErrorMessage_ShouldSetEmptyPropertyName()
    {
        // Arrange
        var errorMessage = "General error";

        // Act
        var validationError = new ValidationError(errorMessage);

        // Assert
        validationError.ErrorMessage.Should().Be(errorMessage);
        validationError.PropertyName.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyErrorMessage_ShouldAcceptIt()
    {
        // Arrange & Act
        var validationError = new ValidationError("");

        // Assert
        validationError.ErrorMessage.Should().BeEmpty();
        validationError.PropertyName.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyPropertyName_ShouldAcceptIt()
    {
        // Arrange & Act
        var validationError = new ValidationError("Error", "");

        // Assert
        validationError.ErrorMessage.Should().Be("Error");
        validationError.PropertyName.Should().BeEmpty();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var validationError = new ValidationError("Error", "Property");

        // Act
        var errorMessageProperty = typeof(ValidationError).GetProperty(nameof(ValidationError.ErrorMessage));
        var propertyNameProperty = typeof(ValidationError).GetProperty(nameof(ValidationError.PropertyName));

        // Assert
        errorMessageProperty.Should().NotBeNull();
        errorMessageProperty!.CanWrite.Should().BeFalse();
        propertyNameProperty.Should().NotBeNull();
        propertyNameProperty!.CanWrite.Should().BeFalse();
    }

    #endregion

    #region Multiple Instances Tests

    [Fact]
    public void MultipleInstances_WithSameValues_ShouldBeIndependent()
    {
        // Arrange & Act
        var error1 = new ValidationError("Error", "Field");
        var error2 = new ValidationError("Error", "Field");

        // Assert
        error1.Should().NotBeSameAs(error2);
        error1.ErrorMessage.Should().Be(error2.ErrorMessage);
        error1.PropertyName.Should().Be(error2.PropertyName);
    }

    #endregion
}

public class ResultTests
{
    #region Success Tests

    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_ShouldHaveNoErrors()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var result1 = Result.Success();
        var result2 = Result.Success();

        // Assert
        result1.Should().NotBeSameAs(result2);
        result1.IsSuccess.Should().Be(result2.IsSuccess);
    }

    #endregion

    #region Failure with Single Error Tests

    [Fact]
    public void Failure_WithErrorMessage_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Validation failed";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().ErrorMessage.Should().Be(errorMessage);
        result.Errors.First().PropertyName.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithErrorMessageAndPropertyName_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Email is invalid";
        var propertyName = "Email";

        // Act
        var result = Result.Failure(errorMessage, propertyName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().ErrorMessage.Should().Be(errorMessage);
        result.Errors.First().PropertyName.Should().Be(propertyName);
    }

    [Fact]
    public void Failure_WithEmptyErrorMessage_ShouldAcceptIt()
    {
        // Act
        var result = Result.Failure("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().ErrorMessage.Should().BeEmpty();
    }

    #endregion

    #region Failure with Multiple Errors Tests

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailureResult()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1", "Field1"),
            new ValidationError("Error 2", "Field2"),
            new ValidationError("Error 3", "Field3")
        };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Failure_WithEmptyErrorCollection_ShouldCreateFailureResult()
    {
        // Arrange
        var errors = Array.Empty<ValidationError>();

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithSingleErrorInCollection_ShouldWork()
    {
        // Arrange
        var errors = new[] { new ValidationError("Single error", "Field") };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region IsFailure Property Tests

    [Fact]
    public void IsFailure_ForSuccessResult_ShouldBeFalse()
    {
        // Arrange
        var result = Result.Success();

        // Act & Assert
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void IsFailure_ForFailureResult_ShouldBeTrue()
    {
        // Arrange
        var result = Result.Failure("Error");

        // Act & Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void IsFailure_ShouldBeOppositeOfIsSuccess()
    {
        // Arrange
        var successResult = Result.Success();
        var failureResult = Result.Failure("Error");

        // Act & Assert
        successResult.IsFailure.Should().Be(!successResult.IsSuccess);
        failureResult.IsFailure.Should().Be(!failureResult.IsSuccess);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Failure_WithManyErrors_ShouldHandleCorrectly()
    {
        // Arrange
        var errors = Enumerable.Range(1, 100)
            .Select(i => new ValidationError($"Error {i}", $"Field{i}"))
            .ToList();

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(100);
    }

    [Fact]
    public void Result_ErrorsCollection_ShouldBeEnumerable()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1", "Field1"),
            new ValidationError("Error 2", "Field2")
        };
        var result = Result.Failure(errors);

        // Act
        var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

        // Assert
        errorMessages.Should().Contain("Error 1");
        errorMessages.Should().Contain("Error 2");
    }

    #endregion
}

public class ResultGenericTests
{
    #region Success Tests

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result<int>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithReferenceType_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithComplexObject_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        var result = Result<TestObject>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(value);
        result.Value!.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public void Success_WithNullValue_ShouldAcceptIt()
    {
        // Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    #endregion

    #region Failure with Single Error Tests

    [Fact]
    public void Failure_WithErrorMessage_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Validation failed";

        // Act
        var result = Result<int>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().Be(default(int));
        result.Errors.Should().HaveCount(1);
        result.Errors.First().ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_WithErrorMessageAndPropertyName_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Invalid email";
        var propertyName = "Email";

        // Act
        var result = Result<string>.Failure(errorMessage, propertyName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().ErrorMessage.Should().Be(errorMessage);
        result.Errors.First().PropertyName.Should().Be(propertyName);
    }

    [Fact]
    public void Failure_ForReferenceType_ValueShouldBeNull()
    {
        // Act
        var result = Result<string>.Failure("Error");

        // Assert
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Failure_ForValueType_ValueShouldBeDefault()
    {
        // Act
        var result = Result<int>.Failure("Error");

        // Assert
        result.Value.Should().Be(0);
    }

    #endregion

    #region Failure with Multiple Errors Tests

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailureResult()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1", "Field1"),
            new ValidationError("Error 2", "Field2")
        };

        // Act
        var result = Result<string>.Failure(errors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Failure_WithEmptyErrorCollection_ShouldCreateFailureResult()
    {
        // Arrange
        var errors = Array.Empty<ValidationError>();

        // Act
        var result = Result<int>.Failure(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ResultGeneric_ShouldInheritFromResult()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act & Assert
        result.Should().BeAssignableTo<Result>();
    }

    [Fact]
    public void ResultGeneric_WhenCastToBase_ShouldMaintainProperties()
    {
        // Arrange
        var genericResult = Result<int>.Success(42);

        // Act
        Result baseResult = genericResult;

        // Assert
        baseResult.IsSuccess.Should().BeTrue();
        baseResult.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ResultGeneric_Failure_WhenCastToBase_ShouldMaintainErrors()
    {
        // Arrange
        var genericResult = Result<int>.Failure("Error", "Field");

        // Act
        Result baseResult = genericResult;

        // Assert
        baseResult.IsFailure.Should().BeTrue();
        baseResult.Errors.Should().HaveCount(1);
    }

    #endregion

    #region Different Type Tests

    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Success_WithDifferentIntValues_ShouldWork(int value)
    {
        // Act
        var result = Result<int>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Success_WithBool_ShouldWork()
    {
        // Act
        var resultTrue = Result<bool>.Success(true);
        var resultFalse = Result<bool>.Success(false);

        // Assert
        resultTrue.Value.Should().BeTrue();
        resultFalse.Value.Should().BeFalse();
    }

    [Fact]
    public void Success_WithGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = Result<Guid>.Success(guid);

        // Assert
        result.Value.Should().Be(guid);
    }

    [Fact]
    public void Success_WithList_ShouldWork()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        var result = Result<List<int>>.Success(list);

        // Assert
        result.Value.Should().BeSameAs(list);
        result.Value.Should().HaveCount(3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Success_WithDefaultValue_ShouldWork()
    {
        // Act
        var result = Result<int>.Success(default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public void Failure_MultipleErrorsWithSamePropertyName_ShouldBeAllowed()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1", "Email"),
            new ValidationError("Error 2", "Email"),
            new ValidationError("Error 3", "Email")
        };

        // Act
        var result = Result<string>.Failure(errors);

        // Assert
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().AllSatisfy(e => e.PropertyName.Should().Be("Email"));
    }

    #endregion

    #region Consistency Tests

    [Fact]
    public void IsFailure_ShouldBeOppositeOfIsSuccess()
    {
        // Arrange
        var successResult = Result<int>.Success(42);
        var failureResult = Result<int>.Failure("Error");

        // Act & Assert
        successResult.IsFailure.Should().Be(!successResult.IsSuccess);
        failureResult.IsFailure.Should().Be(!failureResult.IsSuccess);
    }

    [Fact]
    public void Success_ShouldNeverHaveErrors()
    {
        // Arrange
        Result[] results =
        [
            Result<int>.Success(1),
            Result<string>.Success("test"),
            Result<bool>.Success(true)
        ];

        // Act & Assert
        results.Should().AllSatisfy(r =>
        {
            r.IsSuccess.Should().BeTrue();
            r.Errors.Should().BeEmpty();
        });
    }

    [Fact]
    public void Failure_ShouldAlwaysHaveIsSuccessFalse()
    {
        // Arrange
        var results = new Result[]
        {
            Result<int>.Failure("Error 1"),
            Result<string>.Failure("Error 2", "Field"),
            Result<bool>.Failure(new[] { new ValidationError("Error 3") })
        };

        // Act & Assert
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeFalse());
    }

    #endregion

    #region Test Helper Class

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
