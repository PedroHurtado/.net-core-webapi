namespace Menus.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemAllergenIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private async Task<(MenuItemResponse MenuItem, CreateAllergen.Response Allergen)> CreateMenuItemWithAllergenAsync(
        string allergenCode = "GLUTEN",
        string allergenName = "Gluten")
    {
        var menuItem = await CreateMenuItemAsync(name: "Test MenuItem");
        var allergen = await CreateAllergenAsync(code: allergenCode, name: allergenName);

        var request = new AddMenuItemAllergen.Request(AllergenId: allergen.Id);
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", request);

        return (menuItem, allergen);
    }

    #region Success Tests

    [Fact]
    public async Task RemoveAllergen_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var (menuItem, allergen) = await CreateMenuItemWithAllergenAsync();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveAllergen_WithValidData_ShouldPersistChanges()
    {
        // Arrange
        var (menuItem, allergen) = await CreateMenuItemWithAllergenAsync(
            allergenCode: "GLUTEN_PERSIST",
            allergenName: "Gluten");

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Allergens.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveAllergen_WithMultipleAllergens_ShouldRemoveOnlySpecified()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Paella Mixta");
        var glutenAllergen = await CreateAllergenAsync(code: "GLUTEN_MULTI", name: "Gluten");
        var seafoodAllergen = await CreateAllergenAsync(code: "SEAFOOD_MULTI", name: "Mariscos");

        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens",
            new AddMenuItemAllergen.Request(glutenAllergen.Id));
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens",
            new AddMenuItemAllergen.Request(seafoodAllergen.Id));

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/allergens/{glutenAllergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Allergens.Should().ContainSingle();
        result.Allergens.Should().Contain(a => a.Id == seafoodAllergen.Id);
        result.Allergens.Should().NotContain(a => a.Id == glutenAllergen.Id);
    }

    [Fact]
    public async Task RemoveAllergen_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        var allergen = await CreateAllergenAsync(code: "SHELLFISH_PRESERVE", name: "Crustaceos");
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens",
            new AddMenuItemAllergen.Request(allergen.Id));

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    [Fact]
    public async Task RemoveAllergen_ShouldPreserveExistingPriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var existingOption = menuItem.PriceOptions.First();
        var allergen = await CreateAllergenAsync(code: "NUTS_PRESERVE", name: "Frutos secos");
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens",
            new AddMenuItemAllergen.Request(allergen.Id));

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().Contain(p =>
            p.PortionType == existingOption.PortionType &&
            p.Price == existingOption.Price);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task RemoveAllergen_WithNonExistentMenuItemId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{nonExistentId}/allergens/GLUTEN");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveAllergen_WithNonExistentAllergenInMenuItem_ShouldReturnNotFound()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/allergens/SULFITOS");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveAllergen_WithAllergenNotInMenuItem_ShouldReturnNotFound()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var allergen = await CreateAllergenAsync(code: "LACTOSE_NOT_IN", name: "Lactosa");

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/allergens/{allergen.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
