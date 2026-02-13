namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Queries;

public class GetPlanTests
{
    private readonly Mock<GetPlan.IRepository> _repositoryMock = new();
    private readonly GetPlan.Service _service;
    private readonly Plan.Create _createPlan;

    public GetPlanTests()
    {
        var planValidator = new PlanValidator();
        _createPlan = new Plan.Create(planValidator);

        _service = new GetPlan.Service(_repositoryMock.Object);
    }

    private Plan CreatePlan(
        string name = "Plan Básico",
        string description = "Ideal para empezar")
    {
        return _createPlan.Execute(new CreatePlanCommand(
            name,
            description
        ));
    }

    #region Service Tests

    [Fact]
    public async Task HandleAsync_WithExistingId_ReturnsResponse()
    {
        var plan = CreatePlan();
        _repositoryMock.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var response = await _service.HandleAsync(plan.Id);

        response.Should().NotBeNull();
        response.Id.Should().Be(plan.Id);
        response.Name.Should().Be("Plan Básico");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var plan = CreatePlan(
            name: "Plan Premium",
            description: "Para profesionales"
        );
        _repositoryMock.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var response = await _service.HandleAsync(plan.Id);

        response.Id.Should().Be(plan.Id);
        response.Name.Should().Be("Plan Premium");
        response.Description.Should().Be("Para profesionales");
        response.IsActive.Should().BeFalse();
        response.Features.Should().BeEmpty();
        response.PricingTiers.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var plan = CreatePlan();
        _repositoryMock.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id);

        _repositoryMock.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanNotFound_ThrowsException()
    {
        var nonExistentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException("Plan not found"));

        var act = () => _service.HandleAsync(nonExistentId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidId_ReturnsOkResult()
    {
        var serviceMock = new Mock<GetPlan.IService>();
        var expectedId = Guid.NewGuid();
        var expectedResponse = new PlanResponse(
            expectedId,
            "Plan Test",
            "Descripción",
            false,
            false,
            [],
            []
        );

        serviceMock.Setup(s => s.HandleAsync(expectedId)).ReturnsAsync(expectedResponse);

        var result = await GetPlan.Handler(serviceMock.Object, expectedId);

        result.Should().BeOfType<Ok<PlanResponse>>();
        var okResult = (Ok<PlanResponse>)result;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithId()
    {
        var serviceMock = new Mock<GetPlan.IService>();
        var expectedId = Guid.NewGuid();
        var expectedResponse = new PlanResponse(
            expectedId,
            "Plan Test",
            "Descripción",
            false,
            false,
            [],
            []
        );

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<Guid>())).ReturnsAsync(expectedResponse);

        await GetPlan.Handler(serviceMock.Object, expectedId);

        serviceMock.Verify(s => s.HandleAsync(expectedId), Times.Once);
    }

    #endregion
}
