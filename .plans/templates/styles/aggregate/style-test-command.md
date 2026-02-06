# Estilo: Test de Comandos de Aggregate/Entity

## Principio

- **Arrange**: Preparar estado con `Testable{Aggregate}` → sin cadenas de comandos
- **Act**: Ejecutar UN solo comando resuelto por `DomainFixture`
- **Assert**: Verificar estado nuevo válido, `ValidationException`, o guards (`KeyNotFoundException`, `ConflictException`)

---

## Test de Comando Create

El Create no necesita `Testable` porque no hay estado previo.

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.Commands.{Aggregate}Tests;

public class {Aggregate}CreateTests(DomainFixture fixture)
{
    private readonly {Aggregate}.Create _create = fixture.Get<{Aggregate}.Create>();

    [Fact]
    public void Execute_WithValidCommand_Returns{Aggregate}()
    {
        var command = new Create{Aggregate}Command({validParam1}, {validParam2});

        var result = _create.Execute(command);

        result.{Property1}.Should().Be({validParam1});
        result.{Property2}.Should().Be({validParam2});
    }

    [Fact]
    public void Execute_With{InvalidCase}_ThrowsValidationException()
    {
        var command = new Create{Aggregate}Command({invalidParam1}, {invalidParam2});

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{{{Aggregate}ValidationMessages.{Property}{Rule}}}*");
    }
}
```

---

## Test de Comando que modifica estado

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.Commands.{Aggregate}Tests;

public class {Aggregate}{Command}Tests(DomainFixture fixture)
{
    private readonly {Aggregate}.{Command} _{command} = fixture.Get<{Aggregate}.{Command}>();

    [Fact]
    public void Execute_WithValidCommand_{ExpectedResult}()
    {
        var aggregate = new Testable{Aggregate}(Guid.NewGuid())
            .With{Property}({value})
            .With{Item}(new {ValueObject}({param1}, {param2}));

        var command = new {Command}{Aggregate}Command({newValue});

        var result = _{command}.Execute(aggregate, command);

        result.{Property}.Should().Be({expected});
    }

    [Fact]
    public void Execute_With{InvalidCase}_ThrowsValidationException()
    {
        var aggregate = new Testable{Aggregate}(Guid.NewGuid())
            .With{Property}({value});

        var command = new {Command}{Aggregate}Command({invalidValue});

        var act = () => _{command}.Execute(aggregate, command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{{{Aggregate}ValidationMessages.{Property}{Rule}}}*");
    }
}
```

---

## Test de Guards

```csharp
[Fact]
public void Execute_When{ResourceNotFound}_ThrowsKeyNotFoundException()
{
    var aggregate = new Testable{Aggregate}(Guid.NewGuid());
    // Sin agregar el item que el comando espera encontrar

    var command = new {Command}{Aggregate}Command({value});

    var act = () => _{command}.Execute(aggregate, command);

    act.Should().Throw<KeyNotFoundException>()
        .WithMessage("*{expectedMessage}*");
}

[Fact]
public void Execute_When{ConflictCondition}_ThrowsConflictException()
{
    var aggregate = new Testable{Aggregate}(Guid.NewGuid())
        .With{Property}({conflictingValue});

    var command = new {Command}{Aggregate}Command({value});

    var act = () => _{command}.Execute(aggregate, command);

    act.Should().Throw<ConflictException>()
        .WithMessage("*{expectedMessage}*");
}
```

---

## Reglas

- Namespace: `{Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.Commands.{Aggregate}Tests`
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- **Siempre usar `DomainFixture`** para resolver el comando bajo test
- **Siempre usar `Testable{Aggregate}`** para preparar estado en Arrange
- **NO encadenar comandos** para crear estado previo
- **UN solo comando** en el Act
- Clase: `{Aggregate}{Command}Tests`
- Nomenclatura de tests:
  - `Execute_WithValidCommand_{ExpectedResult}`
  - `Execute_With{InvalidCase}_ThrowsValidationException`
  - `Execute_When{ResourceNotFound}_ThrowsKeyNotFoundException`
  - `Execute_When{ConflictCondition}_ThrowsConflictException`
- Usar FluentAssertions

---

## ⛔ PROHIBIDO

- **NO encadenar comandos** en el Arrange para crear estado
- **NO usar `new {Aggregate}(...)`** directamente → Siempre `Testable{Aggregate}`
- **NO hacer `new` de validators** ni de comandos → `DomainFixture`
