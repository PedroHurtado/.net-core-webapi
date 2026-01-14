namespace Customer.Features.Menus.Domain.MenuAggregate;

public record UpdateMenuCommand(
    string Name,
    string? Description,
    DateTime? EffectiveFrom,
    DateTime? EffectiveUntil,
    int DisplayOrder
);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<UpdateMenuCommand, Menu>
    {
        public override Menu Execute(Menu menu, UpdateMenuCommand command)
        {
            menu.Name = command.Name;
            menu.Description = command.Description;
            menu.EffectiveFrom = command.EffectiveFrom;
            menu.EffectiveUntil = command.EffectiveUntil;
            menu.DisplayOrder = command.DisplayOrder;
            menu.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
