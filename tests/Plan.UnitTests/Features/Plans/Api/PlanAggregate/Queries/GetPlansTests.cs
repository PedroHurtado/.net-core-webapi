namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Queries;

public class GetPlansTests
{
    private readonly Mock<IQuery> _queryMock = new();
    private readonly GetPlans.Service _service;
    private readonly Plan.Create _createPlan;

    public GetPlansTests()
    {
        var planValidator = new PlanValidator();
        _createPlan = new Plan.Create(planValidator);

        _service = new GetPlans.Service(_queryMock.Object);
    }

    private Plan CreatePlan(
        string name = "Plan Test",
        string description = "Descripción de test",
        bool isActive = false)
    {
        var plan = _createPlan.Execute(new CreatePlanCommand(
            name,
            description
        ));

        if (isActive)
        {
            plan.GetType().GetProperty("IsActive")!.SetValue(plan, true);
        }

        return plan;
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithPlans_ReturnsAllPlans()
    {
        var plans = new List<Plan>
        {
            CreatePlan("Plan Básico"),
            CreatePlan("Plan Premium")
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<Plan>()).Returns(plans);

        var response = await _service.HandleAsync(null);

        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_ReturnsCorrectlyMappedResponse()
    {
        var plan = CreatePlan("Plan Test", "Descripción test");
        var plans = new List<Plan> { plan }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<Plan>()).Returns(plans);

        var response = await _service.HandleAsync(null);

        response.Should().HaveCount(1);
        var result = response.First();
        result.Id.Should().Be(plan.Id);
        result.Name.Should().Be("Plan Test");
        result.Description.Should().Be("Descripción test");
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task HandleAsync_WithIsActiveTrue_ReturnsOnlyActivePlans()
    {
        var plans = new List<Plan>
        {
            CreatePlan("Plan Activo", isActive: true),
            CreatePlan("Plan Inactivo", isActive: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<Plan>()).Returns(plans);

        var response = await _service.HandleAsync(isActive: true);

        response.Should().HaveCount(1);
        response.First().Name.Should().Be("Plan Activo");
        response.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveFalse_ReturnsOnlyInactivePlans()
    {
        var plans = new List<Plan>
        {
            CreatePlan("Plan Activo", isActive: true),
            CreatePlan("Plan Inactivo", isActive: false)
        }.AsAsyncQueryable();

        _queryMock.Setup(q => q.Query<Plan>()).Returns(plans);

        var response = await _service.HandleAsync(isActive: false);

        response.Should().HaveCount(1);
        response.First().Name.Should().Be("Plan Inactivo");
        response.First().IsActive.Should().BeFalse();
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_ReturnsOkResult()
    {
        var serviceMock = new Mock<GetPlans.IService>();
        var expectedResponse = new List<PlanResponse>
        {
            new(Guid.NewGuid(), "Plan Test", "Desc", false, false, [], [])
        };

        serviceMock.Setup(s => s.HandleAsync(null)).ReturnsAsync(expectedResponse);

        var result = await GetPlans.Handler(serviceMock.Object, null);

        result.Should().BeOfType<Ok<List<PlanResponse>>>();
        var okResult = (Ok<List<PlanResponse>>)result;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithIsActive()
    {
        var serviceMock = new Mock<GetPlans.IService>();
        serviceMock.Setup(s => s.HandleAsync(true)).ReturnsAsync([]);

        await GetPlans.Handler(serviceMock.Object, true);

        serviceMock.Verify(s => s.HandleAsync(true), Times.Once);
    }

    #endregion
}
