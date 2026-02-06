namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

public record CreateContactInfoCommand(
    string Phone,
    string? Email,
    string? WebsiteUrl
);

public partial record ContactInfo
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<ContactInfo> contactInfoValidator
    ) : AbstractCreateCommand<CreateContactInfoCommand, ContactInfo>
    {
        public override ContactInfo Execute(CreateContactInfoCommand command)
        {
            var contactInfo = new ContactInfo(
                command.Phone,
                command.Email,
                command.WebsiteUrl);

            return contactInfoValidator.ValidateOrThrow(contactInfo);
        }
    }
}
