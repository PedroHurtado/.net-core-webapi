namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class UpdateServiceScheduleTests
{
    private readonly ServiceScheduleValidator _serviceScheduleValidator = new();
    private readonly ReservationPolicyValidator _reservationPolicyValidator = new();
    private readonly ReservationPolicy.Create _createReservationPolicy;
    private readonly ServiceSchedule.Update _updateServiceSchedule;
    private readonly Mock<UpdateServiceSchedule.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateServiceSchedule.Service _service;

    public UpdateServiceScheduleTests()
    {
        _createReservationPolicy = new(_reservationPolicyValidator);
        _updateServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _repositoryMock = new Mock<UpdateServiceSchedule.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new UpdateServiceSchedule.Service(
            _updateServiceSchedule,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static UpdateServiceSchedule.Request CreateValidRequest(
        string name = "Updated Schedule",
        string description = "Updated description",
        TimeSpan? minimumAdvanceTime = null,
        TimeSpan? maximumAdvanceTime = null,
        TimeSpan? slotInterval = null,
        TimeSpan? bufferBetweenReservations = null,
        int maxPartySize = 8,
        int minPartySize = 1,
        Dictionary<ServiceType, TimeSpan>? standardDurations = null)
    {
        return new UpdateServiceSchedule.Request(
            Name: name,
            Description: description,
            MinimumAdvanceTime: minimumAdvanceTime ?? TimeSpan.FromHours(2),
            MaximumAdvanceTime: maximumAdvanceTime ?? TimeSpan.FromDays(30),
            SlotInterval: slotInterval ?? TimeSpan.FromMinutes(30),
            BufferBetweenReservations: bufferBetweenReservations ?? TimeSpan.FromMinutes(15),
            MaxPartySize: maxPartySize,
            MinPartySize: minPartySize,
            StandardDurations: standardDurations ?? new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Lunch, TimeSpan.FromMinutes(90) }
            });
    }

    private ServiceSchedule CreateExistingSchedule()
    {
        var schedule = new ServiceSchedule(Guid.NewGuid());
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.TenantId))!
            .SetValue(schedule, Guid.NewGuid());
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Name))!
            .SetValue(schedule, "Original Name");
        typeof(ServiceSchedule).GetProperty(nameof(ServiceSchedule.Description))!
            .SetValue(schedule, "Original Description");
        return schedule;
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithValidData_UpdatesSchedule()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, request);

        existingSchedule.Name.Should().Be("Updated Schedule");
        existingSchedule.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task HandleAsync_WithValidData_CallsUnitOfWorkSaveChangesAsync()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest();

        await _service.HandleAsync(scheduleId, request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        var scheduleId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ThrowsAsync(new KeyNotFoundException());
        var request = CreateValidRequest();

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
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

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.NameRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(name: new string('a', 101));

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.NameMaxLength}*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithEmptyDescription_ThrowsValidationException(string description)
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(description: description);

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.DescriptionRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(description: new string('a', 501));

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.DescriptionMaxLength}*");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSlotInterval_ThrowsValidationException()
    {
        var scheduleId = Guid.NewGuid();
        var existingSchedule = CreateExistingSchedule();
        _repositoryMock.Setup(r => r.Get(scheduleId)).ReturnsAsync(existingSchedule);
        var request = CreateValidRequest(slotInterval: TimeSpan.FromMinutes(45));

        var act = () => _service.HandleAsync(scheduleId, request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.SlotIntervalMustBeValid}*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContentResult()
    {
        var scheduleId = Guid.NewGuid();
        var request = CreateValidRequest();
        var serviceMock = new Mock<UpdateServiceSchedule.IService>();

        var result = await UpdateServiceSchedule.Handler(serviceMock.Object, scheduleId, request);

        result.Should().BeOfType<NoContent>();
    }

    #endregion
}
