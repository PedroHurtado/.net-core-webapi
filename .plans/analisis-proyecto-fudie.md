# Análisis del Proyecto Fudie y su Integración con WebAPI

**Fecha:** 2025-12-01  
**Analista:** Antigravity AI  
**Versión:** 1.0

---

## 📋 Índice

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Análisis de Fudie](#análisis-de-fudie)
3. [Integración con Program.cs](#integración-con-programcs)
4. [Análisis del Dominio de Pizzas](#análisis-del-dominio-de-pizzas)
5. [Análisis de la Capa de Infraestructura](#análisis-de-la-capa-de-infraestructura)
6. [Patrones y Arquitectura](#patrones-y-arquitectura)
7. [Conclusiones y Recomendaciones](#conclusiones-y-recomendaciones)

---

## 🎯 Resumen Ejecutivo

### Objetivos del Proyecto Fudie

**Fudie** es una **biblioteca de clases reutilizable** para aplicaciones ASP.NET Core que proporciona una infraestructura común para construir APIs siguiendo principios de **Clean Architecture**, **Domain-Driven Design (DDD)** y **CQRS**.

#### Objetivos Principales:

1. **Reutilización de Código**: Extraer funcionalidad común de aplicaciones web en una biblioteca compartida
2. **Estandarización**: Establecer patrones consistentes para manejo de errores, validación y arquitectura
3. **Productividad**: Reducir código boilerplate mediante inyección de dependencias automática y convenciones
4. **Mantenibilidad**: Promover separación de responsabilidades y código testeable
5. **Documentación Automática**: Integración con OpenAPI/Swagger para documentación de APIs

---

## 📦 Análisis de Fudie

### 2.1 Estructura del Proyecto

```
Fudie/
├── Domain/                    # Capa de Dominio
│   └── Entity.cs             # Clase base para entidades DDD
├── Infrastructure/            # Capa de Infraestructura
│   └── Repository.cs         # Interfaces de repositorio
├── DependencyInjection/       # Sistema de DI
│   ├── Injectable.cs         # Atributo para registro automático
│   └── InjectionExtension.cs # Extensiones de IServiceCollection
├── OpenApi/                   # Extensiones OpenAPI
│   ├── CustomProblemDetails.cs
│   └── EndPointExtensions.cs
├── Result.cs                  # Patrón Result
├── ResultExtensions.cs        # Extensiones para Result
├── GlobalExceptionHandler.cs  # Manejo global de excepciones
├── IFeatureModule.cs          # Interface para módulos de features
└── RouteExtension.cs          # Extensiones de routing
```

### 2.2 Componentes Clave

#### **2.2.1 Patrón Result**

**Ubicación:** `Result.cs`, `ResultExtensions.cs`

**Propósito:** Manejo funcional de errores sin excepciones para flujo de negocio.

**Características:**
- `Result` y `Result<T>` para operaciones con/sin valor de retorno
- `IsSuccess` / `IsFailure` para verificación de estado
- `Errors` como colección de `ValidationError`
- Métodos estáticos `Success()` y `Failure()` para creación

**Ejemplo de uso:**
```csharp
public static Result<Pizza> Create(Guid id, string name, string description, string url)
{
    var pizza = new Pizza(id, name, description, url);
    var validationResult = ValidateEntity(pizza, new PizzaValidator());
    
    if (validationResult.IsFailure)
    {
        return Result<Pizza>.Failure(validationResult.Errors);
    }
    
    return Result<Pizza>.Success(pizza);
}
```

**Beneficios:**
- ✅ Errores explícitos en la firma del método
- ✅ Evita excepciones para flujo de negocio
- ✅ Composable y testeable
- ✅ Railway-oriented programming

---

#### **2.2.2 Domain-Driven Design (Entity)**

**Ubicación:** `Domain/Entity.cs`

**Propósito:** Clase base abstracta para todas las entidades del dominio.

**Características:**
```csharp
public abstract class Entity(Guid id)
{
    public Guid Id { get; protected set; } = id;
    
    // Igualdad basada en identidad
    public override bool Equals(object? obj)
    public override int GetHashCode()
    
    // Validación integrada con FluentValidation
    protected static Result ValidateEntity<T>(T entity, AbstractValidator<T> validator)
}
```

**Principios DDD implementados:**
- ✅ **Identidad**: Cada entidad tiene un `Id` único
- ✅ **Igualdad por identidad**: Dos entidades son iguales si tienen el mismo `Id`
- ✅ **Validación de dominio**: Validación integrada en la entidad
- ✅ **Inmutabilidad**: Propiedades con `protected set`
- ✅ **Factory methods**: Métodos estáticos `Create()` para construcción

---

#### **2.2.3 Inyección de Dependencias Automática**

**Ubicación:** `DependencyInjection/Injectable.cs`, `InjectionExtension.cs`

**Propósito:** Registro automático de servicios mediante atributos.

**Características:**

1. **Atributo `[Injectable]`:**
```csharp
[Injectable(ServiceLifetime.Scoped)]
public class Service : IService
{
    // Se registra automáticamente
}
```

2. **Registro automático de interfaces:**
   - Detecta todas las interfaces implementadas
   - Registra solo interfaces de "primer nivel" (no heredadas)
   - Evita duplicados

3. **Método `AddInterfacesFor<T>()`:**
   - Registra interfaces para tipos ya registrados (como `DbContext`)
   - Permite que `ApplicationDbContext` implemente múltiples interfaces

**Ejemplo en Program.cs:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(...)
    .AddInterfacesFor<ApplicationDbContext>();

builder.Services.AddInjectables();
```

**Beneficios:**
- ✅ Reduce código boilerplate
- ✅ Convención sobre configuración
- ✅ Descubrimiento automático de servicios
- ✅ Soporte para Transient, Scoped, Singleton

---

#### **2.2.4 Repository Pattern**

**Ubicación:** `Infrastructure/Repository.cs`

**Propósito:** Interfaces segregadas para acceso a datos.

**Interfaces definidas:**

```csharp
// Operaciones CRUD segregadas
public interface IGet<T, ID>
public interface IAdd<T>
public interface IUpdate<T, ID> : IGet<T, ID>
public interface IRemove<T, ID> : IGet<T, ID>

// Consultas genéricas
public interface IQuery
{
    IQueryable<T> Query<T>() where T : Entity;
}

// Recuperación con excepción
public interface IGetOrThrowAsync
{
    Task<T> GetOrThrowAsync<T, ID>(
        ID id,
        bool tracking = true,
        CancellationToken cancellationToken = default,
        params string[] includeProperties) where T : Entity;
}

// Unit of Work
public interface IUnitOfWork
{
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Acceso a EntityEntry
public interface IRepository
{
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
```

**Principios aplicados:**
- ✅ **Interface Segregation Principle (ISP)**: Interfaces pequeñas y específicas
- ✅ **Dependency Inversion Principle (DIP)**: Dependencia de abstracciones
- ✅ **Single Responsibility Principle (SRP)**: Cada interfaz tiene una responsabilidad

---

#### **2.2.5 Feature Modules (Vertical Slices)**

**Ubicación:** `IFeatureModule.cs`, `RouteExtension.cs`

**Propósito:** Organización de código por features en lugar de capas técnicas.

**Características:**

1. **Interface `IFeatureModule`:**
```csharp
public interface IFeatureModule
{
    void AddRoutes(IEndpointRouteBuilder app);
}
```

2. **Descubrimiento automático:**
   - `MapFeatures()` escanea todos los ensamblados
   - Encuentra clases que implementan `IFeatureModule`
   - Las instancia y registra sus rutas automáticamente

3. **Organización vertical:**
   - Cada feature contiene: endpoints, DTOs, servicios, repositorios
   - Todo relacionado con una funcionalidad en un solo archivo

**Beneficios:**
- ✅ **Alta cohesión**: Todo lo relacionado está junto
- ✅ **Bajo acoplamiento**: Features independientes
- ✅ **Fácil de encontrar**: No hay que buscar en múltiples capas
- ✅ **Screaming Architecture**: La estructura grita lo que hace el sistema

---

#### **2.2.6 Manejo Global de Excepciones**

**Ubicación:** `GlobalExceptionHandler.cs`

**Propósito:** Manejo centralizado y consistente de errores.

**Características:**
- Implementa `IExceptionHandler` de ASP.NET Core
- Convierte excepciones en respuestas RFC 7807 Problem Details
- Logging automático de errores
- Respuestas consistentes para el cliente

**Tipos de errores manejados:**
- `KeyNotFoundException` → 404 Not Found
- `ValidationException` → 422 Unprocessable Entity
- `Exception` genérica → 500 Internal Server Error

---

#### **2.2.7 Extensiones OpenAPI**

**Ubicación:** `OpenApi/`

**Propósito:** Documentación automática de APIs con Swagger.

**Características:**
- `WithStandardOpenApi<T>()`: Extensión para configurar endpoints
- `GlobalErrorResponsesOperationFilter`: Agrega respuestas de error automáticamente
- `CustomProblemDetails`: Modelo RFC 7807 para errores

---

## 🔌 Integración con Program.cs

### 3.1 Análisis de Program.cs

**Ubicación:** `src/webapi/Program.cs`

El archivo `Program.cs` es el punto de entrada de la aplicación y muestra cómo se integra Fudie:

```csharp
using Fudie;
using Fudie.DependencyInjection;
using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Swagger con esquemas personalizados
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Manejo de tipos anidados y decimales
    options.CustomSchemaIds(...);
    options.MapType<decimal>(...);
});

// 2. Configuración de DbContext con interfaces automáticas
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("PizzaDb");
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
})
.AddInterfacesFor<ApplicationDbContext>(); // ← Registra IQuery, IUnitOfWork, etc.

// 3. Manejo global de excepciones
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. Registro automático de servicios con [Injectable]
builder.Services.AddInjectables();

// 5. Health checks y CORS
builder.Services.AddHealthChecks();
builder.Services.AddCors(...);

var app = builder.Build();

// 6. Middleware
app.UseExceptionHandler();

// 7. Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 8. Mapeo automático de features
app.MapFeatures(); // ← Descubre y registra todos los IFeatureModule

app.MapHealthChecks("/health", ...);
app.UseCors("PermitirTodo");
app.UseHttpsRedirection();

app.Run();
```

### 3.2 Flujo de Integración

```
┌─────────────────────────────────────────────────────────────┐
│                      Program.cs                             │
│                                                             │
│  1. AddDbContext<ApplicationDbContext>()                   │
│     └─> AddInterfacesFor<ApplicationDbContext>()          │
│         └─> Registra: IQuery, IUnitOfWork, IRepository,   │
│                       IGetOrThrowAsync                     │
│                                                             │
│  2. AddInjectables()                                       │
│     └─> Escanea ensamblados                               │
│     └─> Encuentra clases con [Injectable]                 │
│     └─> Registra automáticamente servicios                │
│                                                             │
│  3. MapFeatures()                                          │
│     └─> Escanea ensamblados                               │
│     └─> Encuentra clases que implementan IFeatureModule   │
│     └─> Llama a AddRoutes() de cada feature               │
└─────────────────────────────────────────────────────────────┘
```

---

## 🍕 Análisis del Dominio de Pizzas

### 4.1 Modelo de Dominio

**Ubicación:** `src/webapi/features/pizzas/models/Pizza.cs`

#### **Estructura de la Entidad Pizza**

```csharp
public class Pizza : Entity
{
    // Constantes de negocio
    private const decimal PROFIT = 1.20m;
    
    // Propiedades inmutables
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public string Url { get; protected set; }
    
    // Precio calculado (lógica de negocio)
    public decimal Price => _ingredients.Sum(i => i.Cost) * PROFIT;
    
    // Colección protegida con acceso de solo lectura
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients.ToList().AsReadOnly();
    protected HashSet<Ingredient> _ingredients = [];
    
    // Constructor protegido (solo accesible desde factory methods)
    protected Pizza(Guid id, string name, string description, string url) : base(id)
    
    // Factory method con validación
    public static Result<Pizza> Create(Guid id, string name, string description, string url)
    
    // Métodos de comportamiento
    public Result AddIngredient(Ingredient ingredient)
    public Result RemoveIngredient(Ingredient ingredient)
    public Result Update(string name, string description, string url)
    
    // Validador interno
    protected class PizzaValidator : AbstractValidator<Pizza>
}
```

#### **Principios DDD Aplicados**

1. **Aggregate Root**: `Pizza` es la raíz del agregado que contiene `Ingredients`
2. **Invariantes de Negocio**:
   - El precio se calcula automáticamente: `Cost * 1.20`
   - No se pueden duplicar ingredientes
   - Validación de datos (nombre, descripción, URL)
3. **Encapsulación**:
   - `_ingredients` es privado
   - Solo se expone `IReadOnlyCollection<Ingredient>`
   - Modificación solo a través de métodos `AddIngredient()` / `RemoveIngredient()`
4. **Validación en el Dominio**:
   - FluentValidation integrada
   - Validación en factory method `Create()`
   - Validación en método `Update()`
5. **Inmutabilidad**:
   - Propiedades con `protected set`
   - Constructor protegido
   - Solo modificable a través de métodos específicos

#### **Validaciones Implementadas**

```csharp
protected class PizzaValidator : AbstractValidator<Pizza>
{
    public PizzaValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder de 100 caracteres");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(250).WithMessage("La descripción no puede exceder de 250 caracteres");
        
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("La URL es requerida")
            .MaximumLength(500).WithMessage("La URL no puede exceder de 500 caracteres")
            .Must(BeAValidUrl).WithMessage("La URL no es válida");
    }
}
```

---

### 4.2 Queries (Lectura)

#### **4.2.1 GetPizza (Obtener una pizza por ID)**

**Ubicación:** `src/webapi/features/pizzas/queries/GetPizza.cs`

**Estructura:**
```csharp
public class GetPizza : IFeatureModule
{
    // DTOs de respuesta
    public record IngredientResponse(Guid Id, string Name);
    public record Response(Guid Id, string Name, string Description, 
                          string Url, decimal Price, IEnumerable<IngredientResponse> Ingredients);
    
    // Endpoint
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/pizzas/{id:guid}", async (Guid id, IGet<Pizza, Guid> repository) =>
        {
            var pizza = await repository.Get(id);
            var response = new Response(...);
            return Results.Ok(response);
        })
        .WithOpenApi()
        .WithName("GetPizza")
        .WithSummary("Recuperar una pizza")
        .WithDescription("Endpoint para recuperar una pizza por id con sus ingredientes")
        .WithTags("Pizzas")
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces<CustomProblemDetails>(StatusCodes.Status404NotFound);
    }
    
    // Repositorio específico
    [Injectable]
    public class Repository(IGetOrThrowAsync repository) : IGet<Pizza, Guid>
    {
        public Task<Pizza> Get(Guid id)
        {
            return _repository.GetOrThrowAsync<Pizza, Guid>(
                id, 
                tracking: false,
                includeProperties: nameof(Pizza.Ingredients)
            );
        }
    }
}
```

**Características:**
- ✅ **Vertical Slice**: Todo en un archivo
- ✅ **DTOs específicos**: `Response` e `IngredientResponse`
- ✅ **Repositorio adaptador**: Implementa `IGet<Pizza, Guid>` usando `IGetOrThrowAsync`
- ✅ **No tracking**: `tracking: false` para consultas de solo lectura
- ✅ **Eager loading**: Carga `Ingredients` con `includeProperties`
- ✅ **Documentación OpenAPI**: Configuración completa de Swagger

---

#### **4.2.2 GetPizzas (Listar pizzas con paginación)**

**Ubicación:** `src/webapi/features/pizzas/queries/GetPizzas.cs`

**Estructura:**
```csharp
public class GetPizzas : IFeatureModule
{
    // Query parameters
    public record Query(string? Name, int Page = 1, int Size = 25);
    
    // DTOs
    public record IngredientResponse(Guid Id, string Name);
    public record Response(Guid Id, string Name, string Description, 
                          string Url, decimal Price, IEnumerable<IngredientResponse> Ingredients);
    
    // Endpoint
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/pizzas", async (IService service, IQuery repository, [AsParameters] Query query) =>
        {
            var queryResult = await service.Handler(query);
            return Results.Ok(queryResult);
        })
        .WithStandardOpenApi<List<Response>>(...);
    }
    
    // Servicio
    [Injectable]
    public class Service(IQuery repository) : IService
    {
        public Task<IQueryable<Response>> Handler(Query query)
        {
            var pizzasQuery = _repository.Query<Pizza>().Include(p => p.Ingredients);
            
            var result = pizzasQuery
                .Where(p => query.Name == null || p.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Name)
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .Select(p => new Response(...));
            
            return Task.FromResult(result);
        }
    }
}
```

**Características:**
- ✅ **Paginación**: `Page` y `Size` en query parameters
- ✅ **Filtrado**: Búsqueda por nombre (case-insensitive)
- ✅ **Ordenamiento**: Por nombre
- ✅ **Proyección**: `Select()` a DTOs
- ✅ **IQueryable**: Permite composición adicional si es necesario

---

### 4.3 Commands (Escritura)

#### **4.3.1 CreatePizza (Crear una nueva pizza)**

**Ubicación:** `src/webapi/features/pizzas/commands/CreatePizza.cs`

**Estructura:**
```csharp
public class CreatePizza : IFeatureModule
{
    // DTOs
    public record Request(string Name, string Description, string Url, IEnumerable<Guid> Ingredients);
    public record IngredientResponse(Guid Id, string Name);
    public record Response(Guid Id, string Name, string Description, 
                          string Url, decimal Price, IEnumerable<IngredientResponse> Ingredients);
    
