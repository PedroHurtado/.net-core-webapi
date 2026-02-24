namespace Menus.Features.Menus.Api.MenuAggregate;

public class MenuAggregateDescription : IAggregateDescription
{
    public string Id => "menu";
    public string DisplayName => "Menús";
    public string? Icon => "book-open";
}
