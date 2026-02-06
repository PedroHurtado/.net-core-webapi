namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

public record CreatePriceRangeCommand(
    decimal MinPrice,
    decimal MaxPrice
);

public partial record PriceRange
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<PriceRange> priceRangeValidator
    ) : AbstractCreateCommand<CreatePriceRangeCommand, PriceRange>
    {
        public override PriceRange Execute(CreatePriceRangeCommand command)
        {
            var priceRange = new PriceRange(
                command.MinPrice,
                command.MaxPrice);

            return priceRangeValidator.ValidateOrThrow(priceRange);
        }
    }
}
