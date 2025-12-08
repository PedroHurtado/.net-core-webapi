// src/Customer/Features/Menus/Domain/MenuAggregate/ValueObjects/NutritionalInfo.cs
namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

/// <summary>
/// Información nutricional de un item (valores por ración completa).
/// </summary>
public record NutritionalInfo(
    int Calories,
    decimal Protein,
    decimal Carbohydrates,
    decimal Fat,
    int ServingSize,
    decimal? Fiber = null,
    decimal? Sugar = null,
    decimal? Salt = null
)
{
    /// <summary>
    /// Calcula los valores nutricionales para una porción específica.
    /// </summary>
    /// <param name="portionPercentage">Porcentaje de la ración (ej: 0.25 para tapa).</param>
    public NutritionalInfo GetNutritionForPortion(decimal portionPercentage)
    {
        return this with
        {
            Calories = (int)(Calories * portionPercentage),
            Protein = Protein * portionPercentage,
            Carbohydrates = Carbohydrates * portionPercentage,
            Fat = Fat * portionPercentage,
            Fiber = Fiber.HasValue ? Fiber.Value * portionPercentage : null,
            Sugar = Sugar.HasValue ? Sugar.Value * portionPercentage : null,
            Salt = Salt.HasValue ? Salt.Value * portionPercentage : null
        };
    }
}