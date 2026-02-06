# Estilo: Test Unitario de Slice

## Alcance

Testea desde el Handler hasta el Repository (mock).

- **Se testea**: Handler, Service, lógica, excepciones
- **Se mockea**: IRepository, IUnitOfWork, IEntityLookup
- **Se instancia real**: Comandos de dominio y Validators (resueltos por `DomainFixture`)

---

## Slice de Creación

No necesita `Testable` porque no hay estado previo.

```csharp
namespace {Project}.UnitTests.{Feature}.Api.{Aggregate}AggregateTests.Commands;

public class Create{Aggregate}Tests(DomainFixture fixture)
{
    private readonly Mock<Create{Aggregate}.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Create{Aggregate}.Service _service;

    public Create{Aggregate}Tests()
    {
        _service = new Create{Aggregate}.Service(
            fixture.Get<{Aggregate}.Create>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var request = new Create{Aggregate}.Request({validParam1}, {validParam2});

        var response = await _service.HandleAsync(request);

        response.{Property}.Should().Be({expected});
        _repository.Verify(r => r.Add(It.IsAny<{Aggregate}>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidData_ThrowsValidationException()
    {
        var request = new Create{Aggregate}.Request({invalidParam1}, {invalidParam2});

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{{{Aggregate}ValidationMessages.{Property}{Rule}}}*");
    }
}
```

---

## Slice que opera sobre estado existente

Usa `Testable{Aggregate}` para preparar el estado que devuelve el repository mock.

```csharp
namespace {Project}.UnitTests.{Feature}.Api.{Aggregate}AggregateTests.Commands;

public class {Action}{Aggregate}Tests(DomainFixture fixture)
{
    private readonly Mock<{Action}{Aggregate}.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly {Action}{Aggregate}.Service _service;

    public {Action}{Aggregate}Tests()
    {
        _service = new {Action}{Aggregate}.Service(
            fixture.Get<{Aggregate}.{Action}>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_{ExpectedResult}()
    {
        var aggregateId = Guid.NewGuid();
        var existing = new Testable{Aggregate}(aggregateId)
            .With{Property}({value})
            .With{Item}(new {ValueObject}({param1}, {param2}));

        _repository.Setup(r => r.Get(aggregateId)).ReturnsAsync(existing);

        var request = new {Action}{Aggregate}.Request({newValue});

        var response = await _service.HandleAsync(aggregateId, request);

        response.{Property}.Should().Be({expected});
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidData_ThrowsValidationException()
    {
        var aggregateId = Guid.NewGuid();
        var existing = new Testable{Aggregate}(aggregateId)
            .With{Property}({value});

        _repository.Setup(r => r.Get(aggregateId)).ReturnsAsync(existing);

        var request = new {Action}{Aggregate}.Request({invalidValue});

        var act = () => _service.HandleAsync(aggregateId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{{{Aggregate}ValidationMessages.{Property}{Rule}}}*");
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var aggregateId = Guid.NewGuid();
        _repository.Setup(r => r.Get(aggregateId))
            .ThrowsAsync(new KeyNotFoundException());

        var request = new {Action}{Aggregate}.Request({value});

        var act = () => _service.HandleAsync(aggregateId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_When{ConflictCondition}_ThrowsConflictException()
    {
        var aggregateId = Guid.NewGuid();
        var existing = new Testable{Aggregate}(aggregateId)
            .With{Property}({conflictingValue});

        _repository.Setup(r => r.Get(aggregateId)).ReturnsAsync(existing);

        var request = new {Action}{Aggregate}.Request({value});

        var act = () => _service.HandleAsync(aggregateId, request);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
```

---

## Handler (opcional)

Si extrajiste el Handler como delegate:

```csharp
[Fact]
public async Task Handler_ReturnsCreatedWithLocation()
{
    var mockService = new Mock<Create{Aggregate}.IService>();
    mockService.Setup(s => s.HandleAsync(It.IsAny<Create{Aggregate}.Request>()))
        .ReturnsAsync(new Create{Aggregate}.Response("123", ...));

    var request = new Create{Aggregate}.Request(...);

    var result = await Create{Aggregate}.Handler(mockService.Object, request);

    var created = result.Should().BeOfType<Created<Create{Aggregate}.Response>>().Subject;
    created.Location.Should().Be("/{aggregates}/123");
}
```

---

## Reglas

- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- **Siempre usar `DomainFixture`** para resolver comandos y validators
- **NO hacer `new` de validators** ni de comandos manualmente
- **NO montar el grafo de dependencias a mano**
- **Usar `Testable{Aggregate}`** para preparar estado en slices que operan sobre estado existente
- **No testear status codes** → Solo excepciones
- **Repository y UnitOfWork** → Mock
- Nomenclatura: `HandleAsync_{Scenario}_{ExpectedResult}`

---

## Excepciones del dominio

| Guard | Excepción | Cuándo |
|-------|-----------|--------|
| ValidationGuard | `ValidationException` | Validación de datos fallida |
| ConflictGuard | `ConflictException` | Estado inválido (duplicado, ya activo, etc.) |
| NotFoundGuard | `KeyNotFoundException` | Entidad no encontrada |

Estas son las ÚNICAS excepciones que el dominio lanza. No inventes otras.

---

## ⛔ PROHIBIDO

- **NO encadenar comandos** para crear estado previo en el Arrange
- **NO hacer `new {Aggregate}(...)`** directamente → Siempre `Testable{Aggregate}`
- **NO hacer `new` de validators o comandos** → `DomainFixture`
