# Estilo: Value Object

## Estructura

```csharp
namespace {Project}.Features.{Feature}.Domain.{Aggregate}Aggregate.ValueObjects;

/// <summary>
/// {Descripción breve del VO}.
/// </summary>
/// <remarks>
/// <para>{Explicación del propósito}.</para>
/// <para>{Detalles adicionales}.</para>
/// </remarks>
public partial record {ValueObjectName}(
    {Type} {Property1},
    {Type} {Property2}
)
{
    // Propiedades calculadas (opcional)
    public {Type} {Computed} => {expression};
}

public static class {ValueObjectName}ValidationMessages
{
    public const string {Property}Required = "{Property} is required";
    public const string {Property}MaxLength = "{Property} cannot exceed {n} characters";
}

/// <summary>
/// Provides validation rules for the <see cref="{ValueObjectName}"/> value object.
/// </summary>
public class {ValueObjectName}Validator : AbstractValidator<{ValueObjectName}>
{
    public {ValueObjectName}Validator()
    {
        RuleFor(x => x.{Property})
            .NotEmpty()
            .WithMessage({ValueObjectName}ValidationMessages.{Property}Required);
    }
}
```

## Reglas

- Namespace: `{Project}.Features.{Feature}.Domain.{Aggregate}Aggregate.ValueObjects`
- **No incluir `using` en el archivo** → Todos los `using` van en `GlobalUsings.cs`
- `partial record` con **positional parameters** (propiedades son los parámetros del constructor)
- Constructor **público** (implícito en positional record)
- **No declarar propiedades** `{ get; }` manualmente → Las genera el positional record
- **No asignar propiedades** en el constructor → Las asigna el positional record
- XML docs con `<summary>`, `<remarks>` en el record
- Solo incluir en el cuerpo del record: propiedades calculadas y métodos query
- **Métodos query**: devuelven datos derivados sin efectos secundarios (ej: `bool IsExpired => ...`, `decimal TotalWithTax => ...`)
- Mensajes de validación en clase estática `{VO}ValidationMessages`
- Validator en el mismo archivo

---

## ⛔ PROHIBIDO

- **NO incluir métodos command** (métodos que producen una nueva instancia o mutan estado) → Van en los comandos Create/Transform
- **NO crear factory methods** (`New`, `From`, `Create`, `WithX`)
- **NO crear métodos estáticos** de creación
- **NO incluir lógica de transformación** en el record → Siempre en `AbstractTransformCommand`
