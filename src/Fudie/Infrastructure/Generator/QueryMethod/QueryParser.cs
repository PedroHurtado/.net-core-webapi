using System.Text;

namespace Fudie.Infrastructure.Generator.QueryMethod;

/// <summary>
/// Parser de nombres de métodos de query basados en convenciones
/// </summary>
public class QueryParser
{
    private static readonly string[] Prefixes = new[]
    {
        "FindFirstBy",
        "FindTopBy", // Será seguido por un número
        "FindBy",
        "CountBy",
        "ExistsBy",
        "DeleteBy"
    };

    private static readonly Dictionary<string, Operator> OperatorKeywords = new()
    {
        { "LessThanEqual", Operator.LessThanOrEqual },
        { "LessThan", Operator.LessThan },
        { "GreaterThanEqual", Operator.GreaterThanOrEqual },
        { "GreaterThan", Operator.GreaterThan },
        { "Between", Operator.Between },
        { "NotIn", Operator.NotIn },
        { "In", Operator.In },
        { "IsNotNull", Operator.IsNotNull },
        { "IsNull", Operator.IsNull },
        { "StartingWith", Operator.StartsWith },
        { "EndingWith", Operator.EndsWith },
        { "Containing", Operator.Contains },
        { "Like", Operator.Like },
        { "Not", Operator.NotEqual },
        { "True", Operator.True },
        { "False", Operator.False }
    };

