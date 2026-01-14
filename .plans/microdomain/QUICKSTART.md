# MicroDomain

## Estructura
```
Features/[Feature]/Domain/[Aggregate]/ → .cs, Commands/, Entities/, ValueObjects/, Enums/
Features/[Feature]/Domain/Shared/ → ValueObjects/, Enums/ compartidos
Features/[Feature]/Api/Commands/ y Queries/
Features/[Feature]/GlobalUsings.cs → TODOS los using (dominio sin using)
```

## Fases
1. Enum→ValueObject→Entity→Aggregate→DbContext (equipo, TDD)
2. Commands (paralelo, TDD)
3. Api (paralelo)

## Entity/Aggregate
- `partial class`, hereda `Entity<TId>` o `AggregateRoot<TId>`
- Constructores: `protected` vacío + `public(TId id)`
- Props: `{ get; protected set; }`, Id usa `{ get; init; }`
- Colecciones: `protected HashSet<T> _x = []` + `public IReadOnlyCollection<T> X => _x.ToList().AsReadOnly()`
- Validator + ValidationMessages en mismo archivo

## ValueObject
- `record` con `{ get; }`, constructor `private`
- Factory `Create()` retorna T con `ValidateOrThrow()`

## Commands
- Archivo: `[Entity]_[Action].cs`
- `partial class` + clase anidada + `[Injectable(ServiceLifetime.Singleton)]`
- Hereda: `AbstractCreateCommand<TCmd,TEntity>` o `AbstractModifyCommand<TCmd,TEntity>` o `AbstractModifyCommand<TEntity>`
- Inyecta validators/commands, NO usa `new`

## Guards
- 404: `NotFoundGuard.ThrowIfNull(entity, id)`
- 409: `ConflictGuard.ThrowIf(cond, msg)` → duplicados/conflictos
- 422: `ValidationGuard.ThrowIf(cond, msg, prop)` o `validator.ValidateOrThrow()`

## DbContext Firestore
- `DbSet<T>` solo agregados raíz
- `entity.ComplexProperty()` → ValueObject embebido
- `entity.SubCollection()` → colección anidada
- `entity.ArrayOf()` → array embebidos
- `entity.ArrayOf().AsReferences()` → array referencias
- `builder.Reference()` → propiedad referencia

## Conventions automáticas
PK auto, pluralización, enum→string, decimal→double, List<enum>→List<string>, GeoPoint auto, ArrayOf auto

## Api
- `static class` con nested: `record Request`, `record Response`, `class Handler`, `static MapEndpoint()`

## Ciclo: Test(red)→Implementar(green)→Refactor→PR
