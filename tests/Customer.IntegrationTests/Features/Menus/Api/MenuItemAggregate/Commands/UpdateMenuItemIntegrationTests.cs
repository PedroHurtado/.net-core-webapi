namespace Customer.IntegrationTests.Features.Menus.Api.MenuItemAggregate.Commands;

public class UpdateMenuItemIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    private static UpdateMenuItem.Request CreateValidRequest(
        string name = "Updated MenuItem",
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isHighRiskItem = false,
        bool requiresAdvanceOrder = false,
        int? minimumAdvanceOrderQuantity = null,
        bool isAlwaysAvailable = true,
        DayOfWeek[]? availableDays = null,
        string? allergenNotes = null)
    {
        return new UpdateMenuItem.Request(
            Name: name,
            Description: description,
            ImageUrl: imageUrl,
            DisplayOrder: displayOrder,
            IsHighRiskItem: isHighRiskItem,
            RequiresAdvanceOrder: requiresAdvanceOrder,
            MinimumAdvanceOrderQuantity: minimumAdvanceOrderQuantity,
            IsAlwaysAvailable: isAlwaysAvailable,
            AvailableDays: availableDays ?? [],
            AllergenNotes: allergenNotes
        );
    }

    #region Success Tests

    [Fact]
    public async Task UpdateMenuItem_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Original Name");

        var request = CreateValidRequest(name: "Updated Name");

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateMenuItem_WithAllFields_ShouldUpdateAllFields()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "Original",
            description: null,
            isAlwaysAvailable: true
        );

        var request = CreateValidRequest(
            name: "Updated Dish",
            description: "Updated Description",
            imageUrl: "https://example.com/updated.jpg",
            displayOrder: 10,
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 4,
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Friday, DayOfWeek.Saturday],
            allergenNotes: "Contains allergens"
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify by getting the updated item from database
        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());

        dbMenuItem.Should().NotBeNull();
        dbMenuItem!.Name.Should().Be("Updated Dish");
        dbMenuItem.Description.Should().Be("Updated Description");
        dbMenuItem.ImageUrl.Should().Be("https://example.com/updated.jpg");
        dbMenuItem.DisplayOrder.Should().Be(10);
        dbMenuItem.IsHighRiskItem.Should().BeTrue();
        dbMenuItem.RequiresAdvanceOrder.Should().BeTrue();
        dbMenuItem.MinimumAdvanceOrderQuantity.Should().Be(4);
        dbMenuItem.IsAlwaysAvailable.Should().BeFalse();
        dbMenuItem.AvailableDays.Should().HaveCount(2);
        dbMenuItem.AllergenNotes.Should().Be("Contains allergens");
    }

    [Fact]
    public async Task UpdateMenuItem_WithValidData_ShouldPersistInDatabase()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(name: "Before Update");

        var request = CreateValidRequest(
            name: "After Update",
            description: "New description",
            displayOrder: 5
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());

        dbMenuItem.Should().NotBeNull();
        dbMenuItem!.Name.Should().Be("After Update");
        dbMenuItem.Description.Should().Be("New description");
        dbMenuItem.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task UpdateMenuItem_RemovingOptionalFields_ShouldSetToNull()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            name: "With Optional Fields",
            description: "Original description",
            imageUrl: "https://example.com/original.jpg",
            allergenNotes: "Original notes"
        );

        var request = CreateValidRequest(
            name: "Without Optional Fields",
            description: null,
            imageUrl: null,
            allergenNotes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());

        dbMenuItem!.Description.Should().BeNull();
        dbMenuItem.ImageUrl.Should().BeNull();
        dbMenuItem.AllergenNotes.Should().BeNull();
    }

    [Fact]
    public async Task UpdateMenuItem_ChangingFromAlwaysAvailableToSpecificDays_ShouldUpdateCorrectly()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(isAlwaysAvailable: true);

        var request = CreateValidRequest(
            name: menuItem.Name,
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday]
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());

        dbMenuItem!.IsAlwaysAvailable.Should().BeFalse();
        dbMenuItem.AvailableDays.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateMenuItem_ChangingFromSpecificDaysToAlwaysAvailable_ShouldClearDays()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            isAlwaysAvailable: false,
            availableDays: [DayOfWeek.Saturday, DayOfWeek.Sunday]
        );

        var request = CreateValidRequest(
            name: menuItem.Name,
            isAlwaysAvailable: true,
            availableDays: []
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());

        dbMenuItem!.IsAlwaysAvailable.Should().BeTrue();
        dbMenuItem.AvailableDays.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateMenuItem_ActivatingHighRiskAndAdvanceOrder_ShouldUpdateCorrectly()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            isHighRiskItem: false,
            requiresAdvanceOrder: false
        );

        var request = CreateValidRequest(
            name: menuItem.Name,
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 4
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());

        dbMenuItem!.IsHighRiskItem.Should().BeTrue();
        dbMenuItem.RequiresAdvanceOrder.Should().BeTrue();
        dbMenuItem.MinimumAdvanceOrderQuantity.Should().Be(4);
    }

    [Fact]
    public async Task UpdateMenuItem_PreservesIsActiveAndIsAvailable()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(name: "Updated Name");

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());

        dbMenuItem!.IsActive.Should().Be(menuItem.IsActive);
        dbMenuItem.IsAvailable.Should().Be(menuItem.IsAvailable);
    }

    [Fact]
    public async Task UpdateMenuItem_PreservesPriceOptions()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(
            priceOptions: [
                new(PortionType.Small, 5.00m),
                new(PortionType.Full, 10.00m)
            ]
        );

        var request = CreateValidRequest(name: "Updated Name");

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());

        dbMenuItem!.PriceOptions.Should().HaveCount(2);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task UpdateMenuItem_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdateMenuItem_WithInvalidName_ShouldReturnUnprocessableEntity(string invalidName)
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(name: invalidName);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateMenuItem_WithNameExceeding100Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(name: new string('a', 101));

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateMenuItem_WithDescriptionExceeding1000Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(description: new string('a', 1001));

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateMenuItem_WithInvalidImageUrl_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(imageUrl: "not-a-url");

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateMenuItem_WithNegativeDisplayOrder_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(displayOrder: -1);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateMenuItem_WithRequiresAdvanceOrderButNotHighRisk_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(
            isHighRiskItem: false,
            requiresAdvanceOrder: true
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateMenuItem_WithNotAlwaysAvailableWithoutDays_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(
            isAlwaysAvailable: false,
            availableDays: []
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task UpdateMenuItem_WithInvalidMinimumAdvanceOrderQuantity_ShouldReturnUnprocessableEntity(int quantity)
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: quantity
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateMenuItem_WithAllergenNotesExceeding500Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(allergenNotes: new string('a', 501));

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task UpdateMenuItem_WithNameExactly100Characters_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var name = new string('a', 100);

        var request = CreateValidRequest(name: name);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());
        dbMenuItem!.Name.Should().Be(name);
    }

    [Fact]
    public async Task UpdateMenuItem_WithDescriptionExactly1000Characters_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();
        var description = new string('a', 1000);

        var request = CreateValidRequest(description: description);

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());
        dbMenuItem!.Description.Should().Be(description);
    }

    [Fact]
    public async Task UpdateMenuItem_WithDisplayOrderZero_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync(displayOrder: 5);

        var request = CreateValidRequest(
            name: menuItem.Name,
            displayOrder: 0
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbMenuItem = await ExecuteWithDbContext(db => db.Set<MenuItem>().FindAsync(menuItem.Id).AsTask());
        dbMenuItem!.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public async Task UpdateMenuItem_WithMinimumQuantityAtLowerBound_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 1
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateMenuItem_WithMinimumQuantityAtUpperBound_ShouldReturnNoContent()
    {
        // Arrange
        var menuItem = await CreateMenuItemAsync();

        var request = CreateValidRequest(
            isHighRiskItem: true,
            requiresAdvanceOrder: true,
            minimumAdvanceOrderQuantity: 100
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menu-items/{menuItem.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}
