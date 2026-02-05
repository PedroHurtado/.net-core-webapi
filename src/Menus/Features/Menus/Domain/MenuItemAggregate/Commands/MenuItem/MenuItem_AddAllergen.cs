namespace Menus.Features.Menus.Domain.MenuItemAggregate;

public record AddAllergenCommand(
    Allergen Allergen
);

public partial class MenuItem
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddAllergen(
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<AddAllergenCommand, MenuItem>
    {
        public override MenuItem Execute(MenuItem menuItem, AddAllergenCommand command)
        {
            ConflictGuard.ThrowIf(
                menuItem.Allergens.Any(a => a.Id == command.Allergen.Id),
                "Allergen already exists in this item");

            menuItem._allergens.Add(command.Allergen);

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}