namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

public record CreateMenuCommand(
    Guid RestaurantId,
    string Name,
    string? Description = null,
    DateTime? EffectiveFrom = null,
    DateTime? EffectiveUntil = null
);

[Injectable]
public class CreateMenu(
    IValidator<Menu> menuValidator
) : ICreateCommand<CreateMenuCommand, Menu>
{
    public Menu Execute(CreateMenuCommand command)
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
