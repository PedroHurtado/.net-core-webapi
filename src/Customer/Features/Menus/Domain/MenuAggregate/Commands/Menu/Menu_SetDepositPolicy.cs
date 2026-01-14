namespace Customer.Features.Menus.Domain.MenuAggregate;

public record SetDepositPolicyCommand(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage = null,
    decimal? MinimumBillForDeposit = null,
    int? MinimumGuestsForDeposit = null
);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class SetDepositPolicy(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<SetDepositPolicyCommand, Menu>
    {
        public override Menu Execute(Menu menu, SetDepositPolicyCommand command)
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
}
