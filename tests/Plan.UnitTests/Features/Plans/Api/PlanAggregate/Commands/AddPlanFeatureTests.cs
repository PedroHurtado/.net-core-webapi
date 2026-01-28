namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class AddPlanFeatureTests
{
    private readonly Mock<AddPlanFeature.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AddPlanFeature.Service _service;
    private readonly Plan.Create _createPlan;

    public AddPlanFeatureTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var featureValidator = new FeatureValidator();
        var createMoney = new Money.Create(moneyValidator);
        var createFeature = new Feature.Create(featureValidator);
        var addFeature = new Plan.AddFeature(createFeature, planValidator);
        _createPlan = new Plan.Create(createMoney, planValidator);

        _service = new AddPlanFeature.Service(
            addFeature,
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

    [Fact]
    public async Task HandleAsync_WithValidData_GetsFromRepository()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new AddPlanFeature.Request(
            Code: "FEATURE_01",
            Name: "Feature One",
            Description: "Description",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        await _service.HandleAsync(plan.Id, request);

        _repository.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_AddsFeature()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new AddPlanFeature.Request(
            Code: "FEATURE_01",
            Name: "Feature One",
            Description: "Description",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        await _service.HandleAsync(plan.Id, request);

        plan.Features.Should().ContainSingle(f => f.Code == "FEATURE_01");
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new AddPlanFeature.Request(
            Code: "FEATURE_01",
            Name: "Feature One",
            Description: "Description",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        await _service.HandleAsync(plan.Id, request);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ReturnsResponseWithFeature()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new AddPlanFeature.Request(
            Code: "FEATURE_01",
            Name: "Feature One",
            Description: "Description",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        var response = await _service.HandleAsync(plan.Id, request);

        response.Id.Should().Be(plan.Id);
        response.Features.Should().ContainSingle(f => f.Code == "FEATURE_01");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateCode_ThrowsConflictException()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new AddPlanFeature.Request(
            Code: "FEATURE_01",
            Name: "Feature One",
            Description: "Description",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        await _service.HandleAsync(plan.Id, request);

        var act = () => _service.HandleAsync(plan.Id, request);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyCode_ThrowsValidationException()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new AddPlanFeature.Request(
            Code: "",
            Name: "Feature One",
            Description: "Description",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        var act = () => _service.HandleAsync(plan.Id, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Code*required*");
    }

    [Fact]
    public async Task HandleAsync_WithNotFound_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repository.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException());

        var request = new AddPlanFeature.Request(
            Code: "FEATURE_01",
            Name: "Feature One",
            Description: "Description",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        var act = () => _service.HandleAsync(nonExistentId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handler_ReturnsCreatedWithLocation()
    {
        var mockService = new Mock<AddPlanFeature.IService>();
        var id = Guid.NewGuid();
        var planResponse = new PlanResponse(
            id,
            "Plan Test",
            "Description",
            new MoneyResponse(9.99m, new CurrencyResponse("EUR", "E", 2)),
            BillingPeriod.Monthly,
            false,
            false,
            [],
            []
        );
        mockService.Setup(s => s.HandleAsync(id, It.IsAny<AddPlanFeature.Request>()))
            .ReturnsAsync(planResponse);

        var request = new AddPlanFeature.Request(
            Code: "FEATURE_01",
            Name: "Feature One",
            Description: "Description",
            Type: FeatureType.Boolean,
            Limit: null,
            Unit: null
        );

        var result = await AddPlanFeature.Handler(mockService.Object, id, request);

        var created = result.Should().BeOfType<Created<PlanResponse>>().Subject;
        created.Location.Should().Be($"/plans/{id}");
    }
}
