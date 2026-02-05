namespace Menus.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class ActivateMenuIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task Activate_WithValidMenu_Returns200()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta para Activar",
            categoryName: "Entrantes",
            menuItemName: "Croquetas"
        );

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Activate_WithValidMenu_ReturnsMenuWithIsActiveTrue()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta Activable",
            categoryName: "Principales",
            menuItemName: "Paella"
        );

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithValidMenu_ShouldPersistActiveState()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta Persistida",
            categoryName: "Postres",
            menuItemName: "Tarta de Queso"
        );

        // Act
        await Client.PostAsync($"/menus/{menu.Id}/activate", null);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAndItemAsync(
            menuName: "Carta Especial",
            categoryName: "Entrantes",
            menuItemName: "Jamón"
        );

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/activate", null);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Name.Should().Be("Carta Especial");
        result.Categories.Should().HaveCount(1);
        result.Categories.First().Items.Should().HaveCount(1);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Activate_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/menus/{nonExistentId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task Activate_WhenAlreadyActive_Returns409()
    {
        // Arrange
        var menu = await CreateActiveMenuAsync(
            menuName: "Carta Ya Activa",
            categoryName: "Entrantes",
            menuItemName: "Croquetas"
        );

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Activate_WithNoCategories_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta sin Categorías");

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Activate_WithEmptyCategories_Returns422()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta con Categoría Vacía",
            categoryName: "Entrantes"
        );

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
