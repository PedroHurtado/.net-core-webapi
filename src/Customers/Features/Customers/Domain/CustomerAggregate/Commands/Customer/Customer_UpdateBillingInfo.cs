namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record UpdateBillingInfoCommand(
    string BusinessName,
    string TaxId,
    CreateAddressCommand BillingAddress
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateBillingInfo(
        BillingInfo.Create billingInfoCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<UpdateBillingInfoCommand, Customer>
    {
        public override Customer Execute(Customer customer, UpdateBillingInfoCommand command)
        {
            var billingInfo = billingInfoCreate.Execute(new CreateBillingInfoCommand(
                command.BusinessName,
                command.TaxId,
                command.BillingAddress));

            customer.BillingInfo = billingInfo;

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
