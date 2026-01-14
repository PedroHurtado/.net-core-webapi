using FluentAssertions;
using Fudie.Domain;

namespace Fudie.UnitTests.Domain;

public class AbstractModifyCommandTests
{
    #region Test Classes

    private class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
        public string Name { get; set; } = string.Empty;
    }

    private record TestCommand(string Value);

    private class TestModifyCommand : AbstractModifyCommand<TestEntity>
    {
        public TestEntity? ExecutedEntity { get; private set; }
        public int ExecuteCallCount { get; private set; }

        public override TestEntity Execute(TestEntity entity)
        {
            ExecutedEntity = entity;
            ExecuteCallCount++;
            return entity;
        }
    }

    private class TestModifyCommandWithData : AbstractModifyCommand<TestCommand, TestEntity>
    {
        public TestEntity? ExecutedEntity { get; private set; }
        public TestCommand? ExecutedCommand { get; private set; }
        public int ExecuteCallCount { get; private set; }

        public override TestEntity Execute(TestEntity entity, TestCommand command)
        {
            ExecutedEntity = entity;
            ExecutedCommand = command;
            ExecuteCallCount++;
            return entity;
        }
    }

    #endregion

    #region AbstractModifyCommand<TEntity> Tests

    [Fact]
    public async Task ExecuteAsync_ShouldAwaitTaskAndCallExecuteWithResolvedEntity()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid()) { Name = "Test" };
        var entityTask = Task.FromResult(entity);
        var command = new TestModifyCommand();

        // Act
        var result = await command.ExecuteAsync(entityTask);

        // Assert
        command.ExecuteCallCount.Should().Be(1);
        command.ExecutedEntity.Should().BeSameAs(entity);
        result.Should().BeSameAs(entity);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskFails_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Entity not found");
        var failingTask = Task.FromException<TestEntity>(expectedException);
        var command = new TestModifyCommand();

        // Act
        Func<Task> act = () => command.ExecuteAsync(failingTask);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Entity not found");
        command.ExecuteCallCount.Should().Be(0);
    }

    #endregion

    #region AbstractModifyCommand<TCommand, TEntity> Tests

    [Fact]
    public async Task ExecuteAsync_WithCommand_ShouldAwaitTaskAndCallExecuteWithEntityAndCommand()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid()) { Name = "Test" };
        var entityTask = Task.FromResult(entity);
        var testCommand = new TestCommand("UpdatedValue");
        var modifyCommand = new TestModifyCommandWithData();

        // Act
        var result = await modifyCommand.ExecuteAsync(entityTask, testCommand);

        // Assert
        modifyCommand.ExecuteCallCount.Should().Be(1);
        modifyCommand.ExecutedEntity.Should().BeSameAs(entity);
        modifyCommand.ExecutedCommand.Should().BeSameAs(testCommand);
        result.Should().BeSameAs(entity);
    }

    [Fact]
    public async Task ExecuteAsync_WithCommand_WhenTaskFails_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Entity not found");
        var failingTask = Task.FromException<TestEntity>(expectedException);
        var testCommand = new TestCommand("Value");
        var modifyCommand = new TestModifyCommandWithData();

        // Act
        Func<Task> act = () => modifyCommand.ExecuteAsync(failingTask, testCommand);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Entity not found");
        modifyCommand.ExecuteCallCount.Should().Be(0);
    }

    #endregion
}
