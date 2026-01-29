namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Queries;

public class GetMenuItemsIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task GetMenuItems_ReturnsOkAndList()
    {
        // Arrange
        var menuItem1 = await CreateMenuItemAsync(name: "Tortilla Española Test");
        var menuItem2 = await CreateMenuItemAsync(name: "Paella Mixta Test");

        // Act
        var response = await Client.GetAsync("/menu-items");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Should().Contain(m => m.Id == menuItem1.Id);
        result.Should().Contain(m => m.Id == menuItem2.Id);
    }

    [Fact]
    public async Task GetMenuItems_WithNoMenuItems_ReturnsEmptyArray()
    {
        // Act
        var response = await Client.GetAsync("/menu-items");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        result.Should().NotBeNull();
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task GetMenuItems_WithIsActiveTrue_ReturnsOnlyActiveMenuItems()
    {
        // Arrange
        var activeMenuItem = await CreateMenuItemAsync(name: "MenuItem Activo Filter");
        await Client.PostAsync($"/menu-items/{activeMenuItem.Id}/activate", null);

        var inactiveMenuItem = await CreateMenuItemAsync(name: "MenuItem Inactivo Filter");

        // Act
        var response = await Client.GetAsync("/menu-items?isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        result.Should().Contain(m => m.Id == activeMenuItem.Id);
        result.Should().NotContain(m => m.Id == inactiveMenuItem.Id);
    }

    [Fact]
    public async Task GetMenuItems_WithIsActiveFalse_ReturnsOnlyInactiveMenuItems()
    {
        // Arrange
        var activeMenuItem = await CreateMenuItemAsync(name: "MenuItem Activo Filter 2");
        await Client.PostAsync($"/menu-items/{activeMenuItem.Id}/activate", null);

        var inactiveMenuItem = await CreateMenuItemAsync(name: "MenuItem Inactivo Filter 2");

        // Act
        var response = await Client.GetAsync("/menu-items?isActive=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        result.Should().NotContain(m => m.Id == activeMenuItem.Id);
        result.Should().Contain(m => m.Id == inactiveMenuItem.Id);
    }

    [Fact]
    public async Task GetMenuItems_WithIsAvailableTrue_ReturnsOnlyAvailableMenuItems()
    {
        // Arrange
        var availableMenuItem = await CreateMenuItemAsync(name: "MenuItem Disponible Filter");

        var unavailableMenuItem = await CreateMenuItemAsync(name: "MenuItem No Disponible Filter");
        await Client.PostAsync($"/menu-items/{unavailableMenuItem.Id}/mark-unavailable",null);

        // Act
        var response = await Client.GetAsync("/menu-items?isAvailable=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        result.Should().Contain(m => m.Id == availableMenuItem.Id);
        result.Should().NotContain(m => m.Id == unavailableMenuItem.Id);
    }

    [Fact]
    public async Task GetMenuItems_WithIsAvailableFalse_ReturnsOnlyUnavailableMenuItems()
    {
        // Arrange
        var availableMenuItem = await CreateMenuItemAsync(name: "MenuItem Disponible Filter 2");

        var unavailableMenuItem = await CreateMenuItemAsync(name: "MenuItem No Disponible Filter 2");
        await Client.PostAsync($"/menu-items/{unavailableMenuItem.Id}/mark-unavailable", null);

        // Act
        var response = await Client.GetAsync("/menu-items?isAvailable=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        result.Should().NotContain(m => m.Id == availableMenuItem.Id);
        result.Should().Contain(m => m.Id == unavailableMenuItem.Id);
    }

    [Fact]
    public async Task GetMenuItems_WithBothFilters_ReturnsFilteredMenuItems()
    {
        // Arrange
        var activeAvailable = await CreateMenuItemAsync(name: "Activo Disponible Filter");
        await Client.PostAsync($"/menu-items/{activeAvailable.Id}/activate", null);

        var activeUnavailable = await CreateMenuItemAsync(name: "Activo No Disponible Filter");
        await Client.PostAsync($"/menu-items/{activeUnavailable.Id}/activate", null);
        await Client.PostAsync($"/menu-items/{activeUnavailable.Id}/mark-unavailable", null);

        var inactiveAvailable = await CreateMenuItemAsync(name: "Inactivo Disponible Filter");

        // Act
        var response = await Client.GetAsync("/menu-items?isActive=true&isAvailable=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        result.Should().Contain(m => m.Id == activeAvailable.Id);
        result.Should().NotContain(m => m.Id == activeUnavailable.Id);
        result.Should().NotContain(m => m.Id == inactiveAvailable.Id);
    }

    #endregion

    #region Allergens Tests

    [Fact]
    public async Task GetMenuItems_WithAllergens_ReturnsAllergensInResponse()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Tortilla con Alergenos");
        var glutenAllergen = await CreateAllergenAsync(code: "GLUTEN_LIST", name: "Gluten");
        var lactoseAllergen = await CreateAllergenAsync(code: "LACTOSE_LIST", name: "Lactosa");

        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens",
            new AddMenuItemAllergen.Request(glutenAllergen.Id));
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens",
            new AddMenuItemAllergen.Request(lactoseAllergen.Id));

        // Act
        var response = await Client.GetAsync("/menu-items");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>(JsonOptions);
        var item = result!.First(m => m.Id == menuItem.Id);

        item.Allergens.Should().HaveCount(2);
        item.Allergens.Should().Contain(a => a.Id == glutenAllergen.Id && a.Name == "Gluten");
        item.Allergens.Should().Contain(a => a.Id == lactoseAllergen.Id && a.Name == "Lactosa");
    }

    #endregion
}
