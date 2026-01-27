# Estilo: Enum

## Estructura

```csharp
namespace {Project}.Features.{Feature}.Domain.{Aggregate}Aggregate.Enums;

/// <summary>
/// {Descripción breve del enum}.
/// </summary>
/// <remarks>
/// {Contexto de uso del enum}.
/// </remarks>
public enum {EnumName}
{
    /// <summary>
    /// {Descripción del valor}.
    /// </summary>
    /// <remarks>
    /// {Detalle adicional si aplica}.
    /// </remarks>
    Value1 = 1,

    /// <summary>
    /// {Descripción del valor}.
    /// </summary>
    Value2 = 2
}
```

## Reglas

- Namespace: `{Project}.Features.{Feature}.Domain.{Aggregate}Aggregate.Enums`
- **No incluir `using` en el archivo** → Todos los `using` van en `GlobalUsings.cs`
- Valores numéricos explícitos empezando en 1
- XML docs en enum y cada valor
- `<summary>` obligatorio, `<remarks>` opcional
