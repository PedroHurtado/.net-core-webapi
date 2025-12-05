using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Xunit;

namespace Fudie.UnitTests.Generator;

/// <summary>
/// Tests end-to-end para la generación completa de repositorios con query methods
/// </summary>
public class RepositoryGeneratorEndToEndTests
{
    [Fact]
    public void GenerateRepository_WithQueryMethods_ShouldGenerateCompleteCode()
    {
        // Arrange
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface IUserRepository : IGet<User, Guid>, IAdd<User>
    {
        Task<List<User>> FindByEmail(string email);
        Task<User?> FindFirstByAge(int age);
        Task<int> CountByActiveTrue();
        Task<bool> ExistsByEmail(string email);
    }
}";

        var (diagnostics, generatedCode) = GetGeneratedOutput(source);

        // Assert - No debe haber errores
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        // Assert - Debe haber generado código
        Assert.NotEmpty(generatedCode);

        // Assert - Debe contener la clase generada
        Assert.Contains("public class UserRepository", generatedCode);

        // Assert - Debe contener los métodos de query generados
        Assert.Contains("public async Task<List<User>> FindByEmail(string email)", generatedCode);
        Assert.Contains("public async Task<User?> FindFirstByAge(int age)", generatedCode);
        Assert.Contains("public async Task<int> CountByActiveTrue()", generatedCode);
        Assert.Contains("public async Task<bool> ExistsByEmail(string email)", generatedCode);

        // Assert - Debe contener código LINQ correcto
        Assert.Contains(".Where(x => x.Email == email)", generatedCode);
        Assert.Contains(".Where(x => x.Age == age)", generatedCode);
        Assert.Contains(".Where(x => x.Active == true)", generatedCode);
        Assert.Contains(".ToListAsync()", generatedCode);
        Assert.Contains(".FirstOrDefaultAsync()", generatedCode);
        Assert.Contains(".CountAsync()", generatedCode);
        Assert.Contains(".AnyAsync()", generatedCode);
    }

    [Fact]
    public void GenerateRepository_WithInvalidQueryMethod_ShouldReportDiagnostic()
    {
        // Arrange
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public interface IUserRepository : IGet<User, Guid>
    {
        Task<List<User>> FindByInvalidProperty(string value);
    }
}";

        var (diagnostics, _) = GetGeneratedOutput(source);

        // Assert - Debe haber al menos un error
        var errors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.NotEmpty(errors);

        // Debe mencionar la propiedad inválida
        var errorMessages = string.Join(" ", errors.Select(e => e.GetMessage()));
        Assert.Contains("InvalidProperty", errorMessages);
    }

    [Fact]
    public void GenerateRepository_WithWrongParameterCount_ShouldReportDiagnostic()
    {
        // Arrange
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public interface IUserRepository : IGet<User, Guid>
    {
        Task<List<User>> FindByEmailAndAge(string email);  // Falta parámetro age
    }
}";

        var (diagnostics, _) = GetGeneratedOutput(source);

        // Assert - Debe haber un error REPO003 (faltan parámetros)
        var repo003Errors = diagnostics
            .Where(d => d.Id == "REPO003" && d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.NotEmpty(repo003Errors);
    }

    [Fact]
    public void GenerateRepository_WithWrongReturnType_ShouldReportDiagnostic()
    {
        // Arrange
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class User
    {
        public Guid Id { get; set; }
        public bool Active { get; set; }
    }

    public interface IUserRepository : IGet<User, Guid>
    {
        Task<User> CountByActiveTrue();  // Debería retornar Task<int>
    }
}";

        var (diagnostics, _) = GetGeneratedOutput(source);

        // Assert - Debe haber un error REPO005 (tipo de retorno incorrecto)
        var repo005Errors = diagnostics
            .Where(d => d.Id == "REPO005" && d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.NotEmpty(repo005Errors);
    }

    [Fact]
    public void GenerateRepository_WithComplexQuery_ShouldGenerateCorrectCode()
    {
        // Arrange
        var source = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class User
    {
        public Guid Id { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface IUserRepository : IGet<User, Guid>
    {
        Task<List<User>> FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc(int age);
        Task<List<User>> FindTop10ByActiveTrueOrderByCreatedAtDesc();
    }
}";

        var (diagnostics, generatedCode) = GetGeneratedOutput(source);

        // Assert - No debe haber errores
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        // Assert - Debe contener query compleja
        Assert.Contains("x.Age > age && x.Active == true", generatedCode);
        Assert.Contains(".OrderByDescending(x => x.CreatedAt)", generatedCode);
        Assert.Contains(".Take(10)", generatedCode);
    }

    [Fact]
    public void GenerateRepository_WithoutQueryMethods_ShouldGenerateWithoutErrors()
    {
        // Arrange
        var source = @"
using System;
using Fudie.Infrastructure;

namespace TestNamespace
{
    public class User
    {
        public Guid Id { get; set; }
    }

    public interface IUserRepository : IGet<User, Guid>, IAdd<User>
    {
    }
}";

        var (diagnostics, generatedCode) = GetGeneratedOutput(source);

        // Assert - No debe haber errores
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);

        // Assert - Debe haber generado código
        Assert.NotEmpty(generatedCode);
    }

    /// <summary>
    /// Helper para compilar código y obtener diagnósticos y código generado
    /// </summary>
    private (ImmutableArray<Diagnostic> diagnostics, string generatedCode) GetGeneratedOutput(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Fudie.Infrastructure.IGet<,>).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new Fudie.Generator.RepositorySourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var generatedCode = string.Empty;
        var runResult = driver.GetRunResult();

        if (runResult.GeneratedTrees.Any())
        {
            generatedCode = runResult.GeneratedTrees.First().ToString();
        }

        return (diagnostics, generatedCode);
    }
}
