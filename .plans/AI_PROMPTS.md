# 🤖 Prompts Optimizados para Generación con IA

Este archivo contiene prompts listos para copiar y pegar, optimizados para cada paso del desarrollo.

---

## 📋 Índice de Prompts

1. [Prompt 1: Generar Dominio + Tests Unitarios](#prompt-1-generar-dominio--tests-unitarios)
2. [Prompt 2: Generar Persistencia](#prompt-2-generar-persistencia)
3. [Prompt 3: Generar Query Get por ID](#prompt-3-generar-query-get-por-id)
4. [Prompt 4: Generar Query Get Lista](#prompt-4-generar-query-get-lista)
5. [Prompt 5: Generar Command Create](#prompt-5-generar-command-create)
6. [Prompt 6: Generar Command Update](#prompt-6-generar-command-update)
7. [Prompt 7: Generar Command Delete](#prompt-7-generar-command-delete)
8. [Prompt 8: Generar Tests de Integración](#prompt-8-generar-tests-de-integración)
9. [Prompt 9: Revisar y Optimizar](#prompt-9-revisar-y-optimizar)

---

## Prompt 1: Generar Dominio + Tests Unitarios

**Cuándo usar**: Después de completar `domain-specs/[Entidad].md`

**Variables a reemplazar**:
- `[Entidad]`: Nombre de la entidad en PascalCase (ej: `Pedido`, `Cliente`)
- `[entidad]`: Nombre de la entidad en minúsculas (ej: `pedido`, `cliente`)

### 📋 Prompt

```
🎯 ROL: Arquitecto de Software .NET experto en DDD y Clean Architecture

📂 CONTEXTO DEL PROYECTO:
Estoy trabajando en un microservicio .NET 8 que sigue:
- Clean Architecture con Vertical Slices
📦 ENTREGABLES:

### 1. Clase de Dominio
**Archivo**: `src/webapi/features/[entidad]/models/[Entidad].cs`

**Requisitos obligatorios**:
- ✅ Hereda de `Entity` (namespace: `Fudie.Domain`)
- ✅ Constructor `protected` (no público)
- ✅ Factory method estático `Create()` que retorna `Result<[Entidad]>`
- ✅ Propiedades con `protected set` (inmutabilidad)
- ✅ Colecciones: backing field privado (`_items`) + propiedad pública `IReadOnlyCollection<T>`
- ✅ Propiedades calculadas: solo `get` (sin set)
- ✅ Métodos de comportamiento retornan `Result` (no void)
- ✅ Validador anidado: clase `protected class [Entidad]Validator : AbstractValidator<[Entidad]>`
- ✅ Validación en `Create()` usando `ValidateEntity(entity, new [Entidad]Validator())`
- ✅ Namespace: `webapi.features.[entidad].models`

**Ejemplo de estructura**:
```csharp
using Fudie.Domain;
using Fudie;
using FluentValidation;

namespace webapi.features.[entidad].models;

public class [Entidad] : Entity
{
    // Propiedades
    public string Name { get; protected set; }
    public IReadOnlyCollection<Item> Items => _items.ToList().AsReadOnly();
    protected HashSet<Item> _items = [];
    
    // Constructor protegido
    protected [Entidad](Guid id, string name) : base(id)
    {
        Name = name;
    }
    
    // Factory method
    public static Result<[Entidad]> Create(Guid id, string name)
    {
        var entity = new [Entidad](id, name);
        var validation = ValidateEntity(entity, new [Entidad]Validator());
        
        if (validation.IsFailure)
            return Result<[Entidad]>.Failure(validation.Errors);
        
        return Result<[Entidad]>.Success(entity);
    }
    
    // Métodos de comportamiento
    public Result AddItem(Item item)
    {
        // Validaciones de negocio
        if (item == null)
            return Result.Failure("Item no puede ser nulo", "Item");
        
        if (_items.Contains(item))
            return Result.Failure("Item ya existe", "Item");
        
        _items.Add(item);
        return Result.Success();
    }
    
    // Validador anidado
    protected class [Entidad]Validator : AbstractValidator<[Entidad]>
    {
        public [Entidad]Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("Máximo 100 caracteres");
        }
    }
}
```

### 2. Tests Unitarios
**Archivo**: `tests/WebApi.UnitTests/Features/[Entidad]/[Entidad]Tests.cs`

**Requisitos obligatorios**:
- ✅ Usa xUnit (`[Fact]`, `[Theory]`)
- ✅ Usa FluentAssertions (`.Should()`)
- ✅ Mapea 1:1 los ejemplos del Example Mapping de la especificación
- ✅ Nombra tests con patrón: `[Método]_[Escenario]_[ResultadoEsperado]`
- ✅ Estructura AAA: Arrange, Act, Assert
- ✅ Tests para factory method `Create()`
- ✅ Tests para cada método de comportamiento
- ✅ Tests de validación (éxito y fallo)
- ✅ Namespace: `WebApi.UnitTests.Features.[Entidad]`

**Ejemplo de estructura**:
```csharp
using FluentAssertions;
using webapi.features.[entidad].models;

namespace WebApi.UnitTests.Features.[Entidad];

public class [Entidad]Tests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Name";
        
        // Act
        var result = [Entidad].Create(id, name);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
    }
    
    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Act
        var result = [Entidad].Create(Guid.NewGuid(), "");
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }
    
    [Fact]
    public void AddItem_WithValidItem_ShouldReturnSuccess()
    {
        // Arrange
        var entity = [Entidad].Create(Guid.NewGuid(), "Test").Value;
        var item = new Item(Guid.NewGuid(), "Item");
        
        // Act
        var result = entity.AddItem(item);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        entity.Items.Should().Contain(item);
    }
}
```

⚠️ RESTRICCIONES CRÍTICAS:
- ❌ NO uses excepciones para flujo de negocio (usa Result<T>)
- ❌ NO uses constructores públicos (usa factory methods)
- ❌ NO uses setters públicos (usa protected set)
- ❌ NO uses controladores (este proyecto usa Minimal APIs)
- ❌ NO devuelvas void en métodos de comportamiento (usa Result)
- ✅ SÍ usa el patrón Result para todos los métodos que pueden fallar
- ✅ SÍ encapsula colecciones (backing field + IReadOnlyCollection)
- ✅ SÍ valida en el factory method Create()

📤 FORMATO DE RESPUESTA:
Proporciona:
1. Código completo de `[Entidad].cs` (listo para copiar y pegar)
2. Código completo de `[Entidad]Tests.cs` (listo para copiar y pegar)
3. Breve explicación de las decisiones de diseño tomadas

🔍 VALIDACIÓN:
Después de generar, verifica que:
- El código compila sin errores
- Todos los tests pasan
- Sigue exactamente el estilo de `Pizza.cs` y `PizzaTests.cs`
```

---

## Prompt 2: Generar Persistencia

**Cuándo usar**: Después de tener el dominio compilando y tests pasando

### 📋 Prompt

```
🎯 ROL: Experto en Entity Framework Core y persistencia

📂 CONTEXTO:
Ya tengo implementado el dominio de [Entidad] y necesito configurar la persistencia.

📖 ARCHIVOS DE REFERENCIA:
1. @[src/webapi/features/[entidad]/models/[Entidad].cs] - Clase de dominio
2. @[src/webapi/infrastructure/ApplicationDbContext.cs] - DbContext actual
3. Busca ejemplos de configuración en: `src/webapi/infrastructure/Configurations/`

🎯 TAREA:
Genera la configuración de persistencia para Entity Framework Core.

📦 ENTREGABLES:

### 1. Configuración de EF Core
**Archivo**: `src/webapi/infrastructure/Configurations/[Entidad]Configuration.cs`

**Requisitos**:
- ✅ Implementa `IEntityTypeConfiguration<[Entidad]>`
- ✅ Configura nombre de tabla con `ToTable("[Entidades]")`
- ✅ Configura clave primaria con `HasKey(x => x.Id)`
- ✅ Configura propiedades:
  - `IsRequired()` para propiedades obligatorias
  - `HasMaxLength()` según validaciones del dominio
  - `HasColumnType()` si es necesario (ej: decimal(18,2))
- ✅ Configura relaciones (HasMany, WithMany, etc.)
- ✅ Ignora propiedades calculadas con `Ignore(x => x.PropertyName)`
- ✅ Namespace: `webapi.infrastructure.Configurations`

**Ejemplo**:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using webapi.features.[entidad].models;

namespace webapi.infrastructure.Configurations;

public class [Entidad]Configuration : IEntityTypeConfiguration<[Entidad]>
{
    public void Configure(EntityTypeBuilder<[Entidad]> builder)
    {
        builder.ToTable("[Entidades]");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        // Ignorar propiedades calculadas
        builder.Ignore(x => x.CalculatedProperty);
        
        // Relaciones
        builder.HasMany(x => x.Items)
            .WithMany()
            .UsingEntity(j => j.ToTable("[Entidad]Items"));
    }
}
```

### 2. Actualización de ApplicationDbContext
**Archivo**: `src/webapi/infrastructure/ApplicationDbContext.cs`

**Cambios necesarios**:
Agrega esta línea en la clase `ApplicationDbContext`:

```csharp
public required DbSet<[Entidad]> [Entidades] { get; set; }
```

⚠️ RESTRICCIONES:
- Las longitudes máximas deben coincidir con las del validador de dominio
- Propiedades calculadas (solo get) deben ser ignoradas con `Ignore()`
- Para relaciones Many-to-Many, usa tabla de unión
- Usa convenciones de EF Core cuando sea posible

📤 FORMATO DE RESPUESTA:
1. Código completo de `[Entidad]Configuration.cs`
2. Línea exacta para agregar a `ApplicationDbContext.cs`
3. Explicación de las relaciones configuradas
```

---

## Prompt 3: Generar Query Get por ID

**Cuándo usar**: Después de tener persistencia configurada

### 📋 Prompt

```
🎯 ROL: Experto en CQRS y Vertical Slice Architecture

📂 CONTEXTO:
Implemento la funcionalidad de lectura individual de [Entidad].

📖 ARCHIVOS DE REFERENCIA:
1. @[src/webapi/features/[entidad]/models/[Entidad].cs] - Dominio
2. @[src/webapi/features/pizzas/queries/GetPizza.cs] - Ejemplo de query
3. @[.plans/templates/style_guide_examples.md] - Guía de estilo

🎯 TAREA:
Genera el endpoint para obtener un [Entidad] por ID.

📦 ENTREGABLES:

**Archivo**: `src/webapi/features/[entidad]/queries/Get[Entidad].cs`

**Requisitos**:
- ✅ Implementa `IFeatureModule`
- ✅ Record `Response` con todas las propiedades del dominio
- ✅ Records anidados para relaciones (ej: `ItemResponse`)
- ✅ Método `AddRoutes(IEndpointRouteBuilder app)`
- ✅ Endpoint: `GET /[entidades]/{id:guid}`
- ✅ Repositorio adaptador con `[Injectable]` implementando `IGet<[Entidad], Guid>`
- ✅ Usa `IGetOrThrowAsync` con:
  - `tracking: false` (solo lectura)
  - `includeProperties: nameof([Entidad].Items)` si hay relaciones
- ✅ Configuración OpenAPI completa
- ✅ Namespace: `webapi.features.[entidad].queries`

**Estructura**:
```csharp
using System.ComponentModel.DataAnnotations;
using Fudie;
using Fudie.DependencyInjection;
using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.[entidad].models;

namespace webapi.features.[entidad].queries;

public class Get[Entidad] : IFeatureModule
{
    // DTOs anidados para relaciones
    public record ItemResponse(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name
    );
    
    // DTO de respuesta principal
    public record Response(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name,
        [Required][property: Required] IEnumerable<ItemResponse> Items
    );
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/[entidades]/{id:guid}", async (Guid id, IGet<[Entidad], Guid> repository) =>
        {
            var entity = await repository.Get(id);
            var response = new Response(
                entity.Id,
                entity.Name,
                entity.Items.Select(i => new ItemResponse(i.Id, i.Name))
            );
            return Results.Ok(response);
        })
        .WithOpenApi()
        .WithName("Get[Entidad]")
        .WithSummary("Obtener [entidad] por ID")
        .WithDescription("Endpoint para recuperar un [entidad] específico con sus relaciones")
        .WithTags("[Entidades]")
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces<CustomProblemDetails>(StatusCodes.Status404NotFound);
    }
    
    [Injectable]
    public class Repository(IGetOrThrowAsync repository) : IGet<[Entidad], Guid>
    {
        public Task<[Entidad]> Get(Guid id)
        {
            return repository.GetOrThrowAsync<[Entidad], Guid>(
                id,
                tracking: false,
                includeProperties: nameof([Entidad].Items)
            );
        }
    }
}
```

⚠️ RESTRICCIONES:
- Response debe incluir TODAS las propiedades del dominio
- Usa `tracking: false` para optimizar lecturas
- Incluye relaciones con `includeProperties`
- Usa `[Required][property: Required]` en DTOs
- Retorna `Results.Ok(response)`

📤 FORMATO DE RESPUESTA:
Código completo de `Get[Entidad].cs` listo para copiar y pegar.
```

---

## Prompt 4: Generar Query Get Lista

**Cuándo usar**: Después de tener Get por ID funcionando

### 📋 Prompt

```
🎯 ROL: Experto en CQRS y paginación

📂 CONTEXTO:
Implemento el listado paginado de [Entidad].

📖 ARCHIVOS DE REFERENCIA:
1. @[src/webapi/features/[entidad]/models/[Entidad].cs] - Dominio
2. @[src/webapi/features/pizzas/queries/GetPizzas.cs] - Ejemplo de lista paginada

🎯 TAREA:
Genera el endpoint para listar [Entidades] con paginación y filtrado.

📦 ENTREGABLES:

**Archivo**: `src/webapi/features/[entidad]/queries/Get[Entidades].cs`

**Requisitos**:
- ✅ Record `Query(string? Name, int Page = 1, int Size = 25)`
- ✅ Record `Response` (igual que en Get[Entidad])
- ✅ Interface `IService` con `Handler(Query query)`
- ✅ Clase `Service` con `[Injectable]`:
  - Inyecta `IQuery`
  - Usa `Query<[Entidad]>().Include(e => e.Items)`
  - Filtra por nombre (case-insensitive) si se proporciona
  - Ordena por nombre
  - Aplica paginación con `Skip((Page-1)*Size).Take(Size)`
  - Proyecta a Response con `Select()`
- ✅ Endpoint: `GET /[entidades]`
- ✅ Usa `[AsParameters] Query query`
- ✅ Configuración OpenAPI con `.WithStandardOpenApi<List<Response>>()`

**Estructura**:
```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Fudie;
using Fudie.DependencyInjection;
using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.[entidad].models;

namespace webapi.features.[entidad].queries;

public class Get[Entidades] : IFeatureModule
{
    public record Query(string? Name, int Page = 1, int Size = 25);
    
    public record ItemResponse(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name
    );
    
    public record Response(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name,
        [Required][property: Required] IEnumerable<ItemResponse> Items
    );
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/[entidades]", async (IService service, [AsParameters] Query query) =>
        {
            var result = await service.Handler(query);
            return Results.Ok(result);
        })
        .WithStandardOpenApi<List<Response>>(
            name: "Get[Entidades]",
            summary: "Listar [entidades]",
            description: "Endpoint para recuperar [entidades] paginados con filtrado opcional",
            tag: "[Entidades]",
            successStatusCode: StatusCodes.Status200OK
        );
    }
    
    public interface IService
    {
        Task<IQueryable<Response>> Handler(Query query);
    }
    
    [Injectable]
    public class Service(IQuery repository) : IService
    {
        public Task<IQueryable<Response>> Handler(Query query)
        {
            var queryable = repository.Query<[Entidad]>().Include(e => e.Items);
            
            var result = queryable
                .Where(e => query.Name == null || e.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Name)
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .Select(e => new Response(
                    e.Id,
                    e.Name,
                    e.Items.Select(i => new ItemResponse(i.Id, i.Name))
                ));
            
            return Task.FromResult(result);
        }
    }
}
```

⚠️ RESTRICCIONES:
- Filtrado debe ser case-insensitive
- Paginación por defecto: Page=1, Size=25
- Retorna `IQueryable<Response>` para permitir composición
- Usa `Include()` para eager loading de relaciones

📤 FORMATO DE RESPUESTA:
Código completo de `Get[Entidades].cs` listo para copiar y pegar.
```

---

## Prompt 5: Generar Command Create

**Cuándo usar**: Después de tener queries funcionando

### 📋 Prompt

```
🎯 ROL: Experto en CQRS y Commands

📂 CONTEXTO:
Implemento la creación de [Entidad].

📖 ARCHIVOS DE REFERENCIA:
1. @[src/webapi/features/[entidad]/models/[Entidad].cs] - Dominio
2. @[src/webapi/features/pizzas/commands/CreatePizza.cs] - Ejemplo de command
3. @[.plans/templates/style_guide_examples.md] - Guía de estilo

🎯 TAREA:
Genera el endpoint para crear [Entidad].

📦 ENTREGABLES:

**Archivo**: `src/webapi/features/[entidad]/commands/Create[Entidad].cs`

**Requisitos**:
- ✅ Record `Request` con propiedades necesarias (sin ID)
- ✅ Record `Response` (igual que en queries)
- ✅ Interface `IService` con `HandlerAsync(Request request)`
- ✅ Clase `Service` con `[Injectable]`:
  - Inyecta `IAdd<[Entidad]>`, `IUnitOfWork`
  - Si hay relaciones, inyecta `IGetOrThrowAsync` para lookups
  - Llama a `[Entidad].Create().ValueOrThrow()`
  - Agrega relaciones con métodos del dominio usando `.SuccessOrThrow()`
  - Llama a `repository.Add(entity)`
  - Llama a `unitOfWork.SaveChangesAsync()`
  - Retorna Response
- ✅ Repositorio con `[Injectable]` implementando `IAdd<[Entidad]>`
  - Usa `Entry(entity).State = EntityState.Added`
- ✅ Endpoint: `POST /[entidades]`
- ✅ Configuración OpenAPI con:
  - `successStatusCode: StatusCodes.Status201Created`
  - `additionalErrorCodes: [422, 404]` si hay lookups

**Estructura**:
```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Fudie;
using Fudie.DependencyInjection;
using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.[entidad].models;
using webapi.features.items.models; // Si hay relaciones

namespace webapi.features.[entidad].commands;

public class Create[Entidad] : IFeatureModule
{
    public record Request(
        [Required][property: Required] string Name,
        IEnumerable<Guid> ItemIds
    );
    
    public record ItemResponse(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name
    );
    
    public record Response(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name,
        [Required][property: Required] IEnumerable<ItemResponse> Items
    );
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/[entidades]", async (IService service, Request request) =>
        {
            var response = await service.HandlerAsync(request);
            return Results.Created("", response);
        })
        .WithStandardOpenApi<Response>(
            name: "Create[Entidad]",
            summary: "Crear [entidad]",
            description: "Endpoint para crear un nuevo [entidad]",
            tag: "[Entidades]",
            successStatusCode: StatusCodes.Status201Created,
            additionalErrorCodes: [StatusCodes.Status422UnprocessableEntity, StatusCodes.Status404NotFound]
        );
    }
    
    public interface IService
    {
        Task<Response> HandlerAsync(Request request);
    }
    
    [Injectable]
    public class Service(
        IAdd<[Entidad]> repository,
        IGetOrThrowAsync lookupRepository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<Response> HandlerAsync(Request request)
        {
            // 1. Crear entidad con validación
            var entity = [Entidad].Create(Guid.NewGuid(), request.Name).ValueOrThrow();
            
            // 2. Agregar relaciones (si aplica)
            foreach (var itemId in request.ItemIds)
            {
                var item = await lookupRepository.GetOrThrowAsync<Item, Guid>(itemId);
                entity.AddItem(item).SuccessOrThrow();
            }
            
            // 3. Persistir
            repository.Add(entity);
            await unitOfWork.SaveChangesAsync();
            
            // 4. Retornar respuesta
            return new Response(
                entity.Id,
                entity.Name,
                entity.Items.Select(i => new ItemResponse(i.Id, i.Name))
            );
        }
    }
    
    [Injectable]
    public class Repository(IRepository repository) : IAdd<[Entidad]>
    {
        public void Add([Entidad] entity)
        {
            repository.Entry(entity).State = EntityState.Added;
        }
    }
}
```

⚠️ RESTRICCIONES:
- Usa `Guid.NewGuid()` para generar ID
- Usa `.ValueOrThrow()` y `.SuccessOrThrow()` (de ResultExtensions)
- NO uses try-catch (GlobalExceptionHandler lo maneja)
- Retorna `Results.Created("", response)`
- Valida existencia de relaciones con `GetOrThrowAsync`

📤 FORMATO DE RESPUESTA:
Código completo de `Create[Entidad].cs` listo para copiar y pegar.
```

---

## Prompt 6: Generar Command Update

### 📋 Prompt

```
🎯 ROL: Experto en CQRS y Commands

📂 CONTEXTO:
Implemento la actualización de [Entidad].

📖 ARCHIVOS DE REFERENCIA:
1. @[src/webapi/features/[entidad]/models/[Entidad].cs] - Dominio
2. @[src/webapi/features/pizzas/commands/UpdatePizza.cs] - Ejemplo de update

🎯 TAREA:
Genera el endpoint para actualizar [Entidad].

📦 ENTREGABLES:

**Archivo**: `src/webapi/features/[entidad]/commands/Update[Entidad].cs`

**Requisitos**:
- ✅ Record `Request` (sin ID, viene del route)
- ✅ Interface `IService` con `HandlerAsync(Guid id, Request request)`
- ✅ Service inyecta `IUpdate<[Entidad], Guid>`, `IUnitOfWork`, `IGetOrThrowAsync`
- ✅ Obtiene entidad existente con `repository.Get(id)`
- ✅ Actualiza propiedades con método `Update()` del dominio
- ✅ Sincroniza colecciones (elimina viejos, agrega nuevos)
- ✅ NO llama a `Add()`, solo a `SaveChangesAsync()`
- ✅ Repositorio implementa `IUpdate<[Entidad], Guid>`
- ✅ Endpoint: `PUT /[entidades]/{id:guid}`
- ✅ Retorna `Results.Ok(response)`

**Estructura**:
```csharp
public class Update[Entidad] : IFeatureModule
{
    public record Request(
        [Required][property: Required] string Name,
        IEnumerable<Guid> ItemIds
    );
    
    public record Response(...); // Igual que en Create
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/[entidades]/{id:guid}", async (Guid id, IService service, Request request) =>
        {
            var response = await service.HandlerAsync(id, request);
            return Results.Ok(response);
        })
        .WithStandardOpenApi<Response>(
            name: "Update[Entidad]",
            summary: "Actualizar [entidad]",
            description: "Endpoint para actualizar un [entidad] existente",
            tag: "[Entidades]",
            successStatusCode: StatusCodes.Status200OK,
            additionalErrorCodes: [StatusCodes.Status422UnprocessableEntity, StatusCodes.Status404NotFound]
        );
    }
    
    public interface IService
    {
        Task<Response> HandlerAsync(Guid id, Request request);
    }
    
    [Injectable]
    public class Service(
        IUpdate<[Entidad], Guid> repository,
        IGetOrThrowAsync lookupRepository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<Response> HandlerAsync(Guid id, Request request)
        {
            // 1. Obtener entidad existente
            var entity = await repository.Get(id);
            
            // 2. Actualizar propiedades básicas
            entity.Update(request.Name).SuccessOrThrow();
            
            // 3. Sincronizar colecciones
            // Eliminar items que ya no están
            var itemsToRemove = entity.Items
                .Where(i => !request.ItemIds.Contains(i.Id))
                .ToList();
            
            foreach (var item in itemsToRemove)
            {
                entity.RemoveItem(item).SuccessOrThrow();
            }
            
            // Agregar nuevos items
            var newItemIds = request.ItemIds
                .Where(id => !entity.Items.Any(i => i.Id == id));
            
            foreach (var itemId in newItemIds)
            {
                var item = await lookupRepository.GetOrThrowAsync<Item, Guid>(itemId);
                entity.AddItem(item).SuccessOrThrow();
            }
            
            // 4. Persistir
            await unitOfWork.SaveChangesAsync();
            
            // 5. Retornar respuesta
            return new Response(...);
        }
    }
    
    [Injectable]
    public class Repository(IGetOrThrowAsync repository) : IUpdate<[Entidad], Guid>
    {
        public Task<[Entidad]> Get(Guid id)
        {
            return repository.GetOrThrowAsync<[Entidad], Guid>(
                id,
                tracking: true, // ← tracking HABILITADO para update
                includeProperties: nameof([Entidad].Items)
            );
        }
    }
}
```

⚠️ RESTRICCIONES:
- ID viene del route, NO del body
- Usa `tracking: true` en Get() para update
- Sincroniza colecciones correctamente
- NO llames a `Add()`, EF Core trackea cambios automáticamente

📤 FORMATO DE RESPUESTA:
Código completo de `Update[Entidad].cs` listo para copiar y pegar.
```

---

## Prompt 7: Generar Command Delete

### 📋 Prompt

```
🎯 ROL: Experto en CQRS y Commands

📂 CONTEXTO:
Implemento la eliminación de [Entidad].

🎯 TAREA:
Genera el endpoint para eliminar [Entidad].

📦 ENTREGABLES:

**Archivo**: `src/webapi/features/[entidad]/commands/Delete[Entidad].cs`

**Requisitos**:
- ✅ Interface `IService` con `HandlerAsync(Guid id)`
- ✅ Service inyecta `IRemove<[Entidad], Guid>`, `IUnitOfWork`
- ✅ Obtiene entidad con `repository.Get(id)`
- ✅ Llama a `repository.Remove(entity)`
- ✅ Llama a `unitOfWork.SaveChangesAsync()`
- ✅ Repositorio implementa `IRemove<[Entidad], Guid>`
- ✅ Endpoint: `DELETE /[entidades]/{id:guid}`
- ✅ Retorna `Results.NoContent()`

**Estructura**:
```csharp
public class Delete[Entidad] : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/[entidades]/{id:guid}", async (Guid id, IService service) =>
        {
            await service.HandlerAsync(id);
            return Results.NoContent();
        })
        .WithStandardOpenApi(
            name: "Delete[Entidad]",
            summary: "Eliminar [entidad]",
            description: "Endpoint para eliminar un [entidad]",
            tag: "[Entidades]",
            successStatusCode: StatusCodes.Status204NoContent,
            additionalErrorCodes: [StatusCodes.Status404NotFound]
        );
    }
    
    public interface IService
    {
        Task HandlerAsync(Guid id);
    }
    
    [Injectable]
    public class Service(
        IRemove<[Entidad], Guid> repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandlerAsync(Guid id)
        {
            var entity = await repository.Get(id);
            repository.Remove(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }
    
    [Injectable]
    public class Repository(IGetOrThrowAsync getRepository, IRepository repository) 
        : IRemove<[Entidad], Guid>
    {
        public Task<[Entidad]> Get(Guid id)
        {
            return getRepository.GetOrThrowAsync<[Entidad], Guid>(id, tracking: true);
        }
        
        public void Remove([Entidad] entity)
        {
            repository.Entry(entity).State = EntityState.Deleted;
        }
    }
}
```

📤 FORMATO DE RESPUESTA:
Código completo de `Delete[Entidad].cs` listo para copiar y pegar.
```

---

## Prompt 8: Generar Tests de Integración

**Cuándo usar**: Después de tener todos los endpoints funcionando

### 📋 Prompt

```
🎯 ROL: Experto en Testing y WebApplicationFactory

📂 CONTEXTO:
Necesito tests de integración completos para [Entidad].

📖 ARCHIVOS DE REFERENCIA:
1. @[src/webapi/features/[entidad]/] - Todos los endpoints
2. @[tests/WebApi.IntegrationTests/Features/Ingredients/] - Ejemplos de tests
3. @[.plans/templates/style_guide_examples.md] - Guía de estilo de tests

🎯 TAREA:
Genera tests de integración para TODOS los endpoints de [Entidad].

📦 ENTREGABLES:

### 1. Tests de Create
**Archivo**: `tests/WebApi.IntegrationTests/Features/[Entidad]/Create[Entidad]Tests.cs`

**Tests requeridos**:
- ✅ `Create[Entidad]_WithValidData_ShouldReturnCreated()`
- ✅ `Create[Entidad]_WithInvalidData_ShouldReturnUnprocessableEntity()`
- ✅ `Create[Entidad]_WithNonExistentRelation_ShouldReturnNotFound()` (si aplica)

### 2. Tests de Get por ID
**Archivo**: `tests/WebApi.IntegrationTests/Features/[Entidad]/Get[Entidad]Tests.cs`

**Tests requeridos**:
- ✅ `Get[Entidad]_WithExistingId_ShouldReturnOk()`
- ✅ `Get[Entidad]_WithNonExistingId_ShouldReturnNotFound()`

### 3. Tests de Get Lista
**Archivo**: `tests/WebApi.IntegrationTests/Features/[Entidad]/Get[Entidades]Tests.cs`

**Tests requeridos**:
- ✅ `Get[Entidades]_ShouldReturnOkWithList()`
- ✅ `Get[Entidades]_WithFilter_ShouldReturnFilteredResults()`
- ✅ `Get[Entidades]_WithPagination_ShouldReturnCorrectPage()`

### 4. Tests de Update
**Archivo**: `tests/WebApi.IntegrationTests/Features/[Entidad]/Update[Entidad]Tests.cs`

**Tests requeridos**:
- ✅ `Update[Entidad]_WithValidData_ShouldReturnOk()`
- ✅ `Update[Entidad]_WithInvalidData_ShouldReturnUnprocessableEntity()`
- ✅ `Update[Entidad]_WithNonExistingId_ShouldReturnNotFound()`

**Estructura de ejemplo**:
```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using webapi.infrastructure;
using webapi.features.[entidad].commands;

namespace WebApi.IntegrationTests.Features.[Entidad];

public class Create[Entidad]Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public Create[Entidad]Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remover DbContext existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                
                // Agregar InMemoryDatabase único
                var databaseName = "TestDatabase_" + Guid.NewGuid();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
                
                // Re-registrar interfaces
                services.AddScoped<IRepository>(sp => sp.GetRequiredService<ApplicationDbContext>());
                services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
                services.AddScoped<IQuery>(sp => sp.GetRequiredService<ApplicationDbContext>());
                services.AddScoped<IGetOrThrowAsync>(sp => sp.GetRequiredService<ApplicationDbContext>());
            });
        });
        
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task Create[Entidad]_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new Create[Entidad].Request("Test Name", []);
        
        // Act
        var response = await _client.PostAsJsonAsync("/[entidades]", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Create[Entidad].Response>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Name");
    }
    
    [Fact]
    public async Task Create[Entidad]_WithInvalidData_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var request = new Create[Entidad].Request("", []); // Nombre vacío
        
        // Act
        var response = await _client.PostAsJsonAsync("/[entidades]", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
```

⚠️ RESTRICCIONES:
- Usa `WebApplicationFactory<Program>`
- InMemoryDatabase con nombre único por clase
- Re-registra interfaces de DbContext
- Usa FluentAssertions
- Cada test debe ser independiente

📤 FORMATO DE RESPUESTA:
Código completo de TODOS los archivos de test, listos para copiar y pegar.
```

---

## Prompt 9: Revisar y Optimizar

**Cuándo usar**: Después de tener todo funcionando

### 📋 Prompt

```
🎯 ROL: Arquitecto Senior de Software - Code Reviewer

📂 CONTEXTO:
He completado la implementación de [Entidad] y necesito una revisión completa.

📖 ARCHIVOS A REVISAR:
1. @[src/webapi/features/[entidad]/] - Toda la feature
2. @[tests/] - Todos los tests
3. @[src/webapi/infrastructure/Configurations/[Entidad]Configuration.cs] - Persistencia

🎯 TAREA:
Revisa el código y sugiere mejoras siguiendo los principios SOLID y Clean Code.

📋 ASPECTOS A REVISAR:

### 1. Arquitectura y Patrones
- ✅ ¿Se sigue correctamente el patrón Vertical Slice?
- ✅ ¿Se aplica CQRS correctamente (separación lectura/escritura)?
- ✅ ¿Se usa Result pattern consistentemente?
- ✅ ¿Las dependencias apuntan en la dirección correcta?

### 2. Principios SOLID
- ✅ **SRP**: ¿Cada clase tiene una sola responsabilidad?
- ✅ **OCP**: ¿El código está abierto a extensión, cerrado a modificación?
- ✅ **LSP**: ¿Las abstracciones son correctas?
- ✅ **ISP**: ¿Las interfaces son específicas?
- ✅ **DIP**: ¿Se depende de abstracciones, no de concreciones?

### 3. Domain-Driven Design
- ✅ ¿Las invariantes de negocio están protegidas?
- ✅ ¿La encapsulación es correcta?
- ✅ ¿Las validaciones están en el lugar correcto?
- ✅ ¿Los métodos de comportamiento tienen nombres ubicuos?

### 4. Calidad de Código
- ✅ ¿Los nombres son descriptivos y consistentes?
- ✅ ¿Hay código duplicado?
- ✅ ¿Los métodos son pequeños y enfocados?
- ✅ ¿Hay magic numbers o strings?
- ✅ ¿Los comentarios son necesarios o el código se auto-documenta?

### 5. Testing
- ✅ ¿La cobertura de tests es adecuada?
- ✅ ¿Los tests son independientes?
- ✅ ¿Los nombres de tests son descriptivos?
- ✅ ¿Se prueban casos edge?

### 6. Performance
- ✅ ¿Se usa `AsNoTracking()` en queries?
- ✅ ¿Se cargan relaciones con `Include()` cuando es necesario?
- ✅ ¿La paginación es eficiente?
- ✅ ¿Hay N+1 queries?

### 7. Seguridad
- ✅ ¿Se validan todos los inputs?
- ✅ ¿Se manejan correctamente los errores?
- ✅ ¿No se expone información sensible en errores?

📤 FORMATO DE RESPUESTA:
1. **Resumen**: Calificación general (1-10) y comentario breve
2. **Fortalezas**: Qué está bien hecho
3. **Mejoras Críticas**: Problemas que DEBEN corregirse
4. **Mejoras Sugeridas**: Optimizaciones opcionales
5. **Código Refactorizado**: Si hay cambios críticos, proporciona el código corregido
```

---

## 📚 Notas de Uso

### Orden de Ejecución
1. Prompt 1 → Dominio + Tests
2. Prompt 2 → Persistencia
3. Prompt 3 → Get por ID
4. Prompt 4 → Get Lista
5. Prompt 5 → Create
6. Prompt 6 → Update
7. Prompt 7 → Delete (opcional)
8. Prompt 8 → Tests de Integración
9. Prompt 9 → Revisión Final

### Tips
- **Copia el prompt completo**, no lo resumas
- **Reemplaza todas las variables** `[Entidad]`, `[entidad]`, `[Entidades]`
- **Adjunta los archivos de referencia** con `@[ruta]`
- **Valida después de cada prompt** (compilar, ejecutar tests)
- **No pases al siguiente prompt** si el anterior tiene errores

### Validación Rápida
Después de cada prompt:
```bash
dotnet build  # Debe compilar sin errores
dotnet test   # Todos los tests deben pasar
```

---

**¡Usa estos prompts para automatizar el 90% del desarrollo! 🚀**
