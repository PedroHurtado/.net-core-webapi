namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record RemoveSocialLinkCommand(string Platform);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveSocialLink(
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<RemoveSocialLinkCommand, Customer>
    {
        public override Customer Execute(Customer customer, RemoveSocialLinkCommand command)
        {
            var existing = customer.SocialLinks.FirstOrDefault(s => s.Platform.Equals(command.Platform, StringComparison.OrdinalIgnoreCase));
            NotFoundGuard.ThrowIfNull(existing, $"Social link for '{command.Platform}' not found");

            customer._socialLinks.Remove(existing!);

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
