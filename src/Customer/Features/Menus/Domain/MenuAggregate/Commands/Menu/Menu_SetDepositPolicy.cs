namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Command data for setting or updating a menu's deposit policy.
/// </summary>
/// <param name="DepositType">The type of deposit calculation to use.</param>
/// <param name="Amount">The base amount for deposit calculations. Must be greater than zero.</param>
/// <param name="Percentage">The percentage (1-100) for percentage-based deposits. Required only for <see cref="Enums.DepositType.PercentageOfBill"/>.</param>
/// <param name="MinimumBillForDeposit">Optional minimum bill amount threshold to trigger the deposit.</param>
/// <param name="MinimumGuestsForDeposit">Optional minimum guest count threshold to trigger the deposit.</param>
public record SetDepositPolicyCommand(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage = null,
    decimal? MinimumBillForDeposit = null,
    int? MinimumGuestsForDeposit = null
);

public partial class Menu
{
    /// <summary>
    /// Command handler for setting or updating the <see cref="DepositPolicy"/> on a menu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates and assigns a deposit policy to the menu. The policy defines
    /// how deposits are calculated for reservations using this menu.
    /// </para>
    /// <para>
    /// The deposit policy is validated through <see cref="DepositPolicy.Create"/> command,
    /// and the menu is validated after modification.
    /// </para>
    /// </remarks>
    /// <param name="menuValidator">The validator for menu instances.</param>
    /// <param name="createDepositPolicy">The command handler for creating deposit policies.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class SetDepositPolicy(
        IValidator<Menu> menuValidator,
        DepositPolicy.Create createDepositPolicy
    ) : AbstractModifyCommand<SetDepositPolicyCommand, Menu>
    {
        /// <summary>
        /// Executes the set deposit policy command.
        /// </summary>
        /// <param name="menu">The menu to set the deposit policy on.</param>
        /// <param name="command">The command containing the deposit policy configuration.</param>
        /// <returns>The updated and validated <see cref="Menu"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the deposit policy or menu data is invalid.</exception>
        public override Menu Execute(Menu menu, SetDepositPolicyCommand command)
        {
            var depositPolicy = createDepositPolicy.Execute(new CreateDepositPolicyCommand(
                command.DepositType,
                command.Amount,
                command.Percentage,
                command.MinimumBillForDeposit,
                command.MinimumGuestsForDeposit
            ));

            menu.DepositPolicy = depositPolicy;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
