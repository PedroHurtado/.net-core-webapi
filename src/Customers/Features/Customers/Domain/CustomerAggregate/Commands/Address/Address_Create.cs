namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

public record CreateAddressCommand(
    string Street,
    string City,
    string PostalCode,
    string Region,
    string Country,
    decimal Latitude,
    decimal Longitude
);

public partial record Address
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        GeoPoint.Create geoPointCreate,
        IValidator<Address> addressValidator
    ) : AbstractCreateCommand<CreateAddressCommand, Address>
    {
        public override Address Execute(CreateAddressCommand command)
        {
            var location = geoPointCreate.Execute(new CreateGeoPointCommand(
                command.Latitude,
                command.Longitude));

            var address = new Address(
                command.Street,
                command.City,
                command.PostalCode,
                command.Region,
                command.Country,
                location);

            return addressValidator.ValidateOrThrow(address);
        }
    }
}
