using Fudie.Generator.QueryMethod;

namespace Fudie.Generator.UnitTests.QueryMethod;

/// <summary>
/// Tests para los modelos de datos del Query Method Generator
/// </summary>
public class ModelsTests
{
    #region QueryType Tests

    [Fact]
    public void QueryType_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<QueryType>();

        // Assert
        Assert.Contains(QueryType.Find, values);
        Assert.Contains(QueryType.Count, values);
        Assert.Contains(QueryType.Exists, values);
        Assert.Contains(QueryType.Delete, values);
        Assert.Equal(4, values.Length);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void Operator_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<Operator>();

        // Assert
        Assert.Contains(Operator.Equal, values);
        Assert.Contains(Operator.NotEqual, values);
        Assert.Contains(Operator.LessThan, values);
        Assert.Contains(Operator.LessThanOrEqual, values);
        Assert.Contains(Operator.GreaterThan, values);
        Assert.Contains(Operator.GreaterThanOrEqual, values);
        Assert.Contains(Operator.Between, values);
        Assert.Contains(Operator.In, values);
        Assert.Contains(Operator.NotIn, values);
        Assert.Contains(Operator.StartsWith, values);
        Assert.Contains(Operator.EndsWith, values);
        Assert.Contains(Operator.Contains, values);
        Assert.Contains(Operator.Like, values);
        Assert.Contains(Operator.IsNull, values);
        Assert.Contains(Operator.IsNotNull, values);
        Assert.Contains(Operator.True, values);
        Assert.Contains(Operator.False, values);
        Assert.Equal(17, values.Length);
    }

    #endregion

    #region Condition Tests

    [Fact]
    public void Condition_ShouldCreateWithRequiredProperties()
    {
        // Arrange & Act
        var condition = new Condition("Name", Operator.Equal);

        // Assert
        Assert.Equal("Name", condition.Property);
        Assert.Equal(Operator.Equal, condition.Op);
        Assert.False(condition.Or);
        Assert.False(condition.IgnoreCase);
    }

    [Fact]
    public void Condition_ShouldCreateWithOrFlag()
    {
        // Arrange & Act
        var condition = new Condition("Age", Operator.GreaterThan, Or: true);

        // Assert
        Assert.Equal("Age", condition.Property);
        Assert.Equal(Operator.GreaterThan, condition.Op);
        Assert.True(condition.Or);
        Assert.False(condition.IgnoreCase);
    }

    [Fact]
    public void Condition_ShouldCreateWithIgnoreCaseFlag()
    {
        // Arrange & Act
        var condition = new Condition("Email", Operator.Equal, IgnoreCase: true);

        // Assert
        Assert.Equal("Email", condition.Property);
        Assert.Equal(Operator.Equal, condition.Op);
        Assert.False(condition.Or);
        Assert.True(condition.IgnoreCase);
    }

    [Fact]
    public void Condition_ShouldSupportRecordEquality()
    {
        // Arrange
        var condition1 = new Condition("Name", Operator.Equal);
        var condition2 = new Condition("Name", Operator.Equal);
        var condition3 = new Condition("Name", Operator.NotEqual);

        // Assert
        Assert.Equal(condition1, condition2);
        Assert.NotEqual(condition1, condition3);
    }

    #endregion

    #region OrderBy Tests

    [Fact]
    public void OrderBy_ShouldCreateWithAscendingByDefault()
    {
        // Arrange & Act
        var orderBy = new OrderBy("CreatedAt");

        // Assert
        Assert.Equal("CreatedAt", orderBy.Property);
        Assert.False(orderBy.Descending);
    }

    [Fact]
    public void OrderBy_ShouldCreateWithDescending()
    {
        // Arrange & Act
        var orderBy = new OrderBy("Score", Descending: true);

        // Assert
        Assert.Equal("Score", orderBy.Property);
        Assert.True(orderBy.Descending);
    }

    [Fact]
    public void OrderBy_ShouldSupportRecordEquality()
    {
        // Arrange
        var orderBy1 = new OrderBy("Name");
        var orderBy2 = new OrderBy("Name");
        var orderBy3 = new OrderBy("Name", Descending: true);

        // Assert
        Assert.Equal(orderBy1, orderBy2);
        Assert.NotEqual(orderBy1, orderBy3);
    }

    #endregion

    #region ParsedQuery Tests

    [Fact]
    public void ParsedQuery_ShouldCreateWithDefaultValues()
    {
        // Arrange & Act
        var query = new ParsedQuery
        {
            Type = QueryType.Find
        };

        // Assert
        Assert.Equal(QueryType.Find, query.Type);
        Assert.False(query.First);
        Assert.Null(query.Top);
        Assert.NotNull(query.Conditions);
        Assert.Empty(query.Conditions);
        Assert.NotNull(query.OrderBy);
        Assert.Empty(query.OrderBy);
    }

    [Fact]
    public void ParsedQuery_ShouldCreateFindFirstQuery()
    {
        // Arrange & Act
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        // Assert
        Assert.Equal(QueryType.Find, query.Type);
        Assert.True(query.First);
        Assert.Null(query.Top);
        Assert.Single(query.Conditions);
        Assert.Equal("Email", query.Conditions[0].Property);
    }

    [Fact]
    public void ParsedQuery_ShouldCreateTopNQuery()
    {
        // Arrange & Act
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Top = 10,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            },
            OrderBy = new List<OrderBy>
            {
                new("Score", Descending: true)
            }
        };

        // Assert
        Assert.Equal(QueryType.Find, query.Type);
        Assert.False(query.First);
        Assert.Equal(10, query.Top);
        Assert.Single(query.Conditions);
        Assert.Single(query.OrderBy);
    }

    [Fact]
    public void ParsedQuery_ShouldCreateCountQuery()
    {
        // Arrange & Act
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Status", Operator.Equal)
            }
        };

        // Assert
        Assert.Equal(QueryType.Count, query.Type);
        Assert.Single(query.Conditions);
    }

    [Fact]
    public void ParsedQuery_ShouldCreateExistsQuery()
    {
        // Arrange & Act
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal, IgnoreCase: true)
            }
        };

        // Assert
        Assert.Equal(QueryType.Exists, query.Type);
        Assert.Single(query.Conditions);
        Assert.True(query.Conditions[0].IgnoreCase);
    }

    [Fact]
    public void ParsedQuery_ShouldCreateDeleteQuery()
    {
        // Arrange & Act
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition>
            {
                new("Active", Operator.False)
            }
        };

        // Assert
        Assert.Equal(QueryType.Delete, query.Type);
        Assert.Single(query.Conditions);
    }

    [Fact]
    public void ParsedQuery_ShouldSupportMultipleConditions()
    {
        // Arrange & Act
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.GreaterThan),
                new("Status", Operator.Equal, Or: true)
            }
        };

        // Assert
        Assert.Equal(3, query.Conditions.Count);
        Assert.False(query.Conditions[0].Or);
        Assert.False(query.Conditions[1].Or);
        Assert.True(query.Conditions[2].Or);
    }

    [Fact]
    public void ParsedQuery_ShouldSupportMultipleOrderBy()
    {
        // Arrange & Act
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            OrderBy = new List<OrderBy>
            {
                new("Category"),
                new("Score", Descending: true),
                new("Name")
            }
        };

        // Assert
        Assert.Equal(3, query.OrderBy.Count);
        Assert.False(query.OrderBy[0].Descending);
        Assert.True(query.OrderBy[1].Descending);
        Assert.False(query.OrderBy[2].Descending);
    }

    #endregion

    #region ParseResult Tests

    [Fact]
    public void ParseResult_Ok_ShouldCreateSuccessResult()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            }
        };

        // Act
        var result = ParseResult.Ok(query);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryType.Find, result.Query.Type);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ErrorPosition);
    }

    [Fact]
    public void ParseResult_Error_ShouldCreateErrorResult()
    {
        // Arrange & Act
        var result = ParseResult.Error("Invalid method name");

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Query);
        Assert.Equal("Invalid method name", result.ErrorMessage);
        Assert.Null(result.ErrorPosition);
    }

    [Fact]
    public void ParseResult_Error_ShouldCreateErrorResultWithPosition()
    {
        // Arrange & Act
        var result = ParseResult.Error("Unexpected token", position: 15);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Query);
        Assert.Equal("Unexpected token", result.ErrorMessage);
        Assert.Equal(15, result.ErrorPosition);
    }

    [Fact]
    public void ParseResult_ShouldSupportRecordEquality()
    {
        // Arrange
        var query = new ParsedQuery { Type = QueryType.Find };
        var result1 = ParseResult.Ok(query);
        var result2 = ParseResult.Ok(query);
        var result3 = ParseResult.Error("Error");

        // Assert
        Assert.Equal(result1, result2);
        Assert.NotEqual(result1, result3);
    }

    #endregion
}
