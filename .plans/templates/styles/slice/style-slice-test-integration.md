# Estilo: Test de Integración de Slice

## Filosofía

Los integration tests son la **fuente de la verdad**. Verifican el pipeline completo: HTTP → routing → validation → handler → service → domain → persistence → response.

**Read-after-write**: Toda mutación (create, update, delete) se verifica con un GET posterior para confirmar que la persistencia es real. No confiar solo en el status code de la respuesta. Esta regla aplica tanto a tests como a helpers.

**Aislamiento de datos**: Los tests deben ser idempotentes. Cada ejecución genera sus propios identificadores únicos para evitar colisiones con datos persistidos de ejecuciones anteriores.

---

## Fixture

Heredar de `{Project}WebApplicationFixture`. Un fixture por proyecto.

```csharp
public class {Project}WebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient Client;

    public {Project}WebApplicationFixture(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }
}
```

> `_factory` es `protected` para que los tests puedan crear clientes adicionales cuando sea necesario (ej: tests de conflicto cross-tenant).

---

## Helpers

### Helper base de creación

Extension method sobre `HttpClient` en un archivo separado bajo `Helpers/`. Crea la entidad mínima válida y devuelve el estado persistido via GET (read-after-write).

```csharp
// Helpers/Create{Aggregate}Helper.cs
public static class Create{Aggregate}Helper
{
    public static async Task<{Aggregate}Response> Create{Aggregate}Async(
        this HttpClient client, string? slug = null)
    {
        slug ??= $"test-{Guid.NewGuid():N}";

        var request = new Create{Aggregate}.Request({validParams con slug});

        var response = await client.PostAsJsonAsync("/{route}", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read-after-write: devolver datos del GET, no del POST
        var getResponse = await client.GetAsync("/{route}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await getResponse.Content.ReadFromJsonAsync<{Aggregate}Response>())!;
    }
}
```

Reglas del helper base:
- **Slug/ID dinámico por defecto**: `$"test-{Guid.NewGuid():N}"` para evitar colisiones entre ejecuciones
- **Parámetro opcional**: permite fijar el slug cuando el test lo necesita (ej: tests de 409)
- **Read-after-write**: devuelve el resultado del GET, nunca del POST/PUT

### Helpers compuestos

Cuando un test necesita un estado que requiere varias mutaciones (crear + actualizar + añadir imagen + activar), se crea un helper compuesto que encadena helpers existentes. Cada helper en su propio archivo.

```csharp
// Helpers/CreateComplete{Aggregate}Helper.cs
public static class CreateComplete{Aggregate}Helper
{
    public static async Task<{Aggregate}Response> CreateComplete{Aggregate}Async(
        this HttpClient client, string? slug = null)
    {
        var created = await client.Create{Aggregate}Async(slug);

        var updateRequest = new Update{Aggregate}.Request({completeData, Slug: created.Slug});
        var updateResponse = await client.PutAsJsonAsync("/{route}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Más mutaciones si es necesario...

        // Read-after-write: siempre devolver datos del GET
        var getResponse = await client.GetAsync("/{route}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await getResponse.Content.ReadFromJsonAsync<{Aggregate}Response>())!;
    }
}
```

### Identificar helpers antes de escribir tests

Antes de implementar, analizar los tests y listar qué estados previos necesitan. Cada estado distinto es un helper potencial:
- `Create{Aggregate}Async` — entidad mínima válida
- `Create{Aggregate}With{Sub}Async` — entidad + sub-entidad (imagen, link, etc.)
- `CreateComplete{Aggregate}Async` — entidad con perfil completo
- `CreateActive{Aggregate}Async` — entidad activa (completa + activada)

### Helper de rival client (tests cross-tenant)

Para tests de conflicto (409) que necesitan otra entidad en otro contexto/tenant:

```csharp
// Helpers/CreateRivalClientHelper.cs
public static class CreateRivalClientHelper
{
    public static HttpClient CreateRivalClient(this WebApplicationFactory<Program> factory)
    {
        var rivalTenantId = Guid.NewGuid();

        var rivalFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(typeof(Guid), _ => rivalTenantId);
            });
        });

        return rivalFactory.CreateClient();
    }
}
```

> Capturar el ID antes del registro para que todas las peticiones del rival usen el mismo valor.

---

## Antes de escribir tests: qué leer

1. **Handlers de cada slice** → Conocer rutas HTTP, métodos, y qué status code retorna cada uno
2. **Validators** → Conocer qué campos son requeridos para provocar 422
3. **Response** → Conocer los campos del response para los asserts del GET posterior

---

## Patrones por status code

### 201 Created (POST) — con read-after-write

```csharp
[Fact]
public async Task Create_WithValidData_Returns201AndPersistsData()
{
    var slug = $"mi-slug-{Guid.NewGuid():N}";

    var request = new Create{Aggregate}.Request({params con slug});

    var response = await Client.PostAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.Created);

    // Read-after-write: verificar persistencia real
    var getResponse = await Client.GetAsync("/{route}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var persisted = await getResponse.Content.ReadFromJsonAsync<{Aggregate}Response>();
    persisted!.{Property}.Should().Be({expected});
    persisted.Slug.Should().Be(slug);
}
```

### 200 OK (PUT/PATCH con response) — con read-after-write

```csharp
[Fact]
public async Task Update_WithValidData_Returns200AndPersistsChanges()
{
    await Client.Create{Aggregate}Async();
    var request = new Update{Aggregate}.Request({newValues});

    var response = await Client.PutAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    // Read-after-write
    var getResponse = await Client.GetAsync("/{route}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var persisted = await getResponse.Content.ReadFromJsonAsync<{Aggregate}Response>();
    persisted!.{UpdatedProperty}.Should().Be({expected});
}
```

