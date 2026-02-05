namespace Menus.UnitTests.Features.Menus.Domain.AllergenAggregate.Commands;

public class AllergenCreateTests
{
    private readonly AllergenValidator _validator = new();
    private readonly Allergen.Create _create;

    public AllergenCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsAllergen()
    {
        var command = new CreateAllergenCommand("GLUTEN", "Gluten");

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Id.Should().Be("GLUTEN");
        result.Name.Should().Be("Gluten");
        result.IconUrl.Should().BeNull();
        result.IsActive.Should().BeTrue();
        result.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsOptionalFields()
    {
        var command = new CreateAllergenCommand(
            Code: "LACTEOS",
            Name: "Lácteos",
            IconUrl: "https://example.com/lacteos.png",
            IsActive: false,
            DisplayOrder: 5);

        var result = _create.Execute(command);

        result.IconUrl.Should().Be("https://example.com/lacteos.png");
        result.IsActive.Should().BeFalse();
        result.DisplayOrder.Should().Be(5);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Execute_WithIsActive_SetsIsActive(bool isActive)
    {
        var command = new CreateAllergenCommand("GLUTEN", "Gluten", IsActive: isActive);

        var result = _create.Execute(command);

        result.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void Execute_WithDefaultIsActive_DefaultsToTrue()
    {
        var command = new CreateAllergenCommand("GLUTEN", "Gluten");

        var result = _create.Execute(command);

        result.IsActive.Should().BeTrue();
    }

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyCode_ThrowsValidationException(string? code)
    {
        var command = new CreateAllergenCommand(code!, "Gluten");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithCodeExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreateAllergenCommand(new string('A', 21), "Gluten");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("gluten")]
    [InlineData("Gluten")]
    [InlineData("GLUTEN-1")]
    [InlineData("GLUTEN 1")]
    public void Execute_WithInvalidCodeFormat_ThrowsValidationException(string code)
    {
        var command = new CreateAllergenCommand(code, "Gluten");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var command = new CreateAllergenCommand("GLUTEN", name!);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreateAllergenCommand("GLUTEN", new string('a', 101));

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithIconUrlExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreateAllergenCommand("GLUTEN", "Gluten", IconUrl: "https://example.com/" + new string('a', 500));

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/icon.png")]
    public void Execute_WithInvalidIconUrl_ThrowsValidationException(string iconUrl)
    {
        var command = new CreateAllergenCommand("GLUTEN", "Gluten", IconUrl: iconUrl);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var command = new CreateAllergenCommand("GLUTEN", "Gluten", DisplayOrder: -1);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}