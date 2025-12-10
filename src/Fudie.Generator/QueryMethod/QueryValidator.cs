using Microsoft.CodeAnalysis;

namespace Fudie.Generator.QueryMethod;

/// <summary>
/// Validador de queries parseadas
/// </summary>
public class QueryValidator
{
    /// <summary>
    /// Valida una query parseada contra un método y tipo de entidad
    /// </summary>
    /// <param name="query">Query parseada</param>
    /// <param name="method">Método a validar</param>
    /// <param name="entityType">Tipo de entidad</param>
    /// <param name="location">Ubicación en el código fuente</param>
    /// <returns>Lista de diagnósticos (vacía si no hay errores)</returns>
    public List<Diagnostic> Validate(
        ParsedQuery query,
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        Location? location)
    {
        var diagnostics = new List<Diagnostic>();

        // 1. Validar existencia de propiedades en condiciones
        foreach (var condition in query.Conditions)
        {
            ValidatePropertyExists(condition.Property, entityType, location, diagnostics);
        }

        // 2. Validar existencia de propiedades en OrderBy
        foreach (var orderBy in query.OrderBy)
        {
            ValidatePropertyExists(orderBy.Property, entityType, location, diagnostics);
        }

        // 3. Validar compatibilidad de operadores con tipos de propiedades
        foreach (var condition in query.Conditions)
        {
            var property = FindProperty(condition.Property, entityType);
            if (property != null)
            {
                ValidateOperatorCompatibility(condition.Op, property, location, diagnostics);
            }
        }

        // 4. Validar cantidad de parámetros
        ValidateParameterCount(query, method, entityType, location, diagnostics);

        // 5. Validar tipo de retorno
        ValidateReturnType(query, method, entityType, location, diagnostics);

        // 6. Validar compatibilidad de tipos de parámetros
        ValidateParameterTypes(query, method, entityType, location, diagnostics);

        return diagnostics;
    }

    /// <summary>
    /// Valida que una propiedad exista en la entidad
    /// </summary>
    private void ValidatePropertyExists(
        string propertyName,
        INamedTypeSymbol entityType,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var property = FindProperty(propertyName, entityType);
        if (property == null)
        {
            var suggestion = FindSimilarPropertyName(propertyName, entityType);
            diagnostics.Add(Diagnostics.CreatePropertyNotFound(
                propertyName,
                entityType.Name,
                suggestion,
                location
            ));
        }
    }

    /// <summary>
    /// Valida que un operador sea compatible con el tipo de propiedad
    /// </summary>
    private void ValidateOperatorCompatibility(
        Operator op,
        IPropertySymbol property,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var propertyType = property.Type;
        var typeName = propertyType.ToDisplayString();

        switch (op)
        {
            case Operator.LessThan:
            case Operator.LessThanOrEqual:
            case Operator.GreaterThan:
            case Operator.GreaterThanOrEqual:
            case Operator.Between:
                if (!IsNumericOrDateType(propertyType))
                {
                    diagnostics.Add(Diagnostics.CreateIncompatibleOperator(
                        op.ToString(),
                        typeName,
                        location
                    ));
                }
                break;

            case Operator.StartsWith:
            case Operator.EndsWith:
            case Operator.Contains:
            case Operator.Like:
                if (!IsStringType(propertyType))
                {
                    diagnostics.Add(Diagnostics.CreateIncompatibleOperator(
                        op.ToString(),
                        typeName,
                        location
                    ));
                }
                break;

            case Operator.True:
            case Operator.False:
                if (!IsBooleanType(propertyType))
                {
                    diagnostics.Add(Diagnostics.CreateIncompatibleOperator(
                        op.ToString(),
                        typeName,
                        location
                    ));
                }
                break;
        }
    }

