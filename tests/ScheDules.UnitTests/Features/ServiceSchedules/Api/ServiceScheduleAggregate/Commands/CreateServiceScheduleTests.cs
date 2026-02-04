namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregateTests.Commands;

public class CreateServiceScheduleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly ServiceScheduleValidator _serviceScheduleValidator = new();
    private readonly ReservationPolicyValidator _reservationPolicyValidator = new();
    private readonly ReservationPolicy.Create _createReservationPolicy;
    private readonly ServiceSchedule.Create _createServiceSchedule;
    private readonly Mock<CreateServiceSchedule.IRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateServiceSchedule.Service _service;

    public CreateServiceScheduleTests()
    {
        _createReservationPolicy = new(_reservationPolicyValidator);
        _createServiceSchedule = new(_createReservationPolicy, _serviceScheduleValidator);
        _repositoryMock = new Mock<CreateServiceSchedule.IRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new CreateServiceSchedule.Service(
            _createServiceSchedule,
            _tenantId,
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static CreateServiceSchedule.Request CreateValidRequest(
        string name = "Service Schedule Test",
        string description = "Test description",
        TimeSpan? minimumAdvanceTime = null,
        TimeSpan? maximumAdvanceTime = null,
        TimeSpan? slotInterval = null,
        TimeSpan? bufferBetweenReservations = null,
        int maxPartySize = 8,
        int minPartySize = 1,
        Dictionary<ServiceType, TimeSpan>? standardDurations = null)
    {
        return new CreateServiceSchedule.Request(
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
                { ServiceType.Breakfast, TimeSpan.FromMinutes(60) },
                { ServiceType.Lunch, TimeSpan.FromMinutes(90) },
                { ServiceType.Dinner, TimeSpan.FromMinutes(120) }
            });
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithValidData_ReturnsServiceScheduleResponse()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.Should().NotBeNull();
        response.Name.Should().Be("Service Schedule Test");
        response.Description.Should().Be("Test description");
    }

    [Fact]
    public async Task HandleAsync_WithValidData_CreatesScheduleAsInactive()
    {
        var request = CreateValidRequest();

        var response = await _service.HandleAsync(request);

        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_CallsRepositoryAdd()
    {
        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        _repositoryMock.Verify(r => r.Add(It.Is<ServiceSchedule>(s => s.Name == "Service Schedule Test")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsUnitOfWorkSaveChangesAsync()
    {
        var request = CreateValidRequest();

        await _service.HandleAsync(request);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Validation Tests - ServiceSchedule

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithEmptyName_ThrowsValidationException(string name)
    {
        var request = CreateValidRequest(name: name);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.NameRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var request = CreateValidRequest(name: new string('a', 101));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.NameMaxLength}*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithEmptyDescription_ThrowsValidationException(string description)
    {
        var request = CreateValidRequest(description: description);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.DescriptionRequired}*");
    }

    [Fact]
    public async Task HandleAsync_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var request = CreateValidRequest(description: new string('a', 501));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.DescriptionMaxLength}*");
    }

    #endregion

    #region Validation Tests - ReservationPolicy

    [Fact]
    public async Task HandleAsync_WithMinimumAdvanceTimeZero_ThrowsValidationException()
    {
        var request = CreateValidRequest(minimumAdvanceTime: TimeSpan.Zero);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MinimumAdvanceTimeMustBeGreaterThanZero}*");
    }

    [Fact]
    public async Task HandleAsync_WithMaxAdvanceLessThanMin_ThrowsValidationException()
    {
        var request = CreateValidRequest(
            minimumAdvanceTime: TimeSpan.FromHours(24),
            maximumAdvanceTime: TimeSpan.FromHours(12));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MaximumAdvanceTimeMustBeGreaterThanMinimum}*");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSlotInterval_ThrowsValidationException()
    {
        var request = CreateValidRequest(slotInterval: TimeSpan.FromMinutes(45));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.SlotIntervalMustBeValid}*");
    }

    [Fact]
    public async Task HandleAsync_WithNegativeBuffer_ThrowsValidationException()
    {
        var request = CreateValidRequest(bufferBetweenReservations: TimeSpan.FromMinutes(-10));

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.BufferCannotBeNegative}*");
    }

    [Fact]
    public async Task HandleAsync_WithMaxPartySizeZero_ThrowsValidationException()
    {
        var request = CreateValidRequest(maxPartySize: 0);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MaxPartySizeMustBeGreaterThanZero}*");
    }

    [Fact]
    public async Task HandleAsync_WithMinPartySizeZero_ThrowsValidationException()
    {
        var request = CreateValidRequest(minPartySize: 0);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MinPartySizeMustBeGreaterThanZero}*");
    }

    [Fact]
    public async Task HandleAsync_WithMinPartySizeGreaterThanMax_ThrowsValidationException()
    {
        var request = CreateValidRequest(minPartySize: 10, maxPartySize: 5);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MinPartySizeCannotExceedMax}*");
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsCreatedResult()
    {
        var request = CreateValidRequest();
        var serviceMock = new Mock<CreateServiceSchedule.IService>();
        var expectedResponse = ServiceScheduleTestHelper.CreateMockResponse();

        serviceMock.Setup(s => s.HandleAsync(request)).ReturnsAsync(expectedResponse);

        var result = await CreateServiceSchedule.Handler(serviceMock.Object, request);

        result.Should().BeOfType<Created<ServiceScheduleResponse>>();
        var createdResult = (Created<ServiceScheduleResponse>)result;
        createdResult.Location.Should().Be($"/service-schedules/{expectedResponse.Id}");
    }

    #endregion
}
