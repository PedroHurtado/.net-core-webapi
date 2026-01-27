namespace Plans.UnitTests.Features.Plans.Api.PlanAggregate;

public class PlanResponseTests
{
    #region PlanResponse.Map Tests

    [Fact]
    public void Map_WithAllProperties_MapsCorrectly()
    {
        // Arrange
        var currency = new TestableCurrency("EUR", "€", 2);
        var price = new TestableMoney(9.99m, currency);
        var feature = new TestableFeature(
            "RESERVATIONS_MONTHLY",
            "Reservas mensuales",
            "Límite de reservas por mes",
            FeatureType.Limit,
            100,
            "reservas/mes");
        var providerConfig = new TestablePaymentProviderConfig("Stripe", "prod_xxx", "price_xxx", true);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Básico");
        plan.SetDescription("Ideal para empezar");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddProviderConfiguration(providerConfig);

        // Act
        var response = PlanResponse.Map(plan);

        // Assert
        response.Id.Should().Be(plan.Id);
        response.Name.Should().Be("Plan Básico");
        response.Description.Should().Be("Ideal para empezar");
        response.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        response.IsActive.Should().BeTrue();
        response.HasActiveProvider.Should().BeTrue();
    }

    [Fact]
    public void Map_MapsComputedProperties()
    {
        // Arrange
        var currency = new TestableCurrency("EUR", "€", 2);
        var price = new TestableMoney(19.99m, currency);
        var feature = new TestableFeature("PRIORITY_SUPPORT", "Soporte prioritario", null, FeatureType.Boolean);
        var providerConfig = new TestablePaymentProviderConfig("Stripe", "prod_xxx", "price_xxx", true);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Pro");
        plan.SetDescription("Para profesionales");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Yearly);
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddProviderConfiguration(providerConfig);

        // Act
        var response = PlanResponse.Map(plan);

