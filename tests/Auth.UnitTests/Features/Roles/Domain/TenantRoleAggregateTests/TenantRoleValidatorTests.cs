namespace Auth.UnitTests.Features.Roles.Domain.TenantRoleAggregateTests;

public class TenantRoleValidatorTests
{
    private readonly TenantRoleValidator _validator = new();

    [Fact]
    public void Validate_WithValidCustomRole_ReturnsSuccess()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Sommelier")
            .WithDescription("Carta de vinos")
            .WithIsSystem(false)
            .WithIsEditable(true)
            .WithIsDeletable(true);

        var result = _validator.Validate(role);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidSystemRole_ReturnsSuccess()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Owner")
            .WithDescription("Propietario. Acceso completo.")
            .WithIsSystem(true)
            .WithIsEditable(false)
            .WithIsDeletable(false);

        var result = _validator.Validate(role);

        result.IsValid.Should().BeTrue();
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var role = new TestableTenantRole(Guid.Empty)
            .WithTenantId(Guid.NewGuid())
            .WithName("Sommelier")
            .WithDescription("Carta de vinos");

        var result = _validator.Validate(role);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == TenantRoleValidationMessages.IdRequired);
    }

    #endregion

    #region TenantId Validation

    [Fact]
    public void TenantId_WhenEmpty_ReturnsError()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.Empty)
            .WithName("Sommelier")
            .WithDescription("Carta de vinos");

        var result = _validator.Validate(role);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == TenantRoleValidationMessages.TenantIdRequired);
    }

    #endregion

    #region Name Validation

    [Fact]
    public void Name_WhenEmpty_ReturnsError()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("")
            .WithDescription("Carta de vinos");

        var result = _validator.Validate(role);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == TenantRoleValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName(new string('a', 101))
            .WithDescription("Carta de vinos");

        var result = _validator.Validate(role);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == TenantRoleValidationMessages.NameMaxLength);
    }

    #endregion

    #region Description Validation

    [Fact]
    public void Description_WhenExceedsMaxLength_ReturnsError()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Sommelier")
            .WithDescription(new string('a', 501));

        var result = _validator.Validate(role);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == TenantRoleValidationMessages.DescriptionMaxLength);
    }

    [Fact]
    public void Description_WhenEmpty_ReturnsSuccess()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Sommelier")
            .WithDescription("");

        var result = _validator.Validate(role);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
