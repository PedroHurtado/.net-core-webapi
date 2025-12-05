using Microsoft.CodeAnalysis;

namespace Fudie.Infrastructure.Generator.QueryMethod;

/// <summary>
/// Definiciones de diagnósticos para el Query Method Generator
/// </summary>
public static class Diagnostics
{
    private const string Category = "Fudie.QueryMethod";

    /// <summary>
    /// REPO001: Propiedad no existe en la entidad
    /// </summary>
    public static DiagnosticDescriptor PropertyNotFound { get; } = new(
        id: "REPO001",
        title: "Property not found",
        messageFormat: "'{0}' no existe en '{1}'.{2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "La propiedad especificada en el nombre del método no existe en la entidad."
    );

    /// <summary>
    /// REPO002: Tipo de parámetro incompatible con tipo de propiedad
    /// </summary>
    public static DiagnosticDescriptor TypeMismatch { get; } = new(
        id: "REPO002",
        title: "Parameter type mismatch",
        messageFormat: "Parámetro '{0}' es '{1}' pero '{2}' es '{3}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "El tipo del parámetro no es compatible con el tipo de la propiedad."
    );

    /// <summary>
    /// REPO003: Falta parámetro requerido
    /// </summary>
    public static DiagnosticDescriptor MissingParameter { get; } = new(
        id: "REPO003",
        title: "Missing parameter",
        messageFormat: "Falta parámetro para '{0}' en '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "El método requiere un parámetro adicional basado en las condiciones del nombre."
    );

    /// <summary>
    /// REPO004: Cantidad incorrecta de parámetros
    /// </summary>
    public static DiagnosticDescriptor WrongParameterCount { get; } = new(
        id: "REPO004",
        title: "Wrong parameter count",
        messageFormat: "'{0}' tiene {1} parámetros pero se esperaban {2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "El método tiene más parámetros de los esperados según el nombre."
    );

    /// <summary>
    /// REPO005: Tipo de retorno incorrecto
    /// </summary>
    public static DiagnosticDescriptor WrongReturnType { get; } = new(
        id: "REPO005",
        title: "Wrong return type",
        messageFormat: "'{0}' debe retornar '{1}', no '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "El tipo de retorno del método no coincide con el esperado según el prefijo."
    );

    /// <summary>
    /// REPO006: Operador incompatible con tipo de propiedad
    /// </summary>
    public static DiagnosticDescriptor IncompatibleOperator { get; } = new(
        id: "REPO006",
        title: "Incompatible operator",
        messageFormat: "Operador '{0}' no es válido para tipo '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "El operador especificado no es compatible con el tipo de la propiedad."
    );

    /// <summary>
    /// REPO007: Error al parsear nombre del método
    /// </summary>
    public static DiagnosticDescriptor ParseError { get; } = new(
        id: "REPO007",
        title: "Parse error",
        messageFormat: "No se pudo parsear '{0}': {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "El nombre del método no sigue la convención esperada o contiene errores de sintaxis."
    );

    /// <summary>
    /// Crea un diagnóstico para propiedad no encontrada
    /// </summary>
    /// <param name="propertyName">Nombre de la propiedad que no existe</param>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="suggestion">Sugerencia de corrección (opcional)</param>
    /// <param name="location">Ubicación en el código fuente</param>
    public static Diagnostic CreatePropertyNotFound(
        string propertyName,
        string entityName,
        string? suggestion,
        Location? location)
    {
        var suggestionText = suggestion != null ? $" Did you mean '{suggestion}'?" : "";
        return Diagnostic.Create(
            PropertyNotFound,
            location,
            propertyName,
            entityName,
            suggestionText
        );
    }

    /// <summary>
    /// Crea un diagnóstico para tipo incompatible
    /// </summary>
    public static Diagnostic CreateTypeMismatch(
        string parameterName,
        string parameterType,
        string propertyName,
        string propertyType,
        Location? location)
    {
        return Diagnostic.Create(
            TypeMismatch,
            location,
            parameterName,
            parameterType,
            propertyName,
            propertyType
        );
    }

    /// <summary>
    /// Crea un diagnóstico para parámetro faltante
    /// </summary>
    public static Diagnostic CreateMissingParameter(
        string propertyName,
        string methodName,
        Location? location)
    {
        return Diagnostic.Create(
            MissingParameter,
            location,
            propertyName,
            methodName
        );
    }

    /// <summary>
    /// Crea un diagnóstico para cantidad incorrecta de parámetros
    /// </summary>
    public static Diagnostic CreateWrongParameterCount(
        string methodName,
        int actualCount,
        int expectedCount,
        Location? location)
    {
        return Diagnostic.Create(
            WrongParameterCount,
            location,
            methodName,
            actualCount,
            expectedCount
        );
    }

    /// <summary>
    /// Crea un diagnóstico para tipo de retorno incorrecto
    /// </summary>
    public static Diagnostic CreateWrongReturnType(
        string methodName,
        string expectedType,
        string actualType,
        Location? location)
    {
        return Diagnostic.Create(
            WrongReturnType,
            location,
            methodName,
            expectedType,
            actualType
        );
    }

    /// <summary>
    /// Crea un diagnóstico para operador incompatible
    /// </summary>
    public static Diagnostic CreateIncompatibleOperator(
        string operatorName,
        string propertyType,
        Location? location)
    {
        return Diagnostic.Create(
            IncompatibleOperator,
            location,
            operatorName,
            propertyType
        );
    }

    /// <summary>
    /// Crea un diagnóstico para error de parsing
    /// </summary>
    public static Diagnostic CreateParseError(
        string methodName,
        string errorMessage,
        Location? location)
    {
        return Diagnostic.Create(
            ParseError,
            location,
            methodName,
            errorMessage
        );
    }
}
