namespace Customers.UnitTests.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class AddressTests
{
    private static readonly GeoPoint ValidLocation = new(38.0389m, -1.4917m);

    #region Street

    [Theory]
    [InlineData("Ctra. Murcia, 23")]
    [InlineData("C/ Gran Vía, 1")]
    public void Street_SetsCorrectValue(string street)
    {
        var address = new Address(street, "La Puebla de Mula", "30193", "Murcia", "España", ValidLocation);

        address.Street.Should().Be(street);
    }

    #endregion

    #region City

    [Theory]
    [InlineData("La Puebla de Mula")]
    [InlineData("Murcia")]
    public void City_SetsCorrectValue(string city)
    {
        var address = new Address("Ctra. Murcia, 23", city, "30193", "Murcia", "España", ValidLocation);

        address.City.Should().Be(city);
    }

    #endregion

    #region PostalCode

    [Theory]
    [InlineData("30193")]
    [InlineData("28001")]
    public void PostalCode_SetsCorrectValue(string postalCode)
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", postalCode, "Murcia", "España", ValidLocation);

        address.PostalCode.Should().Be(postalCode);
    }

    #endregion

    #region Region

    [Theory]
    [InlineData("Murcia")]
    [InlineData("Madrid")]
    public void Region_SetsCorrectValue(string region)
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", region, "España", ValidLocation);

        address.Region.Should().Be(region);
    }

    #endregion

    #region Country

    [Theory]
    [InlineData("España")]
    [InlineData("France")]
    public void Country_SetsCorrectValue(string country)
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", country, ValidLocation);

        address.Country.Should().Be(country);
    }

    #endregion

    #region Location

    [Fact]
    public void Location_SetsCorrectValue()
    {
        var location = new GeoPoint(38.0389m, -1.4917m);
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", location);

        address.Location.Should().Be(location);
    }

    #endregion

    #region FullAddress

    [Fact]
    public void FullAddress_ReturnsFormattedAddress()
    {
        var address = new Address("Ctra. Murcia, 23", "La Puebla de Mula", "30193", "Murcia", "España", ValidLocation);

        address.FullAddress.Should().Be("Ctra. Murcia, 23, 30193 La Puebla de Mula, Murcia, España");
    }

    #endregion
}
