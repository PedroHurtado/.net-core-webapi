namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

/// <summary>
/// Información nutricional de un item (valores por ración completa).
/// </summary>
public record NutritionalInfo
{
    public int Calories { get; }
    public decimal Protein { get; }
    public decimal Carbohydrates { get; }
    public decimal Fat { get; }
    public int ServingSize { get; }
    public decimal? Fiber { get; }
    public decimal? Sugar { get; }
    public decimal? Salt { get; }

    private NutritionalInfo(
        int calories,
        decimal protein,
        decimal carbohydrates,
        decimal fat,
        int servingSize,
        decimal? fiber,
        decimal? sugar,
        decimal? salt)
    {
        Calories = calories;
        Protein = protein;
        Carbohydrates = carbohydrates;
        Fat = fat;
        ServingSize = servingSize;
        Fiber = fiber;
        Sugar = sugar;
        Salt = salt;
    }

    public static NutritionalInfo Create(
        int calories,
        decimal protein,
        decimal carbohydrates,
        decimal fat,
        int servingSize,
        decimal? fiber = null,
        decimal? sugar = null,
        decimal? salt = null)
    {
        var info = new NutritionalInfo(
            calories,
            protein,
            carbohydrates,
            fat,
            servingSize,
            fiber,
            sugar,
            salt);

        return new NutritionalInfoValidator().ValidateOrThrow(info);
    }

    /// <summary>
    /// Calcula los valores nutricionales para una porción específica.
    /// </summary>
    /// <param name="portionPercentage">Porcentaje de la ración (ej: 0.25 para tapa).</param>
    public NutritionalInfo GetNutritionForPortion(decimal portionPercentage)
    {
        // Bypass validation - valores derivados de instancia válida
        return new NutritionalInfo(
            calories: (int)(Calories * portionPercentage),
            protein: Protein * portionPercentage,
            carbohydrates: Carbohydrates * portionPercentage,
            fat: Fat * portionPercentage,
            servingSize: (int)(ServingSize * portionPercentage),
            fiber: Fiber.HasValue ? Fiber.Value * portionPercentage : null,
            sugar: Sugar.HasValue ? Sugar.Value * portionPercentage : null,
            salt: Salt.HasValue ? Salt.Value * portionPercentage : null);
    }
}

/// <summary>
/// Validador de invariantes para NutritionalInfo.
/// </summary>
public class NutritionalInfoValidator : AbstractValidator<NutritionalInfo>
{
    public NutritionalInfoValidator()
    {
        RuleFor(x => x.Calories)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Las calorías deben ser >= 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Las calorías no pueden exceder 10000 kcal");

        RuleFor(x => x.Protein)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Las proteínas deben ser >= 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Las proteínas no pueden exceder 1000g");

        RuleFor(x => x.Carbohydrates)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los carbohidratos deben ser >= 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Los carbohidratos no pueden exceder 1000g");

        RuleFor(x => x.Fat)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Las grasas deben ser >= 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Las grasas no pueden exceder 1000g");

        RuleFor(x => x.ServingSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de porción debe ser > 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("El tamaño de porción no puede exceder 10000g");

        RuleFor(x => x.Fiber)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Fiber.HasValue)
            .WithMessage("La fibra debe ser >= 0")
            .LessThanOrEqualTo(1000)
            .When(x => x.Fiber.HasValue)
            .WithMessage("La fibra no puede exceder 1000g");

        RuleFor(x => x.Sugar)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Sugar.HasValue)
            .WithMessage("El azúcar debe ser >= 0")
            .LessThanOrEqualTo(1000)
            .When(x => x.Sugar.HasValue)
            .WithMessage("El azúcar no puede exceder 1000g");

        RuleFor(x => x.Salt)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Salt.HasValue)
            .WithMessage("La sal debe ser >= 0")
            .LessThanOrEqualTo(100)
            .When(x => x.Salt.HasValue)
            .WithMessage("La sal no puede exceder 100g");
    }
}