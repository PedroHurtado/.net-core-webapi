namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class AddMenuItemAllergenIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    private static AddMenuItemAllergen.Request CreateValidRequest(string allergenId)
    {
        return new AddMenuItemAllergen.Request(AllergenId: allergenId);
    }

    #region Success Tests

    [Fact]
    public async Task AddAllergen_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Tortilla Española");
        var allergen = await CreateAllergenAsync(code: "EGGS", name: "Huevos");
        var request = CreateValidRequest(allergen.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddAllergen_WithValidData_ShouldReturnMenuItemResponse()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Tortilla Española");
        var allergen = await CreateAllergenAsync(code: "EGGS_RESP", name: "Huevos");
        var request = CreateValidRequest(allergen.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);
        result!.Id.Should().Be(menuItem.Id);
        result.Allergens.Should().ContainSingle();
        result.Allergens.Should().Contain(a => a.Id == allergen.Id);
    }

    [Fact]
    public async Task AddAllergen_WithValidData_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pan con Tomate");
        var allergen = await CreateAllergenAsync(code: "GLUTEN", name: "Gluten");
        var request = CreateValidRequest(allergen.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Allergens.Should().ContainSingle();
        result.Allergens.Should().Contain(a => a.Id == allergen.Id && a.Name == "Gluten");
    }

    [Fact]
    public async Task AddAllergen_AddingMultipleAllergens_ShouldPersistAll()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Paella Mixta");
        var glutenAllergen = await CreateAllergenAsync(code: "GLUTEN_MULTI", name: "Gluten");
        var seafoodAllergen = await CreateAllergenAsync(code: "SEAFOOD", name: "Mariscos");

        // Act
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", CreateValidRequest(glutenAllergen.Id));
        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", CreateValidRequest(seafoodAllergen.Id));

        // Assert
        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Allergens.Should().HaveCount(2);
        result.Allergens.Should().Contain(a => a.Id == glutenAllergen.Id);
        result.Allergens.Should().Contain(a => a.Id == seafoodAllergen.Id);
    }

    [Fact]
    public async Task AddAllergen_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        var allergen = await CreateAllergenAsync(code: "SHELLFISH", name: "Crustáceos");
        var request = CreateValidRequest(allergen.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    [Fact]
    public async Task AddAllergen_ShouldPreserveExistingPriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var existingOption = menuItem.PriceOptions.First();
        var allergen = await CreateAllergenAsync(code: "NUTS", name: "Frutos secos");
        var request = CreateValidRequest(allergen.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.PriceOptions.Should().Contain(p =>
            p.PortionType == existingOption.PortionType &&
            p.Price == existingOption.Price);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task AddAllergen_WithNonExistentMenuItemId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var allergen = await CreateAllergenAsync(code: "DAIRY", name: "Lácteos");
        var request = CreateValidRequest(allergen.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{nonExistentId}/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddAllergen_WithNonExistentAllergenId_ShouldReturnNotFound()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest("NON_EXISTENT_ALLERGEN");

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task AddAllergen_WithDuplicateAllergen_ShouldReturnConflict()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Ensalada César");
        var allergen = await CreateAllergenAsync(code: "DAIRY_DUP", name: "Lácteos");

        await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", CreateValidRequest(allergen.Id));

        // Act
        var response = await Client.PostAsJsonAsync($"/menu-items/{menuItem.Id}/allergens", CreateValidRequest(allergen.Id));
               
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion
}
