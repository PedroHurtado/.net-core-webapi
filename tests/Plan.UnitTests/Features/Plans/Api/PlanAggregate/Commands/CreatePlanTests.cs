namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class CreatePlanTests
{
    private readonly Mock<CreatePlan.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreatePlan.Service _service;

    public CreatePlanTests()
    {
        var planValidator = new PlanValidator();
        var planCreate = new Plan.Create(planValidator);

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
            Description: "Ideal para empezar"
        );

        var response = await _service.HandleAsync(request);

        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Name.Should().Be("Plan Básico");
        response.Description.Should().Be("Ideal para empezar");
        response.IsActive.Should().BeFalse();
        response.Features.Should().BeEmpty();
        response.PricingTiers.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_AddsToRepository()
    {
        var request = new CreatePlan.Request(
            Name: "Plan Básico",
            Description: "Ideal para empezar"
        );

        await _service.HandleAsync(request);

        _repository.Verify(r => r.Add(It.IsAny<Plan>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var request = new CreatePlan.Request(
            Name: "Plan Básico",
            Description: "Ideal para empezar"
        );

        await _service.HandleAsync(request);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException()
    {
        var request = new CreatePlan.Request(
            Name: "",
            Description: "Descripción válida"
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
            Description: ""
        );

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Description*required*");
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
                false,
                false,
                [],
                []
            ));

        var request = new CreatePlan.Request(
            Name: "Plan Test",
            Description: "Descripción"
        );

        var result = await CreatePlan.Handler(mockService.Object, request);

        var created = result.Should().BeOfType<Created<PlanResponse>>().Subject;
        created.Location.Should().Be($"/plans/{expectedId}");
    }
}
