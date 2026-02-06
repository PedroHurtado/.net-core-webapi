namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record SetPriceRangeCommand(
    decimal MinPrice,
    decimal MaxPrice
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class SetPriceRange(
        PriceRange.Create priceRangeCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<SetPriceRangeCommand, Customer>
    {
        public override Customer Execute(Customer customer, SetPriceRangeCommand command)
        {
            var priceRange = priceRangeCreate.Execute(new CreatePriceRangeCommand(
                command.MinPrice,
                command.MaxPrice));

            customer.PriceRange = priceRange;

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
