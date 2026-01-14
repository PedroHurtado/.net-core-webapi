// src/Customer/Features/Menus/Domain/MenuAggregate/Enums/PortionType.cs
namespace Customer.Features.Menus.Domain.MenuAggregate.Enums;

/// <summary>
/// Tipo de porción para las opciones de precio de un item.
/// </summary>
public enum PortionType
{
    /// <summary>
    /// Porción pequeña, típicamente 25% de una ración completa.
    /// </summary>
    Small = 1,
    
    /// <summary>
    /// Media ración, típicamente 50% de una ración completa.
    /// </summary>
    Half = 2,
    
    /// <summary>
    /// Ración completa.
    /// </summary>
    Full = 3,
    
    /// <summary>
    /// Precio según mercado, puede variar diariamente.
    /// </summary>
    MarketPrice = 4
}