namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record AddSocialLinkCommand(
    string Platform,
    string Url
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddSocialLink(
        SocialLink.Create socialLinkCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<AddSocialLinkCommand, Customer>
    {
        public override Customer Execute(Customer customer, AddSocialLinkCommand command)
        {
            ConflictGuard.ThrowIf(
                customer.SocialLinks.Any(s => s.Platform.Equals(command.Platform, StringComparison.OrdinalIgnoreCase)),
                $"Social link for '{command.Platform}' already exists");

            var socialLink = socialLinkCreate.Execute(new CreateSocialLinkCommand(
                command.Platform,
                command.Url));

            customer._socialLinks.Add(socialLink);

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
