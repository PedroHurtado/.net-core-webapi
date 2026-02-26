namespace Menus.Features.Menus.Api.AllergenAggregate;

public class AllergenAggregateDescription : IAggregateDescription
{
    public string Id => "allergen";
    public string DisplayName => "Allergens";
    public string? Icon => "alert-triangle";
    public string ReadDescription => "View allergens";
    public string WriteDescription => "Manage allergens";
}