    // Endpoint
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
            description: "Endpoint para crear una nueva pizza con su nombre, descripción, url e ingredientes",
            tag: "Pizzas",
            successStatusCode: StatusCodes.Status201Created,
            additionalErrorCodes: [StatusCodes.Status422UnprocessableEntity, StatusCodes.Status404NotFound]
        );
    }
    
    // Servicio
    [Injectable]
    public class Service(
        IAdd<Pizza> pizzaRepository,
        IGetOrThrowAsync lookupRepository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<Response> HandlerAsync(Request request)
        {
            // 1. Crear entidad de dominio con validación
            var pizza = Pizza.Create(Guid.NewGuid(), request.Name, request.Description, request.Url)
                .ValueOrThrow();
            
            // 2. Agregar ingredientes
            foreach (var ingredientId in request.Ingredients)
            {
                var ingredient = await lookupRepository.GetOrThrowAsync<Ingredient, Guid>(ingredientId);
                pizza.AddIngredient(ingredient).SuccessOrThrow();
            }
            
            // 3. Persistir
            pizzaRepository.Add(pizza);
            await unitOfWork.SaveChangesAsync();
            
            // 4. Retornar respuesta
            return new Response(...);
        }
    }
    
    // Repositorio
    [Injectable]
    public class Repository(IRepository repository) : IAdd<Pizza>
    {
        public void Add(Pizza entity)
        {
            _repository.Entry(entity).State = EntityState.Added;
        }
    }
}
```

**Flujo de Ejecución:**

```
1. Request → Endpoint
   ↓
