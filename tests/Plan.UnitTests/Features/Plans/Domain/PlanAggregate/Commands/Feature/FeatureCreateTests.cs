namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Feature;

public class FeatureCreateTests
{
    private readonly FeatureValidator _validator = new();
    private readonly Plan.Features.Plans.Domain.PlanAggregate.ValueObjects.Feature.Create _create;

    public FeatureCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidBooleanFeature_ReturnsFeature()
    {
        var command = new CreateFeatureCommand("PRIORITY_SUPPORT", "Soporte prioritario", "Respuesta en 24h", FeatureType.Boolean);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Code.Should().Be("PRIORITY_SUPPORT");
        result.Name.Should().Be("Soporte prioritario");
        result.Description.Should().Be("Respuesta en 24h");
        result.Type.Should().Be(FeatureType.Boolean);
        result.Limit.Should().BeNull();
        result.Unit.Should().BeNull();
    }

    [Fact]
    public void Execute_WithValidLimitFeature_ReturnsFeature()
    {
        var command = new CreateFeatureCommand("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Limit, 100, "reservas/mes");

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Code.Should().Be("RESERVATIONS_MONTHLY");
        result.Name.Should().Be("Reservas mensuales");
        result.Type.Should().Be(FeatureType.Limit);
        result.Limit.Should().Be(100);
        result.Unit.Should().Be("reservas/mes");
    }

    [Fact]
    public void Execute_WithValidUnlimitedFeature_ReturnsFeature()
    {
        var command = new CreateFeatureCommand("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Unlimited);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Type.Should().Be(FeatureType.Unlimited);
        result.Limit.Should().BeNull();
    }

    [Theory]
    [InlineData("RESERVATIONS_MONTHLY")]
    [InlineData("ACTIVE_WAITERS")]
    [InlineData("PRIORITY_SUPPORT")]
    public void Execute_WithDifferentCodes_SetsCorrectCode(string code)
    {
        var command = new CreateFeatureCommand(code, "Test Feature", null, FeatureType.Boolean);

        var result = _create.Execute(command);

        result.Code.Should().Be(code);
    }

    [Theory]
    [InlineData(FeatureType.Boolean)]
    [InlineData(FeatureType.Unlimited)]
    public void Execute_WithBooleanOrUnlimitedType_SetsCorrectType(FeatureType type)
    {
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", null, type);

        var result = _create.Execute(command);

        result.Type.Should().Be(type);
        result.Limit.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Execute_WithLimitType_SetsCorrectLimit(int limit)
    {
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", null, FeatureType.Limit, limit, "units");

        var result = _create.Execute(command);

        result.Limit.Should().Be(limit);
    }

    [Theory]
    [InlineData("reservas/mes")]
    [InlineData("camareros")]
    [InlineData("ubicaciones")]
    public void Execute_WithDifferentUnits_SetsCorrectUnit(string unit)
    {
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", null, FeatureType.Limit, 100, unit);

        var result = _create.Execute(command);

        result.Unit.Should().Be(unit);
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithEmptyCode_ThrowsValidationException()
    {
        var command = new CreateFeatureCommand("", "Test Feature", null, FeatureType.Boolean);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithCodeTooLong_ThrowsValidationException()
    {
        var longCode = new string('A', 51);
        var command = new CreateFeatureCommand(longCode, "Test Feature", null, FeatureType.Boolean);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNonUppercaseCode_ThrowsValidationException()
    {
        var command = new CreateFeatureCommand("test_code", "Test Feature", null, FeatureType.Boolean);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithCodeContainingSpaces_ThrowsValidationException()
    {
        var command = new CreateFeatureCommand("TEST CODE", "Test Feature", null, FeatureType.Boolean);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithEmptyName_ThrowsValidationException()
    {
        var command = new CreateFeatureCommand("TEST_CODE", "", null, FeatureType.Boolean);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameTooLong_ThrowsValidationException()
    {
        var longName = new string('a', 101);
        var command = new CreateFeatureCommand("TEST_CODE", longName, null, FeatureType.Boolean);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionTooLong_ThrowsValidationException()
    {
        var longDescription = new string('a', 251);
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", longDescription, FeatureType.Boolean);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithLimitTypeButNoLimit_ThrowsValidationException()
    {
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", null, FeatureType.Limit, null, "units");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithLimitTypeAndZeroLimit_ThrowsValidationException()
    {
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", null, FeatureType.Limit, 0, "units");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithBooleanTypeAndLimit_ThrowsValidationException()
    {
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", null, FeatureType.Boolean, 100);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithUnlimitedTypeAndLimit_ThrowsValidationException()
    {
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", null, FeatureType.Unlimited, 100);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithUnitTooLong_ThrowsValidationException()
    {
        var longUnit = new string('a', 51);
        var command = new CreateFeatureCommand("TEST_CODE", "Test Feature", null, FeatureType.Limit, 100, longUnit);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
