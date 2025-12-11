using FluentAssertions;
using Fudie.Domain;

namespace Fudie.UnitTests.Domain;

public class AbstractCreateCommandTests
{
    #region Test Classes

    private class TestEntity : Entity
    {
        public TestEntity(Guid id) : base(id) { }
        public string Name { get; set; } = string.Empty;
    }

    private record CreateTestEntityCommand(Guid Id, string Name);

    private class TestCreateCommand : AbstractCreateCommand<CreateTestEntityCommand, TestEntity>
    {
        public CreateTestEntityCommand? ExecutedCommand { get; private set; }
        public int ExecuteCallCount { get; private set; }

        public override TestEntity Execute(CreateTestEntityCommand command)
        {
            ExecutedCommand = command;
            ExecuteCallCount++;
            return new TestEntity(command.Id) { Name = command.Name };
        }
    }

    #endregion

    #region AbstractCreateCommand<TCommand, TEntity> Tests

    [Fact]
    public void Execute_ShouldCreateEntityFromCommand()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new CreateTestEntityCommand(id, "TestName");
        var createCommand = new TestCreateCommand();

        // Act
        var result = createCommand.Execute(command);

        // Assert
        createCommand.ExecuteCallCount.Should().Be(1);
        createCommand.ExecutedCommand.Should().BeSameAs(command);
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Name.Should().Be("TestName");
    }

    [Fact]
    public void Execute_ShouldReturnNewEntityInstance()
    {
        // Arrange
        var command1 = new CreateTestEntityCommand(Guid.NewGuid(), "Entity1");
        var command2 = new CreateTestEntityCommand(Guid.NewGuid(), "Entity2");
        var createCommand = new TestCreateCommand();

        // Act
        var result1 = createCommand.Execute(command1);
        var result2 = createCommand.Execute(command2);

        // Assert
        result1.Should().NotBeSameAs(result2);
        result1.Id.Should().NotBe(result2.Id);
        createCommand.ExecuteCallCount.Should().Be(2);
    }

    #endregion
}
