namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class UpdateScheduleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Schedule.Update _updateSchedule;
    private readonly Mock<UpdateSchedule.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateSchedule.Service _service;

    public UpdateScheduleTests()
    {
        _createSchedule = new(_scheduleValidator);
        _updateSchedule = new(_scheduleValidator);
        _repositoryMock = new Mock<UpdateSchedule.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateSchedule.Service(_updateSchedule, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private Schedule CreateExistingSchedule(
        string name = "Horario Original",
        string? description = null)
    {
        var command = new CreateScheduleCommand(_tenantId, name, description);
        return _createSchedule.Execute(command);
    }

    private static UpdateSchedule.Request CreateValidRequest(
        string name = "Horario Actualizado",
        string? description = null)
    {
        return new UpdateSchedule.Request(
            Name: name,
            Description: description);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesSchedule()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var request = CreateValidRequest(name: "Nuevo Nombre", description: "Nueva descripción");

        await _service.HandleAsync(scheduleId, request);

        existingSchedule.Name.Should().Be("Nuevo Nombre");
        existingSchedule.Description.Should().Be("Nueva descripción");
    }

    [Fact]
    public async Task HandleAsync_WithNullDescription_UpdatesDescriptionToNull()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule(description: "Descripción original");
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var request = CreateValidRequest(name: "Horario", description: null);

        await _service.HandleAsync(scheduleId, request);

        existingSchedule.Description.Should().BeNull();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryGet()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, request);

        _repositoryMock.Verify(r => r.Get(scheduleId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string name)
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var request = CreateValidRequest(name: name);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNameExceeding100Characters_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var request = CreateValidRequest(name: new string('a', 101));

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());

        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);

        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(scheduleId, request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateSchedule.IService>();

        serviceMock.Setup(s => s.HandleAsync(scheduleId, request)).Returns(Task.CompletedTask);

        var result = await UpdateSchedule.Handler(serviceMock.Object, scheduleId, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsServiceWithCorrectParameters()
    {
        var scheduleId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateSchedule.IService>();

        await UpdateSchedule.Handler(serviceMock.Object, scheduleId, request);

        serviceMock.Verify(s => s.HandleAsync(scheduleId, request), Times.Once);
    }

    #endregion
}
