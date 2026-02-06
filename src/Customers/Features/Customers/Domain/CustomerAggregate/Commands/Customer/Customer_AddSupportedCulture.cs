namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record AddSupportedCultureCommand(
    string Code
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddSupportedCulture(
        CultureCode.Create cultureCodeCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<AddSupportedCultureCommand, Customer>
    {
        public override Customer Execute(Customer customer, AddSupportedCultureCommand command)
        {
            ConflictGuard.ThrowIf(
                customer.DefaultCulture == command.Code,
                $"Culture '{command.Code}' is already the default culture");

            ConflictGuard.ThrowIf(
                customer.SupportedCultures.Any(c => c.Code == command.Code),
                $"Culture '{command.Code}' is already supported");

            var cultureCode = cultureCodeCreate.Execute(new CreateCultureCodeCommand(command.Code));

            customer._supportedCultures.Add(cultureCode);

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
