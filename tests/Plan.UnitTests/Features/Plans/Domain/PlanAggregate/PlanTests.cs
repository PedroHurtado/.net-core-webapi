namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate;

public class PlanTests
{
    [Fact]
    public void Plan_WithValidData_ShouldHaveCorrectProperties()
    {
        var id = Guid.NewGuid();
        var plan = new TestablePlan(id);
        var feature = new TestableFeature("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Limit, 100, "reservas");
        var tier = new PricingTier(
            BillingPeriod.Monthly,
            new TestableMoney(9.99m, Currency.EUR),
            true,
            [new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", true)]);

        plan.SetName("Plan Básico");
        plan.SetDescription("Plan ideal para empezar");
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddPricingTier(tier);

        plan.Id.Should().Be(id);
        plan.Name.Should().Be("Plan Básico");
        plan.Description.Should().Be("Plan ideal para empezar");
        plan.IsActive.Should().BeTrue();
        plan.Features.Should().ContainSingle();
        plan.PricingTiers.Should().ContainSingle();
    }

    [Fact]
    public void HasActivePricingTierWithProvider_WithActiveTierAndProvider_ShouldReturnTrue()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        var tier = new PricingTier(
            BillingPeriod.Monthly,
            new TestableMoney(9.99m, Currency.EUR),
            true,
            [new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", true)]);

        plan.AddPricingTier(tier);

        plan.HasActivePricingTierWithProvider.Should().BeTrue();
    }

    [Fact]
    public void HasActivePricingTierWithProvider_WithInactiveTier_ShouldReturnFalse()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        var tier = new PricingTier(
            BillingPeriod.Monthly,
            new TestableMoney(9.99m, Currency.EUR),
            false,
            [new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", true)]);

        plan.AddPricingTier(tier);

        plan.HasActivePricingTierWithProvider.Should().BeFalse();
    }

    [Fact]
    public void HasActivePricingTierWithProvider_WithInactiveProvider_ShouldReturnFalse()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        var tier = new PricingTier(
            BillingPeriod.Monthly,
            new TestableMoney(9.99m, Currency.EUR),
            true,
            [new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", false)]);

        plan.AddPricingTier(tier);

        plan.HasActivePricingTierWithProvider.Should().BeFalse();
    }

    [Fact]
    public void HasActivePricingTierWithProvider_WithMultipleTiersOneActive_ShouldReturnTrue()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        var inactiveTier = new PricingTier(
            BillingPeriod.Monthly,
            new TestableMoney(9.99m, Currency.EUR),
            false,
            [new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", true)]);
        var activeTier = new PricingTier(
            BillingPeriod.Yearly,
            new TestableMoney(89.99m, Currency.EUR),
            true,
            [new TestablePaymentProviderConfig("Stripe", "prod_789", "price_012", true)]);

        plan.AddPricingTier(inactiveTier);
        plan.AddPricingTier(activeTier);

        plan.HasActivePricingTierWithProvider.Should().BeTrue();
    }

    [Fact]
    public void Features_ShouldReturnReadOnlyCollection()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Boolean);

        plan.AddFeature(feature);

        plan.Features.Should().BeAssignableTo<IReadOnlyCollection<Feature>>();
        plan.Features.Should().ContainSingle();
    }

    [Fact]
    public void PricingTiers_ShouldReturnReadOnlyCollection()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        var tier = new PricingTier(
            BillingPeriod.Monthly,
            new TestableMoney(9.99m, Currency.EUR),
            true,
            Array.Empty<PaymentProviderConfig>().AsReadOnly());

        plan.AddPricingTier(tier);

        plan.PricingTiers.Should().BeAssignableTo<IReadOnlyCollection<PricingTier>>();
        plan.PricingTiers.Should().ContainSingle();
    }

    [Fact]
    public void Plan_WithMultipleFeatures_ShouldContainAllFeatures()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        var feature1 = new TestableFeature("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Limit, 100, "reservas");
        var feature2 = new TestableFeature("ACTIVE_WAITERS", "Camareros activos", null, FeatureType.Limit, 4, "camareros");
        var feature3 = new TestableFeature("PRIORITY_SUPPORT", "Soporte prioritario", null, FeatureType.Boolean);

        plan.AddFeature(feature1);
        plan.AddFeature(feature2);
        plan.AddFeature(feature3);

        plan.Features.Should().HaveCount(3);
        plan.Features.Should().Contain(f => f.Code == "RESERVATIONS_MONTHLY");
        plan.Features.Should().Contain(f => f.Code == "ACTIVE_WAITERS");
        plan.Features.Should().Contain(f => f.Code == "PRIORITY_SUPPORT");
    }

    [Fact]
    public void Plan_WithMultiplePricingTiers_ShouldContainAllTiers()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        var monthlyTier = new PricingTier(
            BillingPeriod.Monthly,
            new TestableMoney(9.99m, Currency.EUR),
            true,
            Array.Empty<PaymentProviderConfig>().AsReadOnly());
        var yearlyTier = new PricingTier(
            BillingPeriod.Yearly,
            new TestableMoney(89.99m, Currency.EUR),
            true,
            Array.Empty<PaymentProviderConfig>().AsReadOnly());

        plan.AddPricingTier(monthlyTier);
        plan.AddPricingTier(yearlyTier);

        plan.PricingTiers.Should().HaveCount(2);
        plan.PricingTiers.Should().Contain(t => t.BillingPeriod == BillingPeriod.Monthly);
        plan.PricingTiers.Should().Contain(t => t.BillingPeriod == BillingPeriod.Yearly);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Plan_IsActive_ShouldSetCorrectly(bool isActive)
    {
        var plan = new TestablePlan(Guid.NewGuid());

        plan.SetIsActive(isActive);

        plan.IsActive.Should().Be(isActive);
    }
}
