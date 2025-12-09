namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

using FluentValidation;
using Fudie.Validation;

/// <summary>
/// Política de fianza para reservas a nivel de menú.
/// </summary>
public record DepositPolicy
{
    public DepositType DepositType { get; }
    public decimal Amount { get; }
    public decimal? Percentage { get; }
    public decimal? MinimumBillForDeposit { get; }
    public int? MinimumGuestsForDeposit { get; }

    private DepositPolicy(
        DepositType depositType,
        decimal amount,
        decimal? percentage,
        decimal? minimumBillForDeposit,
        int? minimumGuestsForDeposit)
    {
        DepositType = depositType;
        Amount = amount;
        Percentage = percentage;
        MinimumBillForDeposit = minimumBillForDeposit;
        MinimumGuestsForDeposit = minimumGuestsForDeposit;
    }

    public static DepositPolicy Create(
        DepositType depositType,
        decimal amount,
        decimal? percentage = null,
        decimal? minimumBillForDeposit = null,
        int? minimumGuestsForDeposit = null)
    {
        var policy = new DepositPolicy(
            depositType,
            amount,
            percentage,
            minimumBillForDeposit,
            minimumGuestsForDeposit);

        return new DepositPolicyValidator().ValidateOrThrow(policy);
    }

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

/// <summary>
/// Validador de invariantes para DepositPolicy.
/// </summary>
public class DepositPolicyValidator : AbstractValidator<DepositPolicy>
{
    public DepositPolicyValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El importe debe ser mayor que cero");

        RuleFor(x => x.Percentage)
            .NotNull()
            .When(x => x.DepositType == DepositType.PercentageOfBill)
            .WithMessage("Debe especificar el porcentaje para tipo PercentageOfBill");

        RuleFor(x => x.Percentage)
            .Null()
            .When(x => x.DepositType != DepositType.PercentageOfBill)
            .WithMessage("El porcentaje solo aplica para tipo PercentageOfBill");

        RuleFor(x => x.Percentage)
            .InclusiveBetween(1, 100)
            .When(x => x.Percentage.HasValue)
            .WithMessage("El porcentaje debe estar entre 1 y 100");

        RuleFor(x => x.MinimumGuestsForDeposit)
            .GreaterThanOrEqualTo(1)
            .When(x => x.MinimumGuestsForDeposit.HasValue)
            .WithMessage("El mínimo de comensales debe ser al menos 1");

        RuleFor(x => x.MinimumBillForDeposit)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumBillForDeposit.HasValue)
            .WithMessage("El importe mínimo de cuenta no puede ser negativo");
    }
}