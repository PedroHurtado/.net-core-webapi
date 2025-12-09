using Fudie.Domain;
using Fudie.DependencyInjection;

namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

/// <summary>
/// Comando para actualizar un menú existente.
/// </summary>
public record UpdateMenuCommand(
    string Name,
    string? Description,
    DateTime? EffectiveFrom,
    DateTime? EffectiveUntil,
    int DisplayOrder
);

/// <summary>
/// Actualiza un menú existente.
/// </summary>
[Injectable]
public class UpdateMenu : IModifyCommand<UpdateMenuCommand, Menu>
{
    public Result<Menu> Execute(Menu menu, UpdateMenuCommand command)
    {
        menu.Name = command.Name;
        menu.Description = command.Description;
        menu.EffectiveFrom = command.EffectiveFrom;
        menu.EffectiveUntil = command.EffectiveUntil;
        menu.DisplayOrder = command.DisplayOrder;
        menu.UpdatedAt = DateTime.UtcNow;

        return Entity.ValidateEntity(menu, new MenuValidator());
    }
}