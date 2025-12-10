using Fudie.Generator.QueryMethod;
using Xunit;

namespace Fudie.Generator.UnitTests.QueryMethod;

/// <summary>
/// Tests para el parser de nombres de métodos de query
/// </summary>
public class QueryParserTests
{
    #region Prefix Detection Tests

    [Theory]
    [InlineData("FindByName")]
    [InlineData("FindByEmail")]
    [InlineData("FindByAgeAndName")]
    public void Parse_FindByPrefix_ShouldReturnFindQuery(string methodName)
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse(methodName, new[] { "Name", "Email", "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryType.Find, result.Query.Type);
        Assert.False(result.Query.First);
        Assert.Null(result.Query.Top);
    }

    [Theory]
    [InlineData("FindFirstByEmail")]
    [InlineData("FindFirstByName")]
    public void Parse_FindFirstByPrefix_ShouldReturnFindQueryWithFirst(string methodName)
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse(methodName, new[] { "Name", "Email" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryType.Find, result.Query.Type);
        Assert.True(result.Query.First);
        Assert.Null(result.Query.Top);
    }

    [Theory]
    [InlineData("FindTop10ByActiveTrue", 10)]
    [InlineData("FindTop5ByName", 5)]
    [InlineData("FindTop1ByEmail", 1)]
    [InlineData("FindTop100ByStatus", 100)]
    public void Parse_FindTopNByPrefix_ShouldReturnFindQueryWithTop(string methodName, int expectedTop)
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse(methodName, new[] { "Name", "Email", "Active", "Status" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryType.Find, result.Query.Type);
        Assert.False(result.Query.First);
        Assert.Equal(expectedTop, result.Query.Top);
    }

    [Theory]
    [InlineData("CountByActiveTrue")]
    [InlineData("CountByStatus")]
    public void Parse_CountByPrefix_ShouldReturnCountQuery(string methodName)
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse(methodName, new[] { "Active", "Status" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryType.Count, result.Query.Type);
    }

    [Theory]
    [InlineData("ExistsByEmail")]
    [InlineData("ExistsByName")]
    public void Parse_ExistsByPrefix_ShouldReturnExistsQuery(string methodName)
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse(methodName, new[] { "Email", "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryType.Exists, result.Query.Type);
    }

    [Theory]
    [InlineData("DeleteByActiveFalse")]
    [InlineData("DeleteByStatus")]
    public void Parse_DeleteByPrefix_ShouldReturnDeleteQuery(string methodName)
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse(methodName, new[] { "Active", "Status" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryType.Delete, result.Query.Type);
    }

    #endregion

    #region Simple Conditions Tests

    [Fact]
    public void Parse_FindByName_ShouldReturnEqualCondition()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByName", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal("Name", result.Query.Conditions[0].Property);
        Assert.Equal(Operator.Equal, result.Query.Conditions[0].Op);
        Assert.False(result.Query.Conditions[0].Or);
    }

    [Fact]
    public void Parse_FindByNameAndAge_ShouldReturnTwoAndConditions()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameAndAge", new[] { "Name", "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(2, result.Query.Conditions.Count);

        Assert.Equal("Name", result.Query.Conditions[0].Property);
        Assert.Equal(Operator.Equal, result.Query.Conditions[0].Op);
        Assert.False(result.Query.Conditions[0].Or);

        Assert.Equal("Age", result.Query.Conditions[1].Property);
        Assert.Equal(Operator.Equal, result.Query.Conditions[1].Op);
        Assert.False(result.Query.Conditions[1].Or);
    }

    [Fact]
    public void Parse_FindByNameOrAge_ShouldReturnTwoOrConditions()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameOrAge", new[] { "Name", "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(2, result.Query.Conditions.Count);

        Assert.Equal("Name", result.Query.Conditions[0].Property);
        Assert.False(result.Query.Conditions[0].Or);

