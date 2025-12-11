namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

[Injectable(ServiceLifetime.Singleton)]
public class RemoveDepositPolicy(
    IValidator<Menu> menuValidator
) : AbstractModifyCommand<Menu>
{
    public override Menu Execute(Menu entity)
    {
        entity.DepositPolicy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        return menuValidator.ValidateOrThrow(entity);
    }
}