2. Endpoint → Service.HandlerAsync(request)
   ↓
3. Service:
   a. Pizza.Create() → Validación de dominio
   b. lookupRepository.GetOrThrowAsync() → Buscar ingredientes
   c. pizza.AddIngredient() → Validación de negocio
   d. pizzaRepository.Add() → Marcar como Added
   e. unitOfWork.SaveChangesAsync() → Persistir
   ↓
4. Service → Response DTO
   ↓
5. Endpoint → Results.Created()
```

**Características:**
- ✅ **Validación en múltiples niveles**:
  - Validación de dominio en `Pizza.Create()`
  - Validación de negocio en `AddIngredient()`
  - Validación de existencia en `GetOrThrowAsync()`
- ✅ **Uso de Result pattern**: `ValueOrThrow()` y `SuccessOrThrow()`
- ✅ **Unit of Work**: Transacción implícita
- ✅ **Separación de responsabilidades**:
  - Service: Lógica de aplicación
  - Repository: Acceso a datos
  - Entity: Lógica de dominio

---

#### **4.3.2 UpdatePizza (Actualizar una pizza)**

**Ubicación:** `src/webapi/features/pizzas/commands/UpdatePizza.cs`

**Estructura similar a CreatePizza:**
```csharp
public class UpdatePizza : IFeatureModule
{
    public record Request(string Name, string Description, string Url, IEnumerable<Guid> Ingredients);
    
