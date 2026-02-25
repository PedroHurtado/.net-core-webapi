namespace Menus.Features.Menus.Api.MenuItemAggregate;

public class MenuItemAggregateDescription : IAggregateDescription
{
    public string Id => "menu-item";
    public string DisplayName => "Platos";
    public string? Icon => "utensils";
    public string ReadDescription => "View menu items";
    public string WriteDescription => "Manage menu items";
}
