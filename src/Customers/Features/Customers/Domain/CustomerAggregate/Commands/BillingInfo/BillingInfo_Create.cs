namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

public record CreateBillingInfoCommand(
    string BusinessName,
    string TaxId,
    CreateAddressCommand BillingAddress
);

public partial record BillingInfo
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        Address.Create addressCreate,
        IValidator<BillingInfo> billingInfoValidator
    ) : AbstractCreateCommand<CreateBillingInfoCommand, BillingInfo>
    {
        public override BillingInfo Execute(CreateBillingInfoCommand command)
        {
            var billingAddress = addressCreate.Execute(command.BillingAddress);

            var billingInfo = new BillingInfo(
                command.BusinessName,
                command.TaxId,
                billingAddress);

            return billingInfoValidator.ValidateOrThrow(billingInfo);
        }
    }
}
