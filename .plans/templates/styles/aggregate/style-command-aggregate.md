# Estilo: Comandos de Aggregate/Entity

## Guards Disponibles

| Guard | HTTP | Uso |
|-------|------|-----|
| `NotFoundGuard.ThrowIfNull(entity, message)` | 404 | Recurso no encontrado |
| `ConflictGuard.ThrowIf(condition, message)` | 409 | Conflictos de negocio, duplicados |
| `ValidationGuard.ThrowIf(condition, message, prop)` | 422 | Datos inválidos, reglas de negocio |

**Orden en comandos**: 404 → 409 → 422 → Lógica → ValidateOrThrow

---

## Comando Create

```csharp
namespace {Project}.Features.{Feature}.Domain.{Aggregate}Aggregate;

public record Create{Aggregate}Command(
    {Type} {Param1},
    {Type} {Param2},
    bool IsActive = false
);

public partial class {Aggregate}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        {ValueObject}.Create {valueObject}Create,  // Composición
        IValidator<{Aggregate}> {aggregate}Validator
    ) : AbstractCreateCommand<Create{Aggregate}Command, {Aggregate}>
    {
        public override {Aggregate} Execute(Create{Aggregate}Command command)
        {
            // Crear VO usando su comando (NUNCA new)
            var {valueObject} = {valueObject}Create.Execute(
                new Create{ValueObject}Command(command.{Param}));

            var {aggregate} = new {Aggregate}(Guid.NewGuid())
            {
                {Property1} = command.{Property1},
                {Property2} = {valueObject},
                IsActive = command.IsActive
            };

            return {aggregate}Validator.ValidateOrThrow({aggregate});
        }
    }
}
```

---

## Comando Update

```csharp
public record Update{Aggregate}Command(
    {Type} {Param1},
    {Type} {Param2}
);

public partial class {Aggregate}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        {ValueObject}.Create {valueObject}Create,
        IValidator<{Aggregate}> {aggregate}Validator
    ) : AbstractModifyCommand<Update{Aggregate}Command, {Aggregate}>
    {
        public override {Aggregate} Execute({Aggregate} {aggregate}, Update{Aggregate}Command command)
        {
            var {valueObject} = {valueObject}Create.Execute(
                new Create{ValueObject}Command(command.{Param}));

            {aggregate}.{Property1} = command.{Property1};
            {aggregate}.{Property2} = {valueObject};

            return {aggregate}Validator.ValidateOrThrow({aggregate});
        }
    }
}
```

---

## Comando Add{Item} (Colecciones)

```csharp
public record Add{Item}Command(
    {Type} {Key},
    {Type} {Param1}
);

public partial class {Aggregate}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Add{Item}(
        {Item}.Create {item}Create,  // Composición
        IValidator<{Aggregate}> {aggregate}Validator
    ) : AbstractModifyCommand<Add{Item}Command, {Aggregate}>
    {
        public override {Aggregate} Execute({Aggregate} {aggregate}, Add{Item}Command command)
        {
            // 409 - Duplicado
            ConflictGuard.ThrowIf(
                {aggregate}.{Collection}.Any(x => x.{Key} == command.{Key}),
                $"{Item} with {key} '{command.{Key}}' already exists");

            // Crear usando comando (NUNCA new)
            var {item} = {item}Create.Execute(new Create{Item}Command(
                command.{Key},
                command.{Param1}));

            {aggregate}._{collection}.Add({item});

            return {aggregate}Validator.ValidateOrThrow({aggregate});
        }
    }
}
```

---

## Comando Update{Item} (Colecciones)

```csharp
public record Update{Item}Command(
    {Type} {Param1}
);

public partial class {Aggregate}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update{Item}(
        {Item}.Create {item}Create,
        IValidator<{Aggregate}> {aggregate}Validator
    ) : AbstractModifyCommand<Update{Item}Command, {Aggregate}, {KeyType}>
    {
        public override {Aggregate} Execute({Aggregate} {aggregate}, Update{Item}Command command, {KeyType} {key})
        {
            // 404 - No existe
            var existing = {aggregate}.{Collection}.FirstOrDefault(x => x.{Key} == {key});
            NotFoundGuard.ThrowIfNull(existing, $"{Item} with {key} '{{key}}' not found");

            // Crear nuevo (inmutable)
            var updated = {item}Create.Execute(new Create{Item}Command(
                {key},
                command.{Param1}));

            {aggregate}._{collection}.Remove(existing);
            {aggregate}._{collection}.Add(updated);

            return {aggregate}Validator.ValidateOrThrow({aggregate});
        }
    }
}
```

