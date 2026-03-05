# Estilo: Test Unitario de Slice

## Filosofía

El test verifica la **orquestación real**: request → comando de dominio (real) → mutación de entidad → response map (real).

- **Se mockea**: IRepository, IUnitOfWork, IQuery
- **Se ejecuta real**: Comandos de dominio (vía `DomainFixture`), `{Aggregate}Response.Map()`, operadores LINQ (OrderBy, Include, ToListAsync)
- **NO se re-testea**: Validaciones de dominio, 404 del repositorio, excepciones del dominio

### Qué tests escribir

| Tipo de slice | Tests |
|---|---|
| **Todas** | Happy path |
| **Con ConflictGuard propio** (ej: ExistsBySlug) | + Conflict test |
| **Con lógica condicional** (ej: `if slug != request.Slug`) | + Branch alternativo |
| **Query lista (IQuery)** | Happy path (con datos ordenados) + Lista vacía |

---

## Antes de escribir tests: qué leer

1. **`Testable{Aggregate}.cs`** → Conocer los `.With*()` disponibles y qué campos son requeridos
2. **Service constructor de cada slice** → Conocer las dependencias (comando, tenantId, repository, unitOfWork)
3. **`HandleAsync` de cada slice** → Conocer la firma (route params, request, void vs response)

Solo esos 3 puntos. No leer validators, domain commands ni handlers.

---

## Constructor del test

Constructor tradicional con `IClassFixture<DomainFixture>`. NO primary constructor.

```csharp
public class {Action}{Aggregate}Tests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<{Action}{Aggregate}.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly {Action}{Aggregate}.Service _service;

    public {Action}{Aggregate}Tests(DomainFixture fixture)
    {
        _service = new {Action}{Aggregate}.Service(
            fixture.Get<{Aggregate}.{Action}>(),
            _tenantId,
            _repository.Object,
            _unitOfWork.Object);
    }
}
```

**Queries por ID (IRepository)** no necesitan `DomainFixture`:

```csharp
public class Get{Aggregate}Tests
{
    private readonly Mock<Get{Aggregate}.IRepository> _repository = new();
    private readonly Get{Aggregate}.Service _service;

    public Get{Aggregate}Tests()
    {
        _service = new Get{Aggregate}.Service(_repository.Object);
    }
}
```

**Queries lista (IQuery)** no necesitan `DomainFixture`. Se mockea `IQuery` y se usa `AsyncQueryable` de `Fudie.Tests.Common` para proveer `IAsyncQueryProvider`:

```csharp
public class Get{Aggregate}sTests
{
    private readonly Mock<IQuery> _query = new();
    private readonly Get{Aggregate}s.Service _service;

    public Get{Aggregate}sTests()
    {
        _service = new Get{Aggregate}s.Service(_query.Object);
    }
}
```

---

## Testable{Aggregate}: SIEMPRE completo

Los comandos de dominio validan la **entidad completa** antes de ejecutar. Un `Testable{Aggregate}` con campos faltantes lanza `ValidationException`.

**Regla**: Leer `Testable{Aggregate}.cs`, identificar todos los campos requeridos del agregado (los que el validador marca como obligatorios), y configurarlos TODOS en cada test. Esto incluye **enums con `IsInEnum()`** — no confiar en el default `0` del enum, siempre usar `.With{Enum}()` explícito. Después añadir el estado específico del test.

```csharp
// 1. Todos los campos requeridos del agregado
var entity = new Testable{Aggregate}(_tenantId)
    .With{Campo1}(...)
    .With{Campo2}(...)
    .With{CampoN}(...);

// 2. Estado específico del test
    .With{EstadoPrevio}(...);
```

Si un comando tiene precondiciones especiales (ej: Activate requiere perfil completo), añadir los campos extra que satisfagan esa precondición.

---

## Patrones por tipo de slice

### Creación (no necesita entidad previa)

```csharp
[Fact]
public async Task HandleAsync_WithValidRequest_ReturnsResponse()
{
    var request = new Create{Aggregate}.Request({params});

    var response = await _service.HandleAsync(request);

    response.Id.Should().Be(_tenantId);
    response.{Property}.Should().Be({expected});
}
```

