using Fudie.Infrastructure.Generator.QueryMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Fudie.UnitTests.Generator.QueryMethod;

/// <summary>
/// Tests para el validador de queries
/// </summary>
public class QueryValidatorTests
{
    private const string TestEntityCode = @"
        namespace TestNamespace
        {
            public class User
            {
                public System.Guid Id { get; set; }
                public string Name { get; set; }
                public string Email { get; set; }
                public int Age { get; set; }
                public bool Active { get; set; }
                public System.DateTime CreatedAt { get; set; }
                public UserStatus Status { get; set; }
                public string? Description { get; set; }
            }

            public enum UserStatus
            {
                Active,
                Inactive,
                Pending
            }

            public interface IUserRepository
            {
                System.Threading.Tasks.Task<User?> FindByEmail(string email);
                System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByName(string name);
                System.Threading.Tasks.Task<int> CountByActiveTrue();
                System.Threading.Tasks.Task<bool> ExistsByEmail(string email);
            }
        }";

    private (Compilation compilation, INamedTypeSymbol entityType, INamedTypeSymbol interfaceType) GetTestCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(TestEntityCode);
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location)
            });

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();

        var userClass = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "User");

        var interfaceDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax>()
            .First(i => i.Identifier.Text == "IUserRepository");

        var entityType = semanticModel.GetDeclaredSymbol(userClass) as INamedTypeSymbol;
        var interfaceType = semanticModel.GetDeclaredSymbol(interfaceDecl) as INamedTypeSymbol;

        return (compilation, entityType!, interfaceType!);
    }

    #region Property Existence Tests

    [Fact]
    public void Validate_ExistingProperty_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_NonExistingProperty_ShouldReportREPO001()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Emial", Operator.Equal) // Typo: should be "Email"
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Single(diagnostics);
        Assert.Equal("REPO001", diagnostics[0].Id);
        Assert.Contains("Emial", diagnostics[0].GetMessage());
        Assert.Contains("Email", diagnostics[0].GetMessage()); // Should suggest "Email"
    }

    [Fact]
    public void Validate_PropertyInOrderBy_ShouldValidate()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            },
            OrderBy = new List<OrderBy>
            {
                new("CreatedAt", Descending: true)
            }
        };

        var method = interfaceType.GetMembers("FindByName").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_NonExistingPropertyInOrderBy_ShouldReportREPO001()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            },
            OrderBy = new List<OrderBy>
            {
                new("UpdatedAt", Descending: true) // Doesn't exist
            }
        };

        var method = interfaceType.GetMembers("FindByName").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Single(diagnostics);
        Assert.Equal("REPO001", diagnostics[0].Id);
        Assert.Contains("UpdatedAt", diagnostics[0].GetMessage());
    }

    #endregion

    #region Parameter Count Tests

    [Fact]
    public void Validate_CorrectParameterCount_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_MissingParameter_ShouldReportREPO003()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();

        // Query expects 2 parameters (Name and Age) but method only has 1
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByName").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO003");
    }

    [Fact]
    public void Validate_OperatorWithNoParameters_ShouldNotRequireParameter()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True) // True operator requires no parameter
            }
        };

        var method = interfaceType.GetMembers("CountByActiveTrue").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void Validate_FindByWithCorrectReturnType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByName").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_FindFirstByWithCorrectReturnType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_CountByWithCorrectReturnType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        var method = interfaceType.GetMembers("CountByActiveTrue").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_ExistsByWithCorrectReturnType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("ExistsByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    #endregion

    #region Operator Compatibility Tests

    [Fact]
    public void Validate_NumericOperatorOnNumericProperty_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.GreaterThan)
            }
        };

        // Create a mock method
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByAgeGreaterThan(int age);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var methodDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();
        var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_NumericOperatorOnStringProperty_ShouldReportREPO006()
    {
        // Arrange
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.GreaterThan) // GreaterThan not valid for string
            }
        };

        // Create a mock method
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByNameGreaterThan(string name);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var methodDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();
        var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_StringOperatorOnStringProperty_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.StartsWith)
            }
        };

        // Create a mock method
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByNameStartingWith(string name);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var methodDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();
        var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_BooleanOperatorOnBooleanProperty_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        var method = interfaceType.GetMembers("CountByActiveTrue").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_BooleanOperatorOnNonBooleanProperty_ShouldReportREPO006()
    {
        // Arrange
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.True) // True operator only valid for bool
            }
        };

        // Create a mock method
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByNameTrue();
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var methodDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();
        var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    #endregion

    #region Type Compatibility Tests

    [Fact]
    public void Validate_MatchingParameterType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO002");
    }

    #endregion
}
