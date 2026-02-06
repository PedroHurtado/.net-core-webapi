namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class CultureCodeCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly CultureCode.Create _create = fixture.Get<CultureCode.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsCultureCode()
    {
        var command = new CreateCultureCodeCommand("es-ES");

        var result = _create.Execute(command);

        result.Code.Should().Be("es-ES");
    }

    [Fact]
    public void Execute_WithEnglishCode_ReturnsCultureCode()
    {
        var command = new CreateCultureCodeCommand("en-GB");

        var result = _create.Execute(command);

        result.Code.Should().Be("en-GB");
    }

    [Fact]
    public void Execute_WithEmptyCode_ThrowsValidationException()
    {
        var command = new CreateCultureCodeCommand("");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{CultureCodeValidationMessages.CodeRequired}*");
    }

    [Fact]
    public void Execute_WithInvalidFormat_ThrowsValidationException()
    {
        var command = new CreateCultureCodeCommand("español");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{CultureCodeValidationMessages.CodeFormat}*");
    }

    [Fact]
    public void Execute_WithLanguageOnly_ThrowsValidationException()
    {
        var command = new CreateCultureCodeCommand("es");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{CultureCodeValidationMessages.CodeFormat}*");
    }
}
