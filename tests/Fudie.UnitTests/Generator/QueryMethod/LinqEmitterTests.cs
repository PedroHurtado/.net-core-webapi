using Fudie.Generator.QueryMethod;
using Xunit;

namespace Fudie.UnitTests.Generator.QueryMethod;

/// <summary>
/// Tests para el generador de código LINQ
/// </summary>
public class LinqEmitterTests
{
    #region Simple Queries Tests

    [Fact]
    public void Emit_FindByName_ShouldGenerateWhereAndToListAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByName", "User", new[] { "name" });

        // Assert
        Assert.Contains("_query.Query<User>()", code);
        Assert.Contains(".Where(x => x.Name == name)", code);
        Assert.Contains(".ToListAsync()", code);
    }

    [Fact]
    public void Emit_FindFirstByEmail_ShouldGenerateFirstOrDefaultAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindFirstByEmail", "User", new[] { "email" });

        // Assert
        Assert.Contains(".Where(x => x.Email == email)", code);
        Assert.Contains(".FirstOrDefaultAsync()", code);
        Assert.DoesNotContain(".ToListAsync()", code);
    }

    [Fact]
    public void Emit_CountByActiveTrue_ShouldGenerateCountAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        // Act
        var code = emitter.Emit(query, "CountByActiveTrue", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".Where(x => x.Active == true)", code);
        Assert.Contains(".CountAsync()", code);
    }

    [Fact]
    public void Emit_ExistsByEmail_ShouldGenerateAnyAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        // Act
        var code = emitter.Emit(query, "ExistsByEmail", "User", new[] { "email" });

        // Assert
        Assert.Contains(".Where(x => x.Email == email)", code);
        Assert.Contains(".AnyAsync()", code);
    }

    [Fact]
    public void Emit_DeleteByActiveFalse_ShouldGenerateExecuteDeleteAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition>
            {
                new("Active", Operator.False)
            }
        };

        // Act
        var code = emitter.Emit(query, "DeleteByActiveFalse", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".Where(x => x.Active == false)", code);
        Assert.Contains(".ExecuteDeleteAsync()", code);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void Emit_NotEqualOperator_ShouldGenerateNotEqual()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Status", Operator.NotEqual)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByStatusNot", "User", new[] { "status" });

        // Assert
        Assert.Contains("x.Status != status", code);
    }

    [Fact]
    public void Emit_LessThanOperator_ShouldGenerateLessThan()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.LessThan)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByAgeLessThan", "User", new[] { "age" });

        // Assert
        Assert.Contains("x.Age < age", code);
    }

    [Fact]
    public void Emit_GreaterThanOperator_ShouldGenerateGreaterThan()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.GreaterThan)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByAgeGreaterThan", "User", new[] { "age" });

        // Assert
        Assert.Contains("x.Age > age", code);
    }

    [Fact]
    public void Emit_BetweenOperator_ShouldGenerateBetweenCondition()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.Between)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByAgeBetween", "User", new[] { "min", "max" });

        // Assert
        Assert.Contains("x.Age >= min && x.Age <= max", code);
    }

    [Fact]
    public void Emit_InOperator_ShouldGenerateContains()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Status", Operator.In)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByStatusIn", "User", new[] { "statuses" });

        // Assert
        Assert.Contains("statuses.Contains(x.Status)", code);
    }

    [Fact]
    public void Emit_NotInOperator_ShouldGenerateNotContains()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Status", Operator.NotIn)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByStatusNotIn", "User", new[] { "statuses" });

        // Assert
        Assert.Contains("!statuses.Contains(x.Status)", code);
    }

    [Fact]
    public void Emit_StartsWithOperator_ShouldGenerateStartsWith()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.StartsWith)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameStartingWith", "User", new[] { "prefix" });

        // Assert
        Assert.Contains("x.Name.StartsWith(prefix)", code);
    }

    [Fact]
    public void Emit_EndsWithOperator_ShouldGenerateEndsWith()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.EndsWith)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameEndingWith", "User", new[] { "suffix" });

        // Assert
        Assert.Contains("x.Name.EndsWith(suffix)", code);
    }

    [Fact]
    public void Emit_ContainsOperator_ShouldGenerateContains()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Contains)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameContaining", "User", new[] { "text" });

        // Assert
        Assert.Contains("x.Name.Contains(text)", code);
    }

    [Fact]
    public void Emit_LikeOperator_ShouldGenerateEFLike()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Like)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameLike", "User", new[] { "pattern" });

        // Assert
        Assert.Contains("EF.Functions.Like(x.Name, pattern)", code);
    }

    [Fact]
    public void Emit_IsNullOperator_ShouldGenerateNullCheck()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Description", Operator.IsNull)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByDescriptionIsNull", "User", Array.Empty<string>());

        // Assert
        Assert.Contains("x.Description == null", code);
    }

    [Fact]
    public void Emit_IsNotNullOperator_ShouldGenerateNotNullCheck()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Description", Operator.IsNotNull)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByDescriptionIsNotNull", "User", Array.Empty<string>());

        // Assert
        Assert.Contains("x.Description != null", code);
    }

    #endregion

    #region Logical Operators Tests

    [Fact]
    public void Emit_AndConditions_ShouldGenerateAndOperator()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.Equal)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameAndAge", "User", new[] { "name", "age" });

        // Assert
        Assert.Contains("x.Name == name && x.Age == age", code);
    }

    [Fact]
    public void Emit_OrConditions_ShouldGenerateOrOperator()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Email", Operator.Equal, Or: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameOrEmail", "User", new[] { "name", "email" });

        // Assert
        Assert.Contains("x.Name == name || x.Email == email", code);
    }

    [Fact]
    public void Emit_MixedAndOr_ShouldGenerateCorrectPrecedence()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.Equal),
                new("Status", Operator.Equal, Or: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameAndAgeOrStatus", "User", new[] { "name", "age", "status" });

        // Assert
        Assert.Contains("x.Name == name && x.Age == age || x.Status == status", code);
    }

    #endregion

    #region IgnoreCase Tests

    [Fact]
    public void Emit_IgnoreCase_ShouldGenerateToLower()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal, IgnoreCase: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByEmailIgnoreCase", "User", new[] { "email" });

        // Assert
        Assert.Contains("x.Email.ToLower() == email.ToLower()", code);
    }

    #endregion

    #region OrderBy Tests

    [Fact]
    public void Emit_OrderByAscending_ShouldGenerateOrderBy()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            },
            OrderBy = new List<OrderBy>
            {
                new("CreatedAt", Descending: false)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByActiveTrueOrderByCreatedAt", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".OrderBy(x => x.CreatedAt)", code);
    }

    [Fact]
    public void Emit_OrderByDescending_ShouldGenerateOrderByDescending()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            },
            OrderBy = new List<OrderBy>
            {
                new("Score", Descending: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByActiveTrueOrderByScoreDesc", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".OrderByDescending(x => x.Score)", code);
    }

    #endregion

    #region Top Tests

    [Fact]
    public void Emit_Top10_ShouldGenerateTake()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Top = 10,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindTop10ByActiveTrue", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".Take(10)", code);
        Assert.Contains(".ToListAsync()", code);
    }

    #endregion

    #region Complex Query Tests

    [Fact]
    public void Emit_ComplexQuery_ShouldGenerateAllParts()
    {
        // Arrange
        var emitter = new LinqEmitter();
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

        // Act
        var code = emitter.Emit(query, "FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc", "User", new[] { "age" });

        // Assert
        Assert.Contains("_query.Query<User>()", code);
        Assert.Contains(".Where(x => x.Age > age && x.Active == true)", code);
        Assert.Contains(".OrderByDescending(x => x.CreatedAt)", code);
        Assert.Contains(".ToListAsync()", code);
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void EmitMethodSignature_FindBy_ShouldGenerateCorrectSignature()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "FindByName", "User", new[] { ("name", "string") });

        // Assert
        Assert.Contains("public async Task<List<User>> FindByName(string name)", signature);
    }

    [Fact]
    public void EmitMethodSignature_FindFirstBy_ShouldGenerateNullableReturn()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "FindFirstByEmail", "User", new[] { ("email", "string") });

        // Assert
        Assert.Contains("public async Task<User?> FindFirstByEmail(string email)", signature);
    }

    [Fact]
    public void EmitMethodSignature_CountBy_ShouldGenerateIntReturn()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "CountByActiveTrue", "User", Array.Empty<(string, string)>());

        // Assert
        Assert.Contains("public async Task<int> CountByActiveTrue()", signature);
    }

    #endregion
}
