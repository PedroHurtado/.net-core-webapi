# Estilo: Slice

## Estructura General

```csharp
namespace {Project}.Features.{Feature}.Api.{Commands|Queries}.{Aggregates};

public class {Action}{Aggregate} : IFeatureModule
{
    public record Request(...);
    public record Response(...);

    public static Func<IService, Request, Task<IResult>> Handler => 
        async (service, request) =>
        {
            var response = await service.HandleAsync(request);
            return Results.{ResultType}(...);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.Map{Method}("/{route}", Handler);
    }

    public interface IService
    {
        Task<Response> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(...) : IService
    {
        public async Task<Response> HandleAsync(Request request)
        {
            // Lógica
        }
    }

    public interface IRepository : I{RepositoryType}<{Aggregate}, {IdType}> { }
}
```

---

## Handler como Delegate

Extraer el handler permite testearlo unitariamente sin perder coverage:

```csharp
// ✅ Handler extraído - testeable
public static Func<IService, Request, Task<IResult>> Handler => 
    async (service, request) =>
    {
        var response = await service.HandleAsync(request);
        return Results.Created($"/allergens/{response.Id}", response);
    };

public void AddRoutes(IEndpointRouteBuilder app)
{
    app.MapPost("/allergens", Handler);
}
```

---

## Repository Interfaces

El generador crea la implementación automáticamente a partir de la interfaz.

### IAdd (Create)

```csharp
public interface IRepository : IAdd<{Aggregate}> { }
```
Genera: `void Add({Aggregate} entity)`

### IGet (Read)

```csharp
[AsNoTracking]
public interface IRepository : IGet<{Aggregate}, {IdType}> { }
```
Genera: `Task<{Aggregate}> Get({IdType} id)`

### IUpdate (Update)

```csharp
public interface IRepository : IUpdate<{Aggregate}, {IdType}> { }
```
Genera: `Task<{Aggregate}> Get({IdType} id)` (con tracking)

### IRemove (Delete)

```csharp
public interface IRepository : IRemove<{Aggregate}, {IdType}> { }
```
Genera: `Task<{Aggregate}> Get({IdType} id)` + `void Remove({Aggregate} entity)`

### IQuery (List/Search)

```csharp
[Injectable]
public class Service(IQuery query) : IService
{
    public async Task<List<Response>> HandleAsync()
    {
        return await query.Query<{Aggregate}>()
            .Where(...)
            .Select(x => new Response(...))
            .ToListAsync();
    }
}
```
No necesita interfaz IRepository, inyecta `IQuery` directamente.

---

## Repository Attributes

| Atributo | Uso |
|----------|-----|
| `[AsNoTracking]` | Queries de solo lectura (Get, List) |
| `[Include<T>("Nav.Prop")]` | Eager loading de navegaciones |

```csharp
[AsNoTracking]
[Include<Order>("Items.Product", "Customer")]
public interface IRepository : IGet<Order, Guid> { }
```

---

## Slices por Acción

### Create (POST)

```csharp
public class Create{Aggregate} : IFeatureModule
{
    public record Request(...);
    public record Response(...);

    public static Func<IService, Request, Task<IResult>> Handler => 
        async (service, request) =>
        {
            var response = await service.HandleAsync(request);
            return Results.Created($"/{route}/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{route}", Handler);
    }

    [Injectable]
    public class Service(
        {Aggregate}.Create create{Aggregate},
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<Response> HandleAsync(Request request)
        {
            var command = new Create{Aggregate}Command(...);
            var entity = create{Aggregate}.Execute(command);

            repository.Add(entity);
            await unitOfWork.SaveChangesAsync();

            return new Response(...);
        }
    }

    public interface IRepository : IAdd<{Aggregate}> { }
}
```

### Get (GET /{id})

```csharp
public class Get{Aggregate} : IFeatureModule
{
    public record Response(...);

    public static Func<IService, {IdType}, Task<IResult>> Handler => 
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{route}/{id}", Handler);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<Response> HandleAsync({IdType} id)
        {
            var entity = await repository.Get(id);
            return new Response(...);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<{Aggregate}, {IdType}> { }
}
```

### List (GET)

```csharp
public class Get{Aggregates} : IFeatureModule
{
    public record Response(...);

    public static Func<IService, Task<IResult>> Handler => 
        async (service) =>
        {
            var response = await service.HandleAsync();
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{route}", Handler);
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<Response>> HandleAsync()
        {
            return await query.Query<{Aggregate}>()
                .OrderBy(x => x.{Property})
                .Select(x => new Response(...))
                .ToListAsync();
        }
    }
}
```

### Update (PUT)

```csharp
public class Update{Aggregate} : IFeatureModule
{
    public record Request(...);
    public record Response(...);

    public static Func<IService, {IdType}, Request, Task<IResult>> Handler => 
        async (service, id, request) =>
        {
            var response = await service.HandleAsync(id, request);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{route}/{id}", Handler);
    }

    [Injectable]
    public class Service(
        {Aggregate}.Update update{Aggregate},
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<Response> HandleAsync({IdType} id, Request request)
        {
            var entity = await repository.Get(id);

            var command = new Update{Aggregate}Command(...);
            update{Aggregate}.Execute(entity, command);

            await unitOfWork.SaveChangesAsync();

            return new Response(...);
        }
    }

    public interface IRepository : IUpdate<{Aggregate}, {IdType}> { }
}
```

### Delete (DELETE)

```csharp
public class Delete{Aggregate} : IFeatureModule
{
    public static Func<IService, {IdType}, Task<IResult>> Handler => 
        async (service, id) =>
        {
            await service.HandleAsync(id);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{route}/{id}", Handler);
    }

    [Injectable]
    public class Service(
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync({IdType} id)
        {
            var entity = await repository.Get(id);
            repository.Remove(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IRemove<{Aggregate}, {IdType}> { }
}
```

---

## Reglas

- **No `using`** → Van en `GlobalUsings.cs`
- Todo en un archivo: Request, Response, Handler, IService, Service, IRepository
- Handler extraído como `Func<...>` para testabilidad
- Inyectar comandos de dominio (`{Aggregate}.Create`, `{Aggregate}.Update`)
- IRepository define solo la interfaz, el generador crea la implementación
- `IQuery` se inyecta directamente para listados
