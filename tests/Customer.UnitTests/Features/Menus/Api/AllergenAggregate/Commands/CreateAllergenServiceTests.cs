namespace Customer.UnitTests.Features.Menus.Api.AllergenAggregate.Commands;

public class CreateAllergenServiceTests
{
    private readonly AllergenValidator _validator = new();
    private readonly AllergenAgg.Create _createAllergen;
    private readonly Mock<CreateAllergen.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateAllergen.Service _service;

    public CreateAllergenServiceTests()
    {
        _createAllergen = new(_validator);
        _repositoryMock = new Mock<CreateAllergen.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new CreateAllergen.Service(_createAllergen, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var request = new CreateAllergen.Request("GLUTEN", "Gluten");

        var response = await _service.HandleAsync(request);

        response.Should().NotBeNull();
        response.Id.Should().Be("GLUTEN");
        response.Name.Should().Be("Gluten");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var request = new CreateAllergen.Request(
            Code: "LACTEOS",
            Name: "Lácteos",
            IconUrl: "https://example.com/lacteos.png",
            IsActive: false,
            DisplayOrder: 5
        );

        var response = await _service.HandleAsync(request);

        response.Id.Should().Be("LACTEOS");
        response.Name.Should().Be("Lácteos");
        response.IconUrl.Should().Be("https://example.com/lacteos.png");
        response.IsActive.Should().BeFalse();
        response.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_WithDefaultValues_AppliesDefaults()
    {
        var request = new CreateAllergen.Request("GLUTEN", "Gluten");

        var response = await _service.HandleAsync(request);

        response.IconUrl.Should().BeNull();
        response.IsActive.Should().BeTrue();
        response.DisplayOrder.Should().Be(0);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryAdd()
    {
        var request = new CreateAllergen.Request("GLUTEN", "Gluten");

        await _service.HandleAsync(request);

        _repositoryMock.Verify(r => r.Add(It.Is<AllergenAgg>(a => a.Id == "GLUTEN")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var request = new CreateAllergen.Request("GLUTEN", "Gluten");

        await _service.HandleAsync(request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AddsAllergenBeforeSaving()
    {
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Add(It.IsAny<AllergenAgg>())).Callback(() => callOrder.Add("Add"));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        var request = new CreateAllergen.Request("GLUTEN", "Gluten");

        await _service.HandleAsync(request);

        callOrder.Should().ContainInOrder("Add", "SaveChanges");
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyCode_ThrowsValidationException(string? code)
    {
        var request = new CreateAllergen.Request(code!, "Gluten");

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("gluten")]
    [InlineData("Gluten")]
    [InlineData("GLUTEN-1")]
    public async Task HandleAsync_WithInvalidCodeFormat_ThrowsValidationException(string code)
    {
        var request = new CreateAllergen.Request(code, "Gluten");

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string? name)
    {
        var request = new CreateAllergen.Request("GLUTEN", name!);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidIconUrl_ThrowsValidationException()
    {
        var request = new CreateAllergen.Request("GLUTEN", "Gluten", IconUrl: "not-a-url");

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var request = new CreateAllergen.Request("GLUTEN", "Gluten", DisplayOrder: -1);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallRepository()
    {
        var request = new CreateAllergen.Request("", "Gluten");

        try { await _service.HandleAsync(request); } catch { }

        _repositoryMock.Verify(r => r.Add(It.IsAny<AllergenAgg>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var request = new CreateAllergen.Request("", "Gluten");

        try { await _service.HandleAsync(request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
