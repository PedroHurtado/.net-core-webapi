namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

[Injectable]
public class RemoveDepositPolicy(
    IValidator<Menu> menuValidator
) : IModifyCommand<Menu>
{
    

    Menu IModifyCommand<Menu>.Execute(Menu entity)
    {
        entity.DepositPolicy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        return menuValidator.ValidateOrThrow(entity);
    }
}
