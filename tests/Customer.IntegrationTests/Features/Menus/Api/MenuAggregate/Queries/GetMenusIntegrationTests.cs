namespace Customer.IntegrationTests.Features.Menus.Api.MenuAggregate.Queries;

public class GetMenusIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task List_WithNoFilter_Returns200()
    {
        // Arrange
        await CreateMenuAsync(name: "Carta 1");
        await CreateMenuAsync(name: "Carta 2");

        // Act
        var response = await Client.GetAsync("/menus");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<MenuResponse>>(JsonOptions);
        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task List_WithIsActiveTrue_ReturnsActiveMenusOnly()
    {
        // Arrange
        var activeMenu = await CreateActiveMenuAsync(menuName: "Menú Activo");
        await CreateMenuAsync(name: "Menú Inactivo");

        // Act
        var response = await Client.GetAsync("/menus?isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<MenuResponse>>(JsonOptions);

        result.Should().Contain(m => m.Id == activeMenu.Id);
        result.Should().OnlyContain(m => m.IsActive);
    }

    [Fact]
    public async Task List_WithIsActiveFalse_ReturnsInactiveMenusOnly()
    {
        // Arrange
        await CreateActiveMenuAsync(menuName: "Menú Activo Filter");
        var inactiveMenu = await CreateMenuAsync(name: "Menú Inactivo Filter");

        // Act
        var response = await Client.GetAsync("/menus?isActive=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<MenuResponse>>(JsonOptions);

        result.Should().Contain(m => m.Id == inactiveMenu.Id);
        result.Should().OnlyContain(m => !m.IsActive);
    }

    [Fact]
    public async Task List_ReturnsMenusOrderedByDisplayOrderThenName()
    {
        // Arrange
        var menuC = await CreateMenuAsync(name: "C Menu");
        var menuA = await CreateMenuAsync(name: "A Menu");
        var menuB = await CreateMenuAsync(name: "B Menu");

        // Act
        var response = await Client.GetAsync("/menus");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<MenuResponse>>(JsonOptions);

        var menuIds = result!.Select(m => m.Id).ToList();
        var indexA = menuIds.IndexOf(menuA.Id);
        var indexB = menuIds.IndexOf(menuB.Id);
        var indexC = menuIds.IndexOf(menuC.Id);

        indexA.Should().BeLessThan(indexB);
        indexB.Should().BeLessThan(indexC);
    }

    #endregion
}
