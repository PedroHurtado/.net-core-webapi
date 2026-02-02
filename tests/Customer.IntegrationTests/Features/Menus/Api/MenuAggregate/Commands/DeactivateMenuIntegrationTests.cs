namespace Customer.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class DeactivateMenuIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task Deactivate_WithActiveMenu_Returns200()
    {
        // Arrange
        var menu = await CreateActiveMenuAsync(
            menuName: "Carta Activa",
            categoryName: "Entrantes",
            menuItemName: "Croquetas"
        );

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Deactivate_WithActiveMenu_ReturnsMenuWithIsActiveFalse()
    {
        // Arrange
        var menu = await CreateActiveMenuAsync(
            menuName: "Carta para Desactivar",
            categoryName: "Principales",
            menuItemName: "Paella"
        );

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithActiveMenu_ShouldPersistInactiveState()
    {
        // Arrange
        var menu = await CreateActiveMenuAsync(
            menuName: "Carta Persistida",
            categoryName: "Postres",
            menuItemName: "Tarta"
        );

        // Act
        await Client.PostAsync($"/menus/{menu.Id}/deactivate", null);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menu = await CreateActiveMenuAsync(
            menuName: "Carta Especial Activa",
            categoryName: "Entrantes",
            menuItemName: "Jamón"
        );

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/deactivate", null);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Name.Should().Be("Carta Especial Activa");
        result.Categories.Should().HaveCount(1);
        result.Categories.First().Items.Should().HaveCount(1);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Deactivate_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/menus/{nonExistentId}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task Deactivate_WhenAlreadyInactive_Returns409()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta Inactiva");

        // Act
        var response = await Client.PostAsync($"/menus/{menu.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion
}
