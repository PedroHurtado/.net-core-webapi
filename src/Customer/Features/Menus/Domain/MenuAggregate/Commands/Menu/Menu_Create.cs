namespace Customer.Features.Menus.Domain.MenuAggregate;

public record CreateMenuCommand(
    Guid RestaurantId,
    string Name,
    string? Description = null,
    DateTime? EffectiveFrom = null,
    DateTime? EffectiveUntil = null
);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Menu> menuValidator
    ) : AbstractCreateCommand<CreateMenuCommand, Menu>
    {
        public override Menu Execute(CreateMenuCommand command)
        {
            var menu = new Menu(Guid.NewGuid())
            {
                RestaurantId = command.RestaurantId,
                Name = command.Name,
                Description = command.Description,
                EffectiveFrom = command.EffectiveFrom,
                EffectiveUntil = command.EffectiveUntil,
                DisplayOrder = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
