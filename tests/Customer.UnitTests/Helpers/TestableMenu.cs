namespace Customer.UnitTests.Helpers;

public class TestableMenu : Menu
{
    public TestableMenu() : base() { }
    public TestableMenu(Guid id) : base(id) { }

    public new Guid TenantId { get => base.TenantId; set => base.TenantId = value; }
    public new string Name { get => base.Name; set => base.Name = value; }
    public new string? Description { get => base.Description; set => base.Description = value; }
    public new bool IsActive { get => base.IsActive; set => base.IsActive = value; }
    public new int DisplayOrder { get => base.DisplayOrder; set => base.DisplayOrder = value; }
    public new DateTime? EffectiveFrom { get => base.EffectiveFrom; set => base.EffectiveFrom = value; }
    public new DateTime? EffectiveUntil { get => base.EffectiveUntil; set => base.EffectiveUntil = value; }
    public new DepositPolicy? DepositPolicy { get => base.DepositPolicy; set => base.DepositPolicy = value; }

    public void AddCategoryDirect(MenuCategory category) => _categories.Add(category);
}
