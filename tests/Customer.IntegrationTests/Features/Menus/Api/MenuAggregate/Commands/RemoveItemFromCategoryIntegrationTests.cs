namespace Customer.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveItemFromCategoryIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task RemoveItem_WithExistingItem_Returns204()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta con Item",
            categoryName: "Entrantes",
            menuItemName: "Croquetas"
        );
        var categoryId = menu.Categories.First().Id;
        var menuItemId = menu.Categories.First().Items.First().MenuItem.Id;

        // Act
        var response = await Client.DeleteAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveItem_WithExistingItem_ShouldRemoveItem()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta para Eliminar Item",
            categoryName: "Entrantes",
            menuItemName: "Jamón Ibérico"
        );
        var categoryId = menu.Categories.First().Id;
        var menuItemId = menu.Categories.First().Items.First().MenuItem.Id;

        // Act
        await Client.DeleteAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItemId}");

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Categories.First().Items.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveItem_ShouldPreserveOtherItems()
    {
        // Arrange
        var menuItem1 = await CreateMenuItemAsync(name: "Croquetas");
        var menuItem2 = await CreateMenuItemAsync(name: "Jamón");
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Múltiples Items",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;

        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items",
            new AddItemToCategory.Request(menuItem1.Id, 0, null));
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}/items",
            new AddItemToCategory.Request(menuItem2.Id, 1, null));

        // Act
        await Client.DeleteAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItem1.Id}");

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Categories.First().Items.Should().HaveCount(1);
        result.Categories.First().Items.First().MenuItem.Id.Should().Be(menuItem2.Id);
    }

    [Fact]
    public async Task RemoveItem_ShouldPreserveCategoryProperties()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta Especial",
            categoryName: "Entrantes Especiales",
            menuItemName: "Caviar"
        );
        var categoryId = menu.Categories.First().Id;
        var menuItemId = menu.Categories.First().Items.First().MenuItem.Id;

        // Act
        await Client.DeleteAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{menuItemId}");

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var category = result!.Categories.First();
        category.Id.Should().Be(categoryId);
        category.Name.Should().Be("Entrantes Especiales");
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task RemoveItem_WithNonExistentMenuId_Returns404()
    {
        // Arrange
        var nonExistentMenuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menus/{nonExistentMenuId}/categories/{categoryId}/items/{menuItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItem_WithNonExistentCategoryId_Returns404()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta sin Categoría");
        var nonExistentCategoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menus/{menu.Id}/categories/{nonExistentCategoryId}/items/{menuItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItem_WithNonExistentItemId_Returns404()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Categoría",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var nonExistentItemId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menus/{menu.Id}/categories/{categoryId}/items/{nonExistentItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