### 204 NoContent (PUT/PATCH/DELETE sin response) — con read-after-write

```csharp
// Mutación void
[Fact]
public async Task {Action}_WithValidData_Returns204AndPersistsChanges()
{
    await Client.Create{Aggregate}Async();
    var request = new {Action}{Aggregate}.Request({values});

    var response = await Client.PutAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Read-after-write
    var getResponse = await Client.GetAsync("/{route}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var persisted = await getResponse.Content.ReadFromJsonAsync<{Aggregate}Response>();
    persisted!.{Property}.Should().Be({expected});
}

// Delete
[Fact]
public async Task Delete_WithExistingEntity_Returns204AndIsGone()
{
    await Client.Create{Aggregate}Async();

    var response = await Client.DeleteAsync("/{route}/{id}");

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Read-after-write: confirmar que ya no existe
    var getResponse = await Client.GetAsync("/{route}/{id}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### 200 OK (GET query)

```csharp
[Fact]
public async Task Get_WithExistingEntity_Returns200WithData()
{
    var created = await Client.Create{Aggregate}Async();

    var response = await Client.GetAsync("/{route}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadFromJsonAsync<{Aggregate}Response>();
    body!.{Property}.Should().Be({expected});
}
```

### 404 Not Found

```csharp
// GET sin entidad
[Fact]
public async Task Get_WithoutEntity_Returns404()
{
    var response = await Client.GetAsync("/{route}");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}

// Mutación sin entidad previa
[Fact]
public async Task {Action}_WithoutEntity_Returns404()
{
    var request = new {Action}{Aggregate}.Request({values});

    var response = await Client.PutAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### 409 Conflict

```csharp
// Mismo contexto (ej: Create con slug duplicado)
[Fact]
public async Task Create_WithDuplicateSlug_Returns409()
{
    var created = await Client.Create{Aggregate}Async();

    var request = new Create{Aggregate}.Request({params con slug: created.Slug});
    var response = await Client.PostAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
}

// Cross-tenant (ej: Update con slug que pertenece a otro tenant)
[Fact]
public async Task Update_WithDuplicateSlug_Returns409()
{
    var rivalClient = _factory.CreateRivalClient();
    var rival = await rivalClient.Create{Aggregate}Async();

    await Client.Create{Aggregate}Async();

    var request = new Update{Aggregate}.Request({params con slug: rival.Slug});
    var response = await Client.PutAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
}
```

### 422 Unprocessable Entity

```csharp
[Fact]
public async Task Create_WithInvalidData_Returns422()
{
    var request = new Create{Aggregate}.Request({invalidParams});

    var response = await Client.PostAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
}
```

---

## Qué tests escribir por slice

| Tipo de slice | Tests |
|---|---|
| **Create** | 201 + read-after-write, 422, 409 (si tiene ConflictGuard) |
| **Get** | 200, 404 |
| **Update/Mutación con response** | 200 + read-after-write, 404, 422, 409 (si tiene ConflictGuard) |
| **Update/Mutación void** | 204 + read-after-write, 404, 422, 409 (si tiene ConflictGuard) |
| **Delete/Remove** | 204 + read-after-write (GET→404), 404 |

### 422: qué validar

No testear cada campo individualmente. Un solo test con el campo más obvio vacío es suficiente para verificar que el pipeline de validación funciona. La cobertura exhaustiva de validaciones ya está en los domain unit tests.

---

## Namespace y ubicación

```
tests/{Project}.IntegrationTests/{Feature}/Api/{Aggregate}Aggregate/Commands/{Action}{Aggregate}Tests.cs
tests/{Project}.IntegrationTests/{Feature}/Api/{Aggregate}Aggregate/Queries/{Action}{Aggregate}Tests.cs
tests/{Project}.IntegrationTests/Helpers/Create{Aggregate}Helper.cs
tests/{Project}.IntegrationTests/Helpers/Create{Aggregate}With{Sub}Helper.cs
```

---

## GlobalUsings a añadir

```csharp
global using System.Net;
global using System.Net.Http.Json;
global using Microsoft.AspNetCore.Mvc.Testing;
global using {Project}.Features.{Feature}.Api.{Aggregate}Aggregate;
global using {Project}.Features.{Feature}.Api.{Aggregate}Aggregate.Commands;
global using {Project}.Features.{Feature}.Api.{Aggregate}Aggregate.Queries;
global using {Project}.IntegrationTests.Helpers;
```

---

## Reglas

- No `using` en archivos de test → `GlobalUsings.cs`
- Un fixture por proyecto, `_factory` protected, helpers fuera del fixture
- No DbContext directo → siempre a través del HttpClient
- Testear status codes, no excepciones
- **Read-after-write en toda mutación** → GET posterior para verificar persistencia real. Aplica a tests Y a helpers
- **Nunca devolver datos de una response de mutación** → siempre hacer GET y devolver ese resultado
- **Datos únicos por ejecución** → slugs e identificadores con `Guid.NewGuid()` para evitar colisiones en stores persistentes
- Helpers como extension methods sobre `HttpClient`, un archivo por helper
- Identificar los helpers necesarios antes de escribir tests
- Nomenclatura: `{Action}_{Scenario}_Returns{StatusCode}`
- 422: un test representativo, no uno por cada campo
- No verificar response body en 4xx → solo status code
