namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePlanFeatureTests
{
    private readonly Mock<UpdatePlanFeature.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdatePlanFeature.Service _service;
    private readonly Plan.Create _createPlan;
    private readonly Plan.AddFeature _addFeature;

    public UpdatePlanFeatureTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var featureValidator = new FeatureValidator();
        var createMoney = new Money.Create(moneyValidator);
        var featureCreate = new Feature.Create(featureValidator);
        var updateFeature = new Plan.UpdateFeature(featureCreate, planValidator);
        _createPlan = new Plan.Create(createMoney, planValidator);
        _addFeature = new Plan.AddFeature(featureCreate, planValidator);

        _service = new UpdatePlanFeature.Service(
            updateFeature,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private Plan CreatePlanWithFeature(
        string featureCode = "FEATURE_01",
        string featureName = "Feature One",
        FeatureType featureType = FeatureType.Boolean)
    {
        var plan = _createPlan.Execute(new CreatePlanCommand(
            "Plan Test",
            "Descripción de test",
            9.99m,
            "EUR",
            BillingPeriod.Monthly
        ));

        _addFeature.Execute(plan, new AddFeatureCommand(
            featureCode,
            featureName,
            "Description",
            featureType
        ));

        return plan;
    }

    [Fact]
    public async Task HandleAsync_WithValidData_GetsFromRepository()
    {
        var plan = CreatePlanWithFeature();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlanFeature.Request(
            Name: "Feature Actualizada",
            Description: "Nueva descripción",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        await _service.HandleAsync(plan.Id, "FEATURE_01", request);

        _repository.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_UpdatesFeature()
    {
        var plan = CreatePlanWithFeature();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlanFeature.Request(
            Name: "Feature Actualizada",
            Description: "Nueva descripción",
            Type: FeatureType.Limit,
            Limit: 100,
            Unit: "items"
        );

        await _service.HandleAsync(plan.Id, "FEATURE_01", request);

        var feature = plan.Features.First(f => f.Code == "FEATURE_01");
        feature.Name.Should().Be("Feature Actualizada");
        feature.Description.Should().Be("Nueva descripción");
        feature.Type.Should().Be(FeatureType.Limit);
        feature.Limit.Should().Be(100);
        feature.Unit.Should().Be("items");
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var plan = CreatePlanWithFeature();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlanFeature.Request(
            Name: "Feature Actualizada",
            Description: "Nueva descripción",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        await _service.HandleAsync(plan.Id, "FEATURE_01", request);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingFeature_ThrowsKeyNotFoundException()
    {
        var plan = CreatePlanWithFeature();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlanFeature.Request(
            Name: "Feature Actualizada",
            Description: "Nueva descripción",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        var act = () => _service.HandleAsync(plan.Id, "NON_EXISTENT", request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException()
    {
        var plan = CreatePlanWithFeature();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlanFeature.Request(
            Name: "",
            Description: "Descripción válida",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        var act = () => _service.HandleAsync(plan.Id, "FEATURE_01", request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name*required*");
    }

    [Fact]
    public async Task HandleAsync_WithNotFoundPlan_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repository.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException());

        var request = new UpdatePlanFeature.Request(
            Name: "Feature Actualizada",
            Description: "Nueva descripción",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        var act = () => _service.HandleAsync(nonExistentId, "FEATURE_01", request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handler_ReturnsNoContent()
    {
        var mockService = new Mock<UpdatePlanFeature.IService>();
        var id = Guid.NewGuid();

        var request = new UpdatePlanFeature.Request(
            Name: "Feature Actualizada",
            Description: "Nueva descripción",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        var result = await UpdatePlanFeature.Handler(mockService.Object, id, "FEATURE_01", request);

        result.Should().BeOfType<NoContent>();
    }
}