    [Injectable]
    public class Service(
        IUpdate<Pizza, Guid> pizzaRepository,
        IGetOrThrowAsync lookupRepository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<Response> HandlerAsync(Guid id, Request request)
        {
            // 1. Obtener pizza existente
            var pizza = await pizzaRepository.Get(id);
            
            // 2. Actualizar propiedades
            pizza.Update(request.Name, request.Description, request.Url).SuccessOrThrow();
            
            // 3. Actualizar ingredientes
            // ... (lógica de sincronización de ingredientes)
            
            // 4. Persistir
            await unitOfWork.SaveChangesAsync();
            
            return new Response(...);
        }
    }
}
```

---

## 🗄️ Análisis de la Capa de Infraestructura

### 5.1 ApplicationDbContext

**Ubicación:** `src/webapi/infrastructure/ApplicationDbContext.cs`

#### **Estructura Completa**

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
    DbContext(options), 
    IGetOrThrowAsync,    // ← Implementa interfaces de Fudie
    IQuery, 
    IRepository, 
    IUnitOfWork
{
    // DbSets
    public required DbSet<Ingredient> Ingredients { get; set; }
    public required DbSet<Pizza> Pizzas { get; set; }
    
    // Implementación de IGetOrThrowAsync
    public async Task<T> GetOrThrowAsync<T, ID>(
        ID id,
        bool tracking = true,
        CancellationToken cancellationToken = default,
        params string[] includeProperties) where T : Entity
    {
        var query = Set<T>().AsQueryable();
        
        // Aplicar includes
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }
        
        if (!tracking)
        {
            query = query.AsNoTracking();
        }
        
        var entity = await query.Where(e => e.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
        return entity ?? throw new KeyNotFoundException($"{typeof(T).Name} with ID '{id}' not found.");
    }
    
    // Implementación de IQuery
    public IQueryable<T> Query<T>() where T : Entity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }
    
    // Configuración de modelos
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

#### **Interfaces Implementadas**

1. **`IGetOrThrowAsync`**: Recuperación con excepción si no existe
   - Soporte para `Include()` dinámico
   - Soporte para tracking/no-tracking
   - Lanza `KeyNotFoundException` si no encuentra la entidad

2. **`IQuery`**: Consultas genéricas de solo lectura
   - Retorna `IQueryable<T>` con `AsNoTracking()`
   - Permite composición de queries

3. **`IRepository`**: Acceso a `EntityEntry`
   - Heredado de `DbContext`
   - Permite manipular el estado de entidades

4. **`IUnitOfWork`**: Persistencia transaccional
   - Heredado de `DbContext`
   - `SaveChanges()` y `SaveChangesAsync()`

#### **Registro en Program.cs**

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("PizzaDb");
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
})
.AddInterfacesFor<ApplicationDbContext>(); // ← Registra todas las interfaces
```

