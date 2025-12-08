namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

using FluentValidation;
using Fudie.Domain;

/// <summary>
/// Fianza específica para un item que sobrescribe la política del menú.
/// </summary>
public record ItemDepositOverride
{
    public decimal DepositAmount { get; }
    public int? MinimumQuantityForDeposit { get; }

    private ItemDepositOverride(
        decimal depositAmount,
        int? minimumQuantityForDeposit)
    {
        DepositAmount = depositAmount;
        MinimumQuantityForDeposit = minimumQuantityForDeposit;
    }

    public static Result<ItemDepositOverride> Create(
        decimal depositAmount,
        int? minimumQuantityForDeposit = null)
    {
        var itemOverride = new ItemDepositOverride(depositAmount, minimumQuantityForDeposit);

        var validation = new ItemDepositOverrideValidator().Validate(itemOverride);
        
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
                .ToList();
            return Result<ItemDepositOverride>.Failure(errors);
        }

        return Result<ItemDepositOverride>.Success(itemOverride);
    }

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

/// <summary>
/// Validador de invariantes para ItemDepositOverride.
/// </summary>
public class ItemDepositOverrideValidator : AbstractValidator<ItemDepositOverride>
{
    public ItemDepositOverrideValidator()
    {
        RuleFor(x => x.DepositAmount)
            .GreaterThan(0)
            .WithMessage("La fianza debe ser mayor que cero");

        RuleFor(x => x.MinimumQuantityForDeposit)
            .GreaterThanOrEqualTo(1)
            .When(x => x.MinimumQuantityForDeposit.HasValue)
            .WithMessage("La cantidad mínima debe ser al menos 1");
    }
}