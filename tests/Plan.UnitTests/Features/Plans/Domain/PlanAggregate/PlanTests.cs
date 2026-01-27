namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate;

public class PlanTests
{
    [Fact]
    public void Plan_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var plan = new TestablePlan(id);
        var price = new TestableMoney(9.99m, Currency.EUR);
        var feature = new TestableFeature("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Limit, 100, "reservas");
        var providerConfig = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456");

        // Act
        plan.SetName("Plan Básico");
        plan.SetDescription("Plan ideal para empezar");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddProviderConfiguration(providerConfig);

        // Assert
        plan.Id.Should().Be(id);
        plan.Name.Should().Be("Plan Básico");
        plan.Description.Should().Be("Plan ideal para empezar");
        plan.Price.Should().Be(price);
        plan.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        plan.IsActive.Should().BeTrue();
        plan.Features.Should().ContainSingle();
        plan.ProviderConfigurations.Should().ContainSingle();
    }

    [Fact]
    public void HasActiveProvider_WithActiveProvider_ShouldReturnTrue()
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());
        var activeConfig = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", true);

        // Act
        plan.AddProviderConfiguration(activeConfig);

        // Assert
        plan.HasActiveProvider.Should().BeTrue();
    }

    [Fact]
    public void HasActiveProvider_WithInactiveProvider_ShouldReturnFalse()
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());
        var inactiveConfig = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", false);

        // Act
        plan.AddProviderConfiguration(inactiveConfig);

        // Assert
        plan.HasActiveProvider.Should().BeFalse();
    }

    [Fact]
    public void HasActiveProvider_WithMultipleProvidersOneActive_ShouldReturnTrue()
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());
        var inactiveConfig = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", false);
        var activeConfig = new TestablePaymentProviderConfig("Paddle", "prod_789", "price_012", true);

        // Act
        plan.AddProviderConfiguration(inactiveConfig);
        plan.AddProviderConfiguration(activeConfig);

        // Assert
        plan.HasActiveProvider.Should().BeTrue();
    }

    [Fact]
    public void Features_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Boolean);

        // Act
        plan.AddFeature(feature);

        // Assert
        plan.Features.Should().BeAssignableTo<IReadOnlyCollection<Feature>>();
        plan.Features.Should().ContainSingle();
    }

    [Fact]
    public void ProviderConfigurations_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());
        var config = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456");

        // Act
        plan.AddProviderConfiguration(config);

        // Assert
        plan.ProviderConfigurations.Should().BeAssignableTo<IReadOnlyCollection<PaymentProviderConfig>>();
        plan.ProviderConfigurations.Should().ContainSingle();
    }

    [Fact]
    public void Plan_WithMultipleFeatures_ShouldContainAllFeatures()
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());
        var feature1 = new TestableFeature("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Limit, 100, "reservas");
        var feature2 = new TestableFeature("ACTIVE_WAITERS", "Camareros activos", null, FeatureType.Limit, 4, "camareros");
        var feature3 = new TestableFeature("PRIORITY_SUPPORT", "Soporte prioritario", null, FeatureType.Boolean);

        // Act
        plan.AddFeature(feature1);
        plan.AddFeature(feature2);
        plan.AddFeature(feature3);

        // Assert
        plan.Features.Should().HaveCount(3);
        plan.Features.Should().Contain(f => f.Code == "RESERVATIONS_MONTHLY");
        plan.Features.Should().Contain(f => f.Code == "ACTIVE_WAITERS");
        plan.Features.Should().Contain(f => f.Code == "PRIORITY_SUPPORT");
    }

    [Fact]
    public void Plan_WithMultipleProviders_ShouldContainAllProviders()
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());
        var stripeConfig = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456");
        var paddleConfig = new TestablePaymentProviderConfig("Paddle", "prod_789", "price_012", false);

        // Act
        plan.AddProviderConfiguration(stripeConfig);
        plan.AddProviderConfiguration(paddleConfig);

        // Assert
        plan.ProviderConfigurations.Should().HaveCount(2);
        plan.ProviderConfigurations.Should().Contain(p => p.Provider == "Stripe");
        plan.ProviderConfigurations.Should().Contain(p => p.Provider == "Paddle");
    }

    [Theory]
    [InlineData(BillingPeriod.Monthly)]
    [InlineData(BillingPeriod.Quarterly)]
    [InlineData(BillingPeriod.Semester)]
    [InlineData(BillingPeriod.Yearly)]
    public void Plan_WithDifferentBillingPeriods_ShouldSetCorrectly(BillingPeriod period)
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());

        // Act
        plan.SetBillingPeriod(period);

        // Assert
        plan.BillingPeriod.Should().Be(period);
    }

    [Fact]
    public void Plan_WithDifferentPrices_ShouldSetCorrectly()
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());
        var price1 = new TestableMoney(9.99m, Currency.EUR);
        var price2 = new TestableMoney(19.99m, Currency.USD);

        // Act
        plan.SetPrice(price1);
        var firstPrice = plan.Price;
        plan.SetPrice(price2);
        var secondPrice = plan.Price;

        // Assert
        firstPrice.Should().Be(price1);
        secondPrice.Should().Be(price2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Plan_IsActive_ShouldSetCorrectly(bool isActive)
    {
        // Arrange
        var plan = new TestablePlan(Guid.NewGuid());

        // Act
        plan.SetIsActive(isActive);

        // Assert
        plan.IsActive.Should().Be(isActive);
    }
}
