namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class BillingInfoTests
{
    private static readonly Address ValidAddress = new(
        "Ctra. Murcia, 23",
        "La Puebla de Mula",
        "30193",
        "Murcia",
        "España",
        new GeoPoint(38.0389m, -1.4917m));

    #region BusinessName

    [Theory]
    [InlineData("Bar Juanjo SL")]
    [InlineData("Juanjo y María SL")]
    public void BusinessName_SetsCorrectValue(string businessName)
    {
        var billingInfo = new BillingInfo(businessName, "B12345678", ValidAddress);

        billingInfo.BusinessName.Should().Be(businessName);
    }

    #endregion

    #region TaxId

    [Theory]
    [InlineData("B12345678")]
    [InlineData("B87654321")]
    public void TaxId_SetsCorrectValue(string taxId)
    {
        var billingInfo = new BillingInfo("Bar Juanjo SL", taxId, ValidAddress);

        billingInfo.TaxId.Should().Be(taxId);
    }

    #endregion

    #region BillingAddress

    [Fact]
    public void BillingAddress_SetsCorrectValue()
    {
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", ValidAddress);

        billingInfo.BillingAddress.Should().Be(ValidAddress);
    }

    #endregion
}
