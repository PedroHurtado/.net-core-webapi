namespace Customer.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class AddItemToCategoryIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    private static AddItemToCategory.Request CreateValidRequest(
        Guid menuItemId,
        int displayOrder = 0,
        AddItemToCategory.PriceOptionData[]? priceOverrides = null)
    {
        return new AddItemToCategory.Request(
            MenuItemId: menuItemId,
            DisplayOrder: displayOrder,
            PriceOverrides: priceOverrides
        );
    }

    #region Success Tests

    [Fact]
    public async Task AddItem_WithValidData_Returns201()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Croquetas de Jamón");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta Principal",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(menuItem.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddItem_WithValidData_ReturnsMenuWithItem()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo a la Gallega");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta de Mariscos",
            categoryName: "Pescados"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(menuItem.Id, displayOrder: 1);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var category = result!.Categories.First();
        category.Items.Should().HaveCount(1);
        category.Items.First().MenuItem.Name.Should().Be("Pulpo a la Gallega");
        category.Items.First().DisplayOrder.Should().Be(1);
    }

    [Fact]
    public async Task AddItem_WithPriceOverrides_Returns201()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Paella Valenciana");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta de Arroces",
            categoryName: "Arroces"
        );
        var categoryId = menu.Categories.First().Id;
        var priceOverrides = new[]
        {
            new AddItemToCategory.PriceOptionData(PortionType.Full, 18.00m, true),
            new AddItemToCategory.PriceOptionData(PortionType.Half, 10.00m, true)
        };
        var request = CreateValidRequest(menuItem.Id, priceOverrides: priceOverrides);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var item = result!.Categories.First().Items.First();
        item.PriceOverrides.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddItem_WithValidData_ShouldPersistItem()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Gambas al Ajillo");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta Persistida",
            categoryName: "Mariscos"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(menuItem.Id);

        // Act
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var category = result!.Categories.First();
        category.Items.Should().Contain(i => i.MenuItem.Id == menuItem.Id);
    }

    [Fact]
    public async Task AddItem_AddingMultipleItems_ShouldPersistAll()
    {
        // Arrange
        var menuItem1 = await CreateMenuItemAsync(name: "Croquetas");
        var menuItem2 = await CreateMenuItemAsync(name: "Jamón Ibérico");
        var menuItem3 = await CreateMenuItemAsync(name: "Queso Manchego");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta Completa",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;

        // Act
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", CreateValidRequest(menuItem1.Id));
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", CreateValidRequest(menuItem2.Id));
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", CreateValidRequest(menuItem3.Id));

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Categories.First().Items.Should().HaveCount(3);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task AddItem_WithNonExistentMenuId_Returns404()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Item Huérfano");
        var nonExistentMenuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = CreateValidRequest(menuItem.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{nonExistentMenuId}/categories/{categoryId}/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItem_WithNonExistentCategoryId_Returns404()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Item sin Categoría");
        var menu = await CreateMenuAsync(name: "Carta sin Categoría");
        var nonExistentCategoryId = Guid.NewGuid();
        var request = CreateValidRequest(menuItem.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{nonExistentCategoryId}/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItem_WithNonExistentMenuItemId_Returns404()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Categoría",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var nonExistentMenuItemId = Guid.NewGuid();
        var request = CreateValidRequest(nonExistentMenuItemId);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task AddItem_WithDuplicateItem_Returns409()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Croquetas Duplicadas");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Duplicado",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;

        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", CreateValidRequest(menuItem.Id));

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", CreateValidRequest(menuItem.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task AddItem_WithNegativeDisplayOrder_Returns422()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Item con Orden Negativo");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Orden Inválido",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(menuItem.Id, displayOrder: -1);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