    /// <summary>
    /// Valida la cantidad de parámetros del método
    /// </summary>
    private void ValidateParameterCount(
        ParsedQuery query,
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var expectedCount = CalculateExpectedParameterCount(query, entityType);
        var actualCount = method.Parameters.Length;

        if (actualCount < expectedCount)
        {
            // Faltan parámetros
            diagnostics.Add(Diagnostics.CreateMissingParameter(
                "parameter",
                method.Name,
                location
            ));
        }
        else if (actualCount > expectedCount)
        {
            // Sobran parámetros
            diagnostics.Add(Diagnostics.CreateWrongParameterCount(
                method.Name,
                actualCount,
                expectedCount,
                location
            ));
        }
    }

    /// <summary>
    /// Calcula la cantidad esperada de parámetros según la query
    /// </summary>
    private int CalculateExpectedParameterCount(ParsedQuery query, INamedTypeSymbol entityType)
    {
        var count = 0;

        foreach (var condition in query.Conditions)
        {
            count += GetParameterCountForOperator(condition.Op);
        }

        return count;
    }

    /// <summary>
    /// Retorna la cantidad de parámetros que requiere un operador
    /// </summary>
    private int GetParameterCountForOperator(Operator op)
    {
        return op switch
        {
            Operator.Between => 2,
            Operator.IsNull => 0,
            Operator.IsNotNull => 0,
            Operator.True => 0,
            Operator.False => 0,
            _ => 1
        };
    }

    /// <summary>
    /// Valida el tipo de retorno del método
    /// </summary>
    private void ValidateReturnType(
        ParsedQuery query,
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var returnType = method.ReturnType;

        // Debe ser Task<T>
        if (returnType is not INamedTypeSymbol namedReturnType ||
            namedReturnType.Name != "Task")
        {
            diagnostics.Add(Diagnostics.CreateWrongReturnType(
                method.Name,
                GetExpectedReturnType(query, entityType),
                returnType.ToDisplayString(),
                location
            ));
            return;
        }

        // Task sin argumentos solo es válido para Delete
        if (namedReturnType.TypeArguments.Length == 0)
        {
            if (query.Type != QueryType.Delete)
            {
                diagnostics.Add(Diagnostics.CreateWrongReturnType(
                    method.Name,
                    GetExpectedReturnType(query, entityType),
                    returnType.ToDisplayString(),
                    location
                ));
            }
            return;
        }

        var taskArgument = namedReturnType.TypeArguments[0];

        switch (query.Type)
        {
            case QueryType.Find:
                // FindBy puede retornar:
                // - Task<T?> o Task<T> (elemento único)
                // - Task<List<T>> o Task<IEnumerable<T>> (lista)
                var isSingleElement = IsNullableOf(taskArgument, entityType) ||
                                     SymbolEqualityComparer.Default.Equals(taskArgument, entityType);
                var isList = IsListOf(taskArgument, entityType) || IsEnumerableOf(taskArgument, entityType);

                if (!isSingleElement && !isList)
                {
                    diagnostics.Add(Diagnostics.CreateWrongReturnType(
                        method.Name,
                        query.First || query.Top == 1
                            ? $"Task<{entityType.Name}?>"
                            : $"Task<List<{entityType.Name}>> or Task<{entityType.Name}?>",
                        returnType.ToDisplayString(),
                        location
                    ));
                }
                break;

            case QueryType.Count:
                // Debe ser Task<int> o Task<long>
                if (taskArgument.SpecialType != SpecialType.System_Int32 &&
                    taskArgument.SpecialType != SpecialType.System_Int64)
                {
                    diagnostics.Add(Diagnostics.CreateWrongReturnType(
                        method.Name,
                        "Task<int>",
                        returnType.ToDisplayString(),
                        location
                    ));
                }
                break;

            case QueryType.Exists:
                // Debe ser Task<bool>
                if (taskArgument.SpecialType != SpecialType.System_Boolean)
                {
                    diagnostics.Add(Diagnostics.CreateWrongReturnType(
                        method.Name,
                        "Task<bool>",
                        returnType.ToDisplayString(),
                        location
                    ));
                }
                break;

            case QueryType.Delete:
                // Debe ser Task<int>
                if (taskArgument.SpecialType != SpecialType.System_Int32)
                {
                    diagnostics.Add(Diagnostics.CreateWrongReturnType(
                        method.Name,
                        "Task<int>",
                        returnType.ToDisplayString(),
                        location
                    ));
                }
                break;
        }
    }

