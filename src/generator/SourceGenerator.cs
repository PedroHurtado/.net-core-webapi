using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace CodeGenerator;

[Generator]
public class AddRepositoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsInterfaceWithBaseTypes(s),
                transform: static (ctx, _) => GetAddInterfaceInfo(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(interfaceDeclarations, 
            (spc, info) => GenerateAddRepository(spc, info!));
    }

    private static bool IsInterfaceWithBaseTypes(SyntaxNode node)
    {
        return node is InterfaceDeclarationSyntax interfaceDecl 
               && interfaceDecl.BaseList?.Types.Count > 0;
    }

    private static AddInterfaceInfo? GetAddInterfaceInfo(GeneratorSyntaxContext context)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(interfaceDecl) as INamedTypeSymbol;
        
        if (symbol == null) return null;

        // Buscar si hereda de IAdd<T>
        INamedTypeSymbol? addInterface = null;
        foreach (var baseInterface in symbol.Interfaces)
        {
            if (baseInterface.Name == "IAdd" && baseInterface.TypeArguments.Length > 0)
            {
                addInterface = baseInterface;
                break;
            }
        }

        if (addInterface == null) return null;

        var entityType = addInterface.TypeArguments[0];

        return new AddInterfaceInfo
        {
            InterfaceName = symbol.Name,
            FullInterfaceName = symbol.ToDisplayString(), // Nombre completo con clase contenedora
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            EntityType = entityType.ToDisplayString()
        };
    }

    private static void GenerateAddRepository(SourceProductionContext context, AddInterfaceInfo info)
    {
        var className = info.InterfaceName.TrimStart('I');
        
        var source = $@"using Microsoft.EntityFrameworkCore;
using webapi.common.infrastructure;
using webapi.common.dependencyinjection;

namespace {info.Namespace};

[Injectable]
public class {className}(IRepository repository) : {info.FullInterfaceName}
{{
    private readonly IRepository _repository = repository;

    public void Add({info.EntityType} entity)
    {{
        _repository.Entry(entity).State = EntityState.Added;
    }}
}}";

        context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private class AddInterfaceInfo
    {
        public string InterfaceName { get; set; } = "";
        public string FullInterfaceName { get; set; } = ""; // Nuevo: nombre completo
        public string Namespace { get; set; } = "";
        public string EntityType { get; set; } = "";
    }
}