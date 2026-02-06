namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents a price range with minimum and maximum values.
/// </summary>
/// <remarks>
/// <para>Used to indicate the typical price range of a customer's offerings.</para>
/// <para>Both values must be non-negative and maximum must be greater than or equal to minimum.</para>
/// </remarks>
public partial record PriceRange(
    decimal MinPrice,
    decimal MaxPrice
);

public static class PriceRangeValidationMessages
{
    public const string MinPriceNonNegative = "Minimum price cannot be negative";
    public const string MaxPriceNonNegative = "Maximum price cannot be negative";
    public const string MaxPriceGreaterThanOrEqualMinPrice = "Maximum price must be greater than or equal to minimum price";
}

/// <summary>
/// Provides validation rules for the <see cref="PriceRange"/> value object.
/// </summary>
public class PriceRangeValidator : AbstractValidator<PriceRange>
{
    public PriceRangeValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage(PriceRangeValidationMessages.MinPriceNonNegative);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage(PriceRangeValidationMessages.MaxPriceNonNegative);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice)
            .WithMessage(PriceRangeValidationMessages.MaxPriceGreaterThanOrEqualMinPrice);
    }
}
