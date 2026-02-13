namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class ActivatePlanTests
{
    private readonly Mock<ActivatePlan.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ActivatePlan.Service _service;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;

    public ActivatePlanTests()
    {
        var planValidator = new PlanValidator();
        var planActivate = new Plan.Activate(planValidator);
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());

        _service = new ActivatePlan.Service(
            planActivate,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private TestablePlan CreateCompletePlan()
    {
        var feature = _createFeature.Execute(new CreateFeatureCommand("FEATURE_01", "Feature One", "Description", FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_123", "price_123", true));
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider]);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Test");
        plan.SetDescription("Descripcion test");
        plan.SetIsActive(false);
        plan.AddFeature(feature);
        plan.AddPricingTier(tier);

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
        plan.SetIsActive(true);
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
            true,
            true,
            [new FeatureResponse("FEATURE_01", "Feature One", "Desc", FeatureType.Boolean, null, null, "Yes")],
            [new PricingTierResponse(
                BillingPeriod.Monthly,
                new MoneyResponse(9.99m, new CurrencyResponse("EUR", "E", 2)),
                true,
                true,
                [new ProviderConfigResponse("Stripe", "prod_123", "price_123", true)])]
        );
        mockService.Setup(s => s.HandleAsync(id))
            .ReturnsAsync(planResponse);

        var result = await ActivatePlan.Handler(mockService.Object, id);

        var ok = result.Should().BeOfType<Ok<PlanResponse>>().Subject;
        ok.Value!.IsActive.Should().BeTrue();
    }
}
