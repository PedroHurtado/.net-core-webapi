using FluentAssertions;
using Schedule.Features.Plan.Models;
using Xunit;

namespace ScheDule.UnitTests.Features.Plan;

public class PlanTests
{
    #region Helper Methods

    private static Feature CreateValidLimitFeature(string code = "RESERVATIONS_MONTHLY", int limit = 100) =>
        new(code, "Reservas mensuales", null, FeatureType.Limit, limit, "reservas/mes");

    private static Feature CreateValidBooleanFeature(string code = "PRIORITY_SUPPORT") =>
        new(code, "Soporte prioritario", "Respuesta en 24h", FeatureType.Boolean);

    private static Feature CreateValidUnlimitedFeature(string code = "RESERVATIONS_MONTHLY") =>
        new(code, "Reservas mensuales", null, FeatureType.Unlimited);

    private static PaymentProviderConfig CreateValidProviderConfig(string provider = "Stripe", bool isActive = true) =>
        new(provider, "prod_xxx", "price_xxx", isActive);

    private static Money CreateValidPrice() => new(9.99m, Currency.EUR);

    private static Schedule.Features.Plan.Models.Plan CreateValidPlan(
        Guid? id = null,
        string name = "Plan Básico",
        string description = "Perfecto para comenzar",
        Money? price = null,
        BillingPeriod billingPeriod = BillingPeriod.Monthly,
        IEnumerable<Feature>? features = null,
        IEnumerable<PaymentProviderConfig>? providerConfigurations = null,
        bool isActive = true)
    {
        return Schedule.Features.Plan.Models.Plan.Create(
            id ?? Guid.NewGuid(),
            name,
            description,
            price ?? CreateValidPrice(),
            billingPeriod,
            features ?? [CreateValidLimitFeature()],
            providerConfigurations ?? [CreateValidProviderConfig()],
            isActive).Value!;
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Plan Básico";
        var description = "Perfecto para comenzar";
        var price = new Money(9.99m, Currency.EUR);
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            id, name, description, price, BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.Price.Should().Be(price);
        result.Value.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        result.Value.IsActive.Should().BeTrue();
        result.Value.Features.Should().HaveCount(1);
        result.Value.ProviderConfigurations.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "", "Description", CreateValidPrice(),
            BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('a', 101);
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), longName, "Description", CreateValidPrice(),
            BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldReturnFailure()
    {
        // Arrange
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "", CreateValidPrice(),
            BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var longDescription = new string('a', 501);
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", longDescription, CreateValidPrice(),
            BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldReturnFailure()
    {
        // Arrange
        var negativePrice = new Money(-10m, Currency.EUR);
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "Description", negativePrice,
            BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Price.Amount");
    }

    [Fact]
    public void Create_WithEmptyFeatures_ShouldReturnFailure()
    {
        // Arrange
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "Description", CreateValidPrice(),
            BillingPeriod.Monthly, [], providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Features");
    }

    [Fact]
    public void Create_WithEmptyProviderConfigurations_ShouldReturnFailure()
    {
        // Arrange
        var features = new[] { CreateValidLimitFeature() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "Description", CreateValidPrice(),
            BillingPeriod.Monthly, features, []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "ProviderConfigurations");
    }

    [Fact]
    public void Create_WithFeatureLimitWithoutValue_ShouldReturnFailure()
    {
        // Arrange
        var invalidFeature = new Feature("RESERVATIONS_MONTHLY", "Reservas", null, FeatureType.Limit, null, "reservas");
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "Description", CreateValidPrice(),
            BillingPeriod.Monthly, [invalidFeature], providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Feature");
    }

    [Fact]
    public void Create_WithFeatureBooleanWithLimit_ShouldReturnFailure()
    {
        // Arrange
        var invalidFeature = new Feature("PRIORITY_SUPPORT", "Soporte", null, FeatureType.Boolean, 50);
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "Description", CreateValidPrice(),
            BillingPeriod.Monthly, [invalidFeature], providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Feature");
    }

    [Fact]
    public void Create_WithDuplicateFeatureCodes_ShouldReturnFailure()
    {
        // Arrange
        var features = new[]
        {
            CreateValidLimitFeature("RESERVATIONS_MONTHLY", 100),
            CreateValidLimitFeature("RESERVATIONS_MONTHLY", 200)
        };
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "Description", CreateValidPrice(),
            BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Feature.Code");
    }

    [Fact]
    public void Create_WithDuplicateActiveProviderConfigurations_ShouldReturnFailure()
    {
        // Arrange
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[]
        {
            CreateValidProviderConfig("Stripe", true),
            new PaymentProviderConfig("Stripe", "prod_yyy", "price_yyy", true)
        };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "Description", CreateValidPrice(),
            BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "ProviderConfiguration.Provider");
    }