**¿Qué hace `AddInterfacesFor<ApplicationDbContext>()`?**

1. Verifica que `ApplicationDbContext` esté registrado
2. Encuentra todas las interfaces implementadas:
   - `IGetOrThrowAsync`
   - `IQuery`
   - `IRepository`
   - `IUnitOfWork`
3. Registra cada interfaz apuntando a la misma instancia de `ApplicationDbContext`
4. Permite inyectar cualquier interfaz en lugar del `DbContext` completo

**Beneficios:**
- ✅ **Dependency Inversion**: Dependencia de abstracciones
- ✅ **Interface Segregation**: Cada servicio solo pide lo que necesita
- ✅ **Testabilidad**: Fácil de mockear interfaces específicas
- ✅ **Reutilización**: Misma instancia de `DbContext` para todas las interfaces

---

### 5.2 Configuración de Entidades

**Ubicación:** `src/webapi/infrastructure/` (archivos de configuración)

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
```

Esto aplica automáticamente todas las clases que implementan `IEntityTypeConfiguration<T>`.

**Ejemplo de configuración:**
```csharp
public class PizzaConfiguration : IEntityTypeConfiguration<Pizza>
{
    public void Configure(EntityTypeBuilder<Pizza> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasMany(p => p.Ingredients)
            .WithMany()
            .UsingEntity(j => j.ToTable("PizzaIngredients"));
    }
}
```

---

## 🏗️ Patrones y Arquitectura

### 6.1 Arquitectura General

```
┌─────────────────────────────────────────────────────────────┐
│                        WebAPI                               │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐  │
│  │              Features (Vertical Slices)             │  │
│  │                                                     │  │
│  │  ┌──────────────┐  ┌──────────────┐               │  │
│  │  │   Pizzas     │  │ Ingredients  │               │  │
│  │  │              │  │              │               │  │
│  │  │ ├─ queries/  │  │ ├─ queries/  │               │  │
│  │  │ ├─ commands/ │  │ ├─ commands/ │               │  │
│  │  │ └─ models/   │  │ └─ models/   │               │  │
│  │  └──────────────┘  └──────────────┘               │  │
│  │                                                     │  │
│  │  Cada feature contiene:                            │  │
│  │  - Endpoints (IFeatureModule)                      │  │
│  │  - DTOs (Request/Response)                         │  │
│  │  - Servicios ([Injectable])                        │  │
│  │  - Repositorios adaptadores ([Injectable])        │  │
│  │  - Modelos de dominio (Entity)                    │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐  │
│  │              Infrastructure                         │  │
│  │                                                     │  │
│  │  - ApplicationDbContext                            │  │
│  │  - Entity Configurations                           │  │
│  └─────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ Referencia
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                         Fudie                               │
│                   (Biblioteca Compartida)                   │
│                                                             │
│  ├─ Domain/              (Entity base)                     │
│  ├─ Infrastructure/      (Repository interfaces)           │
│  ├─ DependencyInjection/ (Injectable, Extensions)          │
│  ├─ OpenApi/             (Swagger extensions)              │
│  ├─ Result.cs            (Result pattern)                  │
│  ├─ GlobalExceptionHandler.cs                              │
│  └─ RouteExtension.cs    (MapFeatures)                     │
└─────────────────────────────────────────────────────────────┘
```

---

### 6.2 Patrones Implementados

#### **6.2.1 Clean Architecture**

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation                         │
│              (Endpoints, DTOs, Validators)              │
│                                                         │
│  - IFeatureModule implementations                      │
│  - Request/Response records                            │
│  - OpenAPI configuration                               │
└─────────────────────────────────────────────────────────┘
                        │
                        ↓
┌─────────────────────────────────────────────────────────┐
│                   Application                           │
│              (Use Cases, Services)                      │
│                                                         │
│  - IService interfaces                                 │
│  - Service implementations ([Injectable])              │
│  - Application logic                                   │
└─────────────────────────────────────────────────────────┘
                        │
                        ↓
┌─────────────────────────────────────────────────────────┐
│                     Domain                              │
│         (Entities, Value Objects, Rules)                │
│                                                         │
│  - Entity (base class)                                 │
│  - Pizza, Ingredient (domain models)                   │
│  - Business rules (AddIngredient, etc.)                │
│  - FluentValidation validators                         │
└─────────────────────────────────────────────────────────┘
                        │
                        ↓
┌─────────────────────────────────────────────────────────┐
│                  Infrastructure                         │
│         (DbContext, Repositories, External)             │
│                                                         │
│  - ApplicationDbContext                                │
│  - Repository implementations                          │
│  - Entity configurations                               │
└─────────────────────────────────────────────────────────┘
```

