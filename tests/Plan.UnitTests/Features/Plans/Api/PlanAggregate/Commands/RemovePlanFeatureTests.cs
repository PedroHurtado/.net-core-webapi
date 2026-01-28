namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class RemovePlanFeatureTests
{
    private readonly Mock<RemovePlanFeature.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RemovePlanFeature.Service _service;
    private readonly Plan.Create _createPlan;
    private readonly Plan.AddFeature _addFeature;

    public RemovePlanFeatureTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var featureValidator = new FeatureValidator();
        var createMoney = new Money.Create(moneyValidator);
        var featureCreate = new Feature.Create(featureValidator);
        var removeFeature = new Plan.RemoveFeature(planValidator);
        _createPlan = new Plan.Create(createMoney, planValidator);
        _addFeature = new Plan.AddFeature(featureCreate, planValidator);

        _service = new RemovePlanFeature.Service(
            removeFeature,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private Plan CreatePlanWithFeatures(int featureCount = 2)
    {
        var plan = _createPlan.Execute(new CreatePlanCommand(
            "Plan Test",
            "Descripción de test",
            9.99m,
            "EUR",
            BillingPeriod.Monthly
        ));

        for (var i = 1; i <= featureCount; i++)
        {
            _addFeature.Execute(plan, new AddFeatureCommand(
                $"FEATURE_{i:D2}",
                $"Feature {i}",
                "Description",
                FeatureType.Boolean
            ));
        }

        return plan;
    }

    [Fact]
    public async Task HandleAsync_WithValidData_GetsFromRepository()
    {
        var plan = CreatePlanWithFeatures();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id, "FEATURE_01");

        _repository.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_RemovesFeature()
    {
        var plan = CreatePlanWithFeatures();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id, "FEATURE_01");

        plan.Features.Should().NotContain(f => f.Code == "FEATURE_01");
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var plan = CreatePlanWithFeatures();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        await _service.HandleAsync(plan.Id, "FEATURE_01");

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
