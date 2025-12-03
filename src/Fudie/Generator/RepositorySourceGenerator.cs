using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Fudie.Generator;

/// <summary>
/// Source Generator que genera implementaciones de repositorios automáticamente
/// </summary>
[Generator]
public class RepositorySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Registrar post-initialization para agregar atributos al compilador
        context.RegisterPostInitializationOutput(ctx =>
        {
            // Los atributos ya están definidos en Fudie/Attributes/*.cs
            // No necesitamos generarlos aquí
        });

        // Pipeline incremental: buscar interfaces con atributos de Fudie
        var repositoryInterfaces = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateInterface(node),
                transform: static (ctx, _) => GetRepositoryInterfaceInfo(ctx))
            .Where(static info => info is not null);

        // Combinar con la compilación
        var compilationAndInterfaces = context.CompilationProvider.Combine(repositoryInterfaces.Collect());

        // Generar código
        context.RegisterSourceOutput(compilationAndInterfaces,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateInterface(SyntaxNode node)
    {
        // Buscar interfaces con atributos
        return node is InterfaceDeclarationSyntax interfaceDecl &&
               interfaceDecl.AttributeLists.Count > 0;
    }

    private static RepositoryInterfaceInfo? GetRepositoryInterfaceInfo(GeneratorSyntaxContext context)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;
        var interfaceSymbol = context.SemanticModel.GetDeclaredSymbol(interfaceDecl) as INamedTypeSymbol;

        if (interfaceSymbol == null)
            return null;

        // Verificar si tiene atributos de Fudie (Include, Tracking, etc.)
        var hasFudieAttributes = interfaceSymbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "Fudie.Attributes");

        if (!hasFudieAttributes)
            return null;

        return new RepositoryInterfaceInfo(interfaceDecl, interfaceSymbol);
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<RepositoryInterfaceInfo> interfaces,
        SourceProductionContext context)
    {
        foreach (var info in interfaces)
        {
            try
            {
                GenerateRepository(compilation, info, context);
            }
            catch (System.Exception ex)
            {
                // Reportar error de generación
                var descriptor = new DiagnosticDescriptor(
                    "FUDIE001",
                    "Repository Generation Error",
                    $"Error generating repository for {info.Symbol.Name}: {ex.Message}",
                    "Fudie.Generator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

                context.ReportDiagnostic(Diagnostic.Create(descriptor, info.Syntax.GetLocation()));
            }
        }
    }

    private static void GenerateRepository(
        Compilation compilation,
        RepositoryInterfaceInfo info,
        SourceProductionContext context)
    {
        var interfaceSymbol = info.Symbol;

        // Extraer información del repositorio
        var repoConfig = ExtractRepositoryConfiguration(interfaceSymbol, compilation, context);

        if (repoConfig == null)
            return;

        // Generar el código
        var code = CodeBuilder.GenerateRepositoryClass(
            className: repoConfig.ClassName,
            namespaceName: repoConfig.Namespace,
            entityTypeName: repoConfig.EntityTypeName,
            idTypeName: repoConfig.IdTypeName,
            config: repoConfig.BuilderConfig);

        // Emitir el código
        var fileName = $"{repoConfig.ClassName}.g.cs";
        context.AddSource(fileName, code);
    }

    private static RepositoryConfiguration? ExtractRepositoryConfiguration(
        INamedTypeSymbol interfaceSymbol,
        Compilation compilation,
        SourceProductionContext context)
    {
        // Obtener namespace
        var namespaceName = interfaceSymbol.ContainingNamespace?.ToDisplayString() ?? "Generated";

        // Nombre de la clase de implementación: ICustomerRepository -> CustomerRepository
        var interfaceName = interfaceSymbol.Name;
        var className = interfaceName.StartsWith("I") && interfaceName.Length > 1
            ? interfaceName.Substring(1)
            : interfaceName + "Impl";

        // Detectar qué interfaces implementa
        var baseInterfaces = interfaceSymbol.AllInterfaces;
        bool implementsIGet = false;
        bool implementsIAdd = false;
        bool implementsIUpdate = false;
        bool implementsIRemove = false;

        string? entityTypeName = null;
        string? idTypeName = null;

        foreach (var baseInterface in baseInterfaces)
        {
            var interfaceFullName = baseInterface.ConstructedFrom.ToDisplayString();

            if (interfaceFullName == "Fudie.Infrastructure.IGet<T, ID>")
            {
                implementsIGet = true;
                entityTypeName = baseInterface.TypeArguments[0].Name;
                idTypeName = baseInterface.TypeArguments[1].ToDisplayString();
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IAdd<T>")
            {
                implementsIAdd = true;
                entityTypeName ??= baseInterface.TypeArguments[0].Name;
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IUpdate<T, ID>")
            {
                implementsIUpdate = true;
                entityTypeName ??= baseInterface.TypeArguments[0].Name;
                idTypeName ??= baseInterface.TypeArguments[1].ToDisplayString();
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IRemove<T, ID>")
            {
                implementsIRemove = true;
                entityTypeName ??= baseInterface.TypeArguments[0].Name;
                idTypeName ??= baseInterface.TypeArguments[1].ToDisplayString();
            }
        }

        // Validar que se encontró el tipo de entidad
        if (entityTypeName == null)
        {
            var descriptor = new DiagnosticDescriptor(
                "FUDIE002",
                "No Entity Type Found",
                $"Interface {interfaceSymbol.Name} does not implement any Fudie repository interfaces (IGet, IAdd, IUpdate, IRemove)",
                "Fudie.Generator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            context.ReportDiagnostic(Diagnostic.Create(descriptor, interfaceSymbol.Locations.FirstOrDefault()));
            return null;
        }

        idTypeName ??= "System.Guid";

        // Obtener el símbolo de la entidad
        var entitySymbol = compilation.GetTypeByMetadataName($"{namespaceName}.{entityTypeName}");
        if (entitySymbol == null)
        {
            // Intentar buscar en otros namespaces
            entitySymbol = FindEntityType(compilation, entityTypeName);
        }

        if (entitySymbol == null)
        {
            var descriptor = new DiagnosticDescriptor(
                "FUDIE003",
                "Entity Type Not Found",
                $"Could not find entity type '{entityTypeName}' in compilation",
                "Fudie.Generator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            context.ReportDiagnostic(Diagnostic.Create(descriptor, interfaceSymbol.Locations.FirstOrDefault()));
            return null;
        }

        // Extraer atributos Include
        var includePaths = ExtractIncludePaths(interfaceSymbol, entitySymbol, compilation, context);

        // Extraer atributos de tracking
        var asNoTracking = HasAttribute(interfaceSymbol, "Fudie.Attributes.AsNoTrackingAttribute") ||
                          (HasAttribute(interfaceSymbol, "Fudie.Attributes.TrackingAttribute") &&
                           GetTrackingAttributeValue(interfaceSymbol) == false);

        var asSplitQuery = HasAttribute(interfaceSymbol, "Fudie.Attributes.AsSplitQueryAttribute");
        var ignoreQueryFilters = HasAttribute(interfaceSymbol, "Fudie.Attributes.IgnoreQueryFiltersAttribute");

        // Crear configuración del builder
        var builderConfig = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = implementsIGet,
            ImplementIAdd = implementsIAdd,
            ImplementIUpdate = implementsIUpdate,
            ImplementIRemove = implementsIRemove,
            IncludePaths = includePaths,
            AsNoTracking = asNoTracking,
            AsSplitQuery = asSplitQuery,
            IgnoreQueryFilters = ignoreQueryFilters
        };

        return new RepositoryConfiguration(
            Namespace: namespaceName,
            ClassName: className,
            EntityTypeName: entityTypeName,
            IdTypeName: idTypeName,
            BuilderConfig: builderConfig
        );
    }

    private static List<PathValidator.IncludePathInfo> ExtractIncludePaths(
        INamedTypeSymbol interfaceSymbol,
        INamedTypeSymbol entitySymbol,
        Compilation compilation,
        SourceProductionContext context)
    {
        var includePaths = new List<PathValidator.IncludePathInfo>();

        var includeAttributes = interfaceSymbol.GetAttributes()
            .Where(attr => attr.AttributeClass?.Name == "IncludeAttribute")
            .ToList();

        foreach (var attr in includeAttributes)
        {
            // El primer argumento es params string[] paths
            if (attr.ConstructorArguments.Length > 0)
            {
                var pathsArgument = attr.ConstructorArguments[0];

                if (pathsArgument.Kind == TypedConstantKind.Array)
                {
                    foreach (var pathValue in pathsArgument.Values)
                    {
                        if (pathValue.Value is string path)
                        {
                            var pathInfo = PathValidator.ValidatePath(
                                path,
                                entitySymbol,
                                compilation,
                                attr.ApplicationSyntaxReference?.GetSyntax().GetLocation());

                            if (!pathInfo.IsValid)
                            {
                                // Reportar error de validación
                                var descriptor = new DiagnosticDescriptor(
                                    "FUDIE004",
                                    "Invalid Include Path",
                                    pathInfo.ErrorMessage ?? "Invalid include path",
                                    "Fudie.Generator",
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true);

                                context.ReportDiagnostic(Diagnostic.Create(
                                    descriptor,
                                    pathInfo.Location ?? attr.ApplicationSyntaxReference?.GetSyntax().GetLocation()));
                            }
                            else
                            {
                                includePaths.Add(pathInfo);
                            }
                        }
                    }
                }
            }
        }

        return includePaths;
    }

    private static bool HasAttribute(INamedTypeSymbol symbol, string fullAttributeName)
    {
        return symbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == fullAttributeName);
    }

    private static bool? GetTrackingAttributeValue(INamedTypeSymbol symbol)
    {
        var trackingAttr = symbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "TrackingAttribute");

        if (trackingAttr?.ConstructorArguments.Length > 0)
        {
            var value = trackingAttr.ConstructorArguments[0].Value;
            if (value is bool boolValue)
            {
                return boolValue;
            }
        }

        return null;
    }

    private static INamedTypeSymbol? FindEntityType(Compilation compilation, string entityTypeName)
    {
        // Buscar en todos los tipos de la compilación
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            foreach (var node in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var symbol = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
                if (symbol?.Name == entityTypeName)
                {
                    return symbol;
                }
            }
        }

        return null;
    }

    private record RepositoryInterfaceInfo(
        InterfaceDeclarationSyntax Syntax,
        INamedTypeSymbol Symbol
    );

    private record RepositoryConfiguration(
        string Namespace,
        string ClassName,
        string EntityTypeName,
        string IdTypeName,
        CodeBuilder.RepositoryConfig BuilderConfig
    );

    
}