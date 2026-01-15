namespace Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

/// <summary>
/// Represents nutritional information for a menu item per full serving.
/// </summary>
/// <remarks>
/// <para>
/// This value object contains standard nutritional values including macronutrients
/// (protein, carbohydrates, fat) and optional micronutrients (fiber, sugar, salt).
/// </para>
/// <para>
/// All values are based on the full serving size. Use <see cref="GetNutritionForPortion"/>
/// to calculate nutritional values for smaller portion sizes.
/// </para>
/// </remarks>
public partial record NutritionalInfo
{
    /// <summary>
    /// Gets the calorie count per full serving.
    /// </summary>
    /// <value>Calories in kcal. Valid range: 0-10,000.</value>
    public int Calories { get; }

    /// <summary>
    /// Gets the protein content per full serving.
    /// </summary>
    /// <value>Protein in grams. Valid range: 0-1,000.</value>
    public decimal Protein { get; }

    /// <summary>
    /// Gets the carbohydrate content per full serving.
    /// </summary>
    /// <value>Carbohydrates in grams. Valid range: 0-1,000.</value>
    public decimal Carbohydrates { get; }

    /// <summary>
    /// Gets the fat content per full serving.
    /// </summary>
    /// <value>Fat in grams. Valid range: 0-1,000.</value>
    public decimal Fat { get; }

    /// <summary>
    /// Gets the serving size in grams.
    /// </summary>
    /// <value>Serving size in grams. Valid range: 1-10,000.</value>
    public int ServingSize { get; }

    /// <summary>
    /// Gets the optional fiber content per full serving.
    /// </summary>
    /// <value>Fiber in grams, or <c>null</c> if not specified. Valid range: 0-1,000.</value>
    public decimal? Fiber { get; }

    /// <summary>
    /// Gets the optional sugar content per full serving.
    /// </summary>
    /// <value>Sugar in grams, or <c>null</c> if not specified. Valid range: 0-1,000.</value>
    public decimal? Sugar { get; }

    /// <summary>
    /// Gets the optional salt content per full serving.
    /// </summary>
    /// <value>Salt in grams, or <c>null</c> if not specified. Valid range: 0-100.</value>
    public decimal? Salt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NutritionalInfo"/> record.
    /// </summary>
    /// <param name="calories">The calorie count.</param>
    /// <param name="protein">The protein content in grams.</param>
    /// <param name="carbohydrates">The carbohydrate content in grams.</param>
    /// <param name="fat">The fat content in grams.</param>
    /// <param name="servingSize">The serving size in grams.</param>
    /// <param name="fiber">Optional fiber content in grams.</param>
    /// <param name="sugar">Optional sugar content in grams.</param>
    /// <param name="salt">Optional salt content in grams.</param>
    protected NutritionalInfo(
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

    /// <summary>
    /// Calculates the nutritional values for a specific portion size.
    /// </summary>
    /// <param name="portionPercentage">
    /// The portion percentage as a decimal (e.g., 0.25 for a small/tapa portion, 0.5 for half).
    /// </param>
    /// <returns>
    /// A new <see cref="NutritionalInfo"/> instance with values scaled to the specified portion.
    /// </returns>
    /// <remarks>
    /// This method bypasses validation since the derived values come from a valid instance.
    /// </remarks>
    public NutritionalInfo GetNutritionForPortion(decimal portionPercentage)
    {
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

public static class NutritionalInfoValidationMessages
{
    public const string MinValue = "{PropertyName} must be >= {ComparisonValue}";
    public const string GreaterThanZero = "{PropertyName} must be > {ComparisonValue}";
    public const string MaxValue = "{PropertyName} cannot exceed {ComparisonValue}";
}

/// <summary>
/// Provides validation rules for the <see cref="NutritionalInfo"/> value object.
/// </summary>
/// <remarks>
/// Validates nutritional information invariants including value ranges
/// for all macronutrients and optional micronutrients.
/// </remarks>
public class NutritionalInfoValidator : AbstractValidator<NutritionalInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NutritionalInfoValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public NutritionalInfoValidator()
    {
        RuleFor(x => x.Calories)
            .GreaterThanOrEqualTo(0)
            .WithMessage(NutritionalInfoValidationMessages.MinValue)
            .LessThanOrEqualTo(10000)
            .WithMessage(NutritionalInfoValidationMessages.MaxValue);

        RuleFor(x => x.Protein)
            .GreaterThanOrEqualTo(0)
            .WithMessage(NutritionalInfoValidationMessages.MinValue)
            .LessThanOrEqualTo(1000)
            .WithMessage(NutritionalInfoValidationMessages.MaxValue);

        RuleFor(x => x.Carbohydrates)
            .GreaterThanOrEqualTo(0)
            .WithMessage(NutritionalInfoValidationMessages.MinValue)
            .LessThanOrEqualTo(1000)
            .WithMessage(NutritionalInfoValidationMessages.MaxValue);

        RuleFor(x => x.Fat)
            .GreaterThanOrEqualTo(0)
            .WithMessage(NutritionalInfoValidationMessages.MinValue)
            .LessThanOrEqualTo(1000)
            .WithMessage(NutritionalInfoValidationMessages.MaxValue);

        RuleFor(x => x.ServingSize)
            .GreaterThan(0)
            .WithMessage(NutritionalInfoValidationMessages.GreaterThanZero)
            .LessThanOrEqualTo(10000)
            .WithMessage(NutritionalInfoValidationMessages.MaxValue);

        RuleFor(x => x.Fiber)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Fiber.HasValue)
            .WithMessage(NutritionalInfoValidationMessages.MinValue)
            .LessThanOrEqualTo(1000)
            .When(x => x.Fiber.HasValue)
            .WithMessage(NutritionalInfoValidationMessages.MaxValue);

        RuleFor(x => x.Sugar)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Sugar.HasValue)
            .WithMessage(NutritionalInfoValidationMessages.MinValue)
            .LessThanOrEqualTo(1000)
            .When(x => x.Sugar.HasValue)
            .WithMessage(NutritionalInfoValidationMessages.MaxValue);

        RuleFor(x => x.Salt)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Salt.HasValue)
            .WithMessage(NutritionalInfoValidationMessages.MinValue)
            .LessThanOrEqualTo(100)
            .When(x => x.Salt.HasValue)
            .WithMessage(NutritionalInfoValidationMessages.MaxValue);
    }
}
