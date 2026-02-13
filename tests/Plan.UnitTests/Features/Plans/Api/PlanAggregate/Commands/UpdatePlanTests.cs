namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePlanTests
{
    private readonly Mock<UpdatePlan.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdatePlan.Service _service;
    private readonly Plan.Create _createPlan;

    public UpdatePlanTests()
    {
        var planValidator = new PlanValidator();
        var planUpdate = new Plan.Update(planValidator);
        _createPlan = new Plan.Create(planValidator);

        _service = new UpdatePlan.Service(
            planUpdate,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    private Plan CreatePlan(
        string name = "Plan Original",
        string description = "Descripción original")
    {
        return _createPlan.Execute(new CreatePlanCommand(
            name,
            description
        ));
    }

    [Fact]
    public async Task HandleAsync_WithValidData_GetsFromRepository()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlan.Request(
            Name: "Plan Actualizado",
            Description: "Nueva descripción"
        );

        await _service.HandleAsync(plan.Id, request);

        _repository.Verify(r => r.Get(plan.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_UpdatesPlan()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlan.Request(
            Name: "Plan Actualizado",
            Description: "Nueva descripción"
        );

        await _service.HandleAsync(plan.Id, request);

        plan.Name.Should().Be("Plan Actualizado");
        plan.Description.Should().Be("Nueva descripción");
    }

    [Fact]
    public async Task HandleAsync_WithValidData_SavesChanges()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlan.Request(
            Name: "Plan Actualizado",
            Description: "Nueva descripción"
        );

        await _service.HandleAsync(plan.Id, request);

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException()
    {
        var plan = CreatePlan();
        _repository.Setup(r => r.Get(plan.Id)).ReturnsAsync(plan);

        var request = new UpdatePlan.Request(
            Name: "",
            Description: "Descripción válida"
        );

        var act = () => _service.HandleAsync(plan.Id, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name*required*");
    }

    [Fact]
    public async Task HandleAsync_WithNotFound_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        _repository.Setup(r => r.Get(nonExistentId))
            .ThrowsAsync(new KeyNotFoundException());

        var request = new UpdatePlan.Request(
            Name: "Plan Actualizado",
            Description: "Nueva descripción"
        );

        var act = () => _service.HandleAsync(nonExistentId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handler_ReturnsNoContent()
    {
        var mockService = new Mock<UpdatePlan.IService>();
        var id = Guid.NewGuid();

        var request = new UpdatePlan.Request(
            Name: "Plan Actualizado",
            Description: "Nueva descripción"
        );

        var result = await UpdatePlan.Handler(mockService.Object, id, request);

        result.Should().BeOfType<NoContent>();
    }
}
