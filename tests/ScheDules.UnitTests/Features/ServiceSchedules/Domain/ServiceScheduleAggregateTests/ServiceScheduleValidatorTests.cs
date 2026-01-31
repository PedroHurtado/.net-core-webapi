namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests;

public class ServiceScheduleValidatorTests
{
    private readonly ServiceScheduleValidator _validator = new();

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var schedule = new TestableServiceSchedule(Guid.Empty);

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceScheduleValidationMessages.IdRequired);
    }

    [Fact]
    public void Id_WhenValid_ReturnsSuccess()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription("Test Description");

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region TenantId Validation

    [Fact]
    public void TenantId_WhenEmpty_ReturnsError()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.Empty);
        schedule.SetName("Test Schedule");
        schedule.SetDescription("Test Description");

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceScheduleValidationMessages.TenantIdRequired);
    }

    [Fact]
    public void TenantId_WhenValid_ReturnsSuccess()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription("Test Description");

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation

    [Fact]
    public void Name_WhenEmpty_ReturnsError()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName(string.Empty);
        schedule.SetDescription("Test Description");

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceScheduleValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName(new string('a', 101));
        schedule.SetDescription("Test Description");

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceScheduleValidationMessages.NameMaxLength);
    }

    [Theory]
    [InlineData("Main Schedule")]
    [InlineData("A")]
    [InlineData("Schedule with exactly 100 characters aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void Name_WhenValid_ReturnsSuccess(string name)
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName(name);
        schedule.SetDescription("Test Description");

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Description Validation

    [Fact]
    public void Description_WhenEmpty_ReturnsError()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription(string.Empty);

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceScheduleValidationMessages.DescriptionRequired);
    }

    [Fact]
    public void Description_WhenExceedsMaxLength_ReturnsError()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription(new string('a', 501));

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceScheduleValidationMessages.DescriptionMaxLength);
    }

    [Theory]
    [InlineData("Main restaurant schedule")]
    [InlineData("A")]
    public void Description_WhenValid_ReturnsSuccess(string description)
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription(description);

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Policy Validation

    [Fact]
    public void Policy_WhenNull_ReturnsError()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription("Test Description");
        schedule.SetPolicy(null!);

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ServiceScheduleValidationMessages.PolicyRequired);
    }

    [Fact]
    public void Policy_WhenValid_ReturnsSuccess()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription("Test Description");
        schedule.SetPolicy(ReservationPolicy.Default());

        var result = _validator.Validate(schedule);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
