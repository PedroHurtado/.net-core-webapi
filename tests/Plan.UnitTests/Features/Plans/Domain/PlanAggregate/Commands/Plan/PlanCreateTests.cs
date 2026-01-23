namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Plan;

public class PlanCreateTests
{
    private readonly PlanValidator _validator = new();
    private readonly PlanAgg.Create _create;
    private readonly MoneyVO.Create _createMoney;
    private readonly FeatureVO.Create _createFeature;
    private readonly PaymentProviderConfigVO.Create _createProviderConfig;

    public PlanCreateTests()
    {
        _create = new(_validator);
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsPlan()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(9.99m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Limit, 100, "reservas"));
        var providerConfig = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_123", "price_123", true));

        var command = new CreatePlanCommand(
            Name: "Plan Básico",
            Description: "Plan ideal para empezar",
            Price: price,
            BillingPeriod: BillingPeriod.Monthly,
            Features: [feature],
            ProviderConfigurations: [providerConfig]
        );

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Plan Básico");
        result.Description.Should().Be("Plan ideal para empezar");
        result.Price.Amount.Should().Be(9.99m);
        result.Price.Currency.Should().Be(CurrencyVO.EUR);
        result.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        result.IsActive.Should().BeTrue();
        result.Features.Should().HaveCount(1);
        result.ProviderConfigurations.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithMultipleFeaturesAndProviders_SetsCollections()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(29.99m, CurrencyVO.USD));
        var feature1 = _createFeature.Execute(new CreateFeatureCommand("RESERVATIONS_MONTHLY", "Reservas mensuales", null, FeatureType.Unlimited));
        var feature2 = _createFeature.Execute(new CreateFeatureCommand("ACTIVE_WAITERS", "Camareros activos", null, FeatureType.Limit, 10, "camareros"));
        var feature3 = _createFeature.Execute(new CreateFeatureCommand("PRIORITY_SUPPORT", "Soporte prioritario", "Respuesta en 24h", FeatureType.Boolean));
        var provider1 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_premium", "price_premium", true));
        var provider2 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Paddle", "paddle_prod_123", "paddle_price_123", false));

        var command = new CreatePlanCommand(
            Name: "Plan Premium",
            Description: "Plan con todas las características",
            Price: price,
            BillingPeriod: BillingPeriod.Yearly,
            Features: [feature1, feature2, feature3],
            ProviderConfigurations: [provider1, provider2]
        );

        var result = _create.Execute(command);

        result.Features.Should().HaveCount(3);
        result.ProviderConfigurations.Should().HaveCount(2);
        result.HasActiveProvider.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithIsActiveFalse_CreatesInactivePlan()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(5.00m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("BASIC", "Básico", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_old", "price_old", true));

        var command = new CreatePlanCommand(
            Name: "Plan Legacy",
            Description: "Plan antiguo",
            Price: price,
            BillingPeriod: BillingPeriod.Monthly,
            Features: [feature],
            ProviderConfigurations: [provider],
            IsActive: false
        );

        var result = _create.Execute(command);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithDifferentBillingPeriods_SetsBillingPeriod()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(24.99m, CurrencyVO.GBP));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod", "price", true));

        var quarterlyCommand = new CreatePlanCommand(
            "Quarterly Plan",
            "Plan trimestral",
            price,
            BillingPeriod.Quarterly,
            [feature],
            [provider]
        );

        var result = _create.Execute(quarterlyCommand);

        result.BillingPeriod.Should().Be(BillingPeriod.Quarterly);
    }

    #region Validation Throws - Basic Properties

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod", "price", true));

        var command = new CreatePlanCommand(
            name!,
            "Description",
            price,
            BillingPeriod.Monthly,
            [feature],
            [provider]
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod", "price", true));

        var command = new CreatePlanCommand(
            new string('a', 101),
            "Description",
            price,
            BillingPeriod.Monthly,
            [feature],
            [provider]
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyDescription_ThrowsValidationException(string? description)
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod", "price", true));

        var command = new CreatePlanCommand(
            "Plan Name",
            description!,
            price,
            BillingPeriod.Monthly,
            [feature],
            [provider]
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod", "price", true));

        var command = new CreatePlanCommand(
            "Plan Name",
            new string('a', 501),
            price,
            BillingPeriod.Monthly,
            [feature],
            [provider]
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion

    #region Validation Throws - Features (422)

    [Fact]
    public void Execute_WithNoFeatures_ThrowsValidationException()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod", "price", true));

        var command = new CreatePlanCommand(
            "Plan Name",
            "Description",
            price,
            BillingPeriod.Monthly,
            [],
            [provider]
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*at least one feature*");
    }

    #endregion

    #region Conflict Throws - Features (409)

    [Fact]
    public void Execute_WithDuplicateFeatureCodes_ThrowsConflictException()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature1 = _createFeature.Execute(new CreateFeatureCommand("DUPLICATE_CODE", "Feature 1", null, FeatureType.Boolean));
        var feature2 = _createFeature.Execute(new CreateFeatureCommand("DUPLICATE_CODE", "Feature 2", null, FeatureType.Limit, 10, "units"));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod", "price", true));

        var command = new CreatePlanCommand(
            "Plan Name",
            "Description",
            price,
            BillingPeriod.Monthly,
            [feature1, feature2],
            [provider]
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*duplicate feature*");
    }

    #endregion

    #region Validation Throws - Provider Configurations (422)

    [Fact]
    public void Execute_WithNoActiveProviderConfiguration_ThrowsValidationException()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST", "Test", null, FeatureType.Boolean));
        var provider1 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod", "price", false));
        var provider2 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Paddle", "prod2", "price2", false));

        var command = new CreatePlanCommand(
            "Plan Name",
            "Description",
            price,
            BillingPeriod.Monthly,
            [feature],
            [provider1, provider2]
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*at least one active provider*");
    }

    #endregion

    #region Conflict Throws - Provider Configurations (409)

    [Fact]
    public void Execute_WithDuplicateActiveProviders_ThrowsConflictException()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST", "Test", null, FeatureType.Boolean));
        var provider1 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_1", "price_1", true));
        var provider2 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_2", "price_2", true));

        var command = new CreatePlanCommand(
            "Plan Name",
            "Description",
            price,
            BillingPeriod.Monthly,
            [feature],
            [provider1, provider2]
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*multiple active configurations*same provider*");
    }

    [Fact]
    public void Execute_WithDuplicateProvidersButOnlyOneActive_Succeeds()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST", "Test", null, FeatureType.Boolean));
        var provider1 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_1", "price_1", true));
        var provider2 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_2", "price_2", false));

        var command = new CreatePlanCommand(
            "Plan Name",
            "Description",
            price,
            BillingPeriod.Monthly,
            [feature],
            [provider1, provider2]
        );

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.ProviderConfigurations.Should().HaveCount(2);
        result.ProviderConfigurations.Count(p => p.IsActive).Should().Be(1);
    }

    #endregion
}
