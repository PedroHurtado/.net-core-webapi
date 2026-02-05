namespace Menus.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateCategoryItemIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private static UpdateCategoryItem.Request CreateValidRequest(
        int displayOrder = 0,
        UpdateCategoryItem.PriceOptionData[]? priceOverrides = null)
    {
        return new UpdateCategoryItem.Request(
            DisplayOrder: displayOrder,
            PriceOverrides: priceOverrides
        );
    }

    #region Success Tests

    [Fact]
    public async Task UpdateItem_WithValidData_Returns204()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta Principal",
            categoryName: "Entrantes",
            menuItemName: "Croquetas"
        );
        var categoryId = menu.Categories.First().Id;
        var menuItemId = menu.Categories.First().Items.First().MenuItem.Id;
        var request = CreateValidRequest(displayOrder: 5);

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItemId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateItem_WithDisplayOrder_ShouldPersistChanges()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta para Actualizar",
            categoryName: "Entrantes",
            menuItemName: "Jamón Ibérico"
        );
        var categoryId = menu.Categories.First().Id;
        var menuItemId = menu.Categories.First().Items.First().MenuItem.Id;
        var request = CreateValidRequest(displayOrder: 10);

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItemId}", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var item = result!.Categories.First().Items.First();
        item.DisplayOrder.Should().Be(10);
    }

    [Fact]
    public async Task UpdateItem_WithPriceOverrides_Returns204()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta con Overrides",
            categoryName: "Principales",
            menuItemName: "Paella"
        );
        var categoryId = menu.Categories.First().Id;
        var menuItemId = menu.Categories.First().Items.First().MenuItem.Id;
        var priceOverrides = new[]
        {
            new UpdateCategoryItem.PriceOptionData(PortionType.Full, 25.00m, true),
            new UpdateCategoryItem.PriceOptionData(PortionType.Half, 15.00m, true)
        };
        var request = CreateValidRequest(priceOverrides: priceOverrides);

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItemId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateItem_WithPriceOverrides_ShouldPersistOverrides()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta con Precios",
            categoryName: "Arroces",
            menuItemName: "Arroz Negro"
        );
        var categoryId = menu.Categories.First().Id;
        var menuItemId = menu.Categories.First().Items.First().MenuItem.Id;
        var priceOverrides = new[]
        {
            new UpdateCategoryItem.PriceOptionData(PortionType.Full, 22.00m, true)
        };
        var request = CreateValidRequest(priceOverrides: priceOverrides);

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItemId}", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var item = result!.Categories.First().Items.First();
        item.PriceOverrides.Should().HaveCount(1);
        item.PriceOverrides.First().Price.Should().Be(22.00m);
    }

    [Fact]
    public async Task UpdateItem_WithNullPriceOverrides_ShouldClearOverrides()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Gambas");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta para Limpiar",
            categoryName: "Mariscos"
        );
        var categoryId = menu.Categories.First().Id;

        var addRequest = new AddItemToCategory.Request(
            MenuItemId: menuItem.Id,
            DisplayOrder: 0,
            PriceOverrides: [new AddItemToCategory.PriceOptionData(PortionType.Full, 30.00m, true)]
        );
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", addRequest);

        var request = CreateValidRequest(priceOverrides: null);

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var item = result!.Categories.First().Items.First();
        item.PriceOverrides.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateItem_ShouldPreserveMenuItem()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta con Item",
            categoryName: "Entrantes",
            menuItemName: "Tortilla Española"
        );
        var categoryId = menu.Categories.First().Id;
        var originalItem = menu.Categories.First().Items.First();
        var request = CreateValidRequest(displayOrder: 99);

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{originalItem.MenuItem.Id}", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var item = result!.Categories.First().Items.First();
        item.MenuItem.Id.Should().Be(originalItem.MenuItem.Id);
        item.MenuItem.Name.Should().Be("Tortilla Española");
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task UpdateItem_WithNonExistentMenuId_Returns404()
    {
        // Arrange
        var nonExistentMenuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{nonExistentMenuId}/categories/{categoryId}/items/{menuItemId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateItem_WithNonExistentCategoryId_Returns404()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta sin Categoría");
        var nonExistentCategoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{nonExistentCategoryId}/items/{menuItemId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateItem_WithNonExistentItemId_Returns404()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Categoría",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var nonExistentItemId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{nonExistentItemId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task UpdateItem_WithNegativeDisplayOrder_Returns422()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta con Orden Negativo",
            categoryName: "Entrantes",
            menuItemName: "Croquetas"
        );
        var categoryId = menu.Categories.First().Id;
        var menuItemId = menu.Categories.First().Items.First().MenuItem.Id;
        var request = CreateValidRequest(displayOrder: -1);

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItemId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
