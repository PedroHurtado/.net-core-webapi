# Estilo: Test de Enum

## Estructura

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.EnumsTests;

public class {EnumName}Tests
{
    [Theory]
    [InlineData({EnumName}.Value1, "Value1")]
    [InlineData({EnumName}.Value2, "Value2")]
    public void ToString_ReturnsExpectedStringName({EnumName} value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData({EnumName}.Value1, 1)]
    [InlineData({EnumName}.Value2, 2)]
    public void Value_ReturnsExpectedInteger({EnumName} value, int expectedValue)
    {
        ((int)value).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_HasExpectedMemberCount()
    {
        Enum.GetValues<{EnumName}>().Should().HaveCount({n});
    }
}
```

## Reglas

- Namespace: `{Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.EnumsTests`
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- Clase: `{EnumName}Tests`
- Tests obligatorios:
  - `ToString_ReturnsExpectedStringName` → Theory con todos los valores
  - `Value_ReturnsExpectedInteger` → Theory con todos los valores
  - `Enum_HasExpectedMemberCount` → Fact
- Usar FluentAssertions (`.Should().Be()`)
