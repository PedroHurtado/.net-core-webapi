namespace Customers.Features.Customers.Domain.CustomerAggregate;

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<Customer>
    {
        public override Customer Execute(Customer customer)
        {
            ConflictGuard.ThrowIf(customer.IsActive, "Customer is already active");
            ValidationGuard.ThrowIf(!customer.IsProfileComplete, "Customer profile must be complete before activation", nameof(customer.IsProfileComplete));

            customer.IsActive = true;

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
