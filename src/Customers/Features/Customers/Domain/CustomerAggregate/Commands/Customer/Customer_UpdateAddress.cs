namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record UpdateAddressCommand(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    decimal Latitude,
    decimal Longitude
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateAddress(
        Address.Create addressCreate,
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<UpdateAddressCommand, Customer>
    {
        public override Customer Execute(Customer customer, UpdateAddressCommand command)
        {
            var address = addressCreate.Execute(new CreateAddressCommand(
                command.Street,
                command.City,
                command.PostalCode,
                command.Region,
                command.Country,
                command.Latitude,
                command.Longitude));

            customer.Address = address;

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