    /// <summary>
    /// Obtiene el tipo de retorno esperado como string
    /// </summary>
    private string GetExpectedReturnType(ParsedQuery query, INamedTypeSymbol entityType)
    {
        return query.Type switch
        {
            QueryType.Find when query.First || query.Top == 1 => $"Task<{entityType.Name}?>",
            QueryType.Find => $"Task<List<{entityType.Name}>>",
            QueryType.Count => "Task<int>",
            QueryType.Exists => "Task<bool>",
            QueryType.Delete => "Task<int>",
            _ => "Task"
        };
    }

    /// <summary>
    /// Valida los tipos de los parámetros
    /// </summary>
    private void ValidateParameterTypes(
        ParsedQuery query,
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var parameterIndex = 0;

        foreach (var condition in query.Conditions)
        {
            var property = FindProperty(condition.Property, entityType);
            if (property == null)
                continue; // Ya reportado en ValidatePropertyExists

            var paramCount = GetParameterCountForOperator(condition.Op);

            for (int i = 0; i < paramCount; i++)
            {
                if (parameterIndex >= method.Parameters.Length)
                    break; // Ya reportado en ValidateParameterCount

                var parameter = method.Parameters[parameterIndex];
                var propertyType = property.Type;

                // Para In/NotIn, el parámetro debe ser IEnumerable<PropertyType>
                if (condition.Op == Operator.In || condition.Op == Operator.NotIn)
                {
                    if (!IsEnumerableOf(parameter.Type, propertyType))
                    {
                        diagnostics.Add(Diagnostics.CreateTypeMismatch(
                            parameter.Name,
                            parameter.Type.ToDisplayString(),
                            property.Name,
                            $"IEnumerable<{propertyType.ToDisplayString()}>",
                            location
                        ));
                    }
                }
                else
                {
                    // Validar compatibilidad de tipos
                    if (!AreTypesCompatible(parameter.Type, propertyType))
                    {
                        diagnostics.Add(Diagnostics.CreateTypeMismatch(
                            parameter.Name,
                            parameter.Type.ToDisplayString(),
                            property.Name,
                            propertyType.ToDisplayString(),
                            location
                        ));
                    }
                }

                parameterIndex++;
            }
        }
    }

    #region Helper Methods

