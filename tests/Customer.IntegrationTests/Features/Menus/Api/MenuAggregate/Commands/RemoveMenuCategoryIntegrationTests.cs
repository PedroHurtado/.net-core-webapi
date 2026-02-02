namespace Customer.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveMenuCategoryIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task RemoveCategory_WithEmptyCategory_Returns204()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Categoría Vacía",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;

        // Act
        var response = await Client.DeleteAsync($"/menus/{menu.Id}/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveCategory_WithEmptyCategory_ShouldRemoveCategory()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta para Eliminar Categoría",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;

        // Act
        await Client.DeleteAsync($"/menus/{menu.Id}/categories/{categoryId}");

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveCategory_ShouldPreserveOtherCategories()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Múltiples Categorías",
            categoryName: "Entrantes"
        );
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", new AddMenuCategory.Request("Postres", null, 1));

        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var updatedMenu = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);
        var entrantesCategory = updatedMenu!.Categories.First(c => c.Name == "Entrantes");

        // Act
        await Client.DeleteAsync($"/menus/{menu.Id}/categories/{entrantesCategory.Id}");

        // Assert
        var finalResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await finalResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Categories.Should().HaveCount(1);
        result.Categories.First().Name.Should().Be("Postres");
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task RemoveCategory_WithNonExistentMenuId_Returns404()
    {
        // Arrange
        var nonExistentMenuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menus/{nonExistentMenuId}/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveCategory_WithNonExistentCategoryId_Returns404()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta sin Categoría");
        var nonExistentCategoryId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menus/{menu.Id}/categories/{nonExistentCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task RemoveCategory_WithCategoryContainingItems_Returns422()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta con Items",
            categoryName: "Entrantes",
            menuItemName: "Croquetas"
        );
        var categoryId = menu.Categories.First().Id;

        // Act
        var response = await Client.DeleteAsync($"/menus/{menu.Id}/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
