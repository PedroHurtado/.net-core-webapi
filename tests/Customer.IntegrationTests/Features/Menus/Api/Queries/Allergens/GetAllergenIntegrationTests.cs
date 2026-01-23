namespace Customer.IntegrationTests.Features.Menus.Api.Queries.Allergens;

public class GetAllergenIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task GetAllergen_WithExistingId_ShouldReturnOkAndAllergen()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(name: "Gluten");

        // Act
        var response = await Client.GetAsync($"/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetAllergen.Response>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(allergen.Id);
        result.Name.Should().Be("Gluten");
    }

    [Fact]
    public async Task GetAllergen_WithAllFields_ShouldReturnCompleteResponse()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(
            name: "Lácteos",
            iconUrl: "https://example.com/lacteos.png",
            isActive: false,
            displayOrder: 5
        );

        // Act
        var response = await Client.GetAsync($"/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetAllergen.Response>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(allergen.Id);
        result.Name.Should().Be("Lácteos");
        result.IconUrl.Should().Be("https://example.com/lacteos.png");
        result.IsActive.Should().BeFalse();
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task GetAllergen_WithNullIconUrl_ShouldReturnNullIconUrl()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(iconUrl: null);

        // Act
        var response = await Client.GetAsync($"/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetAllergen.Response>();
        result!.IconUrl.Should().BeNull();
    }

    [Fact]
    public async Task GetAllergen_WithDefaultValues_ShouldReturnDefaults()
    {
        // Arrange
        var allergen = await CreateAllergenAsync();

        // Act
        var response = await Client.GetAsync($"/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetAllergen.Response>();
        result!.IsActive.Should().BeTrue();
        result.DisplayOrder.Should().Be(0);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task GetAllergen_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = "NONEXISTENT_" + Guid.NewGuid().ToString("N")[..8].ToUpper();

        // Act
        var response = await Client.GetAsync($"/allergens/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
