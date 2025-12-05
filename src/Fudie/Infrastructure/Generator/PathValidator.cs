using Microsoft.CodeAnalysis;


namespace Fudie.Infrastructure.Generator;

/// <summary>
/// Valida paths de navegación para Include en Entity Framework
/// </summary>
internal static class PathValidator
{
    /// <summary>
    /// Información sobre el resultado de validación de un path
    /// </summary>
    public record IncludePathInfo(
        string OriginalPath,
        string[] Segments,
        SegmentInfo[] SegmentDetails,
        bool IsValid,
        string? ErrorMessage,
        Location? Location
    );

    /// <summary>
    /// Información detallada sobre un segmento del path
    /// </summary>
    public record SegmentInfo(
        string PropertyName,
        INamedTypeSymbol ContainingType,
        IPropertySymbol PropertySymbol,
        ITypeSymbol PropertyType,
        INamedTypeSymbol? ElementType,
        bool IsCollection
    );

    /// <summary>
    /// Valida un path de navegación completo
    /// </summary>
    /// <param name="path">Path a validar (ej: "Orders.OrderItems.Product")</param>
    /// <param name="rootEntityType">Tipo de entidad raíz</param>
    /// <param name="compilation">Compilación para resolver tipos</param>
    /// <param name="location">Ubicación en código fuente (opcional)</param>
    /// <returns>Información de validación del path</returns>
    public static IncludePathInfo ValidatePath(
        string path,
        INamedTypeSymbol rootEntityType,
        Compilation compilation,
        Location? location = null)
    {
        // ✅ VALIDACIÓN 1: Path no puede ser null/empty/whitespace
        if (string.IsNullOrWhiteSpace(path))
        {
            return new IncludePathInfo(
                OriginalPath: path ?? "",
                Segments: Array.Empty<string>(),
                SegmentDetails: Array.Empty<SegmentInfo>(),
                IsValid: false,
                ErrorMessage: "Include path cannot be null or whitespace.",
                Location: location
            );
        }

        // ✅ VALIDACIÓN 2: RootEntityType no puede ser null
        if (rootEntityType == null)
        {
            return new IncludePathInfo(
                OriginalPath: path,
                Segments: Array.Empty<string>(),
                SegmentDetails: Array.Empty<SegmentInfo>(),
                IsValid: false,
                ErrorMessage: "Root entity type cannot be null.",
                Location: location
            );
        }

        // ✅ VALIDACIÓN 3: Compilation no puede ser null
        if (compilation == null)
        {
            return new IncludePathInfo(
                OriginalPath: path,
                Segments: Array.Empty<string>(),
                SegmentDetails: Array.Empty<SegmentInfo>(),
                IsValid: false,
                ErrorMessage: "Compilation cannot be null.",
                Location: location
            );
        }

        // Split por '.' y trim de cada segmento
        var segments = path.Split('.')
            .Select(s => s.Trim())
            .ToArray();

        // ✅ VALIDACIÓN 4: No debe haber segmentos vacíos (dots consecutivos, leading/trailing dots)
        if (segments.Any(string.IsNullOrWhiteSpace))
        {
            return new IncludePathInfo(
                OriginalPath: path,
                Segments: segments,
                SegmentDetails: Array.Empty<SegmentInfo>(),
                IsValid: false,
                ErrorMessage: "Include path contains empty segments. Check for consecutive dots (..), leading dots, or trailing dots.",
                Location: location
            );
        }

        // Validar cada segmento del path
        var segmentDetails = new List<SegmentInfo>();
        var currentType = rootEntityType;

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];

            // Buscar propiedad en el tipo actual
            var property = FindProperty(currentType, segment);

            if (property == null)
            {
                var similarProperty = FindSimilarPropertyName(currentType, segment);
                var suggestion = similarProperty != null
                    ? $" Did you mean '{similarProperty}'?"
                    : "";

                return new IncludePathInfo(
                    OriginalPath: path,
                    Segments: segments,
                    SegmentDetails: segmentDetails.ToArray(),
                    IsValid: false,
                    ErrorMessage: $"Property '{segment}' does not exist on type '{currentType.Name}'.{suggestion}",
                    Location: location
                );
            }

            // Verificar que sea una propiedad de navegación (no escalar)
            var (isNavigation, propertyType, elementType, isCollection) = 
                GetNavigationElementType(property, compilation);

            if (!isNavigation)
            {
                return new IncludePathInfo(
                    OriginalPath: path,
                    Segments: segments,
                    SegmentDetails: segmentDetails.ToArray(),
                    IsValid: false,
                    ErrorMessage: $"Property '{segment}' on type '{currentType.Name}' is not a navigation property. " +
                                $"It has type '{propertyType.ToDisplayString()}' which is a scalar value. " +
                                $"Only entity types and collections can be included.",
                    Location: location
                );
            }

