# Estilo: Response Tests (API)

## Propósito

Tests unitarios para los métodos `Map` de los records de respuesta. Verifican que el mapeo dominio → API es correcto.

---

## Ubicación

- Archivo: `{Aggregate}ResponseTests.cs`
- Namespace: `{Microservice}.UnitTests.Features.{Aggregate}s.Api.{Aggregate}Aggregate`

---

## Estructura
```csharp
namespace {Microservice}.UnitTests.Features.{Aggregate}s.Api.{Aggregate}Aggregate;

public class {Aggregate}ResponseTests
{
    #region {Aggregate}Response.Map

    [Fact]
    public void {Aggregate}Response_Map_MapsAllProperties()
    {
        var {aggregate} = new Testable{Aggregate}(Guid.NewGuid())
            .WithName("Test")
            .With{Property}({value});

        var response = {Aggregate}Response.Map({aggregate});

        response.Id.Should().Be({aggregate}.Id);
        response.Name.Should().Be("Test");
        response.{Property}.Should().Be({value});
    }

    [Fact]
    public void {Aggregate}Response_Map_WithNullOptionalFields_MapsNullValues()
    {
        var {aggregate} = new Testable{Aggregate}(Guid.NewGuid())
            .WithName("Minimal");

        var response = {Aggregate}Response.Map({aggregate});

        response.Description.Should().BeNull();
        response.{OptionalProperty}.Should().BeNull();
    }

    [Fact]
    public void {Aggregate}Response_Map_With{Children}_Maps{Children}()
    {
        var {child} = new Testable{Child}(Guid.NewGuid())
            .WithName("Child");

        var {aggregate} = new Testable{Aggregate}(Guid.NewGuid())
            .WithName("Test")
            .With{Child}({child});

        var response = {Aggregate}Response.Map({aggregate});

        response.{Children}.Should().HaveCount(1);
        response.{Children}.First().Name.Should().Be("Child");
    }

    #endregion

    #region {ValueObject}Response.Map

    [Fact]
    public void {ValueObject}Response_Map_MapsAllProperties()
    {
        var {valueObject} = new Testable{ValueObject}({param1}, {param2});

        var response = {ValueObject}Response.Map({valueObject});

        response.{Property1}.Should().Be({param1});
        response.{Property2}.Should().Be({param2});
    }

    #endregion

    #region {Child}Response.Map

    [Fact]
    public void {Child}Response_Map_MapsAllProperties()
    {
        var {child} = new Testable{Child}(Guid.NewGuid())
            .WithName("Test")
            .WithIsActive(true);

        var response = {Child}Response.Map({child});

        response.Id.Should().Be({child}.Id);
        response.Name.Should().Be("Test");
        response.IsActive.Should().BeTrue();
    }

    #endregion
}
```

---

## Reglas

- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- **No `TenantId`** → Nunca verificar en respuestas de API
- Nombre de test: `{Record}_Map_{Escenario}`
- Una región por cada record: `#region {Record}.Map`
- Tests por record: propiedades completas, nullables opcionales, colecciones hijas
- Usar Testables con patrón `With` fluent para preparar estado
- Asserts con FluentAssertions
- Orden de regiones: raíz primero, hijos después (mismo orden que el archivo de Response)