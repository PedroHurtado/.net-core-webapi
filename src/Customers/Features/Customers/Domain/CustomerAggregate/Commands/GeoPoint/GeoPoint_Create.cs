namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

public record CreateGeoPointCommand(
    decimal Latitude,
    decimal Longitude
);

public partial record GeoPoint
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<GeoPoint> geoPointValidator
    ) : AbstractCreateCommand<CreateGeoPointCommand, GeoPoint>
    {
        public override GeoPoint Execute(CreateGeoPointCommand command)
        {
            var geoPoint = new GeoPoint(
                command.Latitude,
                command.Longitude);

            return geoPointValidator.ValidateOrThrow(geoPoint);
        }
    }
}