**Dependencias:**
- ✅ Presentation → Application → Domain ← Infrastructure
- ✅ Domain no depende de nada (núcleo puro)
- ✅ Infrastructure depende de Domain
- ✅ Inversión de dependencias mediante interfaces

---

#### **6.2.2 CQRS (Command Query Responsibility Segregation)**

**Separación clara entre lectura y escritura:**

**Queries (Lectura):**
- `GetPizza.cs` - Obtener una pizza
- `GetPizzas.cs` - Listar pizzas
- Usan `IQuery` o `IGet<T, ID>`
- No modifican estado
- Pueden usar proyecciones y DTOs optimizados
- `AsNoTracking()` para mejor rendimiento

**Commands (Escritura):**
- `CreatePizza.cs` - Crear pizza
- `UpdatePizza.cs` - Actualizar pizza
- Usan `IAdd<T>`, `IUpdate<T, ID>`, `IUnitOfWork`
- Modifican estado
- Validación de dominio
- Transacciones

**Beneficios:**
- ✅ Optimización independiente de lectura/escritura
- ✅ Modelos diferentes para lectura y escritura
- ✅ Escalabilidad (se pueden separar físicamente)

---

#### **6.2.3 Vertical Slice Architecture**

**Organización por features en lugar de capas técnicas:**

```
❌ Arquitectura tradicional (horizontal):
/Controllers
  - PizzaController.cs
  - IngredientController.cs
/Services
  - PizzaService.cs
  - IngredientService.cs
/Repositories
  - PizzaRepository.cs
  - IngredientRepository.cs
/Models
  - Pizza.cs
  - Ingredient.cs

✅ Vertical Slices (por feature):
/features
  /pizzas
    /queries
      - GetPizza.cs       (endpoint + service + repository + DTOs)
      - GetPizzas.cs      (endpoint + service + repository + DTOs)
    /commands
      - CreatePizza.cs    (endpoint + service + repository + DTOs)
      - UpdatePizza.cs    (endpoint + service + repository + DTOs)
    /models
      - Pizza.cs          (domain model)
  /ingredients
    /queries
      - GetIngredient.cs
      - GetIngredients.cs
    /commands
      - CreateIngredient.cs
    /models
      - Ingredient.cs
```

**Beneficios:**
- ✅ **Alta cohesión**: Todo lo relacionado está junto
- ✅ **Bajo acoplamiento**: Features independientes
- ✅ **Fácil de encontrar**: No hay que buscar en múltiples carpetas
- ✅ **Fácil de modificar**: Cambios localizados
- ✅ **Fácil de eliminar**: Borrar una feature completa
- ✅ **Screaming Architecture**: La estructura grita lo que hace

---

#### **6.2.4 Repository Pattern con Interface Segregation**

**Interfaces segregadas en lugar de un repositorio genérico:**

```csharp
// ❌ Repositorio genérico tradicional
public interface IRepository<T>
{
    T Get(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// ✅ Interfaces segregadas (Fudie)
public interface IGet<T, ID> { Task<T> Get(ID id); }
public interface IAdd<T> { void Add(T entity); }
public interface IUpdate<T, ID> : IGet<T, ID> { }
public interface IRemove<T, ID> : IGet<T, ID> { void Remove(T entity); }
public interface IQuery { IQueryable<T> Query<T>() where T : Entity; }
```