        Assert.Equal("Age", result.Query.Conditions[1].Property);
        Assert.True(result.Query.Conditions[1].Or);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void Parse_FindByNameNot_ShouldReturnNotEqualOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameNot", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.NotEqual, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByAgeLessThan_ShouldReturnLessThanOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByAgeLessThan", new[] { "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal("Age", result.Query.Conditions[0].Property);
        Assert.Equal(Operator.LessThan, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByAgeLessThanEqual_ShouldReturnLessThanOrEqualOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByAgeLessThanEqual", new[] { "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.LessThanOrEqual, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByAgeGreaterThan_ShouldReturnGreaterThanOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByAgeGreaterThan", new[] { "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.GreaterThan, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByAgeGreaterThanEqual_ShouldReturnGreaterThanOrEqualOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByAgeGreaterThanEqual", new[] { "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.GreaterThanOrEqual, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByAgeBetween_ShouldReturnBetweenOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByAgeBetween", new[] { "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.Between, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByStatusIn_ShouldReturnInOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByStatusIn", new[] { "Status" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.In, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByStatusNotIn_ShouldReturnNotInOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByStatusNotIn", new[] { "Status" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.NotIn, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByNameStartingWith_ShouldReturnStartsWithOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameStartingWith", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.StartsWith, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByNameEndingWith_ShouldReturnEndsWithOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameEndingWith", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.EndsWith, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByNameContaining_ShouldReturnContainsOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameContaining", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.Contains, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByNameLike_ShouldReturnLikeOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameLike", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.Like, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByNameIsNull_ShouldReturnIsNullOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameIsNull", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.IsNull, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByNameIsNotNull_ShouldReturnIsNotNullOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameIsNotNull", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.IsNotNull, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByActiveTrue_ShouldReturnTrueOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByActiveTrue", new[] { "Active" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.True, result.Query.Conditions[0].Op);
    }

    [Fact]
    public void Parse_FindByActiveFalse_ShouldReturnFalseOperator()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByActiveFalse", new[] { "Active" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal(Operator.False, result.Query.Conditions[0].Op);
    }

    #endregion

    #region IgnoreCase Tests

    [Fact]
    public void Parse_FindByNameIgnoreCase_ShouldSetIgnoreCaseFlag()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameIgnoreCase", new[] { "Name" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal("Name", result.Query.Conditions[0].Property);
        Assert.Equal(Operator.Equal, result.Query.Conditions[0].Op);
        Assert.True(result.Query.Conditions[0].IgnoreCase);
    }

    #endregion

    #region OrderBy Tests

    [Fact]
    public void Parse_FindByNameOrderByAge_ShouldReturnOrderByAscending()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameOrderByAge", new[] { "Name", "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Single(result.Query.OrderBy);
        Assert.Equal("Age", result.Query.OrderBy[0].Property);
        Assert.False(result.Query.OrderBy[0].Descending);
    }

    [Fact]
    public void Parse_FindByNameOrderByAgeDesc_ShouldReturnOrderByDescending()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameOrderByAgeDesc", new[] { "Name", "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.OrderBy);
        Assert.Equal("Age", result.Query.OrderBy[0].Property);
        Assert.True(result.Query.OrderBy[0].Descending);
    }

    [Fact]
    public void Parse_FindByNameOrderByAgeAsc_ShouldReturnOrderByAscending()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameOrderByAgeAsc", new[] { "Name", "Age" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.OrderBy);
        Assert.Equal("Age", result.Query.OrderBy[0].Property);
        Assert.False(result.Query.OrderBy[0].Descending);
    }

    #endregion

    #region Compound Property Tests

    [Fact]
    public void Parse_FindByCreatedAt_ShouldRecognizeCompoundProperty()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByCreatedAt", new[] { "CreatedAt" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal("CreatedAt", result.Query.Conditions[0].Property);
    }

    [Fact]
    public void Parse_FindByFirstName_ShouldRecognizeCompoundProperty()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByFirstName", new[] { "FirstName" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Single(result.Query.Conditions);
        Assert.Equal("FirstName", result.Query.Conditions[0].Property);
    }

    #endregion

    #region Complex Query Tests

    [Fact]
    public void Parse_ComplexQuery_ShouldParseCorrectly()
    {
        // FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse(
            "FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc",
            new[] { "Age", "Active", "CreatedAt" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(QueryType.Find, result.Query.Type);

        // Conditions
        Assert.Equal(2, result.Query.Conditions.Count);
        Assert.Equal("Age", result.Query.Conditions[0].Property);
        Assert.Equal(Operator.GreaterThan, result.Query.Conditions[0].Op);
        Assert.Equal("Active", result.Query.Conditions[1].Property);
        Assert.Equal(Operator.True, result.Query.Conditions[1].Op);

        // OrderBy
        Assert.Single(result.Query.OrderBy);
        Assert.Equal("CreatedAt", result.Query.OrderBy[0].Property);
        Assert.True(result.Query.OrderBy[0].Descending);
    }

    [Fact]
    public void Parse_FindByNameAndAgeOrStatus_ShouldHandleMixedLogicalOperators()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindByNameAndAgeOrStatus", new[] { "Name", "Age", "Status" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Query);
        Assert.Equal(3, result.Query.Conditions.Count);

        Assert.Equal("Name", result.Query.Conditions[0].Property);
        Assert.False(result.Query.Conditions[0].Or);

        Assert.Equal("Age", result.Query.Conditions[1].Property);
        Assert.False(result.Query.Conditions[1].Or);

        Assert.Equal("Status", result.Query.Conditions[2].Property);
        Assert.True(result.Query.Conditions[2].Or);
    }

    #endregion

    #region Error Cases Tests

    [Fact]
    public void Parse_InvalidPrefix_ShouldReturnError()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("GetByName", new[] { "Name" });

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Query);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Parse_EmptyMethodName_ShouldReturnError()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("", new[] { "Name" });

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Query);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Parse_OnlyPrefix_ShouldReturnError()
    {
        // Arrange
        var parser = new QueryParser();

        // Act
        var result = parser.Parse("FindBy", new[] { "Name" });

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Query);
        Assert.NotNull(result.ErrorMessage);
    }

    #endregion
}
