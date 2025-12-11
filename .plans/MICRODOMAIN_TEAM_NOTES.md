# Patrón MicroDomain - Notas para el Equipo

**Audiencia**: Desarrolladores Junior

---

## 🎯 Objetivo

Máximo paralelismo en desarrollo. Cada desarrollador trabaja en un **comando completo** sin conflictos de merge.

---

## 📁 Estructura
```
Domain/[Aggregate]/
├── [Aggregate].cs              ← Aggregate Root
├── Entities/
│   └── [Entity].cs             ← Entidades hijas
├── ValueObjects/
│   └── [ValueObject].cs        ← Objetos de valor
├── Enums/
│   └── [Enum].cs               ← Enumeraciones
└── Commands/
    └── [Aggregate]_[Action].cs ← Lógica de negocio
```

**Naming de archivos**: `Foo_Create.cs`, `FooBar_Add.cs` (ordenación alfabética por agregado)

---

## 📋 Reglas por Componente

### Entity (incluye Aggregate Root)
```csharp
public class Foo : Entity
{
    protected Foo() { }                    // ← EF Core
    public Foo(Guid id) : base(id) { }     // ← Creación
    
    public string Name { get; set; }       // ← Público, mutable
    public HashSet<Bar> Bars { get; set; } = [];
}

public class FooValidator : AbstractValidator<Foo>
{
    public FooValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

**Reglas**:
- ✅ Dos constructores (protected + public con Guid)
- ✅ Propiedades `{ get; set; }` públicas
- ✅ Colecciones: `HashSet<T>` con inicializador `= []`
- ✅ Validator en mismo archivo, clase separada
- ❌ NO lógica de negocio en la entidad

---

### ValueObject
```csharp
public record Baz
{
    public BazType Type { get; }
    public decimal Amount { get; }
    
    private Baz(BazType type, decimal amount)
    {
        Type = type;
        Amount = amount;
    }
    
    public static Baz Create(BazType type, decimal amount)
    {
        var instance = new Baz(type, amount);
        return new BazValidator().ValidateOrThrow(instance);
    }
}

