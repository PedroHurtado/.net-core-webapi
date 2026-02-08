# Estilo: Test de Integración de Slice

## Filosofía

Los integration tests son la **fuente de la verdad**. Verifican el pipeline completo: HTTP → routing → validation → handler → service → domain → persistence → response.

**Read-after-write**: Toda mutación (create, update, delete) se verifica con un GET posterior para confirmar que la persistencia es real. No confiar solo en el status code de la respuesta.

---

## Fixture

Heredar de `{Project}WebApplicationFixture`. Un fixture por proyecto.

```csharp
public class {Project}WebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;

    public {Project}WebApplicationFixture(WebApplicationFactory<Program> factory)
    {
        Client = factory.CreateClient();
    }
}
```

### Helper de creación en el fixture

Cada agregado expone un helper `Create{Aggregate}Async` que crea una entidad válida y devuelve la response. Los tests de otras slices lo usan para preparar estado.

```csharp
protected async Task<CustomerResponse> CreateCustomerAsync(string slug = "test-slug")
{
    var request = new Create{Aggregate}.Request({validParams});
    var response = await Client.PostAsJsonAsync("/{route}", request);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    return (await response.Content.ReadFromJsonAsync<{Aggregate}Response>())!;
}
```

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
    var request = new Create{Aggregate}.Request({params});

    var response = await Client.PostAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var created = await response.Content.ReadFromJsonAsync<{Aggregate}Response>();
    created!.{Property}.Should().Be({expected});

    // Read-after-write: verificar persistencia real
    var getResponse = await Client.GetAsync("/{route}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var persisted = await getResponse.Content.ReadFromJsonAsync<{Aggregate}Response>();
    persisted!.{Property}.Should().Be({expected});
}
```

### 200 OK (PUT/PATCH con response) — con read-after-write

```csharp
[Fact]
public async Task Update_WithValidData_Returns200AndPersistsChanges()
{
    await Create{Aggregate}Async();
    var request = new Update{Aggregate}.Request({newValues});

    var response = await Client.PutAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    // Read-after-write
    var getResponse = await Client.GetAsync("/{route}");
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
    await Create{Aggregate}Async();
    var request = new {Action}{Aggregate}.Request({values});

    var response = await Client.PutAsJsonAsync("/{route}", request);

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Read-after-write
    var getResponse = await Client.GetAsync("/{route}");
    var persisted = await getResponse.Content.ReadFromJsonAsync<{Aggregate}Response>();
    persisted!.{Property}.Should().Be({expected});
}

// Delete
[Fact]
public async Task Delete_WithExistingEntity_Returns204AndIsGone()
{
    await Create{Aggregate}Async();

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
    var created = await Create{Aggregate}Async();

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
[Fact]
public async Task Create_WithDuplicateSlug_Returns409()
{
    await Create{Aggregate}Async(slug: "duplicate");

    var request = new Create{Aggregate}.Request({paramsWithSlug: "duplicate"});
    var response = await Client.PostAsJsonAsync("/{route}", request);

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
| **Update/Mutación con response** | 200 + read-after-write, 404, 422 |
| **Update/Mutación void** | 204 + read-after-write, 404, 422 |
| **Delete/Remove** | 204 + read-after-write (GET→404), 404 |

### 422: qué validar

No testear cada campo individualmente. Un solo test con el campo más obvio vacío es suficiente para verificar que el pipeline de validación funciona. La cobertura exhaustiva de validaciones ya está en los domain unit tests.

---

## Namespace y ubicación

```
tests/{Project}.IntegrationTests/Features/{Feature}/Api/{Aggregate}Aggregate/Commands/{Action}{Aggregate}Tests.cs
tests/{Project}.IntegrationTests/Features/{Feature}/Api/{Aggregate}Aggregate/Queries/{Action}{Aggregate}Tests.cs
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
```

---

## Reglas

- No `using` en archivos de test → `GlobalUsings.cs`
- Un fixture por proyecto con helpers para crear entidades
- No DbContext directo → siempre a través del HttpClient
- Testear status codes, no excepciones
- **Read-after-write en toda mutación** → GET posterior para verificar persistencia real
- Usar helpers del fixture para preparar estado previo
- Nomenclatura: `{Action}_{Scenario}_Returns{StatusCode}`
- 422: un test representativo, no uno por cada campo
- No verificar response body en 4xx → solo status code
