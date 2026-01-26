# Estilo: Entity

## Estructura

```csharp
namespace {Project}.Features.{Feature}.Domain.{Aggregate}Aggregate.Entities;

/// <summary>
/// {Descripción breve de la entidad}.
/// </summary>
/// <remarks>
/// <para>{Explicación del propósito}.</para>
/// <para>{Relación con el agregado}.</para>
/// </remarks>
public partial class {EntityName} : Entity<{IdType}>
{
    /// <summary>
    /// Gets {descripción de la propiedad}.
    /// </summary>
    /// <value>{Descripción del valor y restricciones}.</value>
    public {Type} {Property} { get; protected set; } = {default};

    /// <summary>
    /// The internal collection of {items}.
    /// </summary>
    protected HashSet<{ItemType}> _{collection} = [];

    /// <summary>
    /// Gets the read-only collection of {items}.
    /// </summary>
    public IReadOnlyCollection<{ItemType}> {Collection} => _{collection}.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected {EntityName}() : base({DefaultId}) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public {EntityName}({IdType} id) : base(id) { }
}

public static class {EntityName}ValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string {Property}Required = "{Property} is required";
}

/// <summary>
/// Provides validation rules for the <see cref="{EntityName}"/> entity.
/// </summary>
public class {EntityName}Validator : AbstractValidator<{EntityName}>
{
    public {EntityName}Validator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage({EntityName}ValidationMessages.IdRequired);
    }
}
```

## Reglas

- Namespace: `{Project}.Features.{Feature}.Domain.{Aggregate}Aggregate.Entities`
- **No incluir `using` en el archivo** → Todos los `using` van en `GlobalUsings.cs`
- Hereda de `Entity<T>`
- `partial class` (permite comandos en archivos separados)
- Propiedades con `{ get; protected set; }`
- Colecciones: backing field `HashSet<T>` + propiedad `IReadOnlyCollection<T>`
- Constructor vacío `protected` para ORM
- Constructor con Id `public`
- XML docs con `<summary>`, `<value>`, `<param>`
- Mensajes de validación en clase estática
- Validator en el mismo archivo
