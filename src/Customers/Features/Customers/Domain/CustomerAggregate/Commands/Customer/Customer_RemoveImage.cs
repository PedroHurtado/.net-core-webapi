namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record RemoveImageCommand(Guid ImageId);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveImage(
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<RemoveImageCommand, Customer>
    {
        public override Customer Execute(Customer customer, RemoveImageCommand command)
        {
            var existing = customer.Images.FirstOrDefault(i => i.Id == command.ImageId);
            NotFoundGuard.ThrowIfNull(existing, "Image not found");

            customer._images.Remove(existing!);

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
