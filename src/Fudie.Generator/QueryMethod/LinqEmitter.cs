using System.Text;

namespace Fudie.Generator.QueryMethod;

/// <summary>
/// Generador de código LINQ para queries parseadas
/// </summary>
public class LinqEmitter
{
    /// <summary>
    /// Genera el código completo del método
    /// </summary>
    /// <param name="query">Query parseada</param>
    /// <param name="methodName">Nombre del método</param>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="parameters">Nombres de los parámetros</param>
    /// <returns>Código C# generado</returns>
    public string Emit(ParsedQuery query, string methodName, string entityName, string[] parameters)
    {
        var sb = new StringBuilder();

        // Generar query base
        sb.Append($"_query.Query<{entityName}>()");

        // Generar Where si hay condiciones
        if (query.Conditions.Count > 0)
        {
            sb.AppendLine();
            sb.Append($"            .Where(x => {GenerateWhereExpression(query.Conditions, parameters)})");
        }

        // Generar OrderBy si existe
        if (query.OrderBy.Count > 0)
        {
            sb.AppendLine();
            sb.Append($"            {GenerateOrderByExpression(query.OrderBy)}");
        }

        // Generar Take si existe
        if (query.Top.HasValue)
        {
            sb.AppendLine();
            sb.Append($"            .Take({query.Top.Value})");
        }

        // Generar ejecución final
        sb.AppendLine();
        sb.Append($"            {GenerateFinalExecution(query)}");

        return sb.ToString();
    }

    /// <summary>
    /// Genera la firma del método
    /// </summary>
    /// <param name="query">Query parseada</param>
    /// <param name="methodName">Nombre del método</param>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="parameters">Parámetros con sus tipos</param>
    /// <returns>Firma del método</returns>
    public string EmitMethodSignature(
        ParsedQuery query,
        string methodName,
        string entityName,
        (string name, string type)[] parameters)
    {
        var returnType = GetReturnType(query, entityName);
        var paramList = string.Join(", ", parameters.Select(p => $"{p.type} {p.name}"));

        return $"public async {returnType} {methodName}({paramList})";
    }

    /// <summary>
    /// Obtiene el tipo de retorno según el tipo de query
    /// </summary>
    private string GetReturnType(ParsedQuery query, string entityName)
    {
        return query.Type switch
        {
            QueryType.Find when query.First || query.Top == 1 => $"Task<{entityName}?>",
            QueryType.Find => $"Task<List<{entityName}>>",
            QueryType.Count => "Task<int>",
            QueryType.Exists => "Task<bool>",
            QueryType.Delete => "Task<int>",
            _ => "Task"
        };
    }

    /// <summary>
    /// Genera la expresión Where completa
    /// </summary>
    private string GenerateWhereExpression(List<Condition> conditions, string[] parameters)
    {
        var sb = new StringBuilder();
        var paramIndex = 0;

        for (int i = 0; i < conditions.Count; i++)
        {
            var condition = conditions[i];

            // Agregar operador lógico si no es la primera condición
            if (i > 0)
            {
                sb.Append(condition.Or ? " || " : " && ");
            }

            // Generar la condición
            var conditionExpr = GenerateConditionExpression(condition, parameters, ref paramIndex);
            sb.Append(conditionExpr);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Genera una expresión de condición individual
    /// </summary>
    private string GenerateConditionExpression(Condition condition, string[] parameters, ref int paramIndex)
    {
        var property = $"x.{condition.Property}";

        // Aplicar ToLower si IgnoreCase
        if (condition.IgnoreCase)
        {
            property = $"{property}.ToLower()";
        }

        switch (condition.Op)
        {
            case Operator.Equal:
                var value = condition.IgnoreCase ? $"{parameters[paramIndex]}.ToLower()" : parameters[paramIndex];
                paramIndex++;
                return $"{property} == {value}";

            case Operator.NotEqual:
                value = condition.IgnoreCase ? $"{parameters[paramIndex]}.ToLower()" : parameters[paramIndex];
                paramIndex++;
                return $"{property} != {value}";

            case Operator.LessThan:
                paramIndex++;
                return $"{property} < {parameters[paramIndex - 1]}";

            case Operator.LessThanOrEqual:
                paramIndex++;
                return $"{property} <= {parameters[paramIndex - 1]}";

            case Operator.GreaterThan:
                paramIndex++;
                return $"{property} > {parameters[paramIndex - 1]}";

            case Operator.GreaterThanOrEqual:
                paramIndex++;
                return $"{property} >= {parameters[paramIndex - 1]}";

            case Operator.Between:
                var min = parameters[paramIndex++];
                var max = parameters[paramIndex++];
                return $"{property} >= {min} && {property} <= {max}";

            case Operator.In:
                var collection = parameters[paramIndex++];
                return $"{collection}.Contains({property})";

            case Operator.NotIn:
                collection = parameters[paramIndex++];
                return $"!{collection}.Contains({property})";

            case Operator.StartsWith:
                value = parameters[paramIndex++];
                return $"{property}.StartsWith({value})";

            case Operator.EndsWith:
                value = parameters[paramIndex++];
                return $"{property}.EndsWith({value})";

            case Operator.Contains:
                value = parameters[paramIndex++];
                return $"{property}.Contains({value})";

            case Operator.Like:
                value = parameters[paramIndex++];
                return $"EF.Functions.Like({property}, {value})";

            case Operator.IsNull:
                return $"{property} == null";

            case Operator.IsNotNull:
                return $"{property} != null";

            case Operator.True:
                return $"{property} == true";

            case Operator.False:
                return $"{property} == false";

            default:
                throw new InvalidOperationException($"Unsupported operator: {condition.Op}");
        }
    }

    /// <summary>
    /// Genera la expresión OrderBy
    /// </summary>
    private string GenerateOrderByExpression(List<OrderBy> orderByList)
    {
        if (orderByList.Count == 0)
            return string.Empty;

        var first = orderByList[0];
        var method = first.Descending ? "OrderByDescending" : "OrderBy";

        return $".{method}(x => x.{first.Property})";
    }

    /// <summary>
    /// Genera la ejecución final según el tipo de query
    /// </summary>
    private string GenerateFinalExecution(ParsedQuery query)
    {
        return query.Type switch
        {
            QueryType.Find when query.First || query.Top == 1 => ".FirstOrDefaultAsync()",
            QueryType.Find => ".ToListAsync()",
            QueryType.Count => ".CountAsync()",
            QueryType.Exists => ".AnyAsync()",
            QueryType.Delete => ".ExecuteDeleteAsync()",
            _ => throw new InvalidOperationException($"Unsupported query type: {query.Type}")
        };
    }

    /// <summary>
    /// Genera el método completo con firma y cuerpo
    /// </summary>
    /// <param name="query">Query parseada</param>
    /// <param name="methodName">Nombre del método</param>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="parameters">Parámetros con sus tipos</param>
    /// <returns>Código completo del método</returns>
    public string EmitFullMethod(
        ParsedQuery query,
        string methodName,
        string entityName,
        (string name, string type)[] parameters)
    {
        var sb = new StringBuilder();

        // Generar firma
        var signature = EmitMethodSignature(query, methodName, entityName, parameters);
        sb.AppendLine($"    {signature}");
        sb.AppendLine("    {");

        // Generar cuerpo
        var paramNames = parameters.Select(p => p.name).ToArray();
        var queryCode = Emit(query, methodName, entityName, paramNames);

        sb.AppendLine($"        return await {queryCode};");
        sb.AppendLine("    }");

        return sb.ToString();
    }
}