**Beneficios:**
- ✅ **Interface Segregation Principle**: Clientes no dependen de métodos que no usan
- ✅ **Testabilidad**: Mockear solo lo necesario
- ✅ **Claridad**: Intención explícita en la firma del constructor
- ✅ **Flexibilidad**: Diferentes implementaciones para diferentes necesidades

---

#### **6.2.5 Result Pattern (Railway-Oriented Programming)**

**Manejo funcional de errores:**

```csharp
// ❌ Excepciones para flujo de negocio
public Pizza CreatePizza(string name, string description, string url)
{
    if (string.IsNullOrEmpty(name))
        throw new ValidationException("Name is required");
    
    var pizza = new Pizza(name, description, url);
    return pizza;
}

// ✅ Result pattern
public static Result<Pizza> Create(Guid id, string name, string description, string url)
{
    var pizza = new Pizza(id, name, description, url);
    var validationResult = ValidateEntity(pizza, new PizzaValidator());
    
    if (validationResult.IsFailure)
    {
        return Result<Pizza>.Failure(validationResult.Errors);
    }
    
    return Result<Pizza>.Success(pizza);
}

// Uso
var result = Pizza.Create(id, name, description, url);
if (result.IsSuccess)
{
    var pizza = result.Value;
}
else
{
    var errors = result.Errors;
}
```

**Beneficios:**
- ✅ **Errores explícitos**: La firma del método indica que puede fallar
- ✅ **Sin excepciones**: Excepciones solo para casos excepcionales
- ✅ **Composable**: Se pueden encadenar operaciones
- ✅ **Testeable**: Fácil de verificar éxito/fallo

---

#### **6.2.6 Dependency Injection con Convenciones**

**Registro automático mediante atributos:**

```csharp
// ❌ Registro manual
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
// ... 50 líneas más

// ✅ Registro automático
[Injectable(ServiceLifetime.Scoped)]
public class UserService : IUserService { }

[Injectable(ServiceLifetime.Scoped)]
public class ProductService : IProductService { }

// En Program.cs
builder.Services.AddInjectables();
```

**Beneficios:**
- ✅ **Convención sobre configuración**: Menos código boilerplate
- ✅ **Descubrimiento automático**: No olvidar registrar servicios
- ✅ **Mantenibilidad**: Registro junto a la clase

---

## 📊 Conclusiones y Recomendaciones

### 7.1 Fortalezas del Proyecto

1. **✅ Arquitectura Limpia y Moderna**
   - Separación clara de responsabilidades
   - Principios SOLID aplicados consistentemente
   - Patrones modernos (CQRS, Vertical Slices, Result)

2. **✅ Reutilización de Código**
   - Fudie como biblioteca compartida
   - Reducción de código boilerplate
   - Convenciones claras

3. **✅ Mantenibilidad**
   - Código organizado por features
   - Alta cohesión, bajo acoplamiento
   - Fácil de encontrar y modificar

4. **✅ Testabilidad**
   - Dependencias inyectadas
   - Interfaces segregadas
   - Lógica de dominio pura

5. **✅ Documentación Automática**
   - OpenAPI/Swagger integrado
   - Respuestas de error estandarizadas (RFC 7807)
   - Esquemas bien definidos

6. **✅ Validación Robusta**
   - FluentValidation en el dominio
   - Validación en múltiples niveles
   - Mensajes de error claros

---

### 7.2 Áreas de Mejora

1. **⚠️ Falta de Logging Estructurado**
   - Agregar `ILogger` en servicios críticos
   - Logging de operaciones de negocio
   - Correlación de requests

2. **⚠️ Falta de Manejo de Concurrencia**
   - Implementar optimistic concurrency (RowVersion)
   - Manejo de conflictos en actualizaciones

3. **⚠️ Falta de Paginación Completa**
   - `GetPizzas` retorna `IQueryable` pero no metadatos de paginación
   - Agregar total de registros, total de páginas, etc.

4. **⚠️ Falta de Autorización**
   - No hay control de acceso
   - Agregar autenticación/autorización

5. **⚠️ Falta de Auditoría**
   - No hay tracking de quién creó/modificó
   - Agregar campos `CreatedBy`, `CreatedAt`, `ModifiedBy`, `ModifiedAt`

6. **⚠️ Falta de Caché**
   - Queries repetitivas sin caché
   - Considerar caché distribuido para lecturas frecuentes

---

### 7.3 Recomendaciones

#### **7.3.1 Corto Plazo**

1. **Agregar Logging**
```csharp
[Injectable]
public class Service(
    IAdd<Pizza> pizzaRepository,
    IGetOrThrowAsync lookupRepository,
    IUnitOfWork unitOfWork,
    ILogger<Service> logger) : IService
{
    public async Task<Response> HandlerAsync(Request request)
    {
        logger.LogInformation("Creating pizza with name {Name}", request.Name);
        // ...
    }
}
```

2. **Agregar Metadatos de Paginación**
```csharp
public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int Size,
    int TotalItems,
    int TotalPages
);
```

3. **Agregar Validación de Request**
```csharp
public class CreatePizzaRequestValidator : AbstractValidator<Request>
{
    public CreatePizzaRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Ingredients).NotEmpty();
    }
}
```

#### **7.3.2 Mediano Plazo**

