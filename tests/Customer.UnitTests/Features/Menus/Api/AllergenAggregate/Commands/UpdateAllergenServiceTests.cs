namespace Customer.UnitTests.Features.Menus.Api.AllergenAggregate.Commands;

public class UpdateAllergenServiceTests
{
    private readonly AllergenValidator _validator = new();
    private readonly AllergenAgg.Create _createAllergen;
    private readonly AllergenAgg.Update _updateAllergen;
    private readonly Mock<UpdateAllergen.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateAllergen.Service _service;

    public UpdateAllergenServiceTests()
    {
        _createAllergen = new(_validator);
        _updateAllergen = new(_validator);
        _repositoryMock = new Mock<UpdateAllergen.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateAllergen.Service(_updateAllergen, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private AllergenAgg CreateExistingAllergen(string code = "GLUTEN") =>
        _createAllergen.Execute(new CreateAllergenCommand(
            Code: code,
            Name: "Original Name",
            IconUrl: "https://example.com/original.png",
            IsActive: true,
            DisplayOrder: 0));

    private void SetupRepositoryGet(AllergenAgg allergen)
    {
        _repositoryMock.Setup(r => r.Get(allergen.Id)).ReturnsAsync(allergen);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request("Updated Name", null, true, 0);

        var response = await _service.HandleAsync("GLUTEN", request);

        response.Should().NotBeNull();
        response.Id.Should().Be("GLUTEN");
        response.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request(
            Name: "Lácteos",
            IconUrl: "https://example.com/lacteos.png",
            IsActive: false,
            DisplayOrder: 5
        );

        var response = await _service.HandleAsync("GLUTEN", request);

        response.Id.Should().Be("GLUTEN");
        response.Name.Should().Be("Lácteos");
        response.IconUrl.Should().Be("https://example.com/lacteos.png");
        response.IsActive.Should().BeFalse();
        response.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_WithNullIconUrl_ClearsIconUrl()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request("Updated Name", null, true, 0);

        var response = await _service.HandleAsync("GLUTEN", request);

        response.IconUrl.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_PreservesAllergenId()
    {
        var allergen = CreateExistingAllergen("LACTEOS");
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request("New Name", null, true, 0);

        var response = await _service.HandleAsync("LACTEOS", request);

        response.Id.Should().Be("LACTEOS");
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request("Updated Name", null, true, 0);

        await _service.HandleAsync("GLUTEN", request);

        _repositoryMock.Verify(r => r.Get("GLUTEN"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request("Updated Name", null, true, 0);

        await _service.HandleAsync("GLUTEN", request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GetsAllergenBeforeSaving()
    {
        var allergen = CreateExistingAllergen();
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Get("GLUTEN"))
            .Callback(() => callOrder.Add("Get"))
            .ReturnsAsync(allergen);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        var request = new UpdateAllergen.Request("Updated Name", null, true, 0);

        await _service.HandleAsync("GLUTEN", request);

        callOrder.Should().ContainInOrder("Get", "SaveChanges");
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string? name)
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request(name!, null, true, 0);

        var act = () => _service.HandleAsync("GLUTEN", request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request(new string('a', 101), null, true, 0);

        var act = () => _service.HandleAsync("GLUTEN", request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidIconUrl_ThrowsValidationException()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request("Updated Name", "not-a-url", true, 0);

        var act = () => _service.HandleAsync("GLUTEN", request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request("Updated Name", null, true, -1);

        var act = () => _service.HandleAsync("GLUTEN", request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var allergen = CreateExistingAllergen();
        SetupRepositoryGet(allergen);
        var request = new UpdateAllergen.Request("", null, true, 0);

        try { await _service.HandleAsync("GLUTEN", request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
