namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

/// <summary>
/// Represents a pricing option for a specific portion type of a menu item.
/// </summary>
/// <remarks>
/// <para>
/// This value object defines a price for a specific portion size of a menu item.
/// Each menu item can have multiple price options for different portion types.
/// </para>
/// <para>
/// For <see cref="Enums.PortionType.MarketPrice"/> items, the price may be <c>null</c>
/// and determined at order time based on current market conditions.
/// </para>
/// </remarks>
public record PriceOption
{
    /// <summary>
    /// Gets the unique identifier for this price option.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the portion type this price applies to.
    /// </summary>
    /// <value>The portion type (Small, Half, Full, or MarketPrice).</value>
    public PortionType PortionType { get; }

    /// <summary>
    /// Gets the price for this portion type.
    /// </summary>
    /// <value>
    /// The price amount, or <c>null</c> for <see cref="Enums.PortionType.MarketPrice"/> items
    /// where the price is determined at order time. Must be non-negative when specified.
    /// </value>
    public decimal? Price { get; }

    /// <summary>
    /// Gets a value indicating whether this price option is currently active.
    /// </summary>
    /// <value><c>true</c> if the price option is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PriceOption"/> record.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="portionType">The portion type.</param>
    /// <param name="price">The price amount.</param>
    /// <param name="isActive">Whether the option is active.</param>
    private PriceOption(
        Guid id,
        PortionType portionType,
        decimal? price,
        bool isActive)
    {
        Id = id;
        PortionType = portionType;
        Price = price;
        IsActive = isActive;
    }

    /// <summary>
    /// Creates a new validated <see cref="PriceOption"/> instance.
    /// </summary>
    /// <param name="id">The unique identifier. Must not be empty.</param>
    /// <param name="portionType">The portion type.</param>
    /// <param name="price">
    /// The price amount. Required for fixed portion types; optional for <see cref="Enums.PortionType.MarketPrice"/>.
    /// Must be non-negative when specified.
    /// </param>
    /// <param name="isActive">Whether the option is active. Defaults to <c>true</c>.</param>
    /// <returns>A validated <see cref="PriceOption"/> instance.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public static PriceOption Create(
        Guid id,
        PortionType portionType,
        decimal? price,
        bool isActive = true)
    {
        var option = new PriceOption(id, portionType, price, isActive);

        return new PriceOptionValidator().ValidateOrThrow(option);
    }

    /// <summary>
    /// Gets a value indicating whether the price requires daily market price updates.
    /// </summary>
    /// <value>
    /// <c>true</c> if the portion type is <see cref="Enums.PortionType.MarketPrice"/>
    /// and no fixed price is set; otherwise, <c>false</c>.
    /// </value>
    public bool RequiresMarketPrice => PortionType == PortionType.MarketPrice && !Price.HasValue;

    /// <summary>
    /// Gets the formatted price for display purposes.
    /// </summary>
    /// <value>
    /// The price formatted as currency, or "S/M" (Subject to Market) for market-priced items without a fixed price.
    /// </value>
    public string DisplayPrice => RequiresMarketPrice ? "S/M" : Price!.Value.ToString("C");
}

/// <summary>
/// Provides validation rules for the <see cref="PriceOption"/> value object.
/// </summary>
/// <remarks>
/// Validates price option invariants including required identifier,
/// price requirements for fixed portion types, and non-negative price values.
/// </remarks>
public class PriceOptionValidator : AbstractValidator<PriceOption>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PriceOptionValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    public PriceOptionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        RuleFor(x => x.Price)
            .NotNull()
            .When(x => x.PortionType != PortionType.MarketPrice)
            .WithMessage("Price is required for fixed portion types");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Price cannot be negative");
    }
}
