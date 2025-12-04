using FluentAssertions;
using FluentValidation;
using Fudie.Domain;

namespace Fudie.UnitTests.Domain;

public class ResultExtensionsTests
{
    #region ValueOrThrow Tests - Success Cases

    [Fact]
    public void ValueOrThrow_WithSuccessResult_ShouldReturnValue()
    {
        // Arrange
        var expectedValue = 42;
        var result = Result<int>.Success(expectedValue);

        // Act
        var actualValue = result.ValueOrThrow();

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void ValueOrThrow_WithSuccessResultAndReferenceType_ShouldReturnValue()
    {
        // Arrange
        var expectedValue = "Hello World";
        var result = Result<string>.Success(expectedValue);

        // Act
        var actualValue = result.ValueOrThrow();

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void ValueOrThrow_WithSuccessResultAndComplexObject_ShouldReturnValue()
    {
        // Arrange
        var expectedValue = new TestObject { Id = 1, Name = "Test" };
        var result = Result<TestObject>.Success(expectedValue);

        // Act
        var actualValue = result.ValueOrThrow();

        // Assert
        actualValue.Should().BeSameAs(expectedValue);
        actualValue.Id.Should().Be(1);
        actualValue.Name.Should().Be("Test");
    }

    [Fact]
    public void ValueOrThrow_WithSuccessResultAndNullValue_ShouldReturnNull()
    {
        // Arrange
        var result = Result<string?>.Success(null);

        // Act
        var actualValue = result.ValueOrThrow();

        // Assert
        actualValue.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ValueOrThrow_WithDifferentIntValues_ShouldReturnValue(int expectedValue)
    {
        // Arrange
        var result = Result<int>.Success(expectedValue);

        // Act
        var actualValue = result.ValueOrThrow();

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    #endregion

    #region ValueOrThrow Tests - Failure Cases

    [Fact]
    public void ValueOrThrow_WithFailureResult_ShouldThrowValidationException()
    {
        // Arrange
        var result = Result<int>.Failure("Validation error");

        // Act
        var act = () => result.ValueOrThrow();

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ValueOrThrow_WithFailureResultAndSingleError_ShouldThrowWithCorrectError()
    {
        // Arrange
        var errorMessage = "Email is invalid";
        var propertyName = "Email";
        var result = Result<string>.Failure(errorMessage, propertyName);

        // Act
        var act = () => result.ValueOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().ErrorMessage.Should().Be(errorMessage);
        exception.Errors.First().PropertyName.Should().Be(propertyName);
    }

    [Fact]
    public void ValueOrThrow_WithFailureResultAndMultipleErrors_ShouldThrowWithAllErrors()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1", "Field1"),
            new ValidationError("Error 2", "Field2"),
            new ValidationError("Error 3", "Field3")
        };
        var result = Result<int>.Failure(errors);

        // Act
        var act = () => result.ValueOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().HaveCount(3);

        var errorList = exception.Errors.ToList();
        errorList[0].ErrorMessage.Should().Be("Error 1");
        errorList[0].PropertyName.Should().Be("Field1");
        errorList[1].ErrorMessage.Should().Be("Error 2");
        errorList[1].PropertyName.Should().Be("Field2");
        errorList[2].ErrorMessage.Should().Be("Error 3");
        errorList[2].PropertyName.Should().Be("Field3");
    }

    [Fact]
    public void ValueOrThrow_WithFailureResultAndEmptyPropertyName_ShouldThrowWithEmptyPropertyName()
    {
        // Arrange
        var errorMessage = "General error";
        var result = Result<string>.Failure(errorMessage);

        // Act
        var act = () => result.ValueOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.First().PropertyName.Should().BeEmpty();
        exception.Errors.First().ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void ValueOrThrow_WithFailureResultAndEmptyErrors_ShouldThrowWithNoErrors()
    {
        // Arrange
        var result = Result<int>.Failure(Array.Empty<ValidationError>());

        // Act
        var act = () => result.ValueOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().BeEmpty();
    }

    #endregion

    #region SuccessOrThrow Tests - Success Cases

    [Fact]
    public void SuccessOrThrow_WithSuccessResult_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var act = () => result.SuccessOrThrow();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SuccessOrThrow_WithSuccessResult_ShouldCompleteSuccessfully()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;

        // Act
        result.SuccessOrThrow();
        executed = true;

        // Assert
        executed.Should().BeTrue();
    }

    #endregion

    #region SuccessOrThrow Tests - Failure Cases

    [Fact]
    public void SuccessOrThrow_WithFailureResult_ShouldThrowValidationException()
    {
        // Arrange
        var result = Result.Failure("Validation error");

        // Act
        var act = () => result.SuccessOrThrow();

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SuccessOrThrow_WithFailureResultAndSingleError_ShouldThrowWithCorrectError()
    {
        // Arrange
        var errorMessage = "Operation failed";
        var propertyName = "Operation";
        var result = Result.Failure(errorMessage, propertyName);

        // Act
        var act = () => result.SuccessOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().ErrorMessage.Should().Be(errorMessage);
        exception.Errors.First().PropertyName.Should().Be(propertyName);
    }

    [Fact]
    public void SuccessOrThrow_WithFailureResultAndMultipleErrors_ShouldThrowWithAllErrors()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1", "Field1"),
            new ValidationError("Error 2", "Field2")
        };
        var result = Result.Failure(errors);

        // Act
        var act = () => result.SuccessOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().HaveCount(2);

        var errorList = exception.Errors.ToList();
        errorList[0].ErrorMessage.Should().Be("Error 1");
        errorList[0].PropertyName.Should().Be("Field1");
        errorList[1].ErrorMessage.Should().Be("Error 2");
        errorList[1].PropertyName.Should().Be("Field2");
    }

    [Fact]
    public void SuccessOrThrow_WithFailureResultAndEmptyPropertyName_ShouldThrowWithEmptyPropertyName()
    {
        // Arrange
        var errorMessage = "General error";
        var result = Result.Failure(errorMessage);

        // Act
        var act = () => result.SuccessOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.First().PropertyName.Should().BeEmpty();
        exception.Errors.First().ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void SuccessOrThrow_WithFailureResultAndEmptyErrors_ShouldThrowWithNoErrors()
    {
        // Arrange
        var result = Result.Failure(Array.Empty<ValidationError>());

        // Act
        var act = () => result.SuccessOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void ValueOrThrow_CalledMultipleTimes_ShouldReturnSameValue()
    {
        // Arrange
        var expectedValue = 42;
        var result = Result<int>.Success(expectedValue);

        // Act
        var value1 = result.ValueOrThrow();
        var value2 = result.ValueOrThrow();
        var value3 = result.ValueOrThrow();

        // Assert
        value1.Should().Be(expectedValue);
        value2.Should().Be(expectedValue);
        value3.Should().Be(expectedValue);
    }

    [Fact]
    public void SuccessOrThrow_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Success();

        // Act & Assert
        result.SuccessOrThrow();
        result.SuccessOrThrow();
        result.SuccessOrThrow();
        // If we reach here, the test passes
    }

    [Fact]
    public void ValueOrThrow_WithFailureResult_CalledMultipleTimes_ShouldThrowEveryTime()
    {
        // Arrange
        var result = Result<int>.Failure("Error");

        // Act & Assert
        var act1 = () => result.ValueOrThrow();
        var act2 = () => result.ValueOrThrow();
        var act3 = () => result.ValueOrThrow();

        act1.Should().Throw<ValidationException>();
        act2.Should().Throw<ValidationException>();
        act3.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SuccessOrThrow_WithFailureResult_CalledMultipleTimes_ShouldThrowEveryTime()
    {
        // Arrange
        var result = Result.Failure("Error");

        // Act & Assert
        var act1 = () => result.SuccessOrThrow();
        var act2 = () => result.SuccessOrThrow();
        var act3 = () => result.SuccessOrThrow();

        act1.Should().Throw<ValidationException>();
        act2.Should().Throw<ValidationException>();
        act3.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ValueOrThrow_WithManyErrors_ShouldThrowWithAllErrors()
    {
        // Arrange
        var errors = Enumerable.Range(1, 50)
            .Select(i => new ValidationError($"Error {i}", $"Field{i}"))
            .ToArray();
        var result = Result<string>.Failure(errors);

        // Act
        var act = () => result.ValueOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().HaveCount(50);
    }

    [Fact]
    public void SuccessOrThrow_WithManyErrors_ShouldThrowWithAllErrors()
    {
        // Arrange
        var errors = Enumerable.Range(1, 50)
            .Select(i => new ValidationError($"Error {i}", $"Field{i}"))
            .ToArray();
        var result = Result.Failure(errors);

        // Act
        var act = () => result.SuccessOrThrow();

        // Assert
        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().HaveCount(50);
    }

    #endregion

    #region Different Generic Types Tests

    [Fact]
    public void ValueOrThrow_WithBoolResult_ShouldWork()
    {
        // Arrange
        var resultTrue = Result<bool>.Success(true);
        var resultFalse = Result<bool>.Success(false);

        // Act
        var valueTrue = resultTrue.ValueOrThrow();
        var valueFalse = resultFalse.ValueOrThrow();

        // Assert
        valueTrue.Should().BeTrue();
        valueFalse.Should().BeFalse();
    }

    [Fact]
    public void ValueOrThrow_WithGuidResult_ShouldWork()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var result = Result<Guid>.Success(expectedGuid);

        // Act
        var actualGuid = result.ValueOrThrow();

        // Assert
        actualGuid.Should().Be(expectedGuid);
    }

    [Fact]
    public void ValueOrThrow_WithListResult_ShouldWork()
    {
        // Arrange
        var expectedList = new List<int> { 1, 2, 3, 4, 5 };
        var result = Result<List<int>>.Success(expectedList);

        // Act
        var actualList = result.ValueOrThrow();

        // Assert
        actualList.Should().BeSameAs(expectedList);
        actualList.Should().HaveCount(5);
    }

    [Fact]
    public void ValueOrThrow_WithDefaultValue_ShouldReturnDefault()
    {
        // Arrange
        var result = Result<int>.Success(default);

        // Act
        var value = result.ValueOrThrow();

        // Assert
        value.Should().Be(0);
    }

    #endregion

    #region Practical Usage Scenarios

    [Fact]
    public void ValueOrThrow_InTryCatchBlock_ShouldBeCatchable()
    {
        // Arrange
        var result = Result<int>.Failure("Error", "Field");
        var exceptionCaught = false;

        // Act
        try
        {
            var value = result.ValueOrThrow();
        }
        catch (ValidationException ex)
        {
            exceptionCaught = true;
            ex.Errors.Should().HaveCount(1);
        }

        // Assert
        exceptionCaught.Should().BeTrue();
    }

    [Fact]
    public void SuccessOrThrow_InTryCatchBlock_ShouldBeCatchable()
    {
        // Arrange
        var result = Result.Failure("Error", "Field");
        var exceptionCaught = false;

        // Act
        try
        {
            result.SuccessOrThrow();
        }
        catch (ValidationException ex)
        {
            exceptionCaught = true;
            ex.Errors.Should().HaveCount(1);
        }

        // Assert
        exceptionCaught.Should().BeTrue();
    }

    [Fact]
    public void ValueOrThrow_ChainedWithOtherOperations_ShouldWork()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var finalValue = result.ValueOrThrow() * 2 + 5;

        // Assert
        finalValue.Should().Be(25);
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
