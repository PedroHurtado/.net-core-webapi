namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class DeactivatePlanTests
{
    private readonly Mock<DeactivatePlan.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeactivatePlan.Service _service;
    private readonly Plan.Create _createPlan;
    private readonly Plan.Activate _activatePlan;
    private readonly Plan.AddProviderConfiguration _addProviderConfig;
    private readonly Plan.AddFeature _addFeature;

    public DeactivatePlanTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var providerConfigValidator = new PaymentProviderConfigValidator();
        var featureValidator = new FeatureValidator();
        var createMoney = new Money.Create(moneyValidator);
        var providerConfigCreate = new PaymentProviderConfig.Create(providerConfigValidator);
        var featureCreate = new Feature.Create(featureValidator);
        var planDeactivate = new Plan.Deactivate(planValidator);
        _createPlan = new Plan.Create(createMoney, planValidator);
        _activatePlan = new Plan.Activate(planValidator);
        _addProviderConfig = new Plan.AddProviderConfiguration(providerConfigCreate, planValidator);
        _addFeature = new Plan.AddFeature(featureCreate, planValidator);

        _service = new DeactivatePlan.Service(
            planDeactivate,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private Plan CreateActivePlan()
    {
        var plan = _createPlan.Execute(new CreatePlanCommand(
            "Plan Test",
            "Descripcion de test",
            9.99m,
            "EUR",
            BillingPeriod.Monthly
        ));

        _addFeature.Execute(plan, new AddFeatureCommand(
            "FEATURE_01",
            "Feature One",
            "Description",
            FeatureType.Boolean,
            null,
            null
        ));

        _addProviderConfig.Execute(plan, new AddProviderConfigurationCommand(
            "Stripe",
            "prod_123",
            "price_123",
            true
        ));

        _activatePlan.Execute(plan, new ActivatePlanCommand());

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
        var plan = _createPlan.Execute(new CreatePlanCommand(
            "Plan Test",
            "Descripcion de test",
            9.99m,
            "EUR",
            BillingPeriod.Monthly
        ));
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
            new MoneyResponse(9.99m, new CurrencyResponse("EUR", "E", 2)),
            BillingPeriod.Monthly,
            false,
            true,
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
