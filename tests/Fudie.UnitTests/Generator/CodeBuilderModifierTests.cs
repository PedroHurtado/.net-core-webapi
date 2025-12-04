using FluentAssertions;
using Fudie.Generator;

namespace Fudie.UnitTests.Generator;

public class CodeBuilderModifierTests
{
    #region AsNoTracking

    [Fact]
    public void GenerateAsNoTracking_WithDefaultVariable_ShouldGenerateCorrectCode()
    {
        // Act
        var result = CodeBuilder.GenerateAsNoTracking();

        // Assert
        result.Should().Be("query = query.AsNoTracking();");
    }

    [Fact]
    public void GenerateAsNoTracking_WithCustomVariable_ShouldUseCustomName()
    {
        // Act
        var result = CodeBuilder.GenerateAsNoTracking("myQuery");

        // Assert
        result.Should().Be("myQuery = myQuery.AsNoTracking();");
    }

    #endregion

    #region AsSplitQuery

    [Fact]
    public void GenerateAsSplitQuery_WithDefaultVariable_ShouldGenerateCorrectCode()
    {
        // Act
        var result = CodeBuilder.GenerateAsSplitQuery();

        // Assert
        result.Should().Be("query = query.AsSplitQuery();");
    }

    [Fact]
    public void GenerateAsSplitQuery_WithCustomVariable_ShouldUseCustomName()
    {
        // Act
        var result = CodeBuilder.GenerateAsSplitQuery("myQuery");

        // Assert
        result.Should().Be("myQuery = myQuery.AsSplitQuery();");
    }

    #endregion

    #region IgnoreQueryFilters

    [Fact]
    public void GenerateIgnoreQueryFilters_WithDefaultVariable_ShouldGenerateCorrectCode()
    {
        // Act
        var result = CodeBuilder.GenerateIgnoreQueryFilters();

        // Assert
        result.Should().Be("query = query.IgnoreQueryFilters();");
    }

    [Fact]
    public void GenerateIgnoreQueryFilters_WithCustomVariable_ShouldUseCustomName()
    {
        // Act
        var result = CodeBuilder.GenerateIgnoreQueryFilters("myQuery");

        // Assert
        result.Should().Be("myQuery = myQuery.IgnoreQueryFilters();");
    }

    #endregion
}