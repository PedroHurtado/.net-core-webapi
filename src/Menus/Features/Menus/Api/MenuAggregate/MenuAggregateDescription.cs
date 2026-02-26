namespace Menus.Features.Menus.Api.MenuAggregate;

public class MenuAggregateDescription : IAggregateDescription
{
    public string Id => "menu";
    public string DisplayName => "Menus";
    public string? Icon => "book-open";
    public string ReadDescription => "View menus";
    public string WriteDescription => "Manage menus";
}
