namespace Menus.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class AddMenuCategoryIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    private static AddMenuCategory.Request CreateValidRequest(
        string name = "Entrantes",
        string? description = null,
        int displayOrder = 0)
    {
        return new AddMenuCategory.Request(
            Name: name,
            Description: description,
            DisplayOrder: displayOrder
        );
    }

    #region Success Tests

    [Fact]
    public async Task AddCategory_WithValidData_Returns201()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Principal");
        var request = CreateValidRequest(name: "Entrantes");

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddCategory_WithValidData_ReturnsMenuWithCategory()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Categoría");
        var request = CreateValidRequest(
            name: "Pescados y Mariscos",
            description: "Productos frescos del día"
        );

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Categories.Should().HaveCount(1);
        var category = result.Categories.First();
        category.Name.Should().Be("Pescados y Mariscos");
        category.Description.Should().Be("Productos frescos del día");
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AddCategory_WithDisplayOrder_ShouldPersistDisplayOrder()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Orden");
        var request = CreateValidRequest(name: "Postres", displayOrder: 5);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", request);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);
        result!.Categories.First().DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task AddCategory_AddingMultipleCategories_ShouldPersistAll()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Completa");

        // Act
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", CreateValidRequest(name: "Entrantes"));
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", CreateValidRequest(name: "Principales"));
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", CreateValidRequest(name: "Postres"));

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Categories.Should().HaveCount(3);
        result.Categories.Select(c => c.Name).Should().Contain(["Entrantes", "Principales", "Postres"]);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task AddCategory_WithNonExistentMenuId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{nonExistentId}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task AddCategory_WithDuplicateName_Returns409()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Duplicado");
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", CreateValidRequest(name: "Entrantes"));

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", CreateValidRequest(name: "Entrantes"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddCategory_WithDuplicateNameCaseInsensitive_Returns409()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Case Insensitive");
        await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", CreateValidRequest(name: "ENTRANTES"));

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", CreateValidRequest(name: "entrantes"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task AddCategory_WithEmptyName_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Nombre Vacío");
        var request = CreateValidRequest(name: "");

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddCategory_WithNegativeDisplayOrder_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Orden Negativo");
        var request = CreateValidRequest(name: "Entrantes", displayOrder: -1);

        // Act
        var response = await Client.PostAsJsonAsync($"/menus/{menu.Id}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
