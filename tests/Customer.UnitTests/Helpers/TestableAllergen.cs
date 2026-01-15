namespace Customer.UnitTests.Helpers;

public class TestableAllergen : Allergen
{
    public TestableAllergen() : base() { }
    public TestableAllergen(string code) : base(code) { }

    public new string Name { get => base.Name; set => base.Name = value; }
    public new string? IconUrl { get => base.IconUrl; set => base.IconUrl = value; }
    public new bool IsActive { get => base.IsActive; set => base.IsActive = value; }
    public new int DisplayOrder { get => base.DisplayOrder; set => base.DisplayOrder = value; }
}
