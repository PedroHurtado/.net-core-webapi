namespace Fudie.Infrastructure.Generator.QueryMethod;

/// <summary>
/// Tipo de query a ejecutar
/// </summary>
public enum QueryType
{
    /// <summary>
    /// Consulta que retorna entidades (FindBy, FindFirstBy, FindTopNBy)
    /// </summary>
    Find,

    /// <summary>
    /// Consulta que cuenta registros (CountBy)
    /// </summary>
    Count,

    /// <summary>
    /// Consulta que verifica existencia (ExistsBy)
    /// </summary>
    Exists,

    /// <summary>
    /// Operación de eliminación (DeleteBy)
    /// </summary>
    Delete
}

/// <summary>
/// Operadores de comparación soportados
/// </summary>
public enum Operator
{
    /// <summary>
    /// Igualdad (=)
    /// </summary>
    Equal,

    /// <summary>
    /// Desigualdad (!=)
    /// </summary>
    NotEqual,

    /// <summary>
    /// Menor que (<)
    /// </summary>
    LessThan,

    /// <summary>
    /// Menor o igual que (<=)
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Mayor que (>)
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Mayor o igual que (>=)
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Entre dos valores (>= min && <= max)
    /// </summary>
    Between,

    /// <summary>
    /// Valor está en colección (Contains)
    /// </summary>
    In,

    /// <summary>
    /// Valor no está en colección (!Contains)
    /// </summary>
    NotIn,

    /// <summary>
    /// Comienza con (StartsWith)
    /// </summary>
    StartsWith,

    /// <summary>
    /// Termina con (EndsWith)
    /// </summary>
    EndsWith,

    /// <summary>
    /// Contiene (Contains)
    /// </summary>
    Contains,

    /// <summary>
    /// Patrón LIKE de SQL
    /// </summary>
    Like,

    /// <summary>
    /// Es nulo (== null)
    /// </summary>
    IsNull,

    /// <summary>
    /// No es nulo (!= null)
    /// </summary>
    IsNotNull,

    /// <summary>
    /// Es verdadero (== true)
    /// </summary>
    True,

    /// <summary>
    /// Es falso (== false)
    /// </summary>
    False
}

/// <summary>
/// Condición de filtrado en una query
/// </summary>
/// <param name="Property">Nombre de la propiedad a filtrar</param>
/// <param name="Op">Operador a aplicar</param>
/// <param name="Or">Si true, se une con OR en lugar de AND</param>
/// <param name="IgnoreCase">Si true, aplicar comparación case-insensitive (solo para strings)</param>
public record Condition(
    string Property,
    Operator Op,
    bool Or = false,
    bool IgnoreCase = false
);

/// <summary>
/// Ordenamiento en una query
/// </summary>
/// <param name="Property">Nombre de la propiedad por la que ordenar</param>
/// <param name="Descending">Si true, orden descendente; si false, ascendente</param>
public record OrderBy(
    string Property,
    bool Descending = false
);

/// <summary>
/// Query parseada desde el nombre de un método
/// </summary>
public record ParsedQuery
{
    /// <summary>
    /// Tipo de query (Find, Count, Exists, Delete)
    /// </summary>
    public QueryType Type { get; init; }

    /// <summary>
    /// Si true, retornar solo el primer resultado (FindFirstBy)
    /// </summary>
    public bool First { get; init; }

    /// <summary>
    /// Número de resultados a limitar (FindTopNBy)
    /// </summary>
    public int? Top { get; init; }

    /// <summary>
    /// Lista de condiciones de filtrado
    /// </summary>
    public List<Condition> Conditions { get; init; } = new();

    /// <summary>
    /// Lista de ordenamientos
    /// </summary>
    public List<OrderBy> OrderBy { get; init; } = new();
}

/// <summary>
/// Resultado del parsing de un nombre de método
/// </summary>
public record ParseResult
{
    /// <summary>
    /// Si true, el parsing fue exitoso
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Query parseada (solo si Success = true)
    /// </summary>
    public ParsedQuery? Query { get; init; }

    /// <summary>
    /// Mensaje de error (solo si Success = false)
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Posición en el nombre del método donde ocurrió el error
    /// </summary>
    public int? ErrorPosition { get; init; }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    public static ParseResult Ok(ParsedQuery query) => new()
    {
        Success = true,
        Query = query
    };

    /// <summary>
    /// Crea un resultado con error
    /// </summary>
    public static ParseResult Error(string message, int? position = null) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorPosition = position
    };
}
