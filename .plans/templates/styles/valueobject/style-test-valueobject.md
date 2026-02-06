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
        var vo = new {ValueObject}({property}, ...);

        vo.{Property}.Should().Be({property});
    }
```

## Reglas

- Namespace: `{Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.ValueObjectsTests`
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- Clase: `{ValueObject}Tests`
- Crear instancias directamente con `new {ValueObject}(...)` (constructor público)
- Organizar con `#region` si hay múltiples propiedades
- Tests por cada propiedad: `{Property}_SetsCorrectValue`
- Usar FluentAssertions
