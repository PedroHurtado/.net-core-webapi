namespace Customer.Features.Menus.Domain.MenuAggregate;

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveDepositPolicy(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<Menu>
    {
        public override Menu Execute(Menu menu)
        {
            menu.DepositPolicy = null;
            menu.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
