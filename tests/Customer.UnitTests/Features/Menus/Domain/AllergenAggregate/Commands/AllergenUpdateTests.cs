namespace Customer.UnitTests.Features.Menus.Domain.AllergenAggregate.Commands;

public class AllergenUpdateTests
{
    private readonly AllergenValidator _validator = new();
    private readonly Allergen.Create _create;
    private readonly Allergen.Update _update;

    public AllergenUpdateTests()
    {
        _create = new(_validator);
        _update = new(_validator);
    }

    private Allergen CreateValidAllergen(string code = "GLUTEN") =>
        _create.Execute(new CreateAllergenCommand(
            Code: code,
            Name: "Original Name",
            IconUrl: "https://example.com/original.png",
            IsActive: true,
            DisplayOrder: 0));

    [Fact]
    public void Execute_WithValidCommand_UpdatesAllergen()
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand("Updated Name", null, true, 0);

        var result = _update.Execute(allergen, command);

        result.Should().NotBeNull();
        result.Id.Should().Be("GLUTEN");
        result.Name.Should().Be("Updated Name");
        result.IconUrl.Should().BeNull();
        result.IsActive.Should().BeTrue();
        result.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void Execute_WithAllFields_UpdatesAllFields()
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand(
            Name: "Lácteos",
            IconUrl: "https://example.com/lacteos.png",
            IsActive: false,
            DisplayOrder: 5);

        var result = _update.Execute(allergen, command);

        result.Name.Should().Be("Lácteos");
        result.IconUrl.Should().Be("https://example.com/lacteos.png");
        result.IsActive.Should().BeFalse();
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_PreservesAllergenId()
    {
        var allergen = CreateValidAllergen("LACTEOS");
        var command = new UpdateAllergenCommand("New Name", null, true, 0);

        var result = _update.Execute(allergen, command);

        result.Id.Should().Be("LACTEOS");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Execute_WithIsActive_SetsIsActive(bool isActive)
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand("Gluten", null, isActive, 0);

        var result = _update.Execute(allergen, command);

        result.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void Execute_WithIconUrl_SetsIconUrl()
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand("Gluten", "https://example.com/new-icon.png", true, 0);

        var result = _update.Execute(allergen, command);

        result.IconUrl.Should().Be("https://example.com/new-icon.png");
    }

    [Fact]
    public void Execute_WithNullIconUrl_ClearsIconUrl()
    {
        var allergen = CreateValidAllergen();
        allergen.IconUrl.Should().NotBeNull();
        var command = new UpdateAllergenCommand("Gluten", null, true, 0);

        var result = _update.Execute(allergen, command);

        result.IconUrl.Should().BeNull();
    }

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand(name!, null, true, 0);

        var act = () => _update.Execute(allergen, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand(new string('a', 101), null, true, 0);

        var act = () => _update.Execute(allergen, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithIconUrlExceedingMaxLength_ThrowsValidationException()
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand("Gluten", "https://example.com/" + new string('a', 500), true, 0);

        var act = () => _update.Execute(allergen, command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/icon.png")]
    public void Execute_WithInvalidIconUrl_ThrowsValidationException(string iconUrl)
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand("Gluten", iconUrl, true, 0);

        var act = () => _update.Execute(allergen, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var allergen = CreateValidAllergen();
        var command = new UpdateAllergenCommand("Gluten", null, true, -1);

        var act = () => _update.Execute(allergen, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
