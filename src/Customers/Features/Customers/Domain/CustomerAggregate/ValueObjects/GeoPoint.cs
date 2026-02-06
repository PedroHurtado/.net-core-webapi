namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents a geographic coordinate point.
/// </summary>
/// <remarks>
/// <para>Used to specify precise locations on Earth using latitude and longitude.</para>
/// <para>Latitude ranges from -90 to 90, longitude from -180 to 180.</para>
/// </remarks>
public partial record GeoPoint(
    decimal Latitude,
    decimal Longitude
);

public static class GeoPointValidationMessages
{
    public const string LatitudeRange = "Latitude must be between -90 and 90";
    public const string LongitudeRange = "Longitude must be between -180 and 180";
}

/// <summary>
/// Provides validation rules for the <see cref="GeoPoint"/> value object.
/// </summary>
public class GeoPointValidator : AbstractValidator<GeoPoint>
{
    public GeoPointValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage(GeoPointValidationMessages.LatitudeRange);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage(GeoPointValidationMessages.LongitudeRange);
    }
}