namespace Menus.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class UpdateMenuCategoryIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private static UpdateMenuCategory.Request CreateValidRequest(
        string name = "Categoría Actualizada",
        string? description = null,
        int displayOrder = 0)
    {
        return new UpdateMenuCategory.Request(
            Name: name,
            Description: description,
            DisplayOrder: displayOrder
        );
    }

    #region Success Tests

    [Fact]
    public async Task UpdateCategory_WithValidData_Returns204()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta Principal",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(name: "Entrantes Fríos");

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateCategory_WithValidData_ShouldPersistChanges()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta para Actualizar",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(
            name: "Entrantes Calientes",
            description: "Platos calientes para empezar",
            displayOrder: 3
        );

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var category = result!.Categories.First(c => c.Id == categoryId);
        category.Name.Should().Be("Entrantes Calientes");
        category.Description.Should().Be("Platos calientes para empezar");
        category.DisplayOrder.Should().Be(3);
    }

    [Fact]
    public async Task UpdateCategory_WithSameName_Returns204()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Mismo Nombre",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(name: "Entrantes");

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateCategory_ShouldPreserveItems()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta con Items",
            categoryName: "Entrantes",
            menuItemName: "Croquetas"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(name: "Entrantes Actualizados");

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        var category = result!.Categories.First(c => c.Id == categoryId);
        category.Name.Should().Be("Entrantes Actualizados");
        category.Items.Should().HaveCount(1);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task UpdateCategory_WithNonExistentMenuId_Returns404()
    {
        // Arrange
        var nonExistentMenuId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{nonExistentMenuId}/categories/{categoryId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_WithNonExistentCategoryId_Returns404()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta sin Categoría");
        var nonExistentCategoryId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{nonExistentCategoryId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task UpdateCategory_WithDuplicateName_Returns409()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Categorías",
            categoryName: "Entrantes"
        );
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", new AddMenuCategory.Request("Postres", null, 1));

        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var updatedMenu = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);
        var postresCategory = updatedMenu!.Categories.First(c => c.Name == "Postres");

        var request = CreateValidRequest(name: "Entrantes");

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{postresCategory.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateCategory_WithDuplicateNameCaseInsensitive_Returns409()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta Case Insensitive",
            categoryName: "ENTRANTES"
        );
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", new AddMenuCategory.Request("Postres", null, 1));

        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var updatedMenu = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);
        var postresCategory = updatedMenu!.Categories.First(c => c.Name == "Postres");

        var request = CreateValidRequest(name: "entrantes");

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{postresCategory.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task UpdateCategory_WithEmptyName_Returns422()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Nombre Vacío",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(name: "");

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateCategory_WithNegativeDisplayOrder_Returns422()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Orden Negativo",
            categoryName: "Entrantes"
        );
        var categoryId = menu.Categories.First().Id;
        var request = CreateValidRequest(displayOrder: -1);

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/categories/{categoryId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
