# Estilo: Test de Value Object

## Estructura

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.ValueObjectsTests;

public class {ValueObject}Tests
{
    [Theory]
    [InlineData({value1})]
    [InlineData({value2})]
    public void {Property}_SetsCorrectValue({Type} {property})
    {
        var vo = new Testable{ValueObject}({property}, ...);

        vo.{Property}.Should().Be({property});
    }

    #region Static Instances

    [Fact]
    public void {Static}_ReturnsCorrect{ValueObject}()
    {
        var instance = {ValueObject}.{Static};

        instance.{Property1}.Should().Be({expected1});
        instance.{Property2}.Should().Be({expected2});
    }

    #endregion

    #region FromX

    [Theory]
    [InlineData("{input1}")]
    [InlineData("{input2}")]
    public void FromX_With{Case}_Returns{Expected}({Type} input)
    {
        var result = {ValueObject}.FromX(input);

        result.Should().Be({ValueObject}.{Expected});
    }

    [Fact]
    public void FromX_WithUnsupportedValue_ThrowsArgumentException()
    {
        var act = () => {ValueObject}.FromX("INVALID");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*{message}*");
    }

    #endregion
}
```

## Reglas

- Namespace: `{Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.ValueObjectsTests`
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- Clase: `{ValueObject}Tests`
- Usar `Testable{ValueObject}` para crear instancias
- Organizar con `#region` si hay estáticos o métodos FromX
- Tests por cada propiedad: `{Property}_SetsCorrectValue`
- Tests por cada estático: `{Static}_ReturnsCorrect{ValueObject}`
- Tests de métodos factory con casos válidos e inválidos
- Usar FluentAssertions
