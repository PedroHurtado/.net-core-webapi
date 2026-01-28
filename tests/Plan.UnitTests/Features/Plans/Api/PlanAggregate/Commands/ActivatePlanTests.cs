namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class ActivatePlanTests
{
    private readonly Mock<ActivatePlan.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ActivatePlan.Service _service;
    private readonly Plan.Create _createPlan;
    private readonly Plan.AddFeature _addFeature;
    private readonly Plan.AddProviderConfiguration _addProviderConfig;

    public ActivatePlanTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var featureValidator = new FeatureValidator();
        var providerConfigValidator = new PaymentProviderConfigValidator();
        var createMoney = new Money.Create(moneyValidator);
        var createFeature = new Feature.Create(featureValidator);
        var createProviderConfig = new PaymentProviderConfig.Create(providerConfigValidator);
        var planActivate = new Plan.Activate(planValidator);
        _createPlan = new Plan.Create(createMoney, planValidator);
        _addFeature = new Plan.AddFeature(createFeature, planValidator);
        _addProviderConfig = new Plan.AddProviderConfiguration(createProviderConfig, planValidator);

        _service = new ActivatePlan.Service(
            planActivate,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private Plan CreatePlan(
        string name = "Plan Test",
        string description = "Descripcion test",
        decimal amount = 9.99m,
        string currencyCode = "EUR",
        BillingPeriod billingPeriod = BillingPeriod.Monthly)
    {
        return _createPlan.Execute(new CreatePlanCommand(
            name,
            description,
            amount,
            currencyCode,
            billingPeriod
        ));
    }

    private Plan CreateCompletePlan()
    {
        var plan = CreatePlan();
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
        return plan;
    }

    [Fact]
    public async Task HandleAsync_WithValidPlan_GetsFromRepository()
    {
        var plan = CreateCompletePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id);

        _repository.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidPlan_ActivatesPlan()
    {
        var plan = CreateCompletePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id);

        plan.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithValidPlan_SavesChanges()
    {
        var plan = CreateCompletePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidPlan_ReturnsResponseWithIsActiveTrue()
    {
        var plan = CreateCompletePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var response = await _service.HandleAsync(plan.Id);

        response.Id.Should().Be(plan.Id);
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithAlreadyActivePlan_ThrowsConflictException()
    {
        var plan = CreateCompletePlan();
        plan.GetType().GetProperty("IsActive")!.SetValue(plan, true);
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var act = () => _service.HandleAsync(plan.Id);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task HandleAsync_WithNotFound_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repository.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException());

        var act = () => _service.HandleAsync(nonExistentId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handler_ReturnsOkWithResponse()
    {
        var mockService = new Mock<ActivatePlan.IService>();
        var id = Guid.NewGuid();
        var planResponse = new PlanResponse(
            id,
            "Plan Test",
            "Description",
            new MoneyResponse(9.99m, new CurrencyResponse("EUR", "E", 2)),
            BillingPeriod.Monthly,
            true,
            true,
            [new FeatureResponse("FEATURE_01", "Feature One", "Desc", FeatureType.Boolean, null, null, "Yes")],
            [new ProviderConfigResponse("Stripe", "prod_123", "price_123", true)]
        );
        mockService.Setup(s => s.HandleAsync(id))
            .ReturnsAsync(planResponse);

        var result = await ActivatePlan.Handler(mockService.Object, id);

        var ok = result.Should().BeOfType<Ok<PlanResponse>>().Subject;
        ok.Value!.IsActive.Should().BeTrue();
    }
}
