using Fudie.Domain;
using Fudie.DependencyInjection;

namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

/// <summary>
/// Comando para configurar la política de fianzas de un menú.
/// </summary>
public record SetDepositPolicyCommand(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage = null,
    decimal? MinimumBillForDeposit = null,
    int? MinimumGuestsForDeposit = null
);

/// <summary>
/// Configura la política de fianzas de un menú existente.
/// </summary>
[Injectable]
public class SetDepositPolicy : IModifyCommand<SetDepositPolicyCommand, Menu>
{
    public Result<Menu> Execute(Menu menu, SetDepositPolicyCommand command)
    {
        var depositPolicyResult = DepositPolicy.Create(
            command.DepositType,
            command.Amount,
            command.Percentage,
            command.MinimumBillForDeposit,
            command.MinimumGuestsForDeposit
        );

        if (depositPolicyResult.IsFailure)
        {
            return Result<Menu>.Failure(depositPolicyResult.Errors);
        }

        menu.DepositPolicy = depositPolicyResult.Value;
        menu.UpdatedAt = DateTime.UtcNow;

        return Result<Menu>.Success(menu);
    }
}