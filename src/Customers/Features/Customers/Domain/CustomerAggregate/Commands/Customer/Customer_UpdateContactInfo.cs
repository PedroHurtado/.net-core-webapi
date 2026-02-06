namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record UpdateContactInfoCommand(
    string Phone,
    string? Email,
    string? WebsiteUrl
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateContactInfo(
        ContactInfo.Create contactInfoCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<UpdateContactInfoCommand, Customer>
    {
        public override Customer Execute(Customer customer, UpdateContactInfoCommand command)
        {
            var contactInfo = contactInfoCreate.Execute(new CreateContactInfoCommand(
                command.Phone,
                command.Email,
                command.WebsiteUrl));

            customer.ContactInfo = contactInfo;

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
