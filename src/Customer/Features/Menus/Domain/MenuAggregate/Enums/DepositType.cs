namespace Customer.Features.Menus.Domain.MenuAggregate.Enums;

/// <summary>
/// Tipo de cálculo de fianza para reservas.
/// </summary>
public enum DepositType
{
    /// <summary>
    /// Fianza calculada como €X por cada comensal.
    /// </summary>
    PerPerson = 1,
    
    /// <summary>
    /// Fianza calculada como un porcentaje del total estimado de la cuenta.
    /// </summary>
    PercentageOfBill = 2,
    
    /// <summary>
    /// Fianza de importe fijo independiente del número de comensales.
    /// </summary>
    FixedAmount = 3
}