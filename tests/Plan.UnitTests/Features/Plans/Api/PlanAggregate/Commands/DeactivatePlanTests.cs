namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class DeactivatePlanTests
{
    private readonly Mock<DeactivatePlan.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeactivatePlan.Service _service;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;

    public DeactivatePlanTests()
    {
        var planValidator = new PlanValidator();
        var planDeactivate = new Plan.Deactivate(planValidator);
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());

        _service = new DeactivatePlan.Service(
            planDeactivate,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private TestablePlan CreateActivePlan()
    {
        var feature = _createFeature.Execute(new CreateFeatureCommand("FEATURE_01", "Feature One", "Description", FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_123", "price_123", true));
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider]);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Test");
        plan.SetDescription("Descripcion de test");
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddPricingTier(tier);

        return plan;
    }

    [Fact]
    public async Task HandleAsync_WithValidData_GetsFromRepository()
    {
        var plan = CreateActivePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id);

        _repository.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_DeactivatesPlan()
    {
        var plan = CreateActivePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id);

        plan.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var plan = CreateActivePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ReturnsPlanResponse()
    {
        var plan = CreateActivePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var response = await _service.HandleAsync(plan.Id);

        response.Id.Should().Be(plan.Id);
        response.Name.Should().Be("Plan Test");
        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithNotFoundPlan_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repository.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(nonExistentId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithAlreadyInactivePlan_ThrowsConflictException()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Test");
        plan.SetDescription("Descripcion de test");
        plan.SetIsActive(false);
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var act = () => _service.HandleAsync(plan.Id);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handler_ReturnsOkWithPlanResponse()
    {
        var mockService = new Mock<DeactivatePlan.IService>();
        var id = Guid.NewGuid();
        var expectedResponse = new PlanResponse(
            id,
            "Plan Test",
            "Descripcion",
            false,
            false,
            [],
            []
        );
        mockService.Setup(s => s.HandleAsync(id))
            .ReturnsAsync(expectedResponse);

        var result = await DeactivatePlan.Handler(mockService.Object, id);

        var okResult = result.Should().BeOfType<Ok<PlanResponse>>().Subject;
        okResult.Value.Should().Be(expectedResponse);
    }
}
