namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;
using Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

public record SetDepositPolicyCommand(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage = null,
    decimal? MinimumBillForDeposit = null,
    int? MinimumGuestsForDeposit = null
);

[Injectable]
public class SetDepositPolicy(
    IValidator<Menu> menuValidator
) : IModifyCommand<SetDepositPolicyCommand, Menu>
{
    public Menu Execute(Menu menu, SetDepositPolicyCommand command)
    {
        var depositPolicy = DepositPolicy.Create(
            command.DepositType,
            command.Amount,
            command.Percentage,
            command.MinimumBillForDeposit,
            command.MinimumGuestsForDeposit
        );

        menu.DepositPolicy = depositPolicy;
        menu.UpdatedAt = DateTime.UtcNow;

        return menuValidator.ValidateOrThrow(menu);
    }
}
