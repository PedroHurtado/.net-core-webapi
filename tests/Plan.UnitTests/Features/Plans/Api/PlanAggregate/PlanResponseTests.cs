namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate;

public class PlanResponseTests
{
    #region PlanResponse.Map Tests

    [Fact]
    public void Map_WithAllProperties_MapsCorrectly()
    {
        var feature = new TestableFeature(
            "RESERVATIONS_MONTHLY",
            "Reservas mensuales",
            "Límite de reservas por mes",
            FeatureType.Limit,
            100,
            "reservas/mes");
        var provider = new TestablePaymentProviderConfig("Stripe", "prod_xxx", "price_xxx", true);
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider]);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Básico");
        plan.SetDescription("Ideal para empezar");
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddPricingTier(tier);

        var response = PlanResponse.Map(plan);

        response.Id.Should().Be(plan.Id);
        response.Name.Should().Be("Plan Básico");
        response.Description.Should().Be("Ideal para empezar");
        response.IsActive.Should().BeTrue();
        response.HasActivePricingTierWithProvider.Should().BeTrue();
    }

    [Fact]
    public void Map_MapsComputedProperties()
    {
        var provider = new TestablePaymentProviderConfig("Stripe", "prod_xxx", "price_xxx", true);
        var tier = new PricingTier(BillingPeriod.Yearly, new TestableMoney(19.99m, Currency.EUR), true, [provider]);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Pro");
        plan.SetDescription("Para profesionales");
        plan.SetIsActive(true);
        plan.AddPricingTier(tier);

        var response = PlanResponse.Map(plan);

        response.HasActivePricingTierWithProvider.Should().Be(plan.HasActivePricingTierWithProvider);
    }

    [Fact]
    public void Map_WithInactivePlan_MapsCorrectly()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Enterprise");
        plan.SetDescription("Para grandes empresas");
        plan.SetIsActive(false);

        var response = PlanResponse.Map(plan);

        response.IsActive.Should().BeFalse();
        response.HasActivePricingTierWithProvider.Should().BeFalse();
        response.Features.Should().BeEmpty();
        response.PricingTiers.Should().BeEmpty();
    }

    [Fact]
    public void Map_WithMultipleFeatures_MapsAllFeatures()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Premium");
        plan.SetDescription("Todo incluido");
        plan.AddFeature(new TestableFeature("RESERVATIONS_MONTHLY", "Reservas", null, FeatureType.Limit, 500));
        plan.AddFeature(new TestableFeature("PRIORITY_SUPPORT", "Soporte prioritario", null, FeatureType.Boolean));
        plan.AddFeature(new TestableFeature("UNLIMITED_MENUS", "Menús ilimitados", null, FeatureType.Unlimited));

        var response = PlanResponse.Map(plan);

        response.Features.Should().HaveCount(3);
        response.Features.Should().Contain(f => f.Code == "RESERVATIONS_MONTHLY" && f.Type == FeatureType.Limit);
        response.Features.Should().Contain(f => f.Code == "PRIORITY_SUPPORT" && f.Type == FeatureType.Boolean);
        response.Features.Should().Contain(f => f.Code == "UNLIMITED_MENUS" && f.Type == FeatureType.Unlimited);
    }

    [Fact]
    public void Map_WithMultiplePricingTiers_MapsAllTiers()
    {
        var provider1 = new TestablePaymentProviderConfig("Stripe", "prod_stripe", "price_stripe", true);
        var provider2 = new TestablePaymentProviderConfig("Paddle", "pro_paddle", "pri_paddle", false);
        var monthlyTier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider1]);
        var yearlyTier = new PricingTier(BillingPeriod.Yearly, new TestableMoney(99.99m, Currency.EUR), true, [provider2]);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Básico");
        plan.SetDescription("Ideal para empezar");
        plan.AddPricingTier(monthlyTier);
        plan.AddPricingTier(yearlyTier);

        var response = PlanResponse.Map(plan);

        response.PricingTiers.Should().HaveCount(2);
        response.PricingTiers.Should().Contain(t => t.BillingPeriod == BillingPeriod.Monthly && t.Price.Amount == 9.99m);
        response.PricingTiers.Should().Contain(t => t.BillingPeriod == BillingPeriod.Yearly && t.Price.Amount == 99.99m);
    }

    [Fact]
    public void Map_PricingTier_MapsProviderConfigurations()
    {
        var provider1 = new TestablePaymentProviderConfig("Stripe", "prod_stripe", "price_stripe", true);
        var provider2 = new TestablePaymentProviderConfig("Paddle", "pro_paddle", "pri_paddle", false);
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider1, provider2]);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Básico");
        plan.SetDescription("Ideal para empezar");
        plan.AddPricingTier(tier);

        var response = PlanResponse.Map(plan);

        var tierResponse = response.PricingTiers.First();
        tierResponse.ProviderConfigurations.Should().HaveCount(2);
        tierResponse.ProviderConfigurations.Should().Contain(p => p.Provider == "Stripe" && p.IsActive);
        tierResponse.ProviderConfigurations.Should().Contain(p => p.Provider == "Paddle" && !p.IsActive);
    }

    [Fact]
    public void Map_WithDifferentBillingPeriods_MapsCorrectly()
    {
        var tier = new PricingTier(BillingPeriod.Semester, new TestableMoney(99.99m, Currency.EUR), true, []);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Semestral");
        plan.SetDescription("Ahorra con el plan semestral");
        plan.AddPricingTier(tier);

        var response = PlanResponse.Map(plan);

        response.PricingTiers.Should().ContainSingle(t => t.BillingPeriod == BillingPeriod.Semester);
    }

    #endregion

    #region MoneyResponse.Map Tests

    [Fact]
    public void MoneyMap_WithEUR_MapsCorrectly()
    {
        var currency = new TestableCurrency("EUR", "€", 2);
        var money = new TestableMoney(9.99m, currency);

        var response = MoneyResponse.Map(money);

        response.Amount.Should().Be(9.99m);
        response.Currency.Code.Should().Be("EUR");
        response.Currency.Symbol.Should().Be("€");
        response.Currency.DecimalPlaces.Should().Be(2);
    }

    [Fact]
    public void MoneyMap_WithUSD_MapsCorrectly()
    {
        var currency = new TestableCurrency("USD", "$", 2);
        var money = new TestableMoney(19.99m, currency);

        var response = MoneyResponse.Map(money);

        response.Amount.Should().Be(19.99m);
        response.Currency.Code.Should().Be("USD");
        response.Currency.Symbol.Should().Be("$");
    }

    [Fact]
    public void MoneyMap_WithZeroAmount_MapsCorrectly()
    {
        var currency = new TestableCurrency("EUR", "€", 2);
        var money = new TestableMoney(0m, currency);

        var response = MoneyResponse.Map(money);

        response.Amount.Should().Be(0m);
    }

    [Fact]
    public void MoneyMap_WithJPY_MapsCorrectly()
    {
        var currency = new TestableCurrency("JPY", "¥", 0);
        var money = new TestableMoney(1000m, currency);

        var response = MoneyResponse.Map(money);

        response.Amount.Should().Be(1000m);
        response.Currency.Code.Should().Be("JPY");
        response.Currency.Symbol.Should().Be("¥");
        response.Currency.DecimalPlaces.Should().Be(0);
    }

    #endregion

    #region CurrencyResponse.Map Tests

    [Fact]
    public void CurrencyMap_WithAllProperties_MapsCorrectly()
    {
        var currency = new TestableCurrency("GBP", "£", 2);

        var response = CurrencyResponse.Map(currency);

        response.Code.Should().Be("GBP");
        response.Symbol.Should().Be("£");
        response.DecimalPlaces.Should().Be(2);
    }

    [Fact]
    public void CurrencyMap_WithZeroDecimalPlaces_MapsCorrectly()
    {
        var currency = new TestableCurrency("JPY", "¥", 0);

        var response = CurrencyResponse.Map(currency);

        response.Code.Should().Be("JPY");
        response.DecimalPlaces.Should().Be(0);
    }

    #endregion

    #region FeatureResponse.Map Tests

    [Fact]
    public void FeatureMap_WithLimitType_MapsCorrectly()
    {
        var feature = new TestableFeature(
            "RESERVATIONS_MONTHLY",
            "Reservas mensuales",
            "Número máximo de reservas por mes",
            FeatureType.Limit,
            100,
            "reservas/mes");

        var response = FeatureResponse.Map(feature);

        response.Code.Should().Be("RESERVATIONS_MONTHLY");
        response.Name.Should().Be("Reservas mensuales");
        response.Description.Should().Be("Número máximo de reservas por mes");
        response.Type.Should().Be(FeatureType.Limit);
        response.Limit.Should().Be(100);
        response.Unit.Should().Be("reservas/mes");
        response.DisplayValue.Should().Be(feature.DisplayValue);
    }

    [Fact]
    public void FeatureMap_WithBooleanType_MapsCorrectly()
    {
        var feature = new TestableFeature(
            "PRIORITY_SUPPORT",
            "Soporte prioritario",
            "Acceso a soporte prioritario 24/7",
            FeatureType.Boolean);

        var response = FeatureResponse.Map(feature);

        response.Code.Should().Be("PRIORITY_SUPPORT");
        response.Name.Should().Be("Soporte prioritario");
        response.Type.Should().Be(FeatureType.Boolean);
        response.Limit.Should().BeNull();
        response.Unit.Should().BeNull();
    }

    [Fact]
    public void FeatureMap_WithUnlimitedType_MapsCorrectly()
    {
        var feature = new TestableFeature("UNLIMITED_MENUS", "Menús ilimitados", null, FeatureType.Unlimited);

        var response = FeatureResponse.Map(feature);

        response.Code.Should().Be("UNLIMITED_MENUS");
        response.Name.Should().Be("Menús ilimitados");
        response.Type.Should().Be(FeatureType.Unlimited);
        response.Limit.Should().BeNull();
    }

    [Fact]
    public void FeatureMap_WithNullDescription_MapsCorrectly()
    {
        var feature = new TestableFeature("BASIC_FEATURE", "Característica básica", null, FeatureType.Boolean);

        var response = FeatureResponse.Map(feature);

        response.Description.Should().BeNull();
    }

    [Fact]
    public void FeatureMap_WithNullUnit_MapsCorrectly()
    {
        var feature = new TestableFeature("USERS_LIMIT", "Usuarios", null, FeatureType.Limit, 5, null);

        var response = FeatureResponse.Map(feature);

        response.Unit.Should().BeNull();
        response.Limit.Should().Be(5);
    }

    #endregion

    #region ProviderConfigResponse.Map Tests

    [Fact]
    public void ProviderConfigMap_WithActiveConfig_MapsCorrectly()
    {
        var config = new TestablePaymentProviderConfig("Stripe", "prod_abc123", "price_xyz789", true);

        var response = ProviderConfigResponse.Map(config);

        response.Provider.Should().Be("Stripe");
        response.ExternalProductId.Should().Be("prod_abc123");
        response.ExternalPriceId.Should().Be("price_xyz789");
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ProviderConfigMap_WithInactiveConfig_MapsCorrectly()
    {
        var config = new TestablePaymentProviderConfig("Paddle", "pro_123", "pri_456", false);

        var response = ProviderConfigResponse.Map(config);

        response.Provider.Should().Be("Paddle");
        response.ExternalProductId.Should().Be("pro_123");
        response.ExternalPriceId.Should().Be("pri_456");
        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ProviderConfigMap_WithDifferentProviders_MapsCorrectly()
    {
        var stripeConfig = new TestablePaymentProviderConfig("Stripe", "prod_stripe", "price_stripe", true);
        var paddleConfig = new TestablePaymentProviderConfig("Paddle", "pro_paddle", "pri_paddle", true);

        var stripeResponse = ProviderConfigResponse.Map(stripeConfig);
        var paddleResponse = ProviderConfigResponse.Map(paddleConfig);

        stripeResponse.Provider.Should().Be("Stripe");
        paddleResponse.Provider.Should().Be("Paddle");
    }

    #endregion
}
