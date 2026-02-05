namespace Menus.IntegrationTests.Features.Menus.Api.MenuAggregate.Queries;

public class GetMenuIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task Get_WithExistingId_Returns200()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Principal");

        // Act
        var response = await Client.GetAsync($"/menus/{menu.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsMenuResponse()
    {
        // Arrange
        var menu = await CreateMenuAsync(
            name: "Carta de Postres",
            description: "Deliciosos postres caseros"
        );

        // Act
        var response = await Client.GetAsync($"/menus/{menu.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Id.Should().Be(menu.Id);
        result.Name.Should().Be("Carta de Postres");
        result.Description.Should().Be("Deliciosos postres caseros");
    }

    [Fact]
    public async Task Get_WithMenuContainingCategories_ReturnsCategoriesInResponse()
    {
        // Arrange
        var menu = await CreateMenuWithCategoryAsync(
            menuName: "Carta Completa",
            categoryName: "Entrantes"
        );

        // Act
        var response = await Client.GetAsync($"/menus/{menu.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Categories.Should().HaveCount(1);
        result.Categories.First().Name.Should().Be("Entrantes");
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Get_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/menus/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
