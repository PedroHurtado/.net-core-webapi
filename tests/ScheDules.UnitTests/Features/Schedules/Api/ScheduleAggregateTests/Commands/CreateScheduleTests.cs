namespace Schedules.UnitTests.Features.Schedules.Api.ScheduleregateTests.Commands;

public class CreateScheduleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ScheduleValidator _scheduleValidator = new();
    private readonly Schedule.Create _createSchedule;
    private readonly Mock<CreateSchedule.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateSchedule.Service _service;

    public CreateScheduleTests()
    {
        _createSchedule = new(_scheduleValidator);
        _repositoryMock = new Mock<CreateSchedule.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new CreateSchedule.Service(_createSchedule, _tenantId, _repositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateSchedule.Request CreateValidRequest(
        string name = "Horario de Verano",
        string? description = null)
    {
        return new CreateSchedule.Request(
            Name: name,
            Description: description);
    }

    #region HandleAsync Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.Should().NotBeNull();
        response.Name.Should().Be("Horario de Verano");
    }

    [Fact]
    public async Task HandleAsync_WithAllFields_ReturnsCompleteResponse()
    {
        var request = CreateValidRequest(
            name: "Horario de Invierno",
            description: "Del 15 septiembre al 15 junio");

        var response = await _service.HandleAsync(request);

        response.Name.Should().Be("Horario de Invierno");
        response.Description.Should().Be("Del 15 septiembre al 15 junio");
    }

    [Fact]
    public async Task HandleAsync_CreatesScheduleAsInactive()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_CreatesScheduleWithEmptyWeeklyHours()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.WeeklyHours.Should().BeEmpty();
        response.HasWeeklyHours.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_CreatesScheduleWithEmptySpecialDates()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.SpecialDates.Should().BeEmpty();
        response.HasSpecialDates.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_CreatesScheduleNotFullyConfigured()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.IsFullyConfigured.Should().BeFalse();
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_CallsRepositoryAdd()
    {
        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        _repositoryMock.Verify(r => r.Add(It.Is<Schedule>(s => s.Name == "Horario de Verano")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AddsScheduleBeforeSaving()
    {
        var callOrder = new List<string>();
        _repositoryMock.Setup(r => r.Add(It.IsAny<Schedule>())).Callback(() => callOrder.Add("Add"));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        callOrder.Should().ContainInOrder("Add", "SaveChanges");
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string name)
    {
        var request = CreateValidRequest(name: name);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithNameExceeding100Characters_ThrowsValidationException()
    {
        var request = CreateValidRequest(name: new string('a', 101));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WithDescriptionExceeding500Characters_ThrowsValidationException()
    {
        var request = CreateValidRequest(description: new string('a', 501));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallRepository()
    {
        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(request); } catch { }

        _repositoryMock.Verify(r => r.Add(It.IsAny<Schedule>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallUnitOfWork()
    {
        var request = CreateValidRequest(name: "");

        try { await _service.HandleAsync(request); } catch { }

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsCreatedResult()
    {
        var request = CreateValidRequest();
        var serviceMock = new Mock<CreateSchedule.IService>();
        var expectedResponse = new ScheduleResponse(
            Id: Guid.NewGuid(),
            Name: "Horario de Verano",
            Description: null,
            IsActive: false,
            HasWeeklyHours: false,
            HasSpecialDates: false,
            IsFullyConfigured: false,
            WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
            SpecialDates: []);

        serviceMock.Setup(s => s.HandleAsync(request)).ReturnsAsync(expectedResponse);

        var result = await CreateSchedule.Handler(serviceMock.Object, request);

        result.Should().BeOfType<Created<ScheduleResponse>>();
        var createdResult = (Created<ScheduleResponse>)result;
        createdResult.Location.Should().Be($"/schedules/{expectedResponse.Id}");
        createdResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handler_CallsServiceWithRequest()
    {
        var request = CreateValidRequest();
        var serviceMock = new Mock<CreateSchedule.IService>();
        var expectedResponse = new ScheduleResponse(
            Id: Guid.NewGuid(),
            Name: "Test",
            Description: null,
            IsActive: false,
            HasWeeklyHours: false,
            HasSpecialDates: false,
            IsFullyConfigured: false,
            WeeklyHours: new Dictionary<DayOfWeek, DayScheduleResponse>(),
            SpecialDates: []);

        serviceMock.Setup(s => s.HandleAsync(It.IsAny<CreateSchedule.Request>())).ReturnsAsync(expectedResponse);

        await CreateSchedule.Handler(serviceMock.Object, request);

        serviceMock.Verify(s => s.HandleAsync(request), Times.Once);
    }

    #endregion
}
