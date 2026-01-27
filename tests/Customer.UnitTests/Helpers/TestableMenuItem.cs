namespace Customer.UnitTests.Helpers;

public class TestableMenuItem : MenuItem
{
    public TestableMenuItem() : base() { }
    public TestableMenuItem(Guid id) : base(id) { }

    public new Guid TenantId { get => base.TenantId; set => base.TenantId = value; }
    public new string Name { get => base.Name; set => base.Name = value; }
    public new string? Description { get => base.Description; set => base.Description = value; }
    public new string? ImageUrl { get => base.ImageUrl; set => base.ImageUrl = value; }
    public new int DisplayOrder { get => base.DisplayOrder; set => base.DisplayOrder = value; }
    public new bool IsActive { get => base.IsActive; set => base.IsActive = value; }
    public new bool IsHighRiskItem { get => base.IsHighRiskItem; set => base.IsHighRiskItem = value; }
    public new bool RequiresAdvanceOrder { get => base.RequiresAdvanceOrder; set => base.RequiresAdvanceOrder = value; }
    public new int? MinimumAdvanceOrderQuantity { get => base.MinimumAdvanceOrderQuantity; set => base.MinimumAdvanceOrderQuantity = value; }
    public new bool IsAvailable { get => base.IsAvailable; set => base.IsAvailable = value; }
    public new bool IsAlwaysAvailable { get => base.IsAlwaysAvailable; set => base.IsAlwaysAvailable = value; }
    public new ItemDepositOverrideVO? DepositOverride { get => base.DepositOverride; set => base.DepositOverride = value; }
    public new NutritionalInfoVO? NutritionalInfo { get => base.NutritionalInfo; set => base.NutritionalInfo = value; }
    public new string? AllergenNotes { get => base.AllergenNotes; set => base.AllergenNotes = value; }

    public void AddPriceOptionDirect(PriceOptionVO priceOption) => _priceOptions.Add(priceOption);
    public void AddAllergen(Allergen allergen) => _allergens.Add(allergen);
    public void AddAvailableDay(DayOfWeek day) => _availableDays.Add(day);
}