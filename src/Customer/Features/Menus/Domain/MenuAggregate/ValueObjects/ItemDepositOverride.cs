// src/Customer/Features/Menus/Domain/MenuAggregate/ValueObjects/ItemDepositOverride.cs
namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

/// <summary>
/// Fianza específica para un item que sobrescribe la política del menú.
/// </summary>
public record ItemDepositOverride(
    decimal DepositAmount,
    int? MinimumQuantityForDeposit = null
)
{
    /// <summary>
    /// Determina si la fianza aplica según la cantidad pedida.
    /// </summary>
    public bool IsApplicable(int quantity)
    {
        if (MinimumQuantityForDeposit.HasValue)
            return quantity >= MinimumQuantityForDeposit.Value;
        
        return true;
    }
}