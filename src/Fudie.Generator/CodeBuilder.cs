using Microsoft.CodeAnalysis;
using System.Text;

namespace Fudie.Generator;

/// <summary>
/// Genera código C# para implementaciones de repositorios
/// </summary>
internal static class CodeBuilder
{
    /// <summary>
    /// Genera el código completo de Include/ThenInclude para un path validado
    /// </summary>
    /// <param name="pathInfo">Información del path validado</param>
    /// <param name="entityTypeName">Nombre del tipo de entidad raíz</param>
    /// <param name="queryVariableName">Nombre de la variable query (ej: "query")</param>
    /// <returns>Código de Include/ThenInclude</returns>
    public static string GenerateIncludeChain(
        PathValidator.IncludePathInfo pathInfo,
        string entityTypeName,
        string queryVariableName = "query")
    {
        if (!pathInfo.IsValid || pathInfo.SegmentDetails.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var parameterNames = new List<string>();

        // Generar nombre de parámetro para la entidad raíz
        var rootParameter = GenerateParameterName(entityTypeName);
        parameterNames.Add(rootParameter);

        // Primera navegación siempre es .Include()
        var firstSegment = pathInfo.SegmentDetails[0];
        sb.Append($"{queryVariableName} = {queryVariableName}.Include({rootParameter} => {rootParameter}.{firstSegment.PropertyName})");

        // Navegaciones subsecuentes son .ThenInclude()
        for (int i = 1; i < pathInfo.SegmentDetails.Length; i++)
        {
            var segment = pathInfo.SegmentDetails[i];
            var previousSegment = pathInfo.SegmentDetails[i - 1];

            // Generar nombre de parámetro para el tipo actual
            string currentParameter;

            if (previousSegment.IsCollection)
            {
                // Si el anterior era colección, usamos el tipo del elemento
                currentParameter = GenerateParameterName(previousSegment.ElementType!.Name);
            }
            else
            {
                // Si no era colección, usamos el tipo de la propiedad
                currentParameter = GenerateParameterName(previousSegment.PropertyType.Name);
            }

            parameterNames.Add(currentParameter);

            sb.AppendLine()
              .Append($"    .ThenInclude({currentParameter} => {currentParameter}.{segment.PropertyName})");
        }

        sb.Append(";");

        return sb.ToString();
    }

    /// <summary>
    /// Genera múltiples cadenas de Include para varios paths
    /// </summary>
    /// <param name="paths">Lista de paths validados</param>
    /// <param name="entityTypeName">Nombre del tipo de entidad</param>
    /// <param name="queryVariableName">Nombre de la variable query</param>
    /// <returns>Código con todos los Includes</returns>
    public static string GenerateMultipleIncludes(
        IEnumerable<PathValidator.IncludePathInfo> paths,
        string entityTypeName,
        string queryVariableName = "query")
    {
        var sb = new StringBuilder();
        var validPaths = paths.Where(p => p.IsValid).ToArray();

        for (int i = 0; i < validPaths.Length; i++)
        {
            var includeCode = GenerateIncludeChain(validPaths[i], entityTypeName, queryVariableName);
            sb.Append(includeCode);

            if (i < validPaths.Length - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Genera código para aplicar AsNoTracking
    /// </summary>
    public static string GenerateAsNoTracking(string queryVariableName = "query")
    {
        return $"{queryVariableName} = {queryVariableName}.AsNoTracking();";
    }

    /// <summary>
    /// Genera código para aplicar AsSplitQuery
    /// </summary>
    public static string GenerateAsSplitQuery(string queryVariableName = "query")
    {
        return $"{queryVariableName} = {queryVariableName}.AsSplitQuery();";
    }

    /// <summary>
    /// Genera código para aplicar IgnoreQueryFilters
    /// </summary>
    public static string GenerateIgnoreQueryFilters(string queryVariableName = "query")
    {
        return $"{queryVariableName} = {queryVariableName}.IgnoreQueryFilters();";
    }

    /// <summary>
    /// Genera el método Get completo para IGet&lt;T, ID&gt;
    /// </summary>
    public static string GenerateGetMethod(
        string entityTypeName,
        string idTypeName,
        IEnumerable<PathValidator.IncludePathInfo> includePaths,
        bool asNoTracking,
        bool asSplitQuery,
        bool ignoreQueryFilters)
    {
        var sb = new StringBuilder();
        var entityParameter = GenerateParameterName(entityTypeName);

        sb.AppendLine($"public async Task<{entityTypeName}> Get({idTypeName} id)");
        sb.AppendLine("{");
        sb.AppendLine($"    IQueryable<{entityTypeName}> query = _entityLookup.Set<{entityTypeName}>();");
        sb.AppendLine();

        // Includes
        var validPaths = includePaths.Where(p => p.IsValid).ToArray();
        if (validPaths.Any())
        {
            sb.AppendLine("    // Apply includes");
            foreach (var path in validPaths)
            {
                var includeCode = GenerateIncludeChain(path, entityTypeName, "query");
                sb.AppendLine($"    {includeCode}");
            }
            sb.AppendLine();
        }

        // Query modifiers
        var hasModifiers = asNoTracking || asSplitQuery || ignoreQueryFilters;
        if (hasModifiers)
        {
            sb.AppendLine("    // Apply query modifiers");
            if (asSplitQuery)
            {
                sb.AppendLine($"    {GenerateAsSplitQuery("query")}");
            }
            if (asNoTracking)
            {
                sb.AppendLine($"    {GenerateAsNoTracking("query")}");
            }
            if (ignoreQueryFilters)
            {
                sb.AppendLine($"    {GenerateIgnoreQueryFilters("query")}");
            }
            sb.AppendLine();
        }

        // Execute query
        sb.AppendLine($"    var entity = await query.FirstOrDefaultAsync({entityParameter} => {entityParameter}.Id == id);");
        sb.AppendLine();
        sb.AppendLine("    if (entity == null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        throw new KeyNotFoundException($\"{entityTypeName} with ID '{{id}}' not found.\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    return entity;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Genera el método Add completo para IAdd&lt;T&gt;
    /// </summary>
    public static string GenerateAddMethod(string entityTypeName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"public void Add({entityTypeName} entity)");
        sb.AppendLine("{");
        sb.AppendLine("    _changeTracker.Entry(entity).State = EntityState.Added;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Genera el método Remove completo para IRemove&lt;T, ID&gt;
    /// </summary>
    public static string GenerateRemoveMethod(string entityTypeName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"public void Remove({entityTypeName} entity)");
        sb.AppendLine("{");
        sb.AppendLine("    _changeTracker.Entry(entity).State = EntityState.Deleted;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Genera el método Get completo para IUpdate&lt;T, ID&gt;
    /// Igual que IGet pero con tracking habilitado (usa Set en lugar de Query)
    /// </summary>
    public static string GenerateUpdateGetMethod(
        string entityTypeName,
        string idTypeName,
        IEnumerable<PathValidator.IncludePathInfo> includePaths,
        bool asSplitQuery)
    {
        var sb = new StringBuilder();
        var entityParameter = GenerateParameterName(entityTypeName);

        sb.AppendLine($"public async Task<{entityTypeName}> Get({idTypeName} id)");
        sb.AppendLine("{");
        sb.AppendLine($"    IQueryable<{entityTypeName}> query = _entityLookup.Set<{entityTypeName}>();  // Tracking habilitado para updates");
        sb.AppendLine();

        // Includes
        var validPaths = includePaths.Where(p => p.IsValid).ToArray();
        if (validPaths.Any())
        {
            sb.AppendLine("    // Apply includes");
            foreach (var path in validPaths)
            {
                var includeCode = GenerateIncludeChain(path, entityTypeName, "query");
                sb.AppendLine($"    {includeCode}");
            }
            sb.AppendLine();
        }

        // Query modifiers (solo AsSplitQuery, tracking ya está habilitado por defecto)
        if (asSplitQuery)
        {
            sb.AppendLine("    // Apply query modifiers");
            sb.AppendLine($"    {GenerateAsSplitQuery("query")}");
            sb.AppendLine();
        }

        // Execute query
        sb.AppendLine($"    var entity = await query.FirstOrDefaultAsync({entityParameter} => {entityParameter}.Id == id);");
        sb.AppendLine();
        sb.AppendLine("    if (entity == null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        throw new KeyNotFoundException($\"{entityTypeName} with ID '{{id}}' not found.\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    return entity;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Genera una clase de repositorio completa
    /// </summary>
    public static string GenerateRepositoryClass(
    string className,
    string namespaceName,
    string entityTypeName,
    string idTypeName,
    RepositoryConfig config)
    {
        var sb = new StringBuilder();

        // Usings
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Fudie.Infrastructure;");
        sb.AppendLine("using Fudie.DependencyInjection;");

        // Additional usings (e.g., entity type namespaces)
        foreach (var additionalUsing in config.AdditionalUsings)
        {
            sb.AppendLine($"using {additionalUsing};");
        }

        sb.AppendLine();

        // Namespace
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();

        // Build list of interfaces for both inheritance and Injectable attributes
        var baseInterfaces = new List<string>();
        if (config.ImplementIGet)
            baseInterfaces.Add($"IGet<{entityTypeName}, {idTypeName}>");
        if (config.ImplementIAdd)
            baseInterfaces.Add($"IAdd<{entityTypeName}>");
        if (config.ImplementIUpdate)
            baseInterfaces.Add($"IUpdate<{entityTypeName}, {idTypeName}>");
        if (config.ImplementIRemove)
            baseInterfaces.Add($"IRemove<{entityTypeName}, {idTypeName}>");

        // Class declaration with [Injectable] attributes
        if (!string.IsNullOrEmpty(config.ContainerInterfaceFullName))
        {
            // Generate multiple [Injectable] with ServiceType
            // First for the container interface
            sb.AppendLine($"[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof({config.ContainerInterfaceFullName}))]");

            // Then for each base interface
            foreach (var baseInterface in config.BaseInterfaceNames)
            {
                sb.AppendLine($"[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof({baseInterface}))]");
            }
        }
        else
        {
            // Legacy behavior: single [Injectable] without ServiceType
            sb.AppendLine("[Injectable(Fudie.DependencyInjection.ServiceLifetime.Scoped)]");
        }

        sb.Append($"public class {className}");

        // Implement container interface if available, otherwise base interfaces
        // Use full name for nested interfaces to resolve correctly
        if (!string.IsNullOrEmpty(config.ContainerInterfaceFullName))
        {
            sb.Append($" : {config.ContainerInterfaceFullName}");
        }
        else if (baseInterfaces.Any())
        {
            sb.Append($" : {string.Join(", ", baseInterfaces)}");
        }

        sb.AppendLine();
        sb.AppendLine("{");

        // Fields
        var fields = new List<string>();
        if (config.ImplementIGet || config.ImplementIUpdate || config.ImplementIRemove)
        {
            fields.Add("private readonly IEntityLookup _entityLookup;");
        }
        if (config.ImplementIAdd || config.ImplementIRemove)
        {
            fields.Add("private readonly IChangeTracker _changeTracker;");
        }

        foreach (var field in fields)
        {
            sb.AppendLine($"    {field}");
        }

        if (fields.Any())
        {
            sb.AppendLine();
        }

        // Constructor
        var constructorParams = new List<string>();
        if (fields.Contains("private readonly IEntityLookup _entityLookup;"))
            constructorParams.Add("IEntityLookup entityLookup");
        if (fields.Contains("private readonly IChangeTracker _changeTracker;"))
            constructorParams.Add("IChangeTracker changeTracker");

        sb.AppendLine($"    public {className}({string.Join(", ", constructorParams)})");
        sb.AppendLine("    {");
        if (constructorParams.Contains("IEntityLookup entityLookup"))
            sb.AppendLine("        _entityLookup = entityLookup;");
        if (constructorParams.Contains("IChangeTracker changeTracker"))
            sb.AppendLine("        _changeTracker = changeTracker;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Methods
        // IMPORTANTE: IUpdate e IRemove heredan de IGet, así que si implementa IUpdate o IRemove,
        // debe generar Get() con tracking habilitado (usa Set<T>() en lugar de Query<T>())
        if (config.ImplementIUpdate || config.ImplementIRemove)
        {
            // IUpdate e IRemove necesitan Get() con tracking habilitado
            var updateGetMethod = GenerateUpdateGetMethod(
                entityTypeName,
                idTypeName,
                config.IncludePaths,
                config.AsSplitQuery);

            // Indent method
            var lines = updateGetMethod.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
            }
            sb.AppendLine();
        }
        else if (config.ImplementIGet)
        {
            // Solo IGet (sin IUpdate ni IRemove): genera Get() con AsNoTracking
            var getMethod = GenerateGetMethod(
                entityTypeName,
                idTypeName,
                config.IncludePaths,
                config.AsNoTracking,
                config.AsSplitQuery,
                config.IgnoreQueryFilters);

            // Indent method
            var lines = getMethod.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
            }
            sb.AppendLine();
        }

        if (config.ImplementIAdd)
        {
            var addMethod = GenerateAddMethod(entityTypeName);
            var lines = addMethod.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
            }
            sb.AppendLine();
        }

        if (config.ImplementIRemove)
        {
            var removeMethod = GenerateRemoveMethod(entityTypeName);
            var lines = removeMethod.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
            }
        }

        // Query Methods
        if (config.QueryMethods.Any())
        {
            var queryMethodsCode = GenerateQueryMethods(config.QueryMethods, entityTypeName);
            if (!string.IsNullOrWhiteSpace(queryMethodsCode))
            {
                sb.Append(queryMethodsCode);
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Genera un nombre de parámetro para lambdas basado en el nombre del tipo
    /// </summary>
    /// <param name="typeName">Nombre del tipo (ej: "Customer", "Order", "OrderItem")</param>
    /// <returns>Nombre de parámetro (ej: "c", "o", "oi")</returns>
    private static string GenerateParameterName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return "x";

        // Detectar si es un nombre compuesto (ej: OrderItem, ProductCategory)
        // Un nombre compuesto tiene una mayúscula después de la primera letra
        bool isCompoundName = typeName.Length > 1 &&
                             typeName.Skip(1).Any(c => char.IsUpper(c));

        if (isCompoundName)
        {
            // Para nombres compuestos, usar las iniciales de las palabras
            // Ej: OrderItem -> oi, ProductCategory -> pc
            var initials = new List<char>();

            // Agregar primera letra
            initials.Add(char.ToLowerInvariant(typeName[0]));

            // Agregar iniciales de palabras siguientes (letras mayúsculas)
            for (int i = 1; i < typeName.Length; i++)
            {
                if (char.IsUpper(typeName[i]))
                {
                    initials.Add(char.ToLowerInvariant(typeName[i]));
                }
            }

            return string.Join("", initials);
        }

        // Para nombres simples, usar solo la primera letra
        return char.ToLowerInvariant(typeName[0]).ToString();
    }


    /// <summary>
    /// Genera métodos de query basados en nombres de métodos
    /// </summary>
    public static string GenerateQueryMethods(
        IEnumerable<QueryMethodInfo> queryMethods,
        string entityTypeName)
    {
        if (queryMethods == null || !queryMethods.Any())
            return string.Empty;

        var sb = new StringBuilder();
        var emitter = new QueryMethod.LinqEmitter();

        foreach (var queryMethod in queryMethods)
        {
            if (!queryMethod.ParseResult.Success || queryMethod.ParseResult.Query == null)
                continue;

            var query = queryMethod.ParseResult.Query;
            var methodName = queryMethod.MethodName;
            var parameters = queryMethod.Parameters.ToArray();

            // Generar firma del método
            var signature = emitter.EmitMethodSignature(
                query,
                methodName,
                entityTypeName,
                parameters);

            sb.AppendLine($"    {signature}");
            sb.AppendLine("    {");

            // Generar cuerpo del método
            var paramNames = parameters.Select(p => p.name).ToArray();
            var queryCode = emitter.Emit(query, methodName, entityTypeName, paramNames);

            sb.AppendLine($"        return await {queryCode};");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Información de un método de query
    /// </summary>
    public class QueryMethodInfo
    {
        public string MethodName { get; set; } = string.Empty;
        public QueryMethod.ParseResult ParseResult { get; set; } = QueryMethod.ParseResult.Error("Not parsed");
        public List<(string name, string type)> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Configuración para generación de repositorio
    /// </summary>
    public class RepositoryConfig
    {
        public bool ImplementIGet { get; set; }
        public bool ImplementIAdd { get; set; }
        public bool ImplementIUpdate { get; set; }
        public bool ImplementIRemove { get; set; }

        public IEnumerable<PathValidator.IncludePathInfo> IncludePaths { get; set; } = Array.Empty<PathValidator.IncludePathInfo>();
        public bool AsNoTracking { get; set; }
        public bool AsSplitQuery { get; set; }
        public bool IgnoreQueryFilters { get; set; }

        public List<QueryMethodInfo> QueryMethods { get; set; } = new();

        public IEnumerable<string> AdditionalUsings { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Nombre de la interfaz contenedora para implementar (ej: "IRepository", "ICustomerRepository")
        /// </summary>
        public string? ContainerInterfaceName { get; set; }

        /// <summary>
        /// Nombre completo de la interfaz contenedora para typeof() (ej: "CreateIngredient.IRepository")
        /// </summary>
        public string? ContainerInterfaceFullName { get; set; }

        /// <summary>
        /// Lista de interfaces base con sus nombres completos para los [Injectable] adicionales
        /// (ej: "IAdd&lt;Ingredient&gt;", "IGet&lt;Ingredient, Guid&gt;")
        /// </summary>
        public List<string> BaseInterfaceNames { get; set; } = new();
    }
}
