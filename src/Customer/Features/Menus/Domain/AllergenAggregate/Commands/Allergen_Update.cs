namespace Customer.Features.Menus.Domain.AllergenAggregate;

/// <summary>
/// Command data for updating an existing allergen.
/// </summary>
/// <param name="Name">The updated display name of the allergen. Required, maximum 100 characters.</param>
/// <param name="IconUrl">The updated URL of the allergen's icon. Maximum 500 characters.</param>
/// <param name="IsActive">Whether the allergen is active.</param>
/// <param name="DisplayOrder">The updated display order for sorting.</param>
public record UpdateAllergenCommand(
    string Name,
    string? IconUrl,
    bool IsActive,
    int DisplayOrder
);

public partial class Allergen
{
    /// <summary>
    /// Command handler for updating an existing <see cref="Allergen"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command updates the allergen's properties including name, icon URL,
    /// active status, and display order.
    /// </para>
    /// <para>
    /// The command validates the allergen using <see cref="AllergenValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="allergenValidator">The validator for allergen instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<Allergen> allergenValidator
    ) : AbstractModifyCommand<UpdateAllergenCommand, Allergen>
    {
        /// <summary>
        /// Executes the update allergen command.
        /// </summary>
        /// <param name="allergen">The allergen instance to update.</param>
        /// <param name="command">The command containing the updated allergen data.</param>
        /// <returns>The updated and validated <see cref="Allergen"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the updated data is invalid.</exception>
        public override Allergen Execute(Allergen allergen, UpdateAllergenCommand command)
        {
            allergen.Name = command.Name;
            allergen.IconUrl = command.IconUrl;
            allergen.IsActive = command.IsActive;
            allergen.DisplayOrder = command.DisplayOrder;

            return allergenValidator.ValidateOrThrow(allergen);
        }
    }
}
