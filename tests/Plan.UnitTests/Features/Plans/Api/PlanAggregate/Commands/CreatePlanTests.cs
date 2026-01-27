namespace Plan.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class CreatePlanTests
{
    private readonly Mock<CreatePlan.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreatePlan.Service _service;

    public CreatePlanTests()
    {
        var moneyValidator = new MoneyValidator();
        var planValidator = new PlanValidator();
        var createMoney = new MoneyVO.Create(moneyValidator);
        var planCreate = new PlanAgg.Create(createMoney, planValidator);

        _service = new CreatePlan.Service(
            planCreate,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ReturnsResponse()
    {
        var request = new CreatePlan.Request(
            Name: "Plan Básico",
            Description: "Ideal para empezar",
            Amount: 9.99m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var response = await _service.HandleAsync(request);

        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Name.Should().Be("Plan Básico");
        response.Description.Should().Be("Ideal para empezar");
        response.Price.Amount.Should().Be(9.99m);
        response.Price.Currency.Code.Should().Be("EUR");
        response.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        response.IsActive.Should().BeFalse();
        response.Features.Should().BeEmpty();
        response.ProviderConfigurations.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_AddsToRepository()
    {
        var request = new CreatePlan.Request(
            Name: "Plan Básico",
            Description: "Ideal para empezar",
            Amount: 9.99m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        await _service.HandleAsync(request);

        _repository.Verify(r => r.Add(It.IsAny<PlanAgg>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var request = new CreatePlan.Request(
            Name: "Plan Básico",
            Description: "Ideal para empezar",
            Amount: 9.99m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        await _service.HandleAsync(request);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException()
    {
        var request = new CreatePlan.Request(
            Name: "",
            Description: "Descripción válida",
            Amount: 9.99m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name*required*");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyDescription_ThrowsValidationException()
    {
        var request = new CreatePlan.Request(
            Name: "Plan Válido",
            Description: "",
            Amount: 9.99m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Description*required*");
    }

    [Fact]
    public async Task HandleAsync_WithNegativeAmount_ThrowsValidationException()
    {
        var request = new CreatePlan.Request(
            Name: "Plan Válido",
            Description: "Descripción válida",
            Amount: -5m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Amount*negative*");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCurrencyCode_ThrowsArgumentException()
    {
        var request = new CreatePlan.Request(
            Name: "Plan Válido",
            Description: "Descripción válida",
            Amount: 9.99m,
            CurrencyCode: "XXX",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*XXX*not supported*");
    }

    [Fact]
    public async Task Handler_ReturnsCreatedWithLocation()
    {
        var mockService = new Mock<CreatePlan.IService>();
        var expectedId = Guid.NewGuid();
        mockService.Setup(s => s.HandleAsync(It.IsAny<CreatePlan.Request>()))
            .ReturnsAsync(new PlanResponse(
                expectedId,
                "Plan Test",
                "Descripción",
                new MoneyResponse(9.99m, new CurrencyResponse("EUR", "€", 2)),
                BillingPeriod.Monthly,
                false,
                false,
                [],
                []
            ));

        var request = new CreatePlan.Request(
            Name: "Plan Test",
            Description: "Descripción",
            Amount: 9.99m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var result = await CreatePlan.Handler(mockService.Object, request);

        var created = result.Should().BeOfType<Created<PlanResponse>>().Subject;
        created.Location.Should().Be($"/plans/{expectedId}");
    }
}
