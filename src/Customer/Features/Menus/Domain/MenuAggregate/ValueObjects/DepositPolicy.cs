// src/Customer/Features/Menus/Domain/MenuAggregate/ValueObjects/DepositPolicy.cs
namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

/// <summary>
/// Política de fianza para reservas a nivel de menú.
/// </summary>
public record DepositPolicy(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage = null,
    decimal? MinimumBillForDeposit = null,
    int? MinimumGuestsForDeposit = null
)
{
    /// <summary>
    /// Determina si la política aplica según los umbrales configurados.
    /// </summary>
    public bool IsApplicable(int guestCount, decimal estimatedBill)
    {
        if (MinimumGuestsForDeposit.HasValue && guestCount < MinimumGuestsForDeposit.Value)
            return false;
        
        if (MinimumBillForDeposit.HasValue && estimatedBill < MinimumBillForDeposit.Value)
            return false;
        
        return true;
    }

    /// <summary>
    /// Calcula el importe de la fianza según el tipo configurado.
    /// </summary>
    public decimal CalculateDeposit(int guestCount, decimal estimatedBill)
    {
        return DepositType switch
        {
            DepositType.PerPerson => Amount * guestCount,
            DepositType.PercentageOfBill => estimatedBill * (Percentage!.Value / 100m),
            DepositType.FixedAmount => Amount,
            _ => 0m
        };
    }
}