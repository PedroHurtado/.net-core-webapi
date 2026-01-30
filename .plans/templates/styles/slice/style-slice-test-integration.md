# Estilo: Test de Integración de Slice

## Fixture a heredar

`{Proyecto}WebApplicationFixture`
## Fixture a heredar

Heredar de `{Proyecto}WebApplicationFixture`, donde `{Proyecto}` es el indicado en la sección "Proyecto" del prompt de la tarea.

---

## Alcance

Testea el endpoint completo via HttpClient.

- **Se testea**: Status codes, respuestas HTTP, persistencia real
- **Se usa**: WebApplicationFactory, HttpClient, base de datos real/emulador

---

## Fixture

Crear un fixture por proyecto que configure:

- WebApplicationFactory con el Program
- HttpClient
- DbContext contra emulador o base de datos de test
- Métodos helper para crear entidades de test
```csharp
public class {Project}WebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public HttpClient Client { get; }
    public IServiceProvider Services => _factory.Services;

    public {Project}WebApplicationFixture(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Configurar DbContext para tests
                // Registrar servicios necesarios
            });
        });

        Client = _factory.CreateClient();
    }

    // Helpers para crear entidades
    public async Task<Create{Aggregate}.Response> Create{Aggregate}Async(...)
    {
        var request = new Create{Aggregate}.Request(...);
        var response = await Client.PostAsJsonAsync("/{route}", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Create{Aggregate}.Response>())!;
    }
}
```

---

## Estructura de Tests
```csharp
namespace {Project}.IntegrationTests.{Feature}.Api.{Aggregate}AggregateTests.{Commands|Queries};

public class {Action}{Aggregate}Tests : {Project}WebApplicationFixture
{
    public {Action}{Aggregate}Tests(WebApplicationFactory<Program> factory) 
        : base(factory) { }

    [Fact]
    public async Task {Action}_{Scenario}_Returns{StatusCode}()
    {
        // Arrange
        var request = new {Action}{Aggregate}.Request(...);

        // Act
        var response = await Client.{Method}AsJsonAsync("/{route}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.{Expected});
    }
}
```

---

## Qué testear (Status Codes)

### Create (POST)
```csharp
[Fact]
public async Task Create_WithValidData_Returns201()
{
    var request = new CreateAllergen.Request("GLUTEN", "Gluten");

    var response = await Client.PostAsJsonAsync("/allergens", request);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    response.Headers.Location.Should().NotBeNull();
}

[Fact]
public async Task Create_WithInvalidData_Returns422()
{
    var request = new CreateAllergen.Request("", "Name"); // Code vacío

    var response = await Client.PostAsJsonAsync("/allergens", request);

    response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
}

[Fact]
public async Task Create_WithDuplicate_Returns409()
{
    await CreateAllergenAsync(code: "DUPLICATE");

    var request = new CreateAllergen.Request("DUPLICATE", "Name");
    var response = await Client.PostAsJsonAsync("/allergens", request);

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
}
```

### Get (GET /{id})
```csharp
[Fact]
public async Task Get_WithExistingId_Returns200()
{
    var created = await CreateAllergenAsync();

    var response = await Client.GetAsync($"/allergens/{created.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public async Task Get_WithNonExistingId_Returns404()
{
    var response = await Client.GetAsync("/allergens/non-existent-id");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### Update (PUT)
```csharp
[Fact]
public async Task Update_WithValidData_Returns200()
{
    var created = await CreateAllergenAsync();
    var request = new UpdateAllergen.Request("Updated Name", null, true, 0);

    var response = await Client.PutAsJsonAsync($"/allergens/{created.Id}", request);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public async Task Update_WithNonExistingId_Returns404()
{
    var request = new UpdateAllergen.Request("Name", null, true, 0);

    var response = await Client.PutAsJsonAsync("/allergens/non-existent-id", request);

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}

[Fact]
public async Task Update_WithInvalidData_Returns422()
{
    var created = await CreateAllergenAsync();
    var request = new UpdateAllergen.Request("", null, true, 0); // Name vacío

    var response = await Client.PutAsJsonAsync($"/allergens/{created.Id}", request);

    response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
}
```

### Delete (DELETE)
```csharp
[Fact]
public async Task Delete_WithExistingId_Returns204()
{
    var created = await CreateAllergenAsync();

    var response = await Client.DeleteAsync($"/allergens/{created.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
}

[Fact]
public async Task Delete_WithNonExistingId_Returns404()
{
    var response = await Client.DeleteAsync("/allergens/non-existent-id");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### List (GET)
```csharp
[Fact]
public async Task List_ReturnsAllItems_Returns200()
{
    await CreateAllergenAsync();
    await CreateAllergenAsync();

    var response = await Client.GetAsync("/allergens");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var items = await response.Content.ReadFromJsonAsync<List<GetAllergens.Response>>();
    items.Should().HaveCountGreaterOrEqualTo(2);
}
```

---

## Resumen de Status Codes

| Escenario | Status Code |
|-----------|-------------|
| Operación exitosa (GET, PUT) | 200 OK |
| Creación exitosa | 201 Created |
| Eliminación exitosa | 204 No Content |
| Recurso no encontrado | 404 Not Found |
| Conflicto (duplicado, estado inválido) | 409 Conflict |
| Validación fallida | 422 Unprocessable Entity |

---

## Reglas
- **No `using`** → Van en `GlobalUsings.cs`
- **Un fixture por proyecto** con helpers para crear entidades
- **No Usar nunca DbContext** siempre a traves de los fixtures
- **Testear status codes** → No excepciones
- **Usar helpers del fixture** para crear datos de test
- Nomenclatura: `{Action}_{Scenario}_Returns{StatusCode}`