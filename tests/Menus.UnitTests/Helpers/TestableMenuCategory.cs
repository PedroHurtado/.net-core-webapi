namespace Menus.UnitTests.Helpers;

public class TestableMenuCategory : MenuCategory
{
    public TestableMenuCategory() : base() { }
    public TestableMenuCategory(Guid id) : base(id) { }

    public new string Name { get => base.Name; set => base.Name = value; }
    public new string? Description { get => base.Description; set => base.Description = value; }
    public new int DisplayOrder { get => base.DisplayOrder; set => base.DisplayOrder = value; }
    public new bool IsActive { get => base.IsActive; set => base.IsActive = value; }

    public void AddItemDirect(CategoryItem item) => _items.Add(item);
}
