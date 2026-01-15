namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate;

public class MenuItemValidatorTests
{
    private readonly MenuItemValidator _validator = new();
    private readonly PriceOptionVO.Create _createPriceOption = new(new PriceOptionValidator());

    private TestableMenuItem CreateValidMenuItem()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Paella Valenciana",
            IsActive = true
        };
        menuItem.AddPriceOption(_createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 15.99m)));
        return menuItem;
    }

    [Fact]
    public void Validate_WithValidMenuItem_ReturnsSuccess()
    {
        var menuItem = CreateValidMenuItem();

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithParameterlessConstructor_ReturnsErrors()
    {
        var menuItem = new TestableMenuItem();

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.IdRequired);
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.TenantIdRequired);
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.NameRequired);
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.PriceOptionsRequired);
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var menuItem = new TestableMenuItem(Guid.Empty)
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Name"
        };
        menuItem.AddPriceOption(_createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 10m)));

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.IdRequired);
    }

    #endregion

    #region TenantId Validation

    [Fact]
    public void TenantId_WhenEmpty_ReturnsError()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.Empty,
            Name = "Valid Name"
        };
        menuItem.AddPriceOption(_createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 10m)));

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.TenantIdRequired);
    }

    #endregion

    #region Name Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Name_WhenEmpty_ReturnsError(string? name)
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = name!
        };
        menuItem.AddPriceOption(_createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 10m)));

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.Name = new string('a', 151);

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.NameMaxLength);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(150)]
    public void Name_WhenWithinMaxLength_ReturnsSuccess(int length)
    {
        var menuItem = CreateValidMenuItem();
        menuItem.Name = new string('a', length);

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.NameMaxLength);
    }

    #endregion

    #region Description Validation

    [Fact]
    public void Description_WhenExceedsMaxLength_ReturnsError()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.Description = new string('a', 1001);

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.DescriptionMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Short description")]
    public void Description_WhenNullOrWithinMaxLength_ReturnsSuccess(string? description)
    {
        var menuItem = CreateValidMenuItem();
        menuItem.Description = description;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.DescriptionMaxLength);
    }

    #endregion

    #region ImageUrl Validation

    [Fact]
    public void ImageUrl_WhenExceedsMaxLength_ReturnsError()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.ImageUrl = "https://example.com/" + new string('a', 500);

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.ImageUrlMaxLength);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/image.png")]
    [InlineData("//example.com/image.png")]
    public void ImageUrl_WhenInvalidUrl_ReturnsError(string imageUrl)
    {
        var menuItem = CreateValidMenuItem();
        menuItem.ImageUrl = imageUrl;

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.ImageUrlInvalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://example.com/image.png")]
    [InlineData("http://example.com/image.png")]
    public void ImageUrl_WhenValidOrEmpty_ReturnsSuccess(string? imageUrl)
    {
        var menuItem = CreateValidMenuItem();
        menuItem.ImageUrl = imageUrl;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.ImageUrlInvalid);
    }

    #endregion

    #region DisplayOrder Validation

    [Fact]
    public void DisplayOrder_WhenNegative_ReturnsError()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.DisplayOrder = -1;

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.DisplayOrderMin);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void DisplayOrder_WhenZeroOrPositive_ReturnsSuccess(int displayOrder)
    {
        var menuItem = CreateValidMenuItem();
        menuItem.DisplayOrder = displayOrder;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.DisplayOrderMin);
    }

    #endregion

    #region RequiresAdvanceOrder and IsHighRiskItem Validation

    [Fact]
    public void RequiresAdvanceOrder_WhenTrueAndIsHighRiskItemFalse_ReturnsError()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.RequiresAdvanceOrder = true;
        menuItem.IsHighRiskItem = false;

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.RequiresAdvanceOrderMustBeHighRisk);
    }

    [Fact]
    public void RequiresAdvanceOrder_WhenTrueAndIsHighRiskItemTrue_ReturnsSuccess()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.RequiresAdvanceOrder = true;
        menuItem.IsHighRiskItem = true;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.RequiresAdvanceOrderMustBeHighRisk);
    }

    [Fact]
    public void RequiresAdvanceOrder_WhenFalse_ReturnsSuccessRegardlessOfHighRisk()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.RequiresAdvanceOrder = false;
        menuItem.IsHighRiskItem = false;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.RequiresAdvanceOrderMustBeHighRisk);
    }

    #endregion

    #region MinimumAdvanceOrderQuantity Validation

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void MinimumAdvanceOrderQuantity_WhenOutsideRange_ReturnsError(int quantity)
    {
        var menuItem = CreateValidMenuItem();
        menuItem.RequiresAdvanceOrder = true;
        menuItem.IsHighRiskItem = true;
        menuItem.MinimumAdvanceOrderQuantity = quantity;

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.MinimumQuantityRange);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void MinimumAdvanceOrderQuantity_WhenWithinRange_ReturnsSuccess(int quantity)
    {
        var menuItem = CreateValidMenuItem();
        menuItem.RequiresAdvanceOrder = true;
        menuItem.IsHighRiskItem = true;
        menuItem.MinimumAdvanceOrderQuantity = quantity;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.MinimumQuantityRange);
    }

    [Fact]
    public void MinimumAdvanceOrderQuantity_WhenSetButRequiresAdvanceOrderFalse_ReturnsError()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.RequiresAdvanceOrder = false;
        menuItem.MinimumAdvanceOrderQuantity = 10;

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.MinimumAdvanceOrderQuantityRequiresAdvanceOrder);
    }

    [Fact]
    public void MinimumAdvanceOrderQuantity_WhenNullAndRequiresAdvanceOrderFalse_ReturnsSuccess()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.RequiresAdvanceOrder = false;
        menuItem.MinimumAdvanceOrderQuantity = null;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.MinimumAdvanceOrderQuantityRequiresAdvanceOrder);
    }

    #endregion

    #region AvailableDays Validation

    [Fact]
    public void AvailableDays_WhenNotAlwaysAvailableAndEmpty_ReturnsError()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.IsAlwaysAvailable = false;

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.AvailableDaysRequired);
    }

    [Fact]
    public void AvailableDays_WhenNotAlwaysAvailableAndHasDays_ReturnsSuccess()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.IsAlwaysAvailable = false;
        menuItem.AddAvailableDay(DayOfWeek.Monday);

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.AvailableDaysRequired);
    }

    [Fact]
    public void AvailableDays_WhenAlwaysAvailableAndEmpty_ReturnsSuccess()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.IsAlwaysAvailable = true;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.AvailableDaysRequired);
    }

    #endregion

    #region PriceOptions Validation

    [Fact]
    public void PriceOptions_WhenEmpty_ReturnsError()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Name"
        };

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.PriceOptionsRequired);
    }

    [Fact]
    public void PriceOptions_WhenNotEmpty_ReturnsSuccess()
    {
        var menuItem = CreateValidMenuItem();

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.PriceOptionsRequired);
    }

    #endregion

    #region AllergenNotes Validation

    [Fact]
    public void AllergenNotes_WhenExceedsMaxLength_ReturnsError()
    {
        var menuItem = CreateValidMenuItem();
        menuItem.AllergenNotes = new string('a', 501);

        var result = _validator.Validate(menuItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuItemValidationMessages.AllergenNotesMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("May contain traces of nuts")]
    public void AllergenNotes_WhenNullOrWithinMaxLength_ReturnsSuccess(string? notes)
    {
        var menuItem = CreateValidMenuItem();
        menuItem.AllergenNotes = notes;

        var result = _validator.Validate(menuItem);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuItemValidationMessages.AllergenNotesMaxLength);
    }

    #endregion
}
