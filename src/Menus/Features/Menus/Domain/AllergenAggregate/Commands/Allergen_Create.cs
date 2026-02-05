namespace Menus.Features.Menus.Domain.AllergenAggregate;

/// <summary>
/// Command data for creating a new allergen.
/// </summary>
/// <param name="Code">The unique code that serves as the allergen's identifier. Required, maximum 20 characters, uppercase letters, numbers, and underscores only.</param>
/// <param name="Name">The display name of the allergen. Required, maximum 100 characters.</param>
/// <param name="IconUrl">Optional URL of the allergen's icon. Maximum 500 characters.</param>
/// <param name="IsActive">Whether the allergen is active. Defaults to true.</param>
/// <param name="DisplayOrder">The display order for sorting. Defaults to 0.</param>
public record CreateAllergenCommand(
    string Code,
    string Name,
    string? IconUrl = null,
    bool IsActive = true,
    int DisplayOrder = 0
);

public partial class Allergen
{
    /// <summary>
    /// Command handler for creating a new <see cref="Allergen"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a new allergen with the provided details. The allergen is created
    /// as active by default with a display order of 0.
    /// </para>
    /// <para>
    /// The command validates the allergen using <see cref="AllergenValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="allergenValidator">The validator for allergen instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Allergen> allergenValidator
    ) : AbstractCreateCommand<CreateAllergenCommand, Allergen>
    {
        /// <summary>
        /// Executes the create allergen command.
        /// </summary>
        /// <param name="command">The command containing the allergen creation data.</param>
        /// <returns>A new validated <see cref="Allergen"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the allergen data is invalid.</exception>
        public override Allergen Execute(CreateAllergenCommand command)
        {
            var allergen = new Allergen(command.Code)
            {
                Name = command.Name,
                IconUrl = command.IconUrl,
                IsActive = command.IsActive,
                DisplayOrder = command.DisplayOrder
            };

            return allergenValidator.ValidateOrThrow(allergen);
        }
    }
}