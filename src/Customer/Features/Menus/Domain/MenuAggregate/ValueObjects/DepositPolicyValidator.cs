namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;
using FluentValidation;

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