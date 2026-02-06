namespace Customers.Features.Customers.Domain.CustomerAggregate;

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemovePriceRange(
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<Customer>
    {
        public override Customer Execute(Customer customer)
        {
            customer.PriceRange = null;

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
