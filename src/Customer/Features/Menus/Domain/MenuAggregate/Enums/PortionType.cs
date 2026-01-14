namespace Customer.Features.Menus.Domain.MenuAggregate.Enums;

/// <summary>
/// Specifies the portion size type for menu item price options.
/// </summary>
/// <remarks>
/// This enum is used in <see cref="ValueObjects.PriceOption"/> to define
/// different portion sizes with corresponding prices for a menu item.
/// </remarks>
public enum PortionType
{
    /// <summary>
    /// A small portion, typically 25% of a full serving.
    /// </summary>
    /// <remarks>
    /// Often referred to as "tapa" in Spanish cuisine.
    /// </remarks>
    Small = 1,

    /// <summary>
    /// A half portion, typically 50% of a full serving.
    /// </summary>
    /// <remarks>
    /// Often referred to as "media ración" in Spanish cuisine.
    /// </remarks>
    Half = 2,

    /// <summary>
    /// A full serving portion.
    /// </summary>
    /// <remarks>
    /// The standard complete portion size.
    /// </remarks>
    Full = 3,

    /// <summary>
    /// Price determined by current market conditions.
    /// </summary>
    /// <remarks>
    /// Used for items whose prices may vary daily based on market prices
    /// (e.g., fresh seafood, seasonal produce). When using this type,
    /// the price may be <c>null</c> and will be determined at order time.
    /// </remarks>
    MarketPrice = 4
}
