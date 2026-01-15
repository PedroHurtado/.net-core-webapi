namespace Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

/// <summary>
/// Command data for creating nutritional information.
/// </summary>
/// <param name="Calories">The calorie count (0-10,000).</param>
/// <param name="Protein">The protein content in grams (0-1,000).</param>
/// <param name="Carbohydrates">The carbohydrate content in grams (0-1,000).</param>
/// <param name="Fat">The fat content in grams (0-1,000).</param>
/// <param name="ServingSize">The serving size in grams (1-10,000).</param>
/// <param name="Fiber">Optional fiber content in grams (0-1,000).</param>
/// <param name="Sugar">Optional sugar content in grams (0-1,000).</param>
/// <param name="Salt">Optional salt content in grams (0-100).</param>
public record CreateNutritionalInfoCommand(
    int Calories,
    decimal Protein,
    decimal Carbohydrates,
    decimal Fat,
    int ServingSize,
    decimal? Fiber = null,
    decimal? Sugar = null,
    decimal? Salt = null
);

public partial record NutritionalInfo
{
    /// <summary>
    /// Command handler for creating a new <see cref="NutritionalInfo"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates nutritional information for a menu item with all
    /// macronutrient and optional micronutrient values.
    /// </para>
    /// <para>
    /// The command validates the nutritional info using <see cref="NutritionalInfoValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="nutritionalInfoValidator">The validator for nutritional info instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<NutritionalInfo> nutritionalInfoValidator
    ) : AbstractCreateCommand<CreateNutritionalInfoCommand, NutritionalInfo>
    {
        /// <summary>
        /// Executes the create nutritional info command.
        /// </summary>
        /// <param name="command">The command containing the nutritional info creation data.</param>
        /// <returns>A new validated <see cref="NutritionalInfo"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the nutritional info data is invalid.</exception>
        public override NutritionalInfo Execute(CreateNutritionalInfoCommand command)
        {
            var info = new NutritionalInfo(
                command.Calories,
                command.Protein,
                command.Carbohydrates,
                command.Fat,
                command.ServingSize,
                command.Fiber,
                command.Sugar,
                command.Salt);

            return nutritionalInfoValidator.ValidateOrThrow(info);
        }
    }
}
