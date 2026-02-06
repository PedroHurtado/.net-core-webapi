namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

public record CreateCustomerImageCommand(
    string Url,
    string? AltText = null,
    int DisplayOrder = 0,
    bool IsCover = false
);

public partial record CustomerImage
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<CustomerImage> customerImageValidator
    ) : AbstractCreateCommand<CreateCustomerImageCommand, CustomerImage>
    {
        public override CustomerImage Execute(CreateCustomerImageCommand command)
        {
            var image = new CustomerImage(
                Guid.NewGuid(),
                command.Url,
                command.AltText,
                command.DisplayOrder,
                command.IsCover);

            return customerImageValidator.ValidateOrThrow(image);
        }
    }
}
