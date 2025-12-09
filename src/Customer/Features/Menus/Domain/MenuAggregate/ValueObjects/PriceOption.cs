namespace Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

using FluentValidation;
using Fudie.Validation;

/// <summary>
/// Opción de precio para un tipo de porción de un item.
/// </summary>
public record PriceOption
{
    public Guid Id { get; }
    public PortionType PortionType { get; }
    public decimal? Price { get; }
    public bool IsActive { get; }

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
    /// Indica si el precio requiere actualización diaria (según mercado sin precio).
    /// </summary>
    public bool RequiresMarketPrice => PortionType == PortionType.MarketPrice && !Price.HasValue;

    /// <summary>
    /// Precio formateado para mostrar.
    /// </summary>
    public string DisplayPrice => RequiresMarketPrice ? "S/M" : Price!.Value.ToString("C");
}

/// <summary>
/// Validador de invariantes para PriceOption.
/// </summary>
public class PriceOptionValidator : AbstractValidator<PriceOption>
{
    public PriceOptionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El Id es requerido");

        RuleFor(x => x.Price)
            .NotNull()
            .When(x => x.PortionType != PortionType.MarketPrice)
            .WithMessage("El precio es requerido para porciones de tipo fijo");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue)
            .WithMessage("El precio no puede ser negativo");
    }
}