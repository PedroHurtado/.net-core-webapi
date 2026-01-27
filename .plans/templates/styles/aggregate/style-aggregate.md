# Estilo: Aggregate

## Estructura

```csharp
namespace {Project}.Features.{Feature}.Domain.{Aggregate}Aggregate;

/// <summary>
/// {Descripción breve del agregado}.
/// </summary>
/// <remarks>
/// <para>{Explicación del patrón y organización}.</para>
/// <para>{Contexto de negocio}.</para>
/// </remarks>
public partial class {AggregateName} : AggregateRoot<{IdType}>
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
    /// <value>A read-only collection of <see cref="{ItemType}"/> instances.</value>
    public IReadOnlyCollection<{ItemType}> {Collection} => _{collection}.ToList().AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether {condición}.
    /// </summary>
    /// <value><c>true</c> if {condición}; otherwise, <c>false</c>.</value>
    public bool {ComputedProperty} => _{collection}.Any(x => x.{Condition});

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected {AggregateName}() : base({DefaultId}) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public {AggregateName}({IdType} id) : base(id) { }
}

public static class {AggregateName}ValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string {Property}Required = "{Property} is required";
}

/// <summary>
/// Provides validation rules for the <see cref="{AggregateName}"/> aggregate root.
/// </summary>
public class {AggregateName}Validator : AbstractValidator<{AggregateName}>
{
    public {AggregateName}Validator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage({AggregateName}ValidationMessages.IdRequired);

        RuleFor(x => x.{Property})
            .NotEmpty()
            .WithMessage({AggregateName}ValidationMessages.{Property}Required);
    }
}
```

## Reglas

- Namespace: `{Project}.Features.{Feature}.Domain.{Aggregate}Aggregate`
- **No incluir `using` en el archivo** → Todos los `using` van en `GlobalUsings.cs`
- Hereda de `AggregateRoot<T>`
- `partial class` (permite comandos en archivos separados)
- Propiedades con `{ get; protected set; }`
- Colecciones: backing field `HashSet<T>` + propiedad `IReadOnlyCollection<T>`
- Constructor vacío `protected` para ORM
- Constructor con Id `public`
- XML docs con `<summary>`, `<value>`, `<remarks>`, `<param>`
- Mensajes de validación en clase estática
- Validator en el mismo archivo