### Void (NoContent) — verificar mutación en la entidad

```csharp
[Fact]
public async Task HandleAsync_WithValidRequest_{MutationDescription}()
{
    var entity = new Testable{Aggregate}(_tenantId)
        .With{AllRequired}()
        .With{SpecificState}();

    _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(entity);

    await _service.HandleAsync(request);

    entity.{Property}.Should().Be({expected});
}
```

### Response — verificar el Map real

```csharp
[Fact]
public async Task HandleAsync_WithValidRequest_ReturnsResponseWith{State}()
{
    var entity = new Testable{Aggregate}(_tenantId)
        .With{AllRequired}()
        .With{SpecificState}();

    _repository.Setup(r => r.Get(_tenantId)).ReturnsAsync(entity);

    var response = await _service.HandleAsync(request);

    response.{Property}.Should().Be({expected});
}
```

### Query por ID (IRepository) — mock del repositorio retorna entidad

```csharp
[Fact]
public async Task HandleAsync_WithExistingEntity_ReturnsResponse()
{
    var entity = new Testable{Aggregate}(Guid.NewGuid())
        .With{AllRequired}();

    _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

    var response = await _service.HandleAsync(entity.Id);

    response.{Property}.Should().Be({expected});
}
```

### Query lista (IQuery + AsyncQueryable) — mock de IQuery retorna colección async

`AsyncQueryable` (de `Fudie.Tests.Common`) provee `IAsyncQueryProvider` para que `ToListAsync`, `Include`, `OrderBy`, etc. funcionen en tests sin EF Core real. `Include` es no-op (EF Core 8 lo ignora para providers no-EF).

```csharp
[Fact]
public async Task HandleAsync_WithEntities_ReturnsOrderedList()
{
    var entity1 = new Testable{Aggregate}(Guid.NewGuid())
        .With{AllRequired}()
        .With{FieldForOrder}("beta");

    var entity2 = new Testable{Aggregate}(Guid.NewGuid())
        .With{AllRequired}()
        .With{FieldForOrder}("alpha");

    _query.Setup(q => q.Query<{Aggregate}>())
        .Returns(AsyncQueryable.Of(entity1, entity2));

    var response = await _service.HandleAsync();

    response.Should().HaveCount(2);
    response[0].{FieldForOrder}.Should().Be("alpha");
    response[1].{FieldForOrder}.Should().Be("beta");
}

[Fact]
public async Task HandleAsync_WithNoEntities_ReturnsEmptyList()
{
    _query.Setup(q => q.Query<{Aggregate}>())
        .Returns(AsyncQueryable.Empty<{Aggregate}>());

    var response = await _service.HandleAsync();

    response.Should().BeEmpty();
}
```

**Métodos de `AsyncQueryable`**:
- `AsyncQueryable.Of(entity1, entity2, ...)` — colección con elementos (params)
- `AsyncQueryable.Of(enumerable)` — colección desde IEnumerable
- `AsyncQueryable.Empty<T>()` — colección vacía

### ConflictGuard (solo si la slice lo tiene)

```csharp
[Fact]
public async Task HandleAsync_When{Conflict}_ThrowsConflictException()
{
    _repository.Setup(r => r.{ConflictMethod}({value})).ReturnsAsync(true);

    var act = () => _service.HandleAsync(request);

    await act.Should().ThrowAsync<ConflictException>();
}
```

---

## Route params en HandleAsync

```csharp
await _service.HandleAsync(guidParam, request);
await _service.HandleAsync("stringParam", request);
await _service.HandleAsync(guidParam);       // sin body
await _service.HandleAsync("stringParam");   // sin body
```

---

## Colecciones en Response

`IReadOnlyCollection` NO soporta `[]`. Usar LINQ:

```csharp
response.Items.Should().HaveCount(1);
response.Items.First().Url.Should().Be("url");
response.Items.Single().Url.Should().Be("url");
```

---

## Helpers reutilizables