    /// <summary>
    /// Parsea un nombre de método y retorna la query parseada
    /// </summary>
    /// <param name="methodName">Nombre del método a parsear</param>
    /// <param name="entityProperties">Propiedades disponibles en la entidad</param>
    /// <returns>Resultado del parsing</returns>
    public ParseResult Parse(string methodName, IEnumerable<string> entityProperties)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            return ParseResult.Error("Method name cannot be empty");
        }

        var properties = entityProperties.ToList();
        var tokens = TokenizePascalCase(methodName);
        var position = 0;

        // 1. Detectar prefijo
        var prefixResult = DetectPrefix(tokens, ref position);
        if (!prefixResult.Success)
        {
            return prefixResult;
        }

        var query = prefixResult.Query!;

        // 2. Verificar que hay contenido después del prefijo
        if (position >= tokens.Count)
        {
            return ParseResult.Error("Method name must have conditions after prefix");
        }

        // 3. Parsear condiciones (hasta encontrar OrderBy o fin)
        var conditionsResult = ParseConditions(tokens, ref position, properties);
        if (!conditionsResult.Success)
        {
            return conditionsResult;
        }

        query = query with { Conditions = conditionsResult.Query!.Conditions };

        // 4. Parsear OrderBy si existe
        if (position < tokens.Count && tokens[position] == "Order")
        {
            var orderByResult = ParseOrderBy(tokens, ref position, properties);
            if (!orderByResult.Success)
            {
                return orderByResult;
            }

            query = query with { OrderBy = orderByResult.Query!.OrderBy };
        }

        // 5. Verificar que no quedan tokens sin procesar
        if (position < tokens.Count)
        {
            return ParseResult.Error($"Unexpected tokens after parsing: {string.Join("", tokens.Skip(position))}", position);
        }

        return ParseResult.Ok(query);
    }

    /// <summary>
    /// Tokeniza un string en PascalCase, separando también números
    /// </summary>
    private List<string> TokenizePascalCase(string input)
    {
        var tokens = new List<string>();
        var currentToken = new StringBuilder();
        var lastWasDigit = false;

        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            var isDigit = char.IsDigit(c);
            var isUpper = char.IsUpper(c);

            // Separar si:
            // 1. Encontramos mayúscula y ya hay contenido
            // 2. Cambiamos de letra a número o viceversa
            if (currentToken.Length > 0 && (isUpper || isDigit != lastWasDigit))
            {
                tokens.Add(currentToken.ToString());
                currentToken.Clear();
            }

            currentToken.Append(c);
            lastWasDigit = isDigit;
        }

        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens;
    }

    /// <summary>
    /// Detecta el prefijo del método
    /// </summary>
    private ParseResult DetectPrefix(List<string> tokens, ref int position)
    {
        var methodStart = string.Join("", tokens);

        // FindFirstBy
        if (methodStart.StartsWith("FindFirstBy"))
        {
            position = ConsumeTokens(tokens, 0, "Find", "First", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Find,
                First = true
            });
        }

        // FindTopNBy
        if (methodStart.StartsWith("FindTop"))
        {
            position = ConsumeTokens(tokens, 0, "Find", "Top");

            // Siguiente token debe ser un número
            if (position >= tokens.Count || !int.TryParse(tokens[position], out var topN))
            {
                return ParseResult.Error("Expected number after 'FindTop'", position);
            }

            position++; // Consumir número

            // Siguiente debe ser "By"
            if (position >= tokens.Count || tokens[position] != "By")
            {
                return ParseResult.Error("Expected 'By' after 'FindTop{N}'", position);
            }

            position++; // Consumir "By"

            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Find,
                Top = topN
            });
        }

        // FindBy
        if (methodStart.StartsWith("FindBy"))
        {
            position = ConsumeTokens(tokens, 0, "Find", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Find
            });
        }

        // CountBy
        if (methodStart.StartsWith("CountBy"))
        {
            position = ConsumeTokens(tokens, 0, "Count", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Count
            });
        }

        // ExistsBy
        if (methodStart.StartsWith("ExistsBy"))
        {
            position = ConsumeTokens(tokens, 0, "Exists", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Exists
            });
        }

        // DeleteBy
        if (methodStart.StartsWith("DeleteBy"))
        {
            position = ConsumeTokens(tokens, 0, "Delete", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Delete
            });
        }

        return ParseResult.Error($"Unknown method prefix. Expected one of: {string.Join(", ", Prefixes)}", 0);
    }

    /// <summary>
    /// Parsea las condiciones del método
    /// </summary>
    private ParseResult ParseConditions(List<string> tokens, ref int position, List<string> properties)
    {
        var conditions = new List<Condition>();
        var isOr = false;

        while (position < tokens.Count)
        {
            // Verificar si llegamos a OrderBy
            if (tokens[position] == "Order")
            {
                break;
            }

            // Parsear una condición
            var conditionResult = ParseSingleCondition(tokens, ref position, properties, isOr);
            if (!conditionResult.Success)
            {
                return conditionResult;
            }

            conditions.Add(conditionResult.Query!.Conditions[0]);

            // Verificar operador lógico (And/Or)
            if (position < tokens.Count)
            {
                if (tokens[position] == "And")
                {
                    position++;
                    isOr = false;
                }
                else if (tokens[position] == "Or")
                {
                    position++;
                    isOr = true;
                }
                else if (tokens[position] == "Order")
                {
                    // Llegamos a OrderBy
                    break;
                }
                else
                {
                    // No hay más operadores lógicos, terminamos
                    break;
                }
            }
        }

        if (conditions.Count == 0)
        {
            return ParseResult.Error("Expected at least one condition", position);
        }

        return ParseResult.Ok(new ParsedQuery
        {
            Conditions = conditions
        });
    }

    /// <summary>
    /// Parsea una única condición
    /// </summary>
    private ParseResult ParseSingleCondition(List<string> tokens, ref int position, List<string> properties, bool isOr)
    {
        var startPosition = position;

        // 1. Intentar encontrar la propiedad (puede ser compuesta)
        var propertyResult = FindProperty(tokens, ref position, properties);
        if (propertyResult == null)
        {
            return ParseResult.Error($"Could not find valid property starting at position {startPosition}", startPosition);
        }

        var propertyName = propertyResult;
        var op = Operator.Equal; // Por defecto
        var ignoreCase = false;

        // 2. Intentar encontrar operador
        if (position < tokens.Count)
        {
            var operatorResult = TryParseOperator(tokens, ref position);
            if (operatorResult.HasValue)
            {
                op = operatorResult.Value;
            }
        }

        // 3. Verificar IgnoreCase
        if (position < tokens.Count && tokens[position] == "Ignore" &&
            position + 1 < tokens.Count && tokens[position + 1] == "Case")
        {
            ignoreCase = true;
            position += 2;
        }

        var condition = new Condition(propertyName, op, isOr, ignoreCase);

        return ParseResult.Ok(new ParsedQuery
        {
            Conditions = new List<Condition> { condition }
        });
    }

    /// <summary>
    /// Intenta encontrar una propiedad en los tokens (puede ser compuesta como CreatedAt)
    /// </summary>
    private string? FindProperty(List<string> tokens, ref int position, List<string> properties)
    {
        // Intentar con múltiples tokens combinados (para propiedades compuestas)
        for (int length = 3; length >= 1; length--)
        {
            if (position + length > tokens.Count)
                continue;

            var combined = string.Join("", tokens.Skip(position).Take(length));

            if (properties.Contains(combined, StringComparer.OrdinalIgnoreCase))
            {
                position += length;
                return combined;
            }
        }

        return null;
    }

    /// <summary>
    /// Intenta parsear un operador
    /// </summary>
    private Operator? TryParseOperator(List<string> tokens, ref int position)
    {
        // Intentar con múltiples tokens (ej: "Less", "Than", "Equal")
        for (int length = 3; length >= 1; length--)
        {
            if (position + length > tokens.Count)
                continue;

            var combined = string.Join("", tokens.Skip(position).Take(length));

            if (OperatorKeywords.TryGetValue(combined, out var op))
            {
                position += length;
                return op;
            }
        }

        return null;
    }

    /// <summary>
    /// Parsea la cláusula OrderBy
    /// </summary>
    private ParseResult ParseOrderBy(List<string> tokens, ref int position, List<string> properties)
    {
        var orderByList = new List<OrderBy>();

        // Consumir "Order" "By"
        if (position >= tokens.Count || tokens[position] != "Order")
        {
            return ParseResult.Error("Expected 'Order'", position);
        }
        position++;

        if (position >= tokens.Count || tokens[position] != "By")
        {
            return ParseResult.Error("Expected 'By' after 'Order'", position);
        }
        position++;

        // Parsear propiedad
        var propertyResult = FindProperty(tokens, ref position, properties);
        if (propertyResult == null)
        {
            return ParseResult.Error("Expected property name after 'OrderBy'", position);
        }

        var descending = false;

        // Verificar Asc/Desc
        if (position < tokens.Count)
        {
            if (tokens[position] == "Desc")
            {
                descending = true;
                position++;
            }
            else if (tokens[position] == "Asc")
            {
                descending = false;
                position++;
            }
        }

        orderByList.Add(new OrderBy(propertyResult, descending));

        return ParseResult.Ok(new ParsedQuery
        {
            OrderBy = orderByList
        });
    }

    /// <summary>
    /// Consume tokens esperados y retorna la nueva posición
    /// </summary>
    private int ConsumeTokens(List<string> tokens, int position, params string[] expected)
    {
        foreach (var exp in expected)
        {
            if (position >= tokens.Count || tokens[position] != exp)
            {
                return position;
            }
            position++;
        }
        return position;
    }
}