1. **Implementar Auditoría**
```csharp
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; }
    public string CreatedBy { get; protected set; }
    public DateTime? ModifiedAt { get; protected set; }
    public string? ModifiedBy { get; protected set; }
}
```

2. **Agregar Autenticación/Autorización**
```csharp
app.MapPost("/pizzas", async (IService service, Request request) => { ... })
    .RequireAuthorization("CanCreatePizza");
```

3. **Implementar Caché**
```csharp
[Injectable]
public class CachedPizzaService(IService inner, IDistributedCache cache) : IService
{
    public async Task<Response> Handler(Query query)
    {
        var cacheKey = $"pizzas:{query.Name}:{query.Page}:{query.Size}";
        var cached = await cache.GetStringAsync(cacheKey);
        
        if (cached != null)
            return JsonSerializer.Deserialize<Response>(cached);
        
        var result = await inner.Handler(query);
        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result));
        
        return result;
    }
}
```

#### **7.3.3 Largo Plazo**

1. **Migrar a Base de Datos Real**
   - Cambiar de `InMemoryDatabase` a SQL Server / PostgreSQL
   - Implementar migraciones
   - Configurar índices

2. **Implementar Event Sourcing (opcional)**
   - Para auditoría completa
   - Para reconstruir estado histórico

3. **Separar Lectura y Escritura Físicamente (CQRS completo)**
   - Base de datos de lectura optimizada
   - Base de datos de escritura normalizada
   - Sincronización mediante eventos

4. **Publicar Fudie como NuGet Package**
   - Versionado semántico
   - Documentación completa
   - Ejemplos de uso

---

### 7.4 Resumen de Objetivos Cumplidos

| Objetivo | Estado | Notas |
|----------|--------|-------|
| Reutilización de código | ✅ Cumplido | Fudie como biblioteca compartida |
| Estandarización | ✅ Cumplido | Patrones consistentes en todo el proyecto |
| Productividad | ✅ Cumplido | Inyección automática, convenciones |
| Mantenibilidad | ✅ Cumplido | Vertical slices, separación de responsabilidades |
| Documentación | ✅ Cumplido | OpenAPI/Swagger automático |
| Validación | ✅ Cumplido | FluentValidation en dominio |
| Manejo de errores | ✅ Cumplido | Result pattern + GlobalExceptionHandler |
| Testabilidad | ✅ Cumplido | Interfaces segregadas, DI |

---

### 7.5 Diagrama de Flujo Completo

```
┌──────────────────────────────────────────────────────────────┐
│                    HTTP Request                              │
│                  POST /pizzas                                │
│                  { name, description, url, ingredients }     │
└──────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌──────────────────────────────────────────────────────────────┐
│                  ASP.NET Core Pipeline                       │
│                                                              │
│  1. Routing (MapFeatures)                                   │
│  2. Model Binding                                           │
│  3. Middleware (CORS, Exception Handler, etc.)              │
└──────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌──────────────────────────────────────────────────────────────┐
│              CreatePizza.IFeatureModule                      │
│                                                              │
│  app.MapPost("/pizzas", async (IService service, Request) │
│  {                                                          │
│      var response = await service.HandlerAsync(request);   │
│      return Results.Created("", response);                 │
│  })                                                         │
└──────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌──────────────────────────────────────────────────────────────┐
│              CreatePizza.Service                             │
│                                                              │
│  1. Pizza.Create() → Validación de dominio                  │
│     ├─ FluentValidation                                     │
│     ├─ Validación de Id                                     │
│     └─ Result<Pizza>                                        │
│                                                              │
│  2. lookupRepository.GetOrThrowAsync<Ingredient>()          │
│     ├─ ApplicationDbContext.GetOrThrowAsync()               │
│     └─ KeyNotFoundException si no existe                    │
│                                                              │
│  3. pizza.AddIngredient(ingredient)                         │
│     ├─ Validación de negocio (no duplicados)                │
│     └─ Result                                               │
│                                                              │
│  4. pizzaRepository.Add(pizza)                              │
│     └─ Entry(pizza).State = EntityState.Added               │
│                                                              │
│  5. unitOfWork.SaveChangesAsync()                           │
│     └─ ApplicationDbContext.SaveChangesAsync()              │
└──────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌──────────────────────────────────────────────────────────────┐
│                  Response Mapping                            │
│                                                              │
│  new Response(                                              │
│      pizza.Id,                                              │
│      pizza.Name,                                            │
│      pizza.Description,                                     │
│      pizza.Url,                                             │
│      pizza.Price,  ← Calculado: Sum(ingredients.Cost)*1.20 │
│      pizza.Ingredients.Select(...)                          │
│  )                                                          │
└──────────────────────────────────────────────────────────────┘
                            │
                            ↓
┌──────────────────────────────────────────────────────────────┐
│                    HTTP Response                             │
│                  201 Created                                 │
│                  { id, name, description, url, price, ... }  │
└──────────────────────────────────────────────────────────────┘
```

---

## 📚 Referencias

- **Clean Architecture**: Robert C. Martin
- **Domain-Driven Design**: Eric Evans
- **Vertical Slice Architecture**: Jimmy Bogard
- **CQRS**: Greg Young
- **Result Pattern**: Vladimir Khorikov
- **RFC 7807**: Problem Details for HTTP APIs

---

**Fin del Análisis**