Si varios tests en la misma clase usan la misma entidad base, extraer un método helper:

```csharp
private Testable{Aggregate} CreateExisting{Aggregate}() => new Testable{Aggregate}(_tenantId)
    .With{AllRequired}();
```

Si varios tests usan el mismo request, extraer helper:

```csharp
private static {Action}{Aggregate}.Request CreateValidRequest(string slug = "default") =>
    new({params});
```

---

## Namespace y ubicación

```
tests/{Project}.UnitTests/Features/{Feature}/Api/{Aggregate}Aggregate/Commands/{Action}{Aggregate}Tests.cs
tests/{Project}.UnitTests/Features/{Feature}/Api/{Aggregate}Aggregate/Queries/{Action}{Aggregate}Tests.cs
```

---

## GlobalUsings a añadir

```csharp
global using Moq;
global using Fudie.Infrastructure;
global using Fudie.Tests.Common;
global using {Project}.Features.{Feature}.Api.{Aggregate}Aggregate;
global using {Project}.Features.{Feature}.Api.{Aggregate}Aggregate.Commands;
global using {Project}.Features.{Feature}.Api.{Aggregate}Aggregate.Queries;
```

---

## Test de seguridad en endpoint (AddRoutes)

Cada slice debe tener un test que verifique los extension methods de seguridad configurados en `AddRoutes`. Esto garantiza que no se olvide ni se configure mal la autorización.

### Patrón

```csharp
[Fact]
public void AddRoutes_ConfiguresEndpointCorrectly()
{
    var slice = new {Action}{Aggregate}();
    var builder = new TestEndpointRouteBuilder();

    slice.AddRoutes(builder);

    var endpoint = builder.DataSources
        .SelectMany(ds => ds.Endpoints)
        .OfType<RouteEndpoint>()
        .Single();

    endpoint.RoutePattern.RawText.Should().Be("{expectedRoute}");
    endpoint.Metadata.Should().ContainSingle(m => m is RequireAuthenticatedAttribute);
    endpoint.Metadata.Should().ContainSingle(m => m is RequireGroupAttribute attr && attr.Group == "{expectedGroup}");
    endpoint.Metadata.Should().ContainSingle(m => m is DescriptionCatalogAttribute attr && attr.Description == "{expectedDescription}");
}
```

### Endpoint público (AllowAnonymous)
```csharp
endpoint.Metadata.Should().ContainSingle(m => m is AllowAnonymousAttribute);
```

### Qué verificar según la spec

| Extension en AddRoutes | Assertion en metadata |
|---|---|
| `RequireAuthenticated()` | `RequireAuthenticatedAttribute` |
| `RequirePlatform()` | `RequirePlatformAttribute` |
| `RequireInternal()` | `RequireInternalAttribute` |
| `RequireGroup("name")` | `RequireGroupAttribute` con `.Group == "name"` |
| `.AllowAnonymous()` | `AllowAnonymousAttribute` |
| `WithDescriptionCatalog("desc")` | `DescriptionCatalogAttribute` con `.Description == "desc"` |

---

## Reglas

- No `using` en archivos de test → `GlobalUsings.cs`
- No XML docs
- `DomainFixture` para resolver comandos (excepto queries sin comando)
- NO `new` de validators ni comandos → `DomainFixture`
- `Testable{Aggregate}` SIEMPRE completo
- Repository, UnitOfWork e IQuery → Mock
- Queries lista con `IQuery` → usar `AsyncQueryable.Of(...)` / `AsyncQueryable.Empty<T>()`
- No verificar `SaveChangesAsync` ni `Repository.Add/Get`
- Nomenclatura: `HandleAsync_{Scenario}_{ExpectedResult}`
- NO re-testear validaciones de dominio, 404 ni excepciones del dominio
- NO primary constructor en la clase de test
- NO encadenar comandos para crear estado → `Testable{Aggregate}`
- NO `new {Aggregate}(...)` → `Testable{Aggregate}`
- NO `[0]` en `IReadOnlyCollection` → `.First()` o `.Single()`
