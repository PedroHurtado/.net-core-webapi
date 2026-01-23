namespace Customer.UnitTests.Features.Menus.Api.Queries.Allergens;

public class GetAllergenServiceTests
{
    private readonly AllergenValidator _validator = new();
    private readonly AllergenAgg.Create _createAllergen;
    private readonly Mock<GetAllergen.IRepository> _repositoryMock;
    private readonly GetAllergen.Service _service;

    public GetAllergenServiceTests()
    {
        _createAllergen = new(_validator);
        _repositoryMock = new Mock<GetAllergen.IRepository>();
        _service = new GetAllergen.Service(_repositoryMock.Object);
    }

    private AllergenAgg CreateAllergen(
        string code,
        string name,
        string? iconUrl = null,
        bool isActive = true,
        int displayOrder = 0)
    {
        return _createAllergen.Execute(new CreateAllergenCommand(code, name, iconUrl, isActive, displayOrder));
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingId_ReturnsResponse()
    {
        var allergen = CreateAllergen("GLUTEN", "Gluten");
        _repositoryMock.Setup(r => r.Get("GLUTEN")).ReturnsAsync(allergen);

        var response = await _service.HandleAsync("GLUTEN");

        response.Should().NotBeNull();
        response.Id.Should().Be("GLUTEN");
        response.Name.Should().Be("Gluten");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var allergen = CreateAllergen(
            "LACTEOS",
            "Lácteos",
            "https://example.com/lacteos.png",
            false,
            5
        );
        _repositoryMock.Setup(r => r.Get("LACTEOS")).ReturnsAsync(allergen);

        var response = await _service.HandleAsync("LACTEOS");

        response.Id.Should().Be("LACTEOS");
        response.Name.Should().Be("Lácteos");
        response.IconUrl.Should().Be("https://example.com/lacteos.png");
        response.IsActive.Should().BeFalse();
        response.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_WithNullIconUrl_ReturnsNullIconUrl()
    {
        var allergen = CreateAllergen("GLUTEN", "Gluten", iconUrl: null);
        _repositoryMock.Setup(r => r.Get("GLUTEN")).ReturnsAsync(allergen);

        var response = await _service.HandleAsync("GLUTEN");

        response.IconUrl.Should().BeNull();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var allergen = CreateAllergen("GLUTEN", "Gluten");
        _repositoryMock.Setup(r => r.Get("GLUTEN")).ReturnsAsync(allergen);

        await _service.HandleAsync("GLUTEN");

        _repositoryMock.Verify(r => r.Get("GLUTEN"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PassesCorrectIdToRepository()
    {
        var allergen = CreateAllergen("NUECES", "Nueces");
        _repositoryMock.Setup(r => r.Get("NUECES")).ReturnsAsync(allergen);

        await _service.HandleAsync("NUECES");

        _repositoryMock.Verify(r => r.Get("NUECES"), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WhenAllergenNotFound_ThrowsException()
    {
        _repositoryMock.Setup(r => r.Get("NONEXISTENT"))
            .ThrowsAsync(new KeyNotFoundException("Allergen not found"));

        var act = () => _service.HandleAsync("NONEXISTENT");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_PropagatesException()
    {
        _repositoryMock.Setup(r => r.Get(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var act = () => _service.HandleAsync("GLUTEN");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion
}