---

## Comando Remove{Item} (Colecciones)

```csharp
public partial class {Aggregate}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Remove{Item}(
        IValidator<{Aggregate}> {aggregate}Validator
    ) : AbstractModifyCommand<{Aggregate}, {KeyType}>
    {
        public override {Aggregate} Execute({Aggregate} {aggregate}, {KeyType} {key})
        {
            // 404 - No existe
            var existing = {aggregate}.{Collection}.FirstOrDefault(x => x.{Key} == {key});
            NotFoundGuard.ThrowIfNull(existing, $"{Item} with {key} '{{key}}' not found");

            // 422 - Último en aggregate activo
            ValidationGuard.ThrowIf(
                {aggregate}.IsActive && {aggregate}.{Collection}.Count <= 1,
                "Cannot remove last {item} from active {aggregate}",
                nameof({aggregate}.{Collection}));

            {aggregate}._{collection}.Remove(existing);

            return {aggregate}Validator.ValidateOrThrow({aggregate});
        }
    }
}
```

---

## Comando Activate/Deactivate

```csharp
public partial class {Aggregate}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<{Aggregate}> {aggregate}Validator
    ) : AbstractModifyCommand<{Aggregate}>
    {
        public override {Aggregate} Execute({Aggregate} {aggregate})
        {
            // 409 - Ya activo
            ConflictGuard.ThrowIf({aggregate}.IsActive, "{Aggregate} is already active");

            // 422 - Requisitos
            ValidationGuard.ThrowIf(
                !{aggregate}.{Collection}.Any(),
                "{Aggregate} must have at least one {item}",
                nameof({aggregate}.{Collection}));

            {aggregate}.IsActive = true;

            return {aggregate}Validator.ValidateOrThrow({aggregate});
        }
    }
}
```

---

## Composición de Comandos

**Siempre inyectar comandos de VOs, nunca instanciar directamente:**

```csharp
// ✅ CORRECTO
public class AddFeature(
    Feature.Create featureCreate,  // Inyectado
    IValidator<Plan> planValidator
)
{
    public override Plan Execute(Plan plan, AddFeatureCommand command)
    {
        var feature = featureCreate.Execute(new CreateFeatureCommand(...));
        plan._features.Add(feature);
        return planValidator.ValidateOrThrow(plan);
    }
}

// ❌ INCORRECTO
var feature = new Feature(...);           // NO
var feature = Feature.New(...);           // NO
var feature = Feature.Create(...);        // NO (método estático)
```

---

## Reglas

- Archivo separado por comando: `{Aggregate}_Create.cs`, `{Aggregate}_Update.cs`, etc.
- Record del comando **fuera** de la clase parcial
- Clase del comando **dentro** de `partial class {Aggregate}`
- `Create` hereda de `AbstractCreateCommand<TCommand, TResult>`
- `Update/Add/Remove` heredan de `AbstractModifyCommand<...>`
- Atributo `[Injectable(ServiceLifetime.Singleton)]`
- Inyectar `IValidator<{Aggregate}>` siempre
- Inyectar `{ValueObject}.Create` para composición

---

## ⛔ PROHIBIDO

- **NO usar `new {ValueObject}(...)`** → Usar `{vo}Create.Execute(...)`
- **No usrar** Testable class
- **NO crear métodos estáticos** (`New`, `From`, `Create`)
- **NO crear factory methods** en Aggregate/Entity
- **NO modificar estado** fuera de comandos
- **NO acceder a backing fields** fuera de comandos


### Excepciones del dominio

| Guard | Excepción | Cuándo |
|-------|-----------|--------|
| ValidationGuard | `ValidationException` | Validación de datos fallida |
| ConflictGuard | `ConflictException` | Estado inválido (duplicado, ya activo, etc.) |
| NotFoundGuard | `KeyNotFoundException` | Entidad no encontrada |

Estas son las ÚNICAS excepciones que el dominio lanza. No inventes otras.