public class BazValidator : AbstractValidator<Baz>
{
    public BazValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
```

**Reglas**:
- ✅ `record` con `{ get; }` (inmutable)
- ✅ Constructor `private`
- ✅ Factory `Create()` retorna `T` (no Result)
- ✅ Validación con `ValidateOrThrow()`
- ❌ NO constructor público

---

### Command
```csharp
namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

public record UpdateFooCommand(
    string Name,
    string? Description,
    int DisplayOrder
);

[Injectable]
public class UpdateFoo(
    IValidator<Foo> fooValidator
) : IModifyCommand<UpdateFooCommand, Foo>
{
    public Foo Execute(Foo entity, UpdateFooCommand command)
    {
        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        return fooValidator.ValidateOrThrow(entity);
    }
}
```

**Reglas**:
- ✅ Record para datos de entrada
- ✅ `[Injectable]` en la clase
- ✅ Inyectar `IValidator<T>` (no instanciar con `new`)
- ✅ Implementa `ICreateCommand<,>`, `IModifyCommand<,>` o `IModifyCommand<>`
- ✅ Retorna `TEntity` (permite logging y mapeo a response)
- ✅ Lanza excepciones si falla
- ❌ NO usar `Result<T>`, `try-catch`, ni `new Validator()`

---

## 📦 Namespaces Requeridos

| Namespace | Proporciona |
|-----------|-------------|
| `Fudie.Domain` | `Entity`, `AggregateRoot`, `ICreateCommand<,>`, `IModifyCommand<,>`, `IModifyCommand<>`, `ConflictException` |
| `Fudie.DependencyInjection` | `[Injectable]` para registro automático en DI |
| `Fudie.Validation` | `ValidationGuard`, `ConflictGuard`, `NotFoundGuard`, `ValidateOrThrow()` |
| `FluentValidation` | `IValidator<T>` para inyectar validators |

---

## 🔌 Interfaces

| Interfaz | Firma | Uso |
|----------|-------|-----|
| `ICreateCommand<TCmd, TEntity>` | `TEntity Execute(TCmd command)` | Crear entidad nueva |
| `IModifyCommand<TCmd, TEntity>` | `TEntity Execute(TEntity entity, TCmd command)` | Modificar existente |
| `IModifyCommand<TEntity>` | `TEntity Execute(TEntity entity)` | Modificar sin datos |

### Default Methods Async

Las interfaces `IModifyCommand` incluyen métodos default para uso fluent con repositorios:
```csharp
// En lugar de:
var entity = await repo.Get(id);
var updated = updateFoo.Execute(entity, cmd);

// Puedes usar:
var updated = await updateFoo.ExecuteAsync(repo.Get(id), cmd);
```

---

## 🛡️ Herramientas de Validación

| Herramienta | HTTP | Cuándo usar |
|-------------|------|-------------|
| `IValidator<T>.ValidateOrThrow(entity)` | 422 | Validación estructural (formato, rangos, requeridos) |
| `ValidationGuard.ThrowIf(condition, message, property)` | 422 | Reglas de negocio que invalidan los datos |
| `ConflictGuard.ThrowIf(condition, message)` | 409 | Conflictos con estado actual (duplicados, transiciones inválidas) |
| `NotFoundGuard.ThrowIfNull(entity)` | 404 | Entidad no existe (detecta nombre automáticamente) |
| `NotFoundGuard.ThrowIfNull(entity, id)` | 404 | Entidad no existe (incluye Id en mensaje) |

### Criterio 422 vs 409

| Pregunta | Código |
|----------|--------|
| ¿El dato en sí mismo es inválido? (formato, rango, vacío) | 422 |
| ¿El dato es válido pero choca con algo que ya existe? | 409 |

---

## 🔄 Flujo del Comando

1. **Buscar** entidades relacionadas con `NotFoundGuard.ThrowIfNull()`
2. **Crear** entidad/value object con datos del command
3. **Validar estructuralmente** con `validator.ValidateOrThrow()`
4. **Validar conflictos** con `ConflictGuard.ThrowIf()`
5. **Modificar** estado del agregado
6. **Retornar** agregado validado con `aggregateValidator.ValidateOrThrow()`

---

## 🔄 Flujo en Handler

### Opción A: Tradicional
```csharp
app.MapPut("/foos/{id}", async (
    Guid id,
    FooUpdateRequest request,
    UpdateFoo updateFoo,
    IGet<Foo, Guid> repo,
    IUnitOfWork uow,
    ILogger<Program> logger) =>
{
    var cmd = new UpdateFooCommand(request.Name, request.Description, request.DisplayOrder);
    var entity = await repo.Get(id);
    var updated = updateFoo.Execute(entity, cmd);
    logger.LogInformation("Updated Foo {Id}", updated.Id);
    await uow.SaveChangesAsync();
    return Results.Ok(MapToResponse(updated));
});
```

### Opción B: Fluent con ExecuteAsync
```csharp
app.MapPut("/foos/{id}", async (
    Guid id,
    FooUpdateRequest request,
    UpdateFoo updateFoo,
    IGet<Foo, Guid> repo,
    IUnitOfWork uow,
    ILogger<Program> logger) =>
{
    var cmd = new UpdateFooCommand(request.Name, request.Description, request.DisplayOrder);
    var updated = await updateFoo.ExecuteAsync(repo.Get(id), cmd);
    logger.LogInformation("Updated Foo {Id}", updated.Id);
    await uow.SaveChangesAsync();
    return Results.Ok(MapToResponse(updated));
});
```

**Nota**: `GlobalExceptionHandler` convierte las excepciones en respuestas HTTP apropiadas.

---

## ⚡ Orden de Desarrollo

### Fase 1: Modelo de Dominio (TODO EL EQUIPO)
```
1. Enums           (0 dependencias)
2. ValueObjects    (solo enums)
3. Entity hoja     (sin hijos)
4. Entity padre    (con colecciones)
5. Aggregate Root  
```

**Objetivo**: 
- Validar Domain Specification contra código real
- Todo el equipo conoce el dominio antes de desarrollar
- Detectar errores de diseño temprano

**Entregable**: PR con modelo completo + tests → Code Review conjunto

---

### Fase 2: Commands (EN PARALELO)
```
6. Commands        (cada dev toma uno o más)
```

**Requisito**: Fase 1 completada y aprobada

---

**Cada paso**: Código + Test → Validar → Siguiente

---

## ❌ Errores Comunes

| Error | Corrección |
|-------|------------|
| Lógica en Entity | Mover a Command |
| Constructor público en ValueObject | Usar factory `Create()` |
| Usar `Result<T>` | Lanzar excepciones con Guards |
| Instanciar validator con `new` en comando | Inyectar `IValidator<T>` |
| Usar `try-catch` para validaciones | Usar Guards apropiados |
| Duplicado → ValidationGuard (422) | Usar ConflictGuard (409) |
| `List<T>` en colecciones | Usar `HashSet<T>` |

---

## 📝 Checklist antes de PR

- [ ] Entity tiene dos constructores
- [ ] Validator en mismo archivo que Entity
- [ ] ValueObject usa factory `Create()` con `ValidateOrThrow()`
- [ ] Command tiene `[Injectable]`
- [ ] Command inyecta validators (no usa `new`)
- [ ] Command usa Guards apropiados (404/409/422)
- [ ] Command retorna entidad validada
- [ ] Tests cubren casos éxito y fallo
- [ ] Archivo nombrado `[Aggregate]_[Action].cs`

---

## 🎯 Beneficio Final

**Un comando = Un archivo = Un desarrollador = Cero conflictos**

---

*¿Dudas? Pregunta antes de implementar.*