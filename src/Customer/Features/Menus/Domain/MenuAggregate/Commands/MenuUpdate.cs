namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

public record UpdateMenuCommand(
    string Name,
    string? Description,
    DateTime? EffectiveFrom,
    DateTime? EffectiveUntil,
    int DisplayOrder
);

[Injectable]
public class UpdateMenu(
    IValidator<Menu> menuValidator
) : IModifyCommand<UpdateMenuCommand, Menu>
{
    public Menu Execute(Menu menu, UpdateMenuCommand command)
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
