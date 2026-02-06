namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record UpdateSocialLinkCommand(
    string Platform,
    string Url
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateSocialLink(
        SocialLink.Create socialLinkCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<UpdateSocialLinkCommand, Customer>
    {
        public override Customer Execute(Customer customer, UpdateSocialLinkCommand command)
        {
            var existing = customer.SocialLinks.FirstOrDefault(s => s.Platform.Equals(command.Platform, StringComparison.OrdinalIgnoreCase));
            NotFoundGuard.ThrowIfNull(existing, $"Social link for '{command.Platform}' not found");

            var updated = socialLinkCreate.Execute(new CreateSocialLinkCommand(
                existing!.Platform,
                command.Url));

            customer._socialLinks.Remove(existing!);
            customer._socialLinks.Add(updated);

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
