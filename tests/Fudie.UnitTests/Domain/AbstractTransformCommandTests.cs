using FluentAssertions;
using Fudie.Domain;

namespace Fudie.UnitTests.Domain;

public class AbstractTransformCommandTests
{
    #region Test Classes

    private record TestValueObject(string Value);

    private record TransformData(string NewValue);

    private class TestTransformCommand : AbstractTransformCommand<TestValueObject>
    {
        public TestValueObject? ExecutedCurrent { get; private set; }
        public int ExecuteCallCount { get; private set; }

        public override TestValueObject Execute(TestValueObject current)
        {
            ExecutedCurrent = current;
            ExecuteCallCount++;
            return current with { Value = current.Value.ToUpperInvariant() };
        }
    }

    private class TestTransformCommandWithData : AbstractTransformCommand<TransformData, TestValueObject>
    {
        public TestValueObject? ExecutedCurrent { get; private set; }
        public TransformData? ExecutedCommand { get; private set; }
        public int ExecuteCallCount { get; private set; }

        public override TestValueObject Execute(TestValueObject current, TransformData command)
        {
            ExecutedCurrent = current;
            ExecutedCommand = command;
            ExecuteCallCount++;
            return current with { Value = command.NewValue };
        }
    }

    #endregion

    #region AbstractTransformCommand<TValueObject> Tests

    [Fact]
    public void Execute_ShouldTransformValueObject()
    {
        // Arrange
        var current = new TestValueObject("hello");
        var command = new TestTransformCommand();

        // Act
        var result = command.Execute(current);

        // Assert
        command.ExecuteCallCount.Should().Be(1);
        command.ExecutedCurrent.Should().BeSameAs(current);
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public void Execute_ShouldReturnNewInstance()
    {
        // Arrange
        var current = new TestValueObject("hello");
        var command = new TestTransformCommand();

        // Act
        var result = command.Execute(current);

        // Assert
        result.Should().NotBeSameAs(current);
    }

    #endregion

    #region AbstractTransformCommand<TCommand, TValueObject> Tests

    [Fact]
    public void Execute_WithCommand_ShouldTransformValueObjectUsingCommandData()
    {
        // Arrange
        var current = new TestValueObject("original");
        var data = new TransformData("transformed");
        var command = new TestTransformCommandWithData();

        // Act
        var result = command.Execute(current, data);

        // Assert
        command.ExecuteCallCount.Should().Be(1);
        command.ExecutedCurrent.Should().BeSameAs(current);
        command.ExecutedCommand.Should().BeSameAs(data);
        result.Value.Should().Be("transformed");
    }

    [Fact]
    public void Execute_WithCommand_ShouldReturnNewInstance()
    {
        // Arrange
        var current = new TestValueObject("original");
        var data = new TransformData("transformed");
        var command = new TestTransformCommandWithData();

        // Act
        var result = command.Execute(current, data);

        // Assert
        result.Should().NotBeSameAs(current);
    }

    #endregion
}
