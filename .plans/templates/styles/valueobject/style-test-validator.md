# Estilo: Test de Validator

## Estructura

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.ValueObjectsTests;

public class {ValueObject}ValidatorTests
{
    private readonly {ValueObject}Validator _validator = new();

    [Fact]
    public void Validate_WithValid{ValueObject}_ReturnsSuccess()
    {
        var vo = new {ValueObject}(...valid...);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    #region {Property} Validation

    [Fact]
    public void {Property}_WhenEmpty_ReturnsError()
    {
        var vo = new {ValueObject}("", ...);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == {ValueObject}ValidationMessages.{Property}Required);
    }

    [Theory]
    [InlineData({invalidValue1})]
    [InlineData({invalidValue2})]
    public void {Property}_When{Condition}_ReturnsError({Type} value)
    {
        var vo = new {ValueObject}(value, ...);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == {ValueObject}ValidationMessages.{Property}{Rule});
    }

    [Theory]
    [InlineData({validValue1})]
    [InlineData({validValue2})]
    public void {Property}_WhenValid_ReturnsSuccess({Type} value)
    {
        var vo = new {ValueObject}(value, ...);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
```

## Reglas

- Namespace: mismo que el test del VO/Aggregate
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- Clase: `{Type}ValidatorTests`
- Campo privado `_validator` instanciado
- Primer test: `Validate_WithValid{Type}_ReturnsSuccess`
- Organizar por propiedad con `#region {Property} Validation`
- Nomenclatura de tests:
  - `{Property}_WhenEmpty_ReturnsError`
  - `{Property}_When{Condition}_ReturnsError`
  - `{Property}_WhenValid_ReturnsSuccess`
- Crear instancias directamente con `new {ValueObject}(...)` (constructor público)
- Verificar mensajes con `{Type}ValidationMessages.{Property}{Rule}`
- Usar FluentAssertions
