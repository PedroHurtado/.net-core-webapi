using Fudie.Domain;
using Fudie.DependencyInjection;

namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

/// <summary>
/// Comando para crear un nuevo menú.
/// </summary>
public record CreateMenuCommand(
    Guid RestaurantId,
    string Name,
    string? Description = null,
    DateTime? EffectiveFrom = null,
    DateTime? EffectiveUntil = null
);

/// <summary>
/// Crea un nuevo menú para un restaurante.
/// </summary>
[Injectable]
public class CreateMenu : ICreateCommand<CreateMenuCommand, Menu>
{
    public Result<Menu> Execute(CreateMenuCommand command)
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

        return Entity.ValidateEntity(menu, new MenuValidator());
    }
}