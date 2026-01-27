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
public partial record {ValueObjectName}
{
    /// <summary>
    /// Gets {descripción de la propiedad}.
    /// </summary>
    /// <value>{Descripción del valor y restricciones}.</value>
    public {Type} {Property} { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="{ValueObjectName}"/> record.
    /// </summary>
    protected {ValueObjectName}(
        {Type} {param1},
        {Type} {param2})
    {
        {Property1} = {param1};
        {Property2} = {param2};
    }

    // Estáticos (opcional)
    public static {ValueObjectName} {Static1} => new(...);
    
    public static {ValueObjectName} FromX(string x) => x switch
    {
        "A" => Static1,
        _ => throw new ArgumentException($"... {x} not supported", nameof(x))
    };
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
- `partial record` (permite comandos en archivo separado)
- Constructor `protected`
- Propiedades con `{ get; }` (inmutables)
- XML docs con `<summary>`, `<value>`, `<remarks>`
- Mensajes de validación en clase estática `{VO}ValidationMessages`
- Validator en el mismo archivo
