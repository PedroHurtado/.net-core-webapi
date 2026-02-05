namespace Menus.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class RemoveMenuItemNutritionalInfoIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private async Task<MenuItemResponse> SetNutritionalInfoAsync(
        Guid menuItemId,
        int calories = 500,
        decimal protein = 25.0m,
        decimal carbohydrates = 40.0m,
        decimal fat = 15.0m,
        int servingSize = 350)
    {
        var request = new SetMenuItemNutritionalInfo.Request(
            Calories: calories,
            Protein: protein,
            Carbohydrates: carbohydrates,
            Fat: fat,
            ServingSize: servingSize,
            Fiber: null,
            Sugar: null,
            Salt: null
        );

        await Client.PutAsJsonAsync($"/menu-items/{menuItemId}/nutritional-info", request);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItemId}");
        return (await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions))!;
    }

    #region Success Tests

    [Fact]
    public async Task RemoveNutritionalInfo_WithExistingNutritionalInfo_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");
        await SetNutritionalInfoAsync(menuItem.Id, calories: 500);

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveNutritionalInfo_WithExistingNutritionalInfo_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");
        await SetNutritionalInfoAsync(menuItem.Id, calories: 250, protein: 30.0m);

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.NutritionalInfo.Should().BeNull();
    }

    [Fact]
    public async Task RemoveNutritionalInfo_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        await SetNutritionalInfoAsync(menuItem.Id, calories: 500);

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    [Fact]
    public async Task RemoveNutritionalInfo_ShouldPreservePriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        await SetNutritionalInfoAsync(menuItem.Id, calories: 500);

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().NotBeEmpty();
    }

    #endregion

    #region Idempotent Tests

    [Fact]
    public async Task RemoveNutritionalInfo_WithoutExistingNutritionalInfo_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Test Item");

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveNutritionalInfo_CalledTwice_ShouldReturnNoContentBothTimes()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        await SetNutritionalInfoAsync(menuItem.Id, calories: 500);

        // Act
        var firstResponse = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");
        var secondResponse = await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveNutritionalInfo_CalledTwice_ShouldProduceSameResult()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        await SetNutritionalInfoAsync(menuItem.Id, calories: 500);

        // Act
        await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");
        await Client.DeleteAsync($"/menu-items/{menuItem.Id}/nutritional-info");

        // Assert
        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.NutritionalInfo.Should().BeNull();
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task RemoveNutritionalInfo_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menu-items/{nonExistentId}/nutritional-info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
