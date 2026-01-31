# Estilo: Test Unitario de Slice

## Alcance

Testea desde el Handler hasta el Repository (mock).

- **Se testea**: Handler, Service, lógica, excepciones
- **Se mockea**: IRepository, IUnitOfWork, IEntityLookup
- **Se instancia real**: Comandos de dominio, Validators

---

## Estructura

```csharp
namespace {Project}.UnitTests.{Feature}.Api.{Aggregate}AggregateTests.{Commands|Queries};

public class {Action}{Aggregate}Tests
{
    private readonly Mock<{Action}{Aggregate}.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly {Action}{Aggregate}.Service _service;

    public {Action}{Aggregate}Tests()
    {
        var validator = new {Aggregate}Validator();
        var create = new {Aggregate}.Create(validator);
        
        _service = new {Action}{Aggregate}.Service(
            create,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    [Fact]
    public async Task HandleAsync_{Scenario}_{ExpectedResult}()
    {
        // Arrange
        var request = new {Action}{Aggregate}.Request(...);

        // Act
        var response = await _service.HandleAsync(request);

        // Assert
        response.{Property}.Should().Be({expected});
        _repository.Verify(r => r.Add(It.IsAny<{Aggregate}>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
```

---

## Qué testear

### Casos exitosos
- Retorna Response con datos correctos
- Llama a repository (Add/Get/Remove)
- Llama a SaveChangesAsync

### Excepciones (NO status codes)

```csharp
[Fact]
public async Task HandleAsync_WithInvalidData_ThrowsValidationException()
{
    var request = new CreateAllergen.Request("", "Name"); // Code vacío

    var act = () => _service.HandleAsync(request);

    await act.Should().ThrowAsync<ValidationException>()
        .WithMessage("*Code*required*");
}

[Fact]
public async Task HandleAsync_WithDuplicate_ThrowsConflictException()
{
    // Arrange: setup que causa conflicto

    var act = () => _service.HandleAsync(request);

    await act.Should().ThrowAsync<ConflictException>();
}

[Fact]
public async Task HandleAsync_WithNotFound_ThrowsKeyNotFoundException()
{
    _repository.Setup(r => r.Get(It.IsAny<string>()))
        .ThrowsAsync(new KeyNotFoundException());

    var act = () => _service.HandleAsync("non-existent-id", request);

    await act.Should().ThrowAsync<KeyNotFoundException>();
}
```

---

## Handler (opcional)

Si extrajiste el Handler como delegate:

```csharp
[Fact]
public async Task Handler_ReturnsCreatedWithLocation()
{
    var mockService = new Mock<CreateAllergen.IService>();
    mockService.Setup(s => s.HandleAsync(It.IsAny<CreateAllergen.Request>()))
        .ReturnsAsync(new CreateAllergen.Response("123", ...));

    var request = new CreateAllergen.Request(...);

    var result = await CreateAllergen.Handler(mockService.Object, request);

    var created = result.Should().BeOfType<Created<CreateAllergen.Response>>().Subject;
    created.Location.Should().Be("/allergens/123");
}
```

---

## Reglas

- **No `using`** → Van en `GlobalUsings.cs`
- **No usar `Testable`** → Usar comandos reales
- **No testear status codes** → Solo excepciones
- **Validators se instancian** → `new {Type}Validator()`
- **Repository y UnitOfWork** → Mock
- Nomenclatura: `HandleAsync_{Scenario}_{ExpectedResult}`


### Excepciones del dominio

| Guard | Excepción | Cuándo |
|-------|-----------|--------|
| ValidationGuard | `ValidationException` | Validación de datos fallida |
| ConflictGuard | `ConflictException` | Estado inválido (duplicado, ya activo, etc.) |
| NotFoundGuard | `KeyNotFoundException` | Entidad no encontrada |

Estas son las ÚNICAS excepciones que el dominio lanza. No inventes otras.