            // Agregar información del segmento
            segmentDetails.Add(new SegmentInfo(
                PropertyName: segment,
                ContainingType: currentType,
                PropertySymbol: property,
                PropertyType: propertyType,
                ElementType: elementType,
                IsCollection: isCollection
            ));

            // Actualizar tipo actual para el siguiente segmento
            currentType = elementType!;
        }

        // Todo válido
        return new IncludePathInfo(
            OriginalPath: path,
            Segments: segments,
            SegmentDetails: segmentDetails.ToArray(),
            IsValid: true,
            ErrorMessage: null,
            Location: location
        );
    }

    /// <summary>
    /// Busca una propiedad por nombre en un tipo
    /// </summary>
    private static IPropertySymbol? FindProperty(INamedTypeSymbol type, string propertyName)
    {
        return type.GetMembers()
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => p.Name == propertyName);
    }

    /// <summary>
    /// Verifica si una propiedad es de navegación y obtiene su tipo
    /// </summary>
    private static (bool isNavigation, ITypeSymbol propertyType, INamedTypeSymbol? elementType, bool isCollection) 
        GetNavigationElementType(IPropertySymbol property, Compilation compilation)
    {
        var propertyType = property.Type;

        // Verificar si es una colección genérica
        if (IsCollectionType(propertyType, out var elementType))
        {
            // Es una colección de entidades
            return (true, propertyType, elementType, true);
        }

        // Verificar si es un tipo de entidad (no es tipo primitivo/built-in)
        if (propertyType is INamedTypeSymbol namedType && !IsScalarType(namedType, compilation))
        {
            // Es una navegación simple (no colección)
            return (true, propertyType, namedType, false);
        }

        // Es un tipo escalar
        return (false, propertyType, null, false);
    }

    /// <summary>
    /// Verifica si un tipo es una colección genérica
    /// </summary>
    private static bool IsCollectionType(ITypeSymbol type, out INamedTypeSymbol? elementType)
    {
        elementType = null;

        if (type is not INamedTypeSymbol namedType)
            return false;

        // Verificar ICollection<T>, IEnumerable<T>, List<T>, HashSet<T>, etc.
        if (namedType.IsGenericType)
        {
            var genericDefinition = namedType.ConstructedFrom.ToDisplayString();

            if (genericDefinition == "System.Collections.Generic.ICollection<T>" ||
                genericDefinition == "System.Collections.Generic.IEnumerable<T>" ||
                genericDefinition == "System.Collections.Generic.List<T>" ||
                genericDefinition == "System.Collections.Generic.HashSet<T>" ||
                genericDefinition == "System.Collections.Generic.IList<T>")
            {
                elementType = namedType.TypeArguments[0] as INamedTypeSymbol;
                return elementType != null;
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica si un tipo es escalar (primitivo, string, decimal, DateTime, etc.)
    /// </summary>
    private static bool IsScalarType(INamedTypeSymbol type, Compilation compilation)
    {
        var typeString = type.ToDisplayString();

        // Tipos primitivos y comunes
        var scalarTypes = new[]
        {
            "string", "System.String",
            "int", "System.Int32",
            "long", "System.Int64",
            "short", "System.Int16",
            "byte", "System.Byte",
            "bool", "System.Boolean",
            "decimal", "System.Decimal",
            "double", "System.Double",
            "float", "System.Single",
            "System.DateTime",
            "System.DateTimeOffset",
            "System.TimeSpan",
            "System.Guid",
            "char", "System.Char"
        };

        return scalarTypes.Contains(typeString) || type.TypeKind == TypeKind.Enum;
    }

    /// <summary>
    /// Busca una propiedad con nombre similar usando distancia de Levenshtein
    /// </summary>
    private static string? FindSimilarPropertyName(INamedTypeSymbol type, string propertyName)
    {
        var properties = type.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(p => p.Name)
            .ToArray();

        // Buscar propiedades con distancia de edición <= 2
        var candidates = properties
            .Select(p => (name: p, distance: CalculateLevenshteinDistance(propertyName.ToLowerInvariant(), p.ToLowerInvariant())))
            .Where(x => x.distance <= 2)
            .OrderBy(x => x.distance)
            .ToArray();

        return candidates.FirstOrDefault().name;
    }

    /// <summary>
    /// Calcula la distancia de Levenshtein entre dos strings
    /// </summary>
    private static int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return string.IsNullOrEmpty(target) ? 0 : target.Length;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        var distance = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= target.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[source.Length, target.Length];
    }
    
}