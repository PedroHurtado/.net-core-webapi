namespace Menus.Features.Menus.Api.AllergenAggregate;

public class AllergenAggregateDescription : IAggregateDescription
{
    public string Id => "allergen";
    public string DisplayName => "Alérgenos";
    public string? Icon => "alert-triangle";
    public string ReadDescription => "View allergens";
    public string WriteDescription => "Manage allergens";
}
