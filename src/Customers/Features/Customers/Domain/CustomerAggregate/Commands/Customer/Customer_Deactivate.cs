namespace Customers.Features.Customers.Domain.CustomerAggregate;

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<Customer>
    {
        public override Customer Execute(Customer customer)
        {
            ConflictGuard.ThrowIf(!customer.IsActive, "Customer is already inactive");

            customer.IsActive = false;

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
