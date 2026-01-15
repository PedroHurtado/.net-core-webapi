namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate;

public class MenuValidatorTests
{
    private readonly MenuValidator _validator = new();

    [Fact]
    public void Validate_WithValidMenu_ReturnsSuccess()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            Description = "Daily lunch specials",
            DisplayOrder = 1,
            IsActive = true
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithParameterlessConstructor_ReturnsErrors()
    {
        var menu = new TestableMenu();

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.IdRequired);
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.TenantIdRequired);
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.NameRequired);
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var menu = new TestableMenu(Guid.Empty)
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Name"
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.IdRequired);
    }

    #endregion

    #region TenantId Validation

    [Fact]
    public void TenantId_WhenEmpty_ReturnsError()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.Empty,
            Name = "Valid Name"
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.TenantIdRequired);
    }

    #endregion

    #region Name Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Name_WhenEmpty_ReturnsError(string? name)
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = name!
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = new string('a', 101)
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.NameMaxLength);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Lunch Menu")]
    public void Name_WhenValidLength_ReturnsSuccess(string name)
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = name
        };

        var result = _validator.Validate(menu);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuValidationMessages.NameMaxLength);
    }

    #endregion

    #region Description Validation

    [Fact]
    public void Description_WhenExceedsMaxLength_ReturnsError()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            Description = new string('a', 501)
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.DescriptionMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Short description")]
    public void Description_WhenValidOrEmpty_ReturnsSuccess(string? description)
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            Description = description
        };

        var result = _validator.Validate(menu);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuValidationMessages.DescriptionMaxLength);
    }

    #endregion

    #region DisplayOrder Validation

    [Fact]
    public void DisplayOrder_WhenNegative_ReturnsError()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            DisplayOrder = -1
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.DisplayOrderMin);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void DisplayOrder_WhenZeroOrPositive_ReturnsSuccess(int displayOrder)
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            DisplayOrder = displayOrder
        };

        var result = _validator.Validate(menu);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuValidationMessages.DisplayOrderMin);
    }

    #endregion

    #region EffectiveDates Validation

    [Fact]
    public void EffectiveDates_WhenStartDateAfterEndDate_ReturnsError()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            EffectiveFrom = new DateTime(2024, 12, 31),
            EffectiveUntil = new DateTime(2024, 1, 1)
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.StartDateMustBeEarlierThanEndDate);
    }

    [Fact]
    public void EffectiveDates_WhenStartDateEqualsEndDate_ReturnsError()
    {
        var sameDate = new DateTime(2024, 6, 15);
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            EffectiveFrom = sameDate,
            EffectiveUntil = sameDate
        };

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuValidationMessages.StartDateMustBeEarlierThanEndDate);
    }

    [Fact]
    public void EffectiveDates_WhenStartDateBeforeEndDate_ReturnsSuccess()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            EffectiveFrom = new DateTime(2024, 1, 1),
            EffectiveUntil = new DateTime(2024, 12, 31)
        };

        var result = _validator.Validate(menu);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuValidationMessages.StartDateMustBeEarlierThanEndDate);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void EffectiveDates_WhenOnlyOneOrNoneSet_ReturnsSuccess(bool hasFrom, bool hasUntil)
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Lunch Menu",
            EffectiveFrom = hasFrom ? new DateTime(2024, 1, 1) : null,
            EffectiveUntil = hasUntil ? new DateTime(2024, 12, 31) : null
        };

        var result = _validator.Validate(menu);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuValidationMessages.StartDateMustBeEarlierThanEndDate);
    }

    #endregion
}
