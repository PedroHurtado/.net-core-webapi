namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record AddImageCommand(
    string Url,
    string? AltText = null,
    int DisplayOrder = 0,
    bool IsCover = false
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddImage(
        CustomerImage.Create customerImageCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<AddImageCommand, Customer>
    {
        public override Customer Execute(Customer customer, AddImageCommand command)
        {
            ConflictGuard.ThrowIf(
                customer.Images.Any(i => i.Url == command.Url),
                "Image with this URL already exists");

            if (command.IsCover)
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

            var image = customerImageCreate.Execute(new CreateCustomerImageCommand(
                command.Url,
                command.AltText,
                command.DisplayOrder,
                command.IsCover));

            customer._images.Add(image);

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