    /// <summary>
    /// Busca una propiedad en la entidad (incluyendo clases base)
    /// </summary>
    private IPropertySymbol? FindProperty(string propertyName, INamedTypeSymbol entityType)
    {
        var currentType = entityType;
        while (currentType != null)
        {
            var property = currentType.GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

            if (property != null)
                return property;

            currentType = currentType.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Busca una propiedad similar usando distancia de Levenshtein
    /// </summary>
    private string? FindSimilarPropertyName(string propertyName, INamedTypeSymbol entityType)
    {
        var properties = GetAllProperties(entityType);
        var minDistance = int.MaxValue;
        string? bestMatch = null;

        foreach (var property in properties)
        {
            var distance = CalculateLevenshteinDistance(propertyName, property.Name);
            if (distance <= 3 && distance < minDistance)
            {
                minDistance = distance;
                bestMatch = property.Name;
            }
        }

        return bestMatch;
    }

    /// <summary>
    /// Obtiene todas las propiedades de la entidad (incluyendo heredadas)
    /// </summary>
    private List<IPropertySymbol> GetAllProperties(INamedTypeSymbol entityType)
    {
        var properties = new List<IPropertySymbol>();
        var currentType = entityType;

        while (currentType != null)
        {
            properties.AddRange(currentType.GetMembers().OfType<IPropertySymbol>());
            currentType = currentType.BaseType;
        }

        return properties;
    }

    /// <summary>
    /// Calcula la distancia de Levenshtein entre dos strings
    /// </summary>
    private int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var distance = new int[sourceLength + 1, targetLength + 1];

        for (int i = 0; i <= sourceLength; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= targetLength; j++)
            distance[0, j] = j;

        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                var cost = char.ToLower(source[i - 1]) == char.ToLower(target[j - 1]) ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost
                );
            }
        }

        return distance[sourceLength, targetLength];
    }

    /// <summary>
    /// Verifica si un tipo es numérico o de fecha
    /// </summary>
    private bool IsNumericOrDateType(ITypeSymbol type)
    {
        var unwrapped = UnwrapNullable(type);

        return unwrapped.SpecialType switch
        {
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_DateTime => true,
            _ => unwrapped.ToDisplayString() switch
            {
                "System.DateOnly" => true,
                "System.TimeOnly" => true,
                "System.DateTimeOffset" => true,
                _ => false
            }
        };
    }

    /// <summary>
    /// Verifica si un tipo es string
    /// </summary>
    private bool IsStringType(ITypeSymbol type)
    {
        return type.SpecialType == SpecialType.System_String;
    }

    /// <summary>
    /// Verifica si un tipo es boolean
    /// </summary>
    private bool IsBooleanType(ITypeSymbol type)
    {
        var unwrapped = UnwrapNullable(type);
        return unwrapped.SpecialType == SpecialType.System_Boolean;
    }

    /// <summary>
    /// Verifica si dos tipos son compatibles
    /// </summary>
    private bool AreTypesCompatible(ITypeSymbol parameterType, ITypeSymbol propertyType)
    {
        // Igualdad exacta
        if (SymbolEqualityComparer.Default.Equals(parameterType, propertyType))
            return true;

        // Unwrap nullables
        var unwrappedParam = UnwrapNullable(parameterType);
        var unwrappedProp = UnwrapNullable(propertyType);

        // Igualdad después de unwrap
        if (SymbolEqualityComparer.Default.Equals(unwrappedParam, unwrappedProp))
            return true;

        // Conversiones numéricas implícitas
        if (IsNumericType(unwrappedParam) && IsNumericType(unwrappedProp))
            return true;

        return false;
    }

    /// <summary>
    /// Verifica si un tipo es numérico
    /// </summary>
    private bool IsNumericType(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            _ => false
        };
    }

    /// <summary>
    /// Desenvuelve un tipo nullable
    /// </summary>
    private ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0];
        }

        return type;
    }

    /// <summary>
    /// Verifica si un tipo es T? (nullable de otro tipo)
    /// </summary>
    private bool IsNullableOf(ITypeSymbol type, ITypeSymbol innerType)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], innerType);
        }

        // Para tipos de referencia, el tipo nullable es el mismo tipo
        if (type.IsReferenceType)
        {
            return SymbolEqualityComparer.Default.Equals(type, innerType);
        }

        return false;
    }

    /// <summary>
    /// Verifica si un tipo es List<T>
    /// </summary>
    private bool IsListOf(ITypeSymbol type, ITypeSymbol elementType)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.Name == "List" &&
            namedType.TypeArguments.Length == 1)
        {
            return SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], elementType);
        }

        return false;
    }

    /// <summary>
    /// Verifica si un tipo es IEnumerable<T>
    /// </summary>
    private bool IsEnumerableOf(ITypeSymbol type, ITypeSymbol elementType)
    {
        if (type is INamedTypeSymbol namedType)
        {
            // Verificar si es IEnumerable<T> directamente
            if (namedType.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
            {
                return SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], elementType);
            }

            // Verificar interfaces implementadas
            foreach (var iface in namedType.AllInterfaces)
            {
                if (iface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>" &&
                    iface.TypeArguments.Length == 1)
                {
                    return SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], elementType);
                }
            }
        }

        return false;
    }

    #endregion
}
