using Fudie.Generator.QueryMethod;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Fudie.UnitTests.Generator.QueryMethod;

/// <summary>
/// Tests para las definiciones de diagnósticos del Query Method Generator
/// </summary>
public class DiagnosticsTests
{
    #region Diagnostic Descriptors Tests

    [Fact]
    public void PropertyNotFound_ShouldHaveCorrectId()
    {
        // Assert
        Assert.Equal("REPO001", Diagnostics.PropertyNotFound.Id);
        Assert.Equal(DiagnosticSeverity.Error, Diagnostics.PropertyNotFound.DefaultSeverity);
        Assert.True(Diagnostics.PropertyNotFound.IsEnabledByDefault);
    }

    [Fact]
    public void TypeMismatch_ShouldHaveCorrectId()
    {
        // Assert
        Assert.Equal("REPO002", Diagnostics.TypeMismatch.Id);
        Assert.Equal(DiagnosticSeverity.Error, Diagnostics.TypeMismatch.DefaultSeverity);
        Assert.True(Diagnostics.TypeMismatch.IsEnabledByDefault);
    }

    [Fact]
    public void MissingParameter_ShouldHaveCorrectId()
    {
        // Assert
        Assert.Equal("REPO003", Diagnostics.MissingParameter.Id);
        Assert.Equal(DiagnosticSeverity.Error, Diagnostics.MissingParameter.DefaultSeverity);
        Assert.True(Diagnostics.MissingParameter.IsEnabledByDefault);
    }

    [Fact]
    public void WrongParameterCount_ShouldHaveCorrectId()
    {
        // Assert
        Assert.Equal("REPO004", Diagnostics.WrongParameterCount.Id);
        Assert.Equal(DiagnosticSeverity.Error, Diagnostics.WrongParameterCount.DefaultSeverity);
        Assert.True(Diagnostics.WrongParameterCount.IsEnabledByDefault);
    }

    [Fact]
    public void WrongReturnType_ShouldHaveCorrectId()
    {
        // Assert
        Assert.Equal("REPO005", Diagnostics.WrongReturnType.Id);
        Assert.Equal(DiagnosticSeverity.Error, Diagnostics.WrongReturnType.DefaultSeverity);
        Assert.True(Diagnostics.WrongReturnType.IsEnabledByDefault);
    }

    [Fact]
    public void IncompatibleOperator_ShouldHaveCorrectId()
    {
        // Assert
        Assert.Equal("REPO006", Diagnostics.IncompatibleOperator.Id);
        Assert.Equal(DiagnosticSeverity.Error, Diagnostics.IncompatibleOperator.DefaultSeverity);
        Assert.True(Diagnostics.IncompatibleOperator.IsEnabledByDefault);
    }

    [Fact]
    public void ParseError_ShouldHaveCorrectId()
    {
        // Assert
        Assert.Equal("REPO007", Diagnostics.ParseError.Id);
        Assert.Equal(DiagnosticSeverity.Error, Diagnostics.ParseError.DefaultSeverity);
        Assert.True(Diagnostics.ParseError.IsEnabledByDefault);
    }

    #endregion

    #region Create Diagnostic Tests

