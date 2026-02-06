namespace Customers.UnitTests.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class CustomerImageTests
{
    #region Id

    [Fact]
    public void Id_SetsCorrectValue()
    {
        var id = Guid.NewGuid();
        var image = new CustomerImage(id, "https://cdn.fudie.com/images/fachada.jpg", null, 0, false);

        image.Id.Should().Be(id);
    }

    #endregion

    #region Url

    [Theory]
    [InlineData("https://cdn.fudie.com/images/fachada.jpg")]
    [InlineData("https://cdn.fudie.com/images/interior.jpg")]
    public void Url_SetsCorrectValue(string url)
    {
        var image = new CustomerImage(Guid.NewGuid(), url, null, 0, false);

        image.Url.Should().Be(url);
    }

    #endregion

    #region AltText

    [Theory]
    [InlineData("Fachada del customer")]
    [InlineData(null)]
    public void AltText_SetsCorrectValue(string? altText)
    {
        var image = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/fachada.jpg", altText, 0, false);

        image.AltText.Should().Be(altText);
    }

    #endregion

    #region DisplayOrder

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void DisplayOrder_SetsCorrectValue(int displayOrder)
    {
        var image = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/fachada.jpg", null, displayOrder, false);

        image.DisplayOrder.Should().Be(displayOrder);
    }

    #endregion

    #region IsCover

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsCover_SetsCorrectValue(bool isCover)
    {
        var image = new CustomerImage(Guid.NewGuid(), "https://cdn.fudie.com/images/fachada.jpg", null, 0, isCover);

        image.IsCover.Should().Be(isCover);
    }

    #endregion
}
