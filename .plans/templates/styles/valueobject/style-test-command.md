# Estilo: Test de Comandos de Value Object

## DomainFixture

```csharp
namespace {Project}.UnitTests.Helpers;

public class DomainFixture
{
    public IServiceProvider ServiceProvider { get; }

    public DomainFixture()
    {
        var services = new ServiceCollection();
        var assembly = typeof({AggregateRoot}).Assembly;
        services.AddDomainCommands(assembly);
        ServiceProvider = services.BuildServiceProvider();
    }

    public T Get<T>() where T : class => ServiceProvider.GetRequiredService<T>();
}
```

Registra todos los comandos y validators del assembly. Resuelve automáticamente el grafo completo de dependencias (comandos anidados + validators).

---

## Test de Comando Create

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.ValueObjectsTests;

public class {ValueObject}CreateTests(DomainFixture fixture)
{
    private readonly {ValueObject}.Create _create = fixture.Get<{ValueObject}.Create>();

    [Fact]
    public void Execute_WithValidCommand_Returns{ValueObject}()
    {
        var command = new Create{ValueObject}Command({validParam1}, {validParam2});

        var result = _create.Execute(command);

        result.{Property1}.Should().Be({validParam1});
        result.{Property2}.Should().Be({validParam2});
    }

    [Fact]
    public void Execute_With{InvalidCase}_ThrowsValidationException()
    {
        var command = new Create{ValueObject}Command({invalidParam1}, {invalidParam2});

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{{{ValueObject}ValidationMessages.{Property}{Rule}}}*");
    }
}
```

---

## Test de Comando Transform con Comando

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.ValueObjectsTests;

public class {ValueObject}{Action}Tests(DomainFixture fixture)
{
    private readonly {ValueObject}.{Action} _{action} = fixture.Get<{ValueObject}.{Action}>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsTransformed{ValueObject}()
    {
        var original = new {ValueObject}({validParam1}, {validParam2});
        var command = new {Action}{ValueObject}Command({newValue});

        var result = _{action}.Execute(original, command);

        result.{Property}.Should().Be({newValue});
    }

    [Fact]
    public void Execute_PreservesUnchangedProperties()
    {
        var original = new {ValueObject}({validParam1}, {validParam2});
        var command = new {Action}{ValueObject}Command({newValue});

        var result = _{action}.Execute(original, command);

        result.{OtherProperty}.Should().Be(original.{OtherProperty});
    }

    [Fact]
    public void Execute_WithInvalidValue_ThrowsValidationException()
    {
        var original = new {ValueObject}({validParam1}, {validParam2});
        var command = new {Action}{ValueObject}Command({invalidValue});

        var act = () => _{action}.Execute(original, command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{{{ValueObject}ValidationMessages.{Property}{Rule}}}*");
    }

    [Fact]
    public void Execute_ReturnsNewInstance()
    {
        var original = new {ValueObject}({validParam1}, {validParam2});
        var command = new {Action}{ValueObject}Command({newValue});

        var result = _{action}.Execute(original, command);

        result.Should().NotBeSameAs(original);
    }
}
```

---

## Test de Comando Transform sin Comando

```csharp
namespace {Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.ValueObjectsTests;

public class {ValueObject}{Action}Tests(DomainFixture fixture)
{
    private readonly {ValueObject}.{Action} _{action} = fixture.Get<{ValueObject}.{Action}>();

    [Fact]
    public void Execute_ReturnsTransformed{ValueObject}()
    {
        var original = new {ValueObject}({validParam1}, {validParam2});

        var result = _{action}.Execute(original);

        result.{Property}.Should().Be({expectedValue});
    }
}
```

---

## Reglas

- Namespace: `{Project}.UnitTests.{Feature}.Domain.{Aggregate}AggregateTests.ValueObjectsTests`
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- **Siempre usar `DomainFixture`** para resolver comandos y validators
- **NO hacer `new` de validators** ni de comandos manualmente
- **NO montar el grafo de dependencias a mano**
- Crear instancias de VOs directamente con `new` (constructor público)
- **NO usar `Testable`** para Value Objects
- Clase: `{ValueObject}CreateTests` para Create, `{ValueObject}{Action}Tests` para Transform
- Nomenclatura de tests:
  - `Execute_WithValidCommand_Returns{ValueObject}`
  - `Execute_With{InvalidCase}_ThrowsValidationException`
  - `Execute_PreservesUnchangedProperties`
  - `Execute_ReturnsNewInstance`
- Usar FluentAssertions
