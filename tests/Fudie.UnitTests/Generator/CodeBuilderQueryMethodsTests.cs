using Fudie.Generator;
using Fudie.Generator.QueryMethod;
using Xunit;

namespace Fudie.UnitTests.Generator;

/// <summary>
/// Tests para la generación de query methods en CodeBuilder
/// </summary>
public class CodeBuilderQueryMethodsTests
{
    [Fact]
    public void GenerateQueryMethods_WithNullQueryMethods_ShouldReturnEmpty()
    {
        // Act
        var result = CodeBuilder.GenerateQueryMethods(null!, "User");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GenerateQueryMethods_WithEmptyQueryMethods_ShouldReturnEmpty()
    {
        // Arrange
        var queryMethods = new List<CodeBuilder.QueryMethodInfo>();

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GenerateQueryMethods_WithFailedParseResult_ShouldSkipMethod()
    {
        // Arrange
        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByInvalid",
                ParseResult = ParseResult.Error("Invalid method name"),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GenerateQueryMethods_WithSimpleFindBy_ShouldGenerateCorrectCode()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByEmail",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("email", "string") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<List<User>> FindByEmail(string email)", result);
        Assert.Contains("return await _query.Query<User>()", result);
        Assert.Contains(".Where(x => x.Email == email)", result);
        Assert.Contains(".ToListAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithFindFirstBy_ShouldGenerateNullableReturn()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindFirstByEmail",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("email", "string") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<User?> FindFirstByEmail(string email)", result);
        Assert.Contains(".FirstOrDefaultAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithCountBy_ShouldGenerateIntReturn()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "CountByActiveTrue",
                ParseResult = ParseResult.Ok(query),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<int> CountByActiveTrue()", result);
        Assert.Contains(".Where(x => x.Active == true)", result);
        Assert.Contains(".CountAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithExistsBy_ShouldGenerateBoolReturn()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "ExistsByEmail",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("email", "string") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<bool> ExistsByEmail(string email)", result);
        Assert.Contains(".AnyAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithDeleteBy_ShouldGenerateDeleteCode()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition>
            {
                new("Active", Operator.False)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "DeleteByActiveFalse",
                ParseResult = ParseResult.Ok(query),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<int> DeleteByActiveFalse()", result);
        Assert.Contains(".ExecuteDeleteAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithMultipleMethods_ShouldGenerateAll()
    {
        // Arrange
        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByEmail",
                ParseResult = ParseResult.Ok(new ParsedQuery
                {
                    Type = QueryType.Find,
                    Conditions = new List<Condition> { new("Email", Operator.Equal) }
                }),
                Parameters = new() { ("email", "string") }
            },
            new()
            {
                MethodName = "CountByActiveTrue",
                ParseResult = ParseResult.Ok(new ParsedQuery
                {
                    Type = QueryType.Count,
                    Conditions = new List<Condition> { new("Active", Operator.True) }
                }),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("FindByEmail", result);
        Assert.Contains("CountByActiveTrue", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithComplexQuery_ShouldGenerateAllParts()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.GreaterThan),
                new("Active", Operator.True)
            },
            OrderBy = new List<OrderBy>
            {
                new("CreatedAt", Descending: true)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("age", "int") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc", result);
        Assert.Contains("int age", result);
        Assert.Contains("x.Age > age && x.Active == true", result);
        Assert.Contains(".OrderByDescending(x => x.CreatedAt)", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithTop_ShouldGenerateTake()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Top = 10,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindTop10ByActiveTrue",
                ParseResult = ParseResult.Ok(query),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains(".Take(10)", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithMultipleParameters_ShouldIncludeAll()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.Equal)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByNameAndAge",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string"), ("age", "int") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("string name, int age", result);
        Assert.Contains("x.Name == name && x.Age == age", result);
    }
}
