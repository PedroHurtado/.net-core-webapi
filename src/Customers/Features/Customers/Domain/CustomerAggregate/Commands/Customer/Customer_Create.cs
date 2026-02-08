namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record CreateCustomerCommand(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    CreateAddressCommand Address,
    CreateContactInfoCommand ContactInfo,
    CreateBillingInfoCommand BillingInfo
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        Address.Create addressCreate,
        ContactInfo.Create contactInfoCreate,
        BillingInfo.Create billingInfoCreate,
        IValidator<Customer> customerValidator
    ) : AbstractCreateCommand<CreateCustomerCommand, Customer>
    {
        public override Customer Execute(CreateCustomerCommand command)
        {
            var address = addressCreate.Execute(command.Address);
            var contactInfo = contactInfoCreate.Execute(command.ContactInfo);
            var billingInfo = billingInfoCreate.Execute(command.BillingInfo);

            var customer = new Customer(command.Id)
            {
                Name = command.Name,
                Slug = command.Slug,
                Description = command.Description,
                EstablishmentType = command.EstablishmentType,
                DefaultCulture = command.DefaultCulture,
                TimeZoneId = command.TimeZoneId,
                IsActive = false,
                Address = address,
                ContactInfo = contactInfo,
                BillingInfo = billingInfo,
                PriceRange = null,
                LogoUrl = null
            };

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}