// src/Customer/Features/Menus/Domain/MenuAggregate/ValueObjects/PriceOption.cs
namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

/// <summary>
/// Opción de precio para un tipo de porción de un item.
/// </summary>
public record PriceOption(
    Guid Id,
    PortionType PortionType,
    decimal? Price,
    bool IsActive = true
)
{
    /// <summary>
    /// Indica si el precio requiere actualización diaria (según mercado sin precio).
    /// </summary>
    public bool RequiresMarketPrice => PortionType == PortionType.MarketPrice && !Price.HasValue;

    /// <summary>
    /// Precio formateado para mostrar.
    /// </summary>
    public string DisplayPrice => RequiresMarketPrice ? "S/M" : Price!.Value.ToString("C");
}