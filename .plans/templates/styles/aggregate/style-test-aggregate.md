# Estilo: Test de Aggregate

## Estructura

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests;

public class {Aggregate}Tests
{
    [Fact]
    public void {Aggregate}_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregate = new Testable{Aggregate}(id);
        var valueObject = new {ValueObject}(...);

        // Act
        aggregate
            .With{Property1}({value1})
            .With{Property2}({value2})
            .With{Item}(valueObject);

        // Assert
        aggregate.Id.Should().Be(id);
        aggregate.{Property1}.Should().Be({value1});
        aggregate.{Property2}.Should().Be({value2});
        aggregate.{Collection}.Should().ContainSingle();
    }

    [Fact]
    public void {ComputedProperty}_With{Condition}_ShouldReturnTrue()
    {
        // Arrange
        var aggregate = new Testable{Aggregate}(Guid.NewGuid());
        var item = new {Item}(...condition true...);

        // Act
        aggregate.With{Item}(item);

        // Assert
        aggregate.{ComputedProperty}.Should().BeTrue();
    }

    [Fact]
    public void {ComputedProperty}_With{OppositeCondition}_ShouldReturnFalse()
    {
        // Arrange
        var aggregate = new Testable{Aggregate}(Guid.NewGuid());
        var item = new {Item}(...condition false...);

        // Act
        aggregate.With{Item}(item);

        // Assert
        aggregate.{ComputedProperty}.Should().BeFalse();
    }

    [Fact]
    public void {Collection}_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var aggregate = new Testable{Aggregate}(Guid.NewGuid());
        var item = new {Item}(...);

        // Act
        aggregate.With{Item}(item);

        // Assert
        aggregate.{Collection}.Should().BeAssignableTo<IReadOnlyCollection<{ItemType}>>();
        aggregate.{Collection}.Should().ContainSingle();
    }

    [Fact]
    public void {Aggregate}_WithMultiple{Items}_ShouldContainAll{Items}()
    {
        // Arrange
        var aggregate = new Testable{Aggregate}(Guid.NewGuid());
        var item1 = new {Item}(...);
        var item2 = new {Item}(...);

        // Act
        aggregate.With{Item}(item1);
        aggregate.With{Item}(item2);

        // Assert
        aggregate.{Collection}.Should().HaveCount(2);
        aggregate.{Collection}.Should().Contain(x => x.{Key} == {key1});
        aggregate.{Collection}.Should().Contain(x => x.{Key} == {key2});
    }

    [Theory]
    [InlineData({EnumValue1})]
    [InlineData({EnumValue2})]
    public void {Aggregate}_With{EnumProperty}_ShouldSetCorrectly({EnumType} value)
    {
        // Arrange
        var aggregate = new Testable{Aggregate}(Guid.NewGuid());

        // Act
        aggregate.With{EnumProperty}(value);

        // Assert
        aggregate.{EnumProperty}.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void {Aggregate}_{BoolProperty}_ShouldSetCorrectly(bool value)
    {
        // Arrange
        var aggregate = new Testable{Aggregate}(Guid.NewGuid());

        // Act
        aggregate.With{BoolProperty}(value);

        // Assert
        aggregate.{BoolProperty}.Should().Be(value);
    }
}
```

## Reglas

- Namespace: `{Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests`
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- Clase: `{Aggregate}Tests`
- Usar `Testable{Aggregate}` para aggregates/entities (tienen setters protegidos, necesitan wrapper con métodos `With`)
- Usar directamente `new {ValueObject}(...)` para Value Objects (son positional records con constructor público)
- Patrón AAA: `// Arrange`, `// Act`, `// Assert`
- Tests obligatorios:
  - `{Aggregate}_WithValidData_ShouldHaveCorrectProperties`
  - `{ComputedProperty}_With{Condition}_ShouldReturn{Bool}` por cada computed
  - `{Collection}_ShouldReturnReadOnlyCollection` por cada colección
  - `{Aggregate}_WithMultiple{Items}_ShouldContainAll{Items}` por cada colección
- Usar Theory para propiedades con múltiples valores válidos (enums, bools)
- Usar FluentAssertions