    [Fact]
    public void CreatePropertyNotFound_ShouldCreateDiagnosticWithoutSuggestion()
    {
        // Arrange & Act
        var diagnostic = Diagnostics.CreatePropertyNotFound(
            "Emial",
            "User",
            null,
            Location.None
        );

        // Assert
        Assert.Equal("REPO001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("Emial", diagnostic.GetMessage());
        Assert.Contains("User", diagnostic.GetMessage());
    }

    [Fact]
    public void CreatePropertyNotFound_ShouldCreateDiagnosticWithSuggestion()
    {
        // Arrange & Act
        var diagnostic = Diagnostics.CreatePropertyNotFound(
            "Emial",
            "User",
            "Email",
            Location.None
        );

        // Assert
        Assert.Equal("REPO001", diagnostic.Id);
        Assert.Contains("Emial", diagnostic.GetMessage());
        Assert.Contains("User", diagnostic.GetMessage());
        Assert.Contains("Did you mean 'Email'?", diagnostic.GetMessage());
    }

    [Fact]
    public void CreateTypeMismatch_ShouldCreateDiagnostic()
    {
        // Arrange & Act
        var diagnostic = Diagnostics.CreateTypeMismatch(
            "age",
            "string",
            "Age",
            "int",
            Location.None
        );

        // Assert
        Assert.Equal("REPO002", diagnostic.Id);
        Assert.Contains("age", diagnostic.GetMessage());
        Assert.Contains("string", diagnostic.GetMessage());
        Assert.Contains("Age", diagnostic.GetMessage());
        Assert.Contains("int", diagnostic.GetMessage());
    }

    [Fact]
    public void CreateMissingParameter_ShouldCreateDiagnostic()
    {
        // Arrange & Act
        var diagnostic = Diagnostics.CreateMissingParameter(
            "Age",
            "FindByNameAndAge",
            Location.None
        );

        // Assert
        Assert.Equal("REPO003", diagnostic.Id);
        Assert.Contains("Age", diagnostic.GetMessage());
        Assert.Contains("FindByNameAndAge", diagnostic.GetMessage());
    }

    [Fact]
    public void CreateWrongParameterCount_ShouldCreateDiagnostic()
    {
        // Arrange & Act
        var diagnostic = Diagnostics.CreateWrongParameterCount(
            "FindByName",
            2,
            1,
            Location.None
        );

        // Assert
        Assert.Equal("REPO004", diagnostic.Id);
        Assert.Contains("FindByName", diagnostic.GetMessage());
        Assert.Contains("2", diagnostic.GetMessage());
        Assert.Contains("1", diagnostic.GetMessage());
    }

    [Fact]
    public void CreateWrongReturnType_ShouldCreateDiagnostic()
    {
        // Arrange & Act
        var diagnostic = Diagnostics.CreateWrongReturnType(
            "CountByActiveTrue",
            "Task<int>",
            "Task<User>",
            Location.None
        );

        // Assert
        Assert.Equal("REPO005", diagnostic.Id);
        Assert.Contains("CountByActiveTrue", diagnostic.GetMessage());
        Assert.Contains("Task<int>", diagnostic.GetMessage());
        Assert.Contains("Task<User>", diagnostic.GetMessage());
    }

    [Fact]
    public void CreateIncompatibleOperator_ShouldCreateDiagnostic()
    {
        // Arrange & Act
        var diagnostic = Diagnostics.CreateIncompatibleOperator(
            "GreaterThan",
            "string",
            Location.None
        );

        // Assert
        Assert.Equal("REPO006", diagnostic.Id);
        Assert.Contains("GreaterThan", diagnostic.GetMessage());
        Assert.Contains("string", diagnostic.GetMessage());
    }

    [Fact]
    public void CreateParseError_ShouldCreateDiagnostic()
    {
        // Arrange & Act
        var diagnostic = Diagnostics.CreateParseError(
            "FindByXyz",
            "Unknown prefix 'FindByXyz'",
            Location.None
        );

        // Assert
        Assert.Equal("REPO007", diagnostic.Id);
        Assert.Contains("FindByXyz", diagnostic.GetMessage());
        Assert.Contains("Unknown prefix", diagnostic.GetMessage());
    }

    #endregion

    #region Category Tests

    [Fact]
    public void AllDiagnostics_ShouldHaveSameCategory()
    {
        // Arrange
        var expectedCategory = "Fudie.QueryMethod";

        // Assert
        Assert.Equal(expectedCategory, Diagnostics.PropertyNotFound.Category);
        Assert.Equal(expectedCategory, Diagnostics.TypeMismatch.Category);
        Assert.Equal(expectedCategory, Diagnostics.MissingParameter.Category);
        Assert.Equal(expectedCategory, Diagnostics.WrongParameterCount.Category);
        Assert.Equal(expectedCategory, Diagnostics.WrongReturnType.Category);
        Assert.Equal(expectedCategory, Diagnostics.IncompatibleOperator.Category);
        Assert.Equal(expectedCategory, Diagnostics.ParseError.Category);
    }

    #endregion
}
