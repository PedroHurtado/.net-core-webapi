namespace Plan.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Represents a feature or limit of the plan.
/// </summary>
/// <remarks>
/// <para>
/// This value object defines a plan feature with its code, name, type, and optional limit.
/// It is designed to be extensible and allow for metrics tracking.
/// </para>
/// <para>
/// Features can be of three types: Boolean (included or not), Limit (with a numeric limit),
/// or Unlimited (no restrictions).
/// </para>
/// </remarks>
public partial record Feature
{
    /// <summary>
    /// Gets the unique code for this feature.
    /// </summary>
    /// <value>A unique uppercase code without spaces (e.g., "RESERVATIONS_MONTHLY").</value>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable name of the feature.
    /// </summary>
    /// <value>The feature name (e.g., "Reservas mensuales").</value>
    public string Name { get; }

    /// <summary>
    /// Gets the optional description of the feature.
    /// </summary>
    /// <value>A description providing additional details about the feature, or null if not provided.</value>
    public string? Description { get; }

    /// <summary>
    /// Gets the type of this feature.
    /// </summary>
    /// <value>The feature type (Boolean, Limit, or Unlimited).</value>
    public FeatureType Type { get; }

    /// <summary>
    /// Gets the numeric limit for this feature.
    /// </summary>
    /// <value>
    /// The limit value when Type is Limit, or null for Boolean and Unlimited types.
    /// Must be greater than 0 when specified.
    /// </value>
    public int? Limit { get; }

    /// <summary>
    /// Gets the unit of measure for the limit.
    /// </summary>
    /// <value>The unit of measure (e.g., "reservas", "camareros", "mesas"), or null if not applicable.</value>
    public string? Unit { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> record.
    /// </summary>
    /// <param name="code">The unique feature code.</param>
    /// <param name="name">The feature name.</param>
    /// <param name="description">The optional description.</param>
    /// <param name="type">The feature type.</param>
    /// <param name="limit">The optional limit value.</param>
    /// <param name="unit">The optional unit of measure.</param>
    protected Feature(
        string code,
        string name,
        string? description,
        FeatureType type,
        int? limit = null,
        string? unit = null)
    {
        Code = code;
        Name = name;
        Description = description;
        Type = type;
        Limit = limit;
        Unit = unit;
    }

    /// <summary>
    /// Gets a value indicating whether this feature configuration is valid.
    /// </summary>
    /// <value>
    /// <c>true</c> if the feature is valid according to its type; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Limit type requires Limit to have a value greater than 0</description></item>
    /// <item><description>Boolean type requires Limit to be null</description></item>
    /// <item><description>Unlimited type requires Limit to be null</description></item>
    /// </list>
    /// </remarks>
    public bool IsValid => Type switch
    {
        FeatureType.Limit => Limit.HasValue && Limit > 0,
        FeatureType.Boolean => !Limit.HasValue,
        FeatureType.Unlimited => !Limit.HasValue,
        _ => false
    };

    /// <summary>
    /// Gets the display value for this feature.
    /// </summary>
    /// <value>
    /// A formatted string representing the feature value based on its type.
    /// </value>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Limit: "{Limit} {Unit}" (e.g., "100 reservas/mes")</description></item>
    /// <item><description>Unlimited: "Ilimitado"</description></item>
    /// <item><description>Boolean: "Incluido"</description></item>
    /// </list>
    /// </remarks>
    public string DisplayValue => Type switch
    {
        FeatureType.Limit => $"{Limit} {Unit}",
        FeatureType.Unlimited => "Ilimitado",
        FeatureType.Boolean => "Incluido",
        _ => ""
    };
}

public static class FeatureValidationMessages
{
    public const string CodeRequired = "Feature code is required";
    public const string CodeMaxLength = "Feature code cannot exceed 50 characters";
    public const string CodeMustBeUppercase = "Feature code must be uppercase";
    public const string CodeCannotContainSpaces = "Feature code cannot contain spaces";
    public const string NameRequired = "Feature name is required";
    public const string NameMaxLength = "Feature name cannot exceed 100 characters";
    public const string DescriptionMaxLength = "Feature description cannot exceed 250 characters";
    public const string LimitRequiredForLimitType = "Limit is required when feature type is Limit";
    public const string LimitMustBeGreaterThanZero = "Limit must be greater than 0";
    public const string LimitNotAllowedForBooleanType = "Limit is not allowed for Boolean feature type";
    public const string LimitNotAllowedForUnlimitedType = "Limit is not allowed for Unlimited feature type";
    public const string UnitMaxLength = "Unit cannot exceed 50 characters";
}

/// <summary>
/// Provides validation rules for the <see cref="Feature"/> value object.
/// </summary>
/// <remarks>
/// Validates feature invariants including code format, type-specific limit requirements,
/// and length constraints for all properties.
/// </remarks>
public class FeatureValidator : AbstractValidator<Feature>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public FeatureValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(FeatureValidationMessages.CodeRequired);

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage(FeatureValidationMessages.CodeMaxLength);

        RuleFor(x => x.Code)
            .Must(code => code == code.ToUpper())
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage(FeatureValidationMessages.CodeMustBeUppercase);

        RuleFor(x => x.Code)
            .Must(code => !code.Contains(' '))
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage(FeatureValidationMessages.CodeCannotContainSpaces);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(FeatureValidationMessages.NameRequired);

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage(FeatureValidationMessages.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(250)
            .When(x => x.Description != null)
            .WithMessage(FeatureValidationMessages.DescriptionMaxLength);

        RuleFor(x => x.Limit)
            .NotNull()
            .When(x => x.Type == FeatureType.Limit)
            .WithMessage(FeatureValidationMessages.LimitRequiredForLimitType);

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .When(x => x.Limit.HasValue)
            .WithMessage(FeatureValidationMessages.LimitMustBeGreaterThanZero);

        RuleFor(x => x.Limit)
            .Null()
            .When(x => x.Type == FeatureType.Boolean)
            .WithMessage(FeatureValidationMessages.LimitNotAllowedForBooleanType);

        RuleFor(x => x.Limit)
            .Null()
            .When(x => x.Type == FeatureType.Unlimited)
            .WithMessage(FeatureValidationMessages.LimitNotAllowedForUnlimitedType);

        RuleFor(x => x.Unit)
            .MaximumLength(50)
            .When(x => x.Unit != null)
            .WithMessage(FeatureValidationMessages.UnitMaxLength);
    }
}
