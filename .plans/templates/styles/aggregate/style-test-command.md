# Estilo: Test de Comandos

## Reglas

- **No usar `Testable`** → Usar comandos para crear instancias
- **Validators se instancian directamente** → `new {Type}Validator()`
- **Comandos se instancian con sus dependencias** → `new {Aggregate}.Create(validator)`
- Para testear Update/Add/Remove, crear la entidad con su comando Create primero

---

## Estructura

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.Commands.{Aggregate}Tests;

public class {Aggregate}{Command}Tests
{
    private readonly {Aggregate}Validator _validator = new();
    private readonly {Aggregate}.Create _create;
    private readonly {Aggregate}.{Command} _{command};
    private readonly {ValueObject}.Create _{valueObject}Create;

    public {Aggregate}{Command}Tests()
    {
        _{valueObject}Create = new(new {ValueObject}Validator());
        _create = new(_{valueObject}Create, _validator);
        _{command} = new(_validator);
    }

    private {Aggregate} Create{Aggregate}()
    {
        return _create.Execute(new Create{Aggregate}Command(...));
    }

    [Fact]
    public void Execute_With{Scenario}_{ExpectedResult}()
    {
        var {aggregate} = Create{Aggregate}();

        var result = _{command}.Execute({aggregate}, new {Command}Command(...));

        result.{Property}.Should().Be({expected});
    }
}
```

---

## ⛔ PROHIBIDO

```csharp
// ❌ NUNCA usar Testable en tests de comandos
var plan = new TestablePlan(Guid.NewGuid());
plan.SetName("Test");

// ❌ NUNCA instanciar entidades/VOs directamente
var feature = new Feature(...);
var money = new Money(...);
```

## ✅ CORRECTO

```csharp
// ✅ Usar comandos para crear instancias
var plan = _createPlan.Execute(new CreatePlanCommand(...));
var feature = _createFeature.Execute(new CreateFeatureCommand(...));
var money = _createMoney.Execute(new CreateMoneyCommand(...));
```
