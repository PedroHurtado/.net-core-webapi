namespace Customer.Features.Menus.Domain.MenuItemAggregate;

/// <summary>
/// Command data for setting nutritional information on a menu item.
/// </summary>
/// <param name="Calories">The calorie count (0-10,000).</param>
/// <param name="Protein">The protein content in grams (0-1,000).</param>
/// <param name="Carbohydrates">The carbohydrate content in grams (0-1,000).</param>
/// <param name="Fat">The fat content in grams (0-1,000).</param>
/// <param name="ServingSize">The serving size in grams (1-10,000).</param>
/// <param name="Fiber">Optional fiber content in grams (0-1,000).</param>
/// <param name="Sugar">Optional sugar content in grams (0-1,000).</param>
/// <param name="Salt">Optional salt content in grams (0-100).</param>
public record SetNutritionalInfoCommand(
    int Calories,
    decimal Protein,
    decimal Carbohydrates,
    decimal Fat,
    int ServingSize,
    decimal? Fiber = null,
    decimal? Sugar = null,
    decimal? Salt = null
);

public partial class MenuItem
{
    /// <summary>
    /// Command handler for setting nutritional information on an existing <see cref="MenuItem"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command sets or replaces the nutritional information for a menu item.
    /// </para>
    /// <para>
    /// If the item already has nutritional information, it will be replaced with the new values.
    /// </para>
    /// </remarks>
    /// <param name="nutritionalInfoCreate">The command for creating nutritional info value objects.</param>
    /// <param name="menuItemValidator">The validator for menu item instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class SetNutritionalInfo(
        NutritionalInfo.Create nutritionalInfoCreate,
        IValidator<MenuItem> menuItemValidator
    ) : AbstractModifyCommand<SetNutritionalInfoCommand, MenuItem>
    {
        /// <summary>
        /// Executes the set nutritional info command.
        /// </summary>
        /// <param name="menuItem">The menu item instance to set nutritional info on.</param>
        /// <param name="command">The set nutritional info command containing the nutritional data.</param>
        /// <returns>The updated and validated <see cref="MenuItem"/> instance.</returns>
        public override MenuItem Execute(MenuItem menuItem, SetNutritionalInfoCommand command)
        {
            var nutritionalInfo = nutritionalInfoCreate.Execute(
                new CreateNutritionalInfoCommand(
                    command.Calories,
                    command.Protein,
                    command.Carbohydrates,
                    command.Fat,
                    command.ServingSize,
                    command.Fiber,
                    command.Sugar,
                    command.Salt
                )
            );

            menuItem.NutritionalInfo = nutritionalInfo;

            return menuItemValidator.ValidateOrThrow(menuItem);
        }
    }
}
