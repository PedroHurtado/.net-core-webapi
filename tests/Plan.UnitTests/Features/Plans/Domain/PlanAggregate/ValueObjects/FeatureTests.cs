namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.ValueObjects;

public class FeatureTests
{
    [Theory]
    [InlineData("RESERVATIONS_MONTHLY")]
    [InlineData("ACTIVE_WAITERS")]
    [InlineData("PRIORITY_SUPPORT")]
    public void Code_SetsCorrectValue(string code)
    {
        var feature = new TestableFeature(code, "Test Feature", null, FeatureType.Boolean);

        feature.Code.Should().Be(code);
    }

    [Theory]
    [InlineData("Reservas mensuales")]
    [InlineData("Camareros activos")]
    [InlineData("Soporte prioritario")]
    public void Name_SetsCorrectValue(string name)
    {
        var feature = new TestableFeature("TEST_CODE", name, null, FeatureType.Boolean);

        feature.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("Descripción de prueba")]
    [InlineData("Otra descripción")]
    [InlineData(null)]
    public void Description_SetsCorrectValue(string? description)
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", description, FeatureType.Boolean);

        feature.Description.Should().Be(description);
    }

    [Theory]
    [InlineData(FeatureType.Boolean)]
    [InlineData(FeatureType.Limit)]
    [InlineData(FeatureType.Unlimited)]
    public void Type_SetsCorrectValue(FeatureType type)
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, type, type == FeatureType.Limit ? 100 : null);

        feature.Type.Should().Be(type);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(4)]
    [InlineData(null)]
    public void Limit_SetsCorrectValue(int? limit)
    {
        var type = limit.HasValue ? FeatureType.Limit : FeatureType.Boolean;
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, type, limit);

        feature.Limit.Should().Be(limit);
    }

    [Theory]
    [InlineData("reservas/mes")]
    [InlineData("camareros")]
    [InlineData(null)]
    public void Unit_SetsCorrectValue(string? unit)
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, 100, unit);

        feature.Unit.Should().Be(unit);
    }

    #region IsValid

    [Fact]
    public void IsValid_WhenLimitTypeWithLimit_ReturnsTrue()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, 100, "units");

        feature.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenLimitTypeWithoutLimit_ReturnsFalse()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, null, "units");

        feature.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenLimitTypeWithZeroLimit_ReturnsFalse()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, 0, "units");

        feature.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenBooleanTypeWithoutLimit_ReturnsTrue()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Boolean);

        feature.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenBooleanTypeWithLimit_ReturnsFalse()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Boolean, 100);

        feature.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenUnlimitedTypeWithoutLimit_ReturnsTrue()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Unlimited);

        feature.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenUnlimitedTypeWithLimit_ReturnsFalse()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Unlimited, 100);

        feature.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenUnknownType_ReturnsFalse()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, (FeatureType)999);

        feature.IsValid.Should().BeFalse();
    }

    #endregion

    #region DisplayValue

    [Fact]
    public void DisplayValue_WhenLimitType_ReturnsFormattedValue()
    {
        var feature = new TestableFeature("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Limit, 100, "reservas/mes");

        feature.DisplayValue.Should().Be("100 reservas/mes");
    }

    [Fact]
    public void DisplayValue_WhenUnlimitedType_ReturnsIlimitado()
    {
        var feature = new TestableFeature("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Unlimited);

        feature.DisplayValue.Should().Be("Ilimitado");
    }

    [Fact]
    public void DisplayValue_WhenBooleanType_ReturnsIncluido()
    {
        var feature = new TestableFeature("PRIORITY_SUPPORT", "Soporte prioritario", null, FeatureType.Boolean);

        feature.DisplayValue.Should().Be("Incluido");
    }

    [Fact]
    public void DisplayValue_WhenUnknownType_ReturnsEmptyString()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, (FeatureType)999);

        feature.DisplayValue.Should().BeEmpty();
    }

    #endregion
}
