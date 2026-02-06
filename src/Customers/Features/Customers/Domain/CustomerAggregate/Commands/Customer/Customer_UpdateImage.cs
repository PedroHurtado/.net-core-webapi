namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record UpdateImageCommand(
    Guid ImageId,
    string? AltText,
    int DisplayOrder,
    bool IsCover
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateImage(
        CustomerImage.Create customerImageCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<UpdateImageCommand, Customer>
    {
        public override Customer Execute(Customer customer, UpdateImageCommand command)
        {
            var existing = customer.Images.FirstOrDefault(i => i.Id == command.ImageId);
            NotFoundGuard.ThrowIfNull(existing, "Image not found");

            if (command.IsCover && !existing!.IsCover)
            {
                var currentCover = customer._images.FirstOrDefault(i => i.IsCover);
                if (currentCover != null)
                {
                    var demoted = customerImageCreate.Execute(new CreateCustomerImageCommand(
                        currentCover.Url,
                        currentCover.AltText,
                        currentCover.DisplayOrder,
                        false));
                    demoted = demoted with { Id = currentCover.Id };
                    customer._images.Remove(currentCover);
                    customer._images.Add(demoted);
                }
            }

            var updated = customerImageCreate.Execute(new CreateCustomerImageCommand(
                existing!.Url,
                command.AltText,
                command.DisplayOrder,
                command.IsCover));
            updated = updated with { Id = existing.Id };

            customer._images.Remove(existing!);
            customer._images.Add(updated);

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
