# AI Generation Style Guide & Examples

Use this document to understand the coding style, patterns, and testing strategies used in this project. When generating new code, strictly follow these examples.

## 1. Domain Entity Style
**Pattern**: DDD with Rich Domain Models.
- Inherit from `Entity`.
- Private/Protected constructors.
- Static Factory Method (`Create`) returning `Result<T>`.
- Encapsulated collections (`_backingField` vs `IReadOnlyCollection`).
- Domain behaviors returning `Result`.
- Nested `FluentValidation` validator.

**Example**: `src/webapi/features/pizzas/models/Pizza.cs`
```csharp
using Fudie.Domain;
using FluentValidation;

namespace webapi.features.pizzas.models;

public class Pizza : Entity
{
    public string Name { get; protected set; }
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients.ToList().AsReadOnly();
    protected HashSet<Ingredient> _ingredients = [];

    protected Pizza(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Result<Pizza> Create(Guid id, string name)
    {
        var pizza = new Pizza(id, name);
        var validation = ValidateEntity(pizza, new PizzaValidator());
        return validation.IsFailure ? Result<Pizza>.Failure(validation.Errors) : Result<Pizza>.Success(pizza);
    }

    protected class PizzaValidator : AbstractValidator<Pizza>
    {
        public PizzaValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
```

## 2. Domain Unit Test Style
**Pattern**: `xUnit` + `FluentAssertions`.
- Test Factory Methods and Business Logic.
- Map "Example Mapping" scenarios 1:1 to Tests.
- Check `IsSuccess` / `IsFailure` and Error messages.

**Example**: `tests/WebApi.UnitTests/Features/Pizzas/PizzaTests.cs`
```csharp
public class PizzaTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange & Act
        var result = Pizza.Create(Guid.NewGuid(), "Margarita");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Margarita");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNameIsEmpty()
    {
        // Act
        var result = Pizza.Create(Guid.NewGuid(), "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Validation");
    }
}
```

## 3. Feature Slice Style (Vertical Slice)
**Pattern**: REPR (Request-Endpoint-Response) with Nested Classes.
- Implement `IFeatureModule`.
- Define `Request`, `Response` records.
- `AddRoutes` method for Minimal API mapping.
- `IService` interface and `Service` implementation (Business Logic).
- `[Injectable]` attribute for DI.
- Use `Result` pattern extensions (`ValueOrThrow`, `SuccessOrThrow`).
- Use `WithStandardOpenApi` extension for Swagger documentation.

**Example**: `src/webapi/features/pizzas/commands/CreatePizza.cs`
```csharp
public class CreatePizza : IFeatureModule
{
    public record Request(string Name, IEnumerable<Guid> Ingredients);
    public record Response(Guid Id, string Name);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/pizzas", async (IService service, Request request) =>
        {
            var response = await service.HandlerAsync(request);
            return Results.Created("", response);
        })
        .WithStandardOpenApi<Response>(
             name: "CreatePizza",
             summary: "Crear una nueva pizza",
             description: "Endpoint para crear una nueva pizza...",
             tag: "Pizzas",
             successStatusCode: StatusCodes.Status201Created
        );
    }

    public interface IService { Task<Response> HandlerAsync(Request request); }

    [Injectable]
    public class Service(IAdd<Pizza> repository, IUnitOfWork uow) : IService
    {
        public async Task<Response> HandlerAsync(Request request)
        {
            var pizza = Pizza.Create(Guid.NewGuid(), request.Name).ValueOrThrow();
            repository.Add(pizza);
            await uow.SaveChangesAsync();
            return new Response(pizza.Id, pizza.Name);
        }
    }
}
```

## 3. Integration Test Style
**Pattern**: `WebApplicationFactory` with `xUnit` and `FluentAssertions`.
- Use `IClassFixture<WebApplicationFactory<Program>>`.
- Configure `DbContext` to use `InMemoryDatabase` with a unique name per test class/run to ensure isolation.
- Use `HttpClient` with `PostAsJsonAsync` / `ReadFromJsonAsync`.
- Assert using `FluentAssertions` (`Should()`).

**Example**: `tests/WebApi.IntegrationTests/Features/Ingredients/CreateIngredientTests.cs`
```csharp
public class CreateIngredientTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CreateIngredientTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace existing DbContext with InMemory for testing
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                var databaseName = "TestDatabase_" + Guid.NewGuid();
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
                
                // Re-register repositories if needed
                services.AddScoped<IRepository>(sp => sp.GetRequiredService<ApplicationDbContext>());
                services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateIngredient_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateIngredient.Request("Tomato", 1.5m);

        // Act
        var response = await _client.PostAsJsonAsync("/ingredients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateIngredient.Response>();
        result.Should().NotBeNull();
        result.Name.Should().Be("Tomato");
    }
}
```

## 4. Persistence Configuration Style
**Pattern**: Entity Framework Core `IEntityTypeConfiguration<T>`.
- Implement `IEntityTypeConfiguration<T>`.
- Configure table name, keys, properties, and relationships.
- Use `builder.HasKey`, `builder.Property`, `builder.HasMany`, etc.

**Example**: `src/webapi/infrastructure/Configurations/PizzaConfiguration.cs` (Hypothetical)
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using webapi.features.pizzas.models;

namespace webapi.infrastructure.Configurations;

public class PizzaConfiguration : IEntityTypeConfiguration<Pizza>
{
    public void Configure(EntityTypeBuilder<Pizza> builder)
    {
        builder.ToTable("Pizzas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(x => x.Ingredients)
            .WithMany(); // Or specific relationship configuration
    }
}
```