        // Assert
        response.HasActiveProvider.Should().Be(plan.HasActiveProvider);
    }

    [Fact]
    public void Map_WithInactivePlan_MapsCorrectly()
    {
        // Arrange
        var currency = new TestableCurrency("EUR", "€", 2);
        var price = new TestableMoney(29.99m, currency);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Enterprise");
        plan.SetDescription("Para grandes empresas");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Quarterly);
        plan.SetIsActive(false);

        // Act
        var response = PlanResponse.Map(plan);

        // Assert
        response.IsActive.Should().BeFalse();
        response.HasActiveProvider.Should().BeFalse();
        response.Features.Should().BeEmpty();
        response.ProviderConfigurations.Should().BeEmpty();
    }

    [Fact]
    public void Map_WithMultipleFeatures_MapsAllFeatures()
    {
        // Arrange
        var currency = new TestableCurrency("EUR", "€", 2);
        var price = new TestableMoney(49.99m, currency);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Premium");
        plan.SetDescription("Todo incluido");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.AddFeature(new TestableFeature("RESERVATIONS_MONTHLY", "Reservas", null, FeatureType.Limit, 500));
        plan.AddFeature(new TestableFeature("PRIORITY_SUPPORT", "Soporte prioritario", null, FeatureType.Boolean));
        plan.AddFeature(new TestableFeature("UNLIMITED_MENUS", "Menús ilimitados", null, FeatureType.Unlimited));

        // Act
        var response = PlanResponse.Map(plan);

        // Assert
        response.Features.Should().HaveCount(3);
        response.Features.Should().Contain(f => f.Code == "RESERVATIONS_MONTHLY" && f.Type == FeatureType.Limit);
        response.Features.Should().Contain(f => f.Code == "PRIORITY_SUPPORT" && f.Type == FeatureType.Boolean);
        response.Features.Should().Contain(f => f.Code == "UNLIMITED_MENUS" && f.Type == FeatureType.Unlimited);
    }

    [Fact]
    public void Map_WithMultipleProviderConfigurations_MapsAllConfigurations()
    {
        // Arrange
        var currency = new TestableCurrency("EUR", "€", 2);
        var price = new TestableMoney(9.99m, currency);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Básico");
        plan.SetDescription("Ideal para empezar");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.AddProviderConfiguration(new TestablePaymentProviderConfig("Stripe", "prod_stripe", "price_stripe", true));
        plan.AddProviderConfiguration(new TestablePaymentProviderConfig("Paddle", "pro_paddle", "pri_paddle", false));

        // Act
        var response = PlanResponse.Map(plan);

        // Assert
        response.ProviderConfigurations.Should().HaveCount(2);
        response.ProviderConfigurations.Should().Contain(p => p.Provider == "Stripe" && p.IsActive);
        response.ProviderConfigurations.Should().Contain(p => p.Provider == "Paddle" && !p.IsActive);
    }

    [Fact]
    public void Map_WithDifferentBillingPeriods_MapsCorrectly()
    {
        // Arrange
        var currency = new TestableCurrency("EUR", "€", 2);
        var price = new TestableMoney(99.99m, currency);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Plan Semestral");
        plan.SetDescription("Ahorra con el plan semestral");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Semester);

        // Act
        var response = PlanResponse.Map(plan);

        // Assert
        response.BillingPeriod.Should().Be(BillingPeriod.Semester);
    }

    #endregion

    #region MoneyResponse.Map Tests

    [Fact]
    public void MoneyMap_WithEUR_MapsCorrectly()
    {
        // Arrange
        var currency = new TestableCurrency("EUR", "€", 2);
        var money = new TestableMoney(9.99m, currency);

        // Act
        var response = MoneyResponse.Map(money);

        // Assert
        response.Amount.Should().Be(9.99m);
        response.Currency.Code.Should().Be("EUR");
        response.Currency.Symbol.Should().Be("€");
        response.Currency.DecimalPlaces.Should().Be(2);
    }

    [Fact]
    public void MoneyMap_WithUSD_MapsCorrectly()
    {
        // Arrange
        var currency = new TestableCurrency("USD", "$", 2);
        var money = new TestableMoney(19.99m, currency);

        // Act
        var response = MoneyResponse.Map(money);

        // Assert
        response.Amount.Should().Be(19.99m);
        response.Currency.Code.Should().Be("USD");
        response.Currency.Symbol.Should().Be("$");
    }

    [Fact]
    public void MoneyMap_WithZeroAmount_MapsCorrectly()
    {
        // Arrange
        var currency = new TestableCurrency("EUR", "€", 2);
        var money = new TestableMoney(0m, currency);

        // Act
        var response = MoneyResponse.Map(money);

        // Assert
        response.Amount.Should().Be(0m);
    }

    [Fact]
    public void MoneyMap_WithJPY_MapsCorrectly()
    {
        // Arrange
        var currency = new TestableCurrency("JPY", "¥", 0);
        var money = new TestableMoney(1000m, currency);

        // Act
        var response = MoneyResponse.Map(money);

        // Assert
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
        // Arrange
        var currency = new TestableCurrency("GBP", "£", 2);

        // Act
        var response = CurrencyResponse.Map(currency);

        // Assert
        response.Code.Should().Be("GBP");
        response.Symbol.Should().Be("£");
        response.DecimalPlaces.Should().Be(2);
    }

    [Fact]
    public void CurrencyMap_WithZeroDecimalPlaces_MapsCorrectly()
    {
        // Arrange
        var currency = new TestableCurrency("JPY", "¥", 0);

        // Act
        var response = CurrencyResponse.Map(currency);

        // Assert
        response.Code.Should().Be("JPY");
        response.DecimalPlaces.Should().Be(0);
    }

    #endregion

    #region FeatureResponse.Map Tests

    [Fact]
    public void FeatureMap_WithLimitType_MapsCorrectly()
    {
        // Arrange
        var feature = new TestableFeature(
            "RESERVATIONS_MONTHLY",
            "Reservas mensuales",
            "Número máximo de reservas por mes",
            FeatureType.Limit,
            100,
            "reservas/mes");

        // Act
        var response = FeatureResponse.Map(feature);

        // Assert
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
        // Arrange
        var feature = new TestableFeature(
            "PRIORITY_SUPPORT",
            "Soporte prioritario",
            "Acceso a soporte prioritario 24/7",
            FeatureType.Boolean);

        // Act
        var response = FeatureResponse.Map(feature);

        // Assert
        response.Code.Should().Be("PRIORITY_SUPPORT");
        response.Name.Should().Be("Soporte prioritario");
        response.Type.Should().Be(FeatureType.Boolean);
        response.Limit.Should().BeNull();
        response.Unit.Should().BeNull();
    }

    [Fact]
    public void FeatureMap_WithUnlimitedType_MapsCorrectly()
    {
        // Arrange
        var feature = new TestableFeature("UNLIMITED_MENUS", "Menús ilimitados", null, FeatureType.Unlimited);

        // Act
        var response = FeatureResponse.Map(feature);

        // Assert
        response.Code.Should().Be("UNLIMITED_MENUS");
        response.Name.Should().Be("Menús ilimitados");
        response.Type.Should().Be(FeatureType.Unlimited);
        response.Limit.Should().BeNull();
    }

    [Fact]
    public void FeatureMap_WithNullDescription_MapsCorrectly()
    {
        // Arrange
        var feature = new TestableFeature("BASIC_FEATURE", "Característica básica", null, FeatureType.Boolean);

        // Act
        var response = FeatureResponse.Map(feature);

        // Assert
        response.Description.Should().BeNull();
    }

    [Fact]
    public void FeatureMap_WithNullUnit_MapsCorrectly()
    {
        // Arrange
        var feature = new TestableFeature("USERS_LIMIT", "Usuarios", null, FeatureType.Limit, 5, null);

        // Act
        var response = FeatureResponse.Map(feature);

        // Assert
        response.Unit.Should().BeNull();
        response.Limit.Should().Be(5);
    }

    #endregion

    #region ProviderConfigResponse.Map Tests

    [Fact]
    public void ProviderConfigMap_WithActiveConfig_MapsCorrectly()
    {
        // Arrange
        var config = new TestablePaymentProviderConfig("Stripe", "prod_abc123", "price_xyz789", true);

        // Act
        var response = ProviderConfigResponse.Map(config);

        // Assert
        response.Provider.Should().Be("Stripe");
        response.ExternalProductId.Should().Be("prod_abc123");
        response.ExternalPriceId.Should().Be("price_xyz789");
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ProviderConfigMap_WithInactiveConfig_MapsCorrectly()
    {
        // Arrange
        var config = new TestablePaymentProviderConfig("Paddle", "pro_123", "pri_456", false);

        // Act
        var response = ProviderConfigResponse.Map(config);

        // Assert
        response.Provider.Should().Be("Paddle");
        response.ExternalProductId.Should().Be("pro_123");
        response.ExternalPriceId.Should().Be("pri_456");
        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ProviderConfigMap_WithDifferentProviders_MapsCorrectly()
    {
        // Arrange
        var stripeConfig = new TestablePaymentProviderConfig("Stripe", "prod_stripe", "price_stripe", true);
        var paddleConfig = new TestablePaymentProviderConfig("Paddle", "pro_paddle", "pri_paddle", true);

        // Act
        var stripeResponse = ProviderConfigResponse.Map(stripeConfig);
        var paddleResponse = ProviderConfigResponse.Map(paddleConfig);

        // Assert
        stripeResponse.Provider.Should().Be("Stripe");
        paddleResponse.Provider.Should().Be("Paddle");
    }

    #endregion
}
