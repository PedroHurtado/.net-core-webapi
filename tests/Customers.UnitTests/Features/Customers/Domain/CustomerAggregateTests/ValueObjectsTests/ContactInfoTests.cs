namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class ContactInfoTests
{
    #region Phone

    [Theory]
    [InlineData("639079481")]
    [InlineData("912345678")]
    public void Phone_SetsCorrectValue(string phone)
    {
        var contactInfo = new ContactInfo(phone, null, null);

        contactInfo.Phone.Should().Be(phone);
    }

    #endregion

    #region Email

    [Theory]
    [InlineData("juanjo@example.com")]
    [InlineData(null)]
    public void Email_SetsCorrectValue(string? email)
    {
        var contactInfo = new ContactInfo("639079481", email, null);

        contactInfo.Email.Should().Be(email);
    }

    #endregion

    #region WebsiteUrl

    [Theory]
    [InlineData("https://facebook.com/elbardeljuanjo")]
    [InlineData(null)]
    public void WebsiteUrl_SetsCorrectValue(string? websiteUrl)
    {
        var contactInfo = new ContactInfo("639079481", null, websiteUrl);

        contactInfo.WebsiteUrl.Should().Be(websiteUrl);
    }

    #endregion
}
