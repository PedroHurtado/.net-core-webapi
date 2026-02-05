namespace Menus.IntegrationTests.Features.Menus.Api.AllergenAggregate.Commands;

public class UpdateAllergenIntegrationTests(WebApplicationFactory<Program> factory)
    : MenusWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task UpdateAllergen_WithValidData_ShouldReturnOkAndUpdatedAllergen()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(name: "Original Name");

        var request = new UpdateAllergen.Request(
            Name: "Updated Name",
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateAllergen.Response>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(allergen.Id);
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAllergen_WithAllFields_ShouldReturnCompleteResponse()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(
            name: "Original",
            iconUrl: null,
            isActive: true,
            displayOrder: 0
        );

        var request = new UpdateAllergen.Request(
            Name: "Updated Allergen",
            IconUrl: "https://example.com/updated-icon.png",
            IsActive: false,
            DisplayOrder: 10
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateAllergen.Response>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(allergen.Id);
        result.Name.Should().Be("Updated Allergen");
        result.IconUrl.Should().Be("https://example.com/updated-icon.png");
        result.IsActive.Should().BeFalse();
        result.DisplayOrder.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAllergen_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(name: "Before Update");

        var request = new UpdateAllergen.Request(
            Name: "After Update",
            IconUrl: "https://example.com/icon.png",
            IsActive: false,
            DisplayOrder: 5
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbAllergen = await ExecuteWithDbContext(db => db.Allergens.FindAsync(allergen.Id).AsTask());

        dbAllergen.Should().NotBeNull();
        dbAllergen!.Name.Should().Be("After Update");
        dbAllergen.IconUrl.Should().Be("https://example.com/icon.png");
        dbAllergen.IsActive.Should().BeFalse();
        dbAllergen.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAllergen_RemovingIconUrl_ShouldSetToNull()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(
            name: "With Icon",
            iconUrl: "https://example.com/original.png"
        );

        var request = new UpdateAllergen.Request(
            Name: "Without Icon",
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateAllergen.Response>();
        result!.IconUrl.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAllergen_TogglingIsActive_ShouldUpdateCorrectly()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(isActive: true);

        var request = new UpdateAllergen.Request(
            Name: allergen.Name,
            IconUrl: allergen.IconUrl,
            IsActive: false,
            DisplayOrder: allergen.DisplayOrder
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateAllergen.Response>();
        result!.IsActive.Should().BeFalse();
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task UpdateAllergen_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = "NONEXISTENT_" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        var request = new UpdateAllergen.Request(
            Name: "Some Name",
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdateAllergen_WithInvalidName_ShouldReturnUnprocessableEntity(string invalidName)
    {
        // Arrange
        var allergen = await CreateAllergenAsync();

        var request = new UpdateAllergen.Request(
            Name: invalidName,
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateAllergen_WithNameExceeding100Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var allergen = await CreateAllergenAsync();

        var request = new UpdateAllergen.Request(
            Name: new string('a', 101),
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateAllergen_WithInvalidIconUrl_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var allergen = await CreateAllergenAsync();

        var request = new UpdateAllergen.Request(
            Name: "Valid Name",
            IconUrl: "not-a-url",
            IsActive: true,
            DisplayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateAllergen_WithNegativeDisplayOrder_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var allergen = await CreateAllergenAsync();

        var request = new UpdateAllergen.Request(
            Name: "Valid Name",
            IconUrl: null,
            IsActive: true,
            DisplayOrder: -1
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task UpdateAllergen_WithNameExactly100Characters_ShouldReturnOk()
    {
        // Arrange
        var allergen = await CreateAllergenAsync();
        var name = new string('a', 100);

        var request = new UpdateAllergen.Request(
            Name: name,
            IconUrl: null,
            IsActive: true,
            DisplayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateAllergen.Response>();
        result!.Name.Should().Be(name);
    }

    [Fact]
    public async Task UpdateAllergen_WithDisplayOrderZero_ShouldReturnOk()
    {
        // Arrange
        var allergen = await CreateAllergenAsync(displayOrder: 5);

        var request = new UpdateAllergen.Request(
            Name: allergen.Name,
            IconUrl: allergen.IconUrl,
            IsActive: allergen.IsActive,
            DisplayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/allergens/{allergen.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateAllergen.Response>();
        result!.DisplayOrder.Should().Be(0);
    }

    #endregion
}