    [Fact]
    public void Create_WithAllProviderConfigurationsInactive_ShouldReturnFailure()
    {
        // Arrange
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[] { CreateValidProviderConfig("Stripe", isActive: false) };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan Básico", "Description", CreateValidPrice(),
            BillingPeriod.Monthly, features, providerConfigs);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "ProviderConfigurations");
    }

    #endregion

    #region Feature Management Tests

    [Fact]
    public void AddFeature_WithValidFeature_ShouldReturnSuccess()
    {
        // Arrange
        var plan = CreateValidPlan();
        var newFeature = CreateValidLimitFeature("ACTIVE_WAITERS", 4);

        // Act
        var result = plan.AddFeature(newFeature);

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.Features.Should().HaveCount(2);
        plan.Features.Should().Contain(f => f.Code == "ACTIVE_WAITERS");
    }

    [Fact]
    public void AddFeature_WithDuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var plan = CreateValidPlan();
        var duplicateFeature = CreateValidLimitFeature("RESERVATIONS_MONTHLY", 200);

        // Act
        var result = plan.AddFeature(duplicateFeature);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Feature.Code");
    }

    [Fact]
    public void AddFeature_WithLowercaseCode_ShouldNormalizeToUppercase()
    {
        // Arrange
        var plan = CreateValidPlan();
        var featureWithLowercase = new Feature("active_waiters", "Camareros", null, FeatureType.Limit, 4, "camareros");

        // Act
        var result = plan.AddFeature(featureWithLowercase);

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.Features.Should().Contain(f => f.Code == "ACTIVE_WAITERS");
    }

    [Fact]
    public void UpdateFeature_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var plan = CreateValidPlan();
        var updatedFeature = CreateValidLimitFeature("RESERVATIONS_MONTHLY", 200);

        // Act
        var result = plan.UpdateFeature("RESERVATIONS_MONTHLY", updatedFeature);

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.GetFeatureByCode("RESERVATIONS_MONTHLY")!.Limit.Should().Be(200);
    }

    [Fact]
    public void UpdateFeature_ChangingToUnlimited_ShouldReturnSuccess()
    {
        // Arrange
        var plan = CreateValidPlan();
        var unlimitedFeature = CreateValidUnlimitedFeature("RESERVATIONS_MONTHLY");

        // Act
        var result = plan.UpdateFeature("RESERVATIONS_MONTHLY", unlimitedFeature);

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.IsFeatureUnlimited("RESERVATIONS_MONTHLY").Should().BeTrue();
    }

    [Fact]
    public void UpdateFeature_WithNonExistentCode_ShouldReturnFailure()
    {
        // Arrange
        var plan = CreateValidPlan();
        var updatedFeature = CreateValidLimitFeature("NONEXISTENT", 100);

        // Act
        var result = plan.UpdateFeature("NONEXISTENT", updatedFeature);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Feature.Code");
    }

    [Fact]
    public void RemoveFeature_WithMultipleFeatures_ShouldReturnSuccess()
    {
        // Arrange
        var features = new[] { CreateValidLimitFeature(), CreateValidBooleanFeature() };
        var plan = CreateValidPlan(features: features);

        // Act
        var result = plan.RemoveFeature("PRIORITY_SUPPORT");

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.Features.Should().HaveCount(1);
        plan.HasFeature("PRIORITY_SUPPORT").Should().BeFalse();
    }

    [Fact]
    public void RemoveFeature_WhenOnlyOneFeature_ShouldReturnFailure()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var result = plan.RemoveFeature("RESERVATIONS_MONTHLY");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Features");
    }

    [Fact]
    public void RemoveFeature_WithNonExistentCode_ShouldReturnFailure()
    {
        // Arrange
        var features = new[] { CreateValidLimitFeature(), CreateValidBooleanFeature() };
        var plan = CreateValidPlan(features: features);

        // Act
        var result = plan.RemoveFeature("NONEXISTENT");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Feature.Code");
    }

    [Fact]
    public void GetFeatureByCode_WithExistingCode_ShouldReturnFeature()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var feature = plan.GetFeatureByCode("RESERVATIONS_MONTHLY");

        // Assert
        feature.Should().NotBeNull();
        feature!.Code.Should().Be("RESERVATIONS_MONTHLY");
    }

    [Fact]
    public void GetFeatureByCode_WithNonExistentCode_ShouldReturnNull()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var feature = plan.GetFeatureByCode("NONEXISTENT");

        // Assert
        feature.Should().BeNull();
    }

    [Fact]
    public void GetFeatureByCode_IsCaseInsensitive()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var feature = plan.GetFeatureByCode("reservations_monthly");

        // Assert
        feature.Should().NotBeNull();
        feature!.Code.Should().Be("RESERVATIONS_MONTHLY");
    }

    [Fact]
    public void GetLimitForFeature_WithLimitType_ShouldReturnLimit()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var limit = plan.GetLimitForFeature("RESERVATIONS_MONTHLY");

        // Assert
        limit.Should().Be(100);
    }

    [Fact]
    public void GetLimitForFeature_WithUnlimitedType_ShouldReturnNull()
    {
        // Arrange
        var plan = CreateValidPlan(features: [CreateValidUnlimitedFeature()]);

        // Act
        var limit = plan.GetLimitForFeature("RESERVATIONS_MONTHLY");

        // Assert
        limit.Should().BeNull();
    }

    [Fact]
    public void GetLimitForFeature_WithNonExistentCode_ShouldReturnNull()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var limit = plan.GetLimitForFeature("NONEXISTENT");

        // Assert
        limit.Should().BeNull();
    }

    [Fact]
    public void HasFeature_WithExistingCode_ShouldReturnTrue()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var hasFeature = plan.HasFeature("RESERVATIONS_MONTHLY");

        // Assert
        hasFeature.Should().BeTrue();
    }

    [Fact]
    public void HasFeature_WithNonExistentCode_ShouldReturnFalse()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var hasFeature = plan.HasFeature("NONEXISTENT");

        // Assert
        hasFeature.Should().BeFalse();
    }

    [Fact]
    public void IsFeatureUnlimited_WithUnlimitedFeature_ShouldReturnTrue()
    {
        // Arrange
        var plan = CreateValidPlan(features: [CreateValidUnlimitedFeature()]);

        // Act
        var isUnlimited = plan.IsFeatureUnlimited("RESERVATIONS_MONTHLY");

        // Assert
        isUnlimited.Should().BeTrue();
    }

    [Fact]
    public void IsFeatureUnlimited_WithLimitFeature_ShouldReturnFalse()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var isUnlimited = plan.IsFeatureUnlimited("RESERVATIONS_MONTHLY");

        // Assert
        isUnlimited.Should().BeFalse();
    }

    #endregion

    #region Provider Configuration Tests

    [Fact]
    public void AddProviderConfiguration_WithValidConfig_ShouldReturnSuccess()
    {
        // Arrange
        var plan = CreateValidPlan();
        var newConfig = CreateValidProviderConfig("Paddle");

        // Act
        var result = plan.AddProviderConfiguration(newConfig);

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.ProviderConfigurations.Should().HaveCount(2);
    }

    [Fact]
    public void AddProviderConfiguration_WithDuplicateActiveProvider_ShouldReturnFailure()
    {
        // Arrange
        var plan = CreateValidPlan();
        var duplicateConfig = new PaymentProviderConfig("Stripe", "prod_new", "price_new", true);

        // Act
        var result = plan.AddProviderConfiguration(duplicateConfig);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "ProviderConfiguration.Provider");
    }

    [Fact]
    public void AddProviderConfiguration_WithInactiveConfigForSameProvider_ShouldReturnSuccess()
    {
        // Arrange
        var plan = CreateValidPlan();
        var inactiveConfig = new PaymentProviderConfig("Stripe", "prod_new", "price_new", false);

        // Act
        var result = plan.AddProviderConfiguration(inactiveConfig);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void UpdateProviderConfiguration_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var result = plan.UpdateProviderConfiguration("Stripe", "prod_updated", "price_updated");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var config = plan.GetActiveConfigForProvider("Stripe");
        config!.ExternalProductId.Should().Be("prod_updated");
        config.ExternalPriceId.Should().Be("price_updated");
    }

    [Fact]
    public void UpdateProviderConfiguration_WithNonExistentProvider_ShouldReturnFailure()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var result = plan.UpdateProviderConfiguration("Nonexistent", "prod_new", "price_new");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "ProviderConfiguration.Provider");
    }

    [Fact]
    public void DeactivateProviderConfiguration_WithMultipleActiveConfigs_ShouldReturnSuccess()
    {
        // Arrange
        var providerConfigs = new[]
        {
            CreateValidProviderConfig("Stripe"),
            CreateValidProviderConfig("Paddle")
        };
        var plan = CreateValidPlan(providerConfigurations: providerConfigs);

        // Act
        var result = plan.DeactivateProviderConfiguration("Stripe");

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.HasActiveProviderConfiguration("Stripe").Should().BeFalse();
        plan.HasActiveProviderConfiguration("Paddle").Should().BeTrue();
    }

    [Fact]
    public void DeactivateProviderConfiguration_WhenOnlyOneActiveConfig_ShouldReturnFailure()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var result = plan.DeactivateProviderConfiguration("Stripe");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "ProviderConfigurations");
    }

    [Fact]
    public void ActivateProviderConfiguration_WithInactiveConfig_ShouldReturnSuccess()
    {
        // Arrange
        var providerConfigs = new[]
        {
            CreateValidProviderConfig("Stripe"),
            new PaymentProviderConfig("Paddle", "prod_paddle", "price_paddle", false)
        };
        var plan = CreateValidPlan(providerConfigurations: providerConfigs);

        // Act
        var result = plan.ActivateProviderConfiguration("Paddle");

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.HasActiveProviderConfiguration("Paddle").Should().BeTrue();
    }

    [Fact]
    public void ActivateProviderConfiguration_WhenAlreadyActiveConfigExists_ShouldReturnFailure()
    {
        // Arrange
        var providerConfigs = new[]
        {
            CreateValidProviderConfig("Stripe"),
            new PaymentProviderConfig("Stripe", "prod_old", "price_old", false)
        };
        var plan = CreateValidPlan(providerConfigurations: providerConfigs);

        // Act
        var result = plan.ActivateProviderConfiguration("Stripe");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "ProviderConfiguration.Provider");
    }

    [Fact]
    public void GetActiveConfigForProvider_WithActiveConfig_ShouldReturnConfig()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var config = plan.GetActiveConfigForProvider("Stripe");

        // Assert
        config.Should().NotBeNull();
        config!.Provider.Should().Be("Stripe");
        config.IsActive.Should().BeTrue();
    }

    [Fact]
    public void GetActiveConfigForProvider_WithNoActiveConfig_ShouldReturnNull()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var config = plan.GetActiveConfigForProvider("Paddle");

        // Assert
        config.Should().BeNull();
    }

    [Fact]
    public void HasActiveProviderConfiguration_WithActiveConfig_ShouldReturnTrue()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var hasActive = plan.HasActiveProviderConfiguration("Stripe");

        // Assert
        hasActive.Should().BeTrue();
    }

    [Fact]
    public void HasActiveProviderConfiguration_WithNoActiveConfig_ShouldReturnFalse()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var hasActive = plan.HasActiveProviderConfiguration("Paddle");

        // Assert
        hasActive.Should().BeFalse();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var plan = CreateValidPlan();
        var newPrice = new Money(19.99m, Currency.EUR);

        // Act
        var result = plan.Update("Plan Premium", "Mejor plan", newPrice, BillingPeriod.Yearly);

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.Name.Should().Be("Plan Premium");
        plan.Description.Should().Be("Mejor plan");
        plan.Price.Should().Be(newPrice);
        plan.BillingPeriod.Should().Be(BillingPeriod.Yearly);
    }

    [Fact]
    public void Update_WithInvalidName_ShouldReturnFailure()
    {
        // Arrange
        var plan = CreateValidPlan();

        // Act
        var result = plan.Update("", "Description", CreateValidPrice(), BillingPeriod.Monthly);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var plan = CreateValidPlan(isActive: false);

        // Act
        var result = plan.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var plan = CreateValidPlan(isActive: true);

        // Act
        var result = plan.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        plan.IsActive.Should().BeFalse();
    }

    #endregion

    #region Value Object Tests - Money

    [Fact]
    public void Money_Zero_ShouldCreateMoneyWithZeroAmount()
    {
        // Act
        var money = Money.Zero(Currency.EUR);

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Money_Add_WithSameCurrency_ShouldReturnSuccess()
    {
        // Arrange
        var money1 = new Money(10m, Currency.EUR);
        var money2 = new Money(5m, Currency.EUR);

        // Act
        var result = money1.Add(money2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Amount.Should().Be(15m);
        result.Value.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Money_Add_WithDifferentCurrencies_ShouldReturnFailure()
    {
        // Arrange
        var money1 = new Money(10m, Currency.EUR);
        var money2 = new Money(5m, Currency.USD);

        // Act
        var result = money1.Add(money2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Money.Currency");
    }

    #endregion

    #region Value Object Tests - Currency

    [Fact]
    public void Currency_FromCode_WithValidCode_ShouldReturnSuccess()
    {
        // Act
        var result = Currency.FromCode("EUR");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Currency_FromCode_WithLowercaseCode_ShouldReturnSuccess()
    {
        // Act
        var result = Currency.FromCode("eur");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Currency_FromCode_WithInvalidCode_ShouldReturnFailure()
    {
        // Act
        var result = Currency.FromCode("INVALID");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency.Code");
    }

    [Fact]
    public void Currency_StaticProperties_ShouldHaveCorrectValues()
    {
        // Assert
        Currency.EUR.Code.Should().Be("EUR");
        Currency.EUR.Symbol.Should().Be("€");
        Currency.EUR.DecimalPlaces.Should().Be(2);

        Currency.USD.Code.Should().Be("USD");
        Currency.USD.Symbol.Should().Be("$");

        Currency.GBP.Code.Should().Be("GBP");
        Currency.GBP.Symbol.Should().Be("£");
    }

    #endregion

    #region Value Object Tests - Feature

    [Fact]
    public void Feature_IsValid_WithValidLimitFeature_ShouldBeTrue()
    {
        // Arrange
        var feature = CreateValidLimitFeature();

        // Assert
        feature.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Feature_IsValid_WithLimitFeatureWithoutValue_ShouldBeFalse()
    {
        // Arrange
        var feature = new Feature("TEST", "Test", null, FeatureType.Limit, null);

        // Assert
        feature.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Feature_IsValid_WithLimitFeatureWithZeroValue_ShouldBeFalse()
    {
        // Arrange
        var feature = new Feature("TEST", "Test", null, FeatureType.Limit, 0);

        // Assert
        feature.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Feature_IsValid_WithBooleanFeature_ShouldBeTrue()
    {
        // Arrange
        var feature = CreateValidBooleanFeature();

        // Assert
        feature.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Feature_IsValid_WithBooleanFeatureWithLimit_ShouldBeFalse()
    {
        // Arrange
        var feature = new Feature("TEST", "Test", null, FeatureType.Boolean, 50);

        // Assert
        feature.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Feature_IsValid_WithUnlimitedFeature_ShouldBeTrue()
    {
        // Arrange
        var feature = CreateValidUnlimitedFeature();

        // Assert
        feature.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Feature_IsValid_WithUnlimitedFeatureWithLimit_ShouldBeFalse()
    {
        // Arrange
        var feature = new Feature("TEST", "Test", null, FeatureType.Unlimited, 100);

        // Assert
        feature.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Feature_DisplayValue_ForLimit_ShouldShowLimitAndUnit()
    {
        // Arrange
        var feature = CreateValidLimitFeature();

        // Assert
        feature.DisplayValue.Should().Be("100 reservas/mes");
    }

    [Fact]
    public void Feature_DisplayValue_ForUnlimited_ShouldShowIlimitado()
    {
        // Arrange
        var feature = CreateValidUnlimitedFeature();

        // Assert
        feature.DisplayValue.Should().Be("Ilimitado");
    }

    [Fact]
    public void Feature_DisplayValue_ForBoolean_ShouldShowIncluido()
    {
        // Arrange
        var feature = CreateValidBooleanFeature();

        // Assert
        feature.DisplayValue.Should().Be("Incluido");
    }

    #endregion

    #region BillingPeriod Tests

    [Theory]
    [InlineData(BillingPeriod.Monthly)]
    [InlineData(BillingPeriod.Quarterly)]
    [InlineData(BillingPeriod.Semester)]
    [InlineData(BillingPeriod.Yearly)]
    public void Create_WithValidBillingPeriod_ShouldReturnSuccess(BillingPeriod billingPeriod)
    {
        // Arrange
        var features = new[] { CreateValidLimitFeature() };
        var providerConfigs = new[] { CreateValidProviderConfig() };

        // Act
        var result = Schedule.Features.Plan.Models.Plan.Create(
            Guid.NewGuid(), "Plan", "Description", CreateValidPrice(),
            billingPeriod, features, providerConfigs);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.BillingPeriod.Should().Be(billingPeriod);
    }

    #endregion
}
