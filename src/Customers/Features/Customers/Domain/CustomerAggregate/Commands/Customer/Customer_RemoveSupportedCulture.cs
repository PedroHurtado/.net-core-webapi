namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record RemoveSupportedCultureCommand(string Code);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveSupportedCulture(
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<RemoveSupportedCultureCommand, Customer>
    {
        public override Customer Execute(Customer customer, RemoveSupportedCultureCommand command)
        {
            var existing = customer.SupportedCultures.FirstOrDefault(c => c.Code == command.Code);
            NotFoundGuard.ThrowIfNull(existing, $"Culture '{command.Code}' not found");

            customer._supportedCultures.Remove(existing!);

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
