namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class SetMenuItemNutritionalInfoIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    private static SetMenuItemNutritionalInfo.Request CreateValidRequest(
        int calories = 500,
        decimal protein = 25.0m,
        decimal carbohydrates = 40.0m,
        decimal fat = 15.0m,
        int servingSize = 350,
        decimal? fiber = null,
        decimal? sugar = null,
        decimal? salt = null)
    {
        return new SetMenuItemNutritionalInfo.Request(
            Calories: calories,
            Protein: protein,
            Carbohydrates: carbohydrates,
            Fat: fat,
            ServingSize: servingSize,
            Fiber: fiber,
            Sugar: sugar,
            Salt: salt
        );
    }

    #region Success Tests

    [Fact]
    public async Task SetNutritionalInfo_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Pulpo al Horno");
        var request = CreateValidRequest(
            calories: 500,
            protein: 25.0m,
            carbohydrates: 40.0m,
            fat: 15.0m,
            servingSize: 350
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithValidData_ShouldPersistChanges()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Jamón Ibérico");
        var request = CreateValidRequest(
            calories: 250,
            protein: 30.0m,
            carbohydrates: 0.5m,
            fat: 16.0m,
            servingSize: 100,
            fiber: 0m,
            sugar: 0m,
            salt: 4.5m
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.NutritionalInfo.Should().NotBeNull();
        result.NutritionalInfo!.Calories.Should().Be(250);
        result.NutritionalInfo.Protein.Should().Be(30.0m);
        result.NutritionalInfo.Carbohydrates.Should().Be(0.5m);
        result.NutritionalInfo.Fat.Should().Be(16.0m);
        result.NutritionalInfo.ServingSize.Should().Be(100);
        result.NutritionalInfo.Salt.Should().Be(4.5m);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithoutOptionalFields_ShouldLeaveFieldsNull()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(
            fiber: null,
            sugar: null,
            salt: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.NutritionalInfo!.Fiber.Should().BeNull();
        result.NutritionalInfo.Sugar.Should().BeNull();
        result.NutritionalInfo.Salt.Should().BeNull();
    }

    [Fact]
    public async Task SetNutritionalInfo_ShouldPreserveOtherProperties()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Gambas al Ajillo",
            description: "Gambas con ajo y guindilla"
        );
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.Name.Should().Be("Gambas al Ajillo");
        result.Description.Should().Be("Gambas con ajo y guindilla");
    }

    [Fact]
    public async Task SetNutritionalInfo_ShouldReplaceExistingNutritionalInfo()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var firstRequest = CreateValidRequest(
            calories: 300,
            protein: 15.0m,
            carbohydrates: 30.0m,
            fat: 10.0m,
            servingSize: 250
        );
        await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", firstRequest);

        var secondRequest = CreateValidRequest(
            calories: 600,
            protein: 35.0m,
            carbohydrates: 50.0m,
            fat: 20.0m,
            servingSize: 400
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menu-items/{menuItem.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuItemResponse>(JsonOptions);

        result!.NutritionalInfo!.Calories.Should().Be(600);
        result.NutritionalInfo.Protein.Should().Be(35.0m);
        result.NutritionalInfo.Carbohydrates.Should().Be(50.0m);
        result.NutritionalInfo.Fat.Should().Be(20.0m);
        result.NutritionalInfo.ServingSize.Should().Be(400);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task SetNutritionalInfo_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{nonExistentId}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task SetNutritionalInfo_WithNegativeCalories_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(calories: -1);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithCaloriesExceedingMax_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(calories: 10001);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithZeroServingSize_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(servingSize: 0);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithServingSizeExceedingMax_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(servingSize: 10001);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithNegativeProtein_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(protein: -1);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithProteinExceedingMax_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(protein: 1001);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithNegativeSalt_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(salt: -1);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetNutritionalInfo_WithSaltExceedingMax_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var request = CreateValidRequest(salt: 101);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}/nutritional-info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
