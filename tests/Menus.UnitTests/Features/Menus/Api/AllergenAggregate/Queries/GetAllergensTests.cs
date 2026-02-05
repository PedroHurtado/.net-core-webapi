namespace Menus.UnitTests.Features.Menus.Api.AllergenAggregate.Queries;

public class GetAllergensTests
{
    private readonly AllergenValidator _validator = new();
    private readonly AllergenAgg.Create _createAllergen;
    private readonly Mock<IQuery> _queryMock;
    private readonly GetAllergens.Service _service;

    public GetAllergensTests()
    {
        _createAllergen = new(_validator);
        _queryMock = new Mock<IQuery>();
        _service = new GetAllergens.Service(_queryMock.Object);
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
    public async Task HandleAsync_WithAllergens_ReturnsAllAllergens()
    {
        var allergens = new List<AllergenAgg>
        {
            CreateAllergen("GLUTEN", "Gluten"),
            CreateAllergen("LACTEOS", "Lácteos")
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<AllergenAgg>()).Returns(allergens);

        var response = await _service.HandleAsync();

        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WithNoAllergens_ReturnsEmptyList()
    {
        var allergens = new List<AllergenAgg>().AsAsyncQueryable();
        _queryMock.Setup(q => q.Query<AllergenAgg>()).Returns(allergens);

        var response = await _service.HandleAsync();

        response.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsAllFields()
    {
        var allergens = new List<AllergenAgg>
        {
            CreateAllergen("GLUTEN", "Gluten", "https://example.com/gluten.png", false, 5)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<AllergenAgg>()).Returns(allergens);

        var response = await _service.HandleAsync();

        response.Should().HaveCount(1);
        var allergen = response.First();
        allergen.Id.Should().Be("GLUTEN");
        allergen.Name.Should().Be("Gluten");
        allergen.IconUrl.Should().Be("https://example.com/gluten.png");
        allergen.IsActive.Should().BeFalse();
        allergen.DisplayOrder.Should().Be(5);
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task HandleAsync_OrdersByDisplayOrderThenByName()
    {
        var allergens = new List<AllergenAgg>
        {
            CreateAllergen("NUECES", "Nueces", displayOrder: 2),
            CreateAllergen("GLUTEN", "Gluten", displayOrder: 1),
            CreateAllergen("APIO", "Apio", displayOrder: 1),
            CreateAllergen("LACTEOS", "Lácteos", displayOrder: 0)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<AllergenAgg>()).Returns(allergens);

        var response = await _service.HandleAsync();

        response.Should().HaveCount(4);
        response[0].Name.Should().Be("Lácteos");
        response[1].Name.Should().Be("Apio");
        response[2].Name.Should().Be("Gluten");
        response[3].Name.Should().Be("Nueces");
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryQuery()
    {
        var allergens = new List<AllergenAgg>().AsAsyncQueryable();
        _queryMock.Setup(q => q.Query<AllergenAgg>()).Returns(allergens);

        await _service.HandleAsync();

        _queryMock.Verify(q => q.Query<AllergenAgg>(), Times.Once);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_ReturnsOkResult()
    {
        var serviceMock = new Mock<GetAllergens.IService>();
        var expectedResponse = new List<GetAllergens.Response>
        {
            new("GLUTEN", "Gluten", null, true, 0),
            new("LACTEOS", "Lácteos", null, true, 1)
        };

        serviceMock.Setup(s => s.HandleAsync()).ReturnsAsync(expectedResponse);

        var result = await GetAllergens.Handler(serviceMock.Object);

        result.Should().BeOfType<Ok<List<GetAllergens.Response>>>();
        var okResult = (Ok<List<GetAllergens.Response>>)result;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsService()
    {
        var serviceMock = new Mock<GetAllergens.IService>();
        var expectedResponse = new List<GetAllergens.Response>();

        serviceMock.Setup(s => s.HandleAsync()).ReturnsAsync(expectedResponse);

        await GetAllergens.Handler(serviceMock.Object);

        serviceMock.Verify(s => s.HandleAsync(), Times.Once);
    }

    #endregion
}
