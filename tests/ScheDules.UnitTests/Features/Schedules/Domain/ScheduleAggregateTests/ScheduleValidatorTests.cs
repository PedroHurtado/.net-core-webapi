namespace Schedules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests;

public class ScheduleValidatorTests
{
    private readonly ScheduleValidator _validator = new();

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.Empty);
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ScheduleValidationMessages.IdRequired);
    }

    [Fact]
    public void Id_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region TenantId Validation

    [Fact]
    public void TenantId_WhenEmpty_ReturnsError()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.Empty);
        schedule.SetName("Test Schedule");

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ScheduleValidationMessages.TenantIdRequired);
    }

    [Fact]
    public void TenantId_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation

    [Fact]
    public void Name_WhenEmpty_ReturnsError()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName(string.Empty);

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ScheduleValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName(new string('a', 101));

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ScheduleValidationMessages.NameMaxLength);
    }

    [Fact]
    public void Name_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Valid Schedule Name");

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Name_WhenAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName(new string('a', 100));

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Description Validation

    [Fact]
    public void Description_WhenExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription(new string('a', 501));

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ScheduleValidationMessages.DescriptionMaxLength);
    }

    [Fact]
    public void Description_WhenNull_ReturnsSuccess()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription(null);

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Description_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription("Valid description");

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Description_WhenAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription(new string('a', 500));

        // Act
        var result = _validator.Validate(schedule);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
