namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests;

public class SubscriptionValidatorTests
{
    private readonly SubscriptionValidator _validator = new();

    private TestableSubscription CreateValidSubscription() =>
        new TestableSubscription(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithStatus(SubscriptionStatus.Active)
            .WithBillingPeriod(BillingPeriod.Monthly)
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero))
            .WithCurrentPeriodEnd(new DateTimeOffset(2026, 2, 28, 23, 59, 59, TimeSpan.Zero))
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

    [Fact]
    public void Validate_WithValidSubscription_ReturnsSuccess()
    {
        var subscription = CreateValidSubscription();

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeTrue();
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var subscription = new TestableSubscription(Guid.Empty)
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero))
            .WithCurrentPeriodEnd(new DateTimeOffset(2026, 2, 28, 23, 59, 59, TimeSpan.Zero))
            .WithFeature(new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.IdRequired);
    }

    #endregion

    #region TenantId Validation

    [Fact]
    public void TenantId_WhenEmpty_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithTenantId(Guid.Empty);

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.TenantIdRequired);
    }

    #endregion

    #region PlanId Validation

    [Fact]
    public void PlanId_WhenEmpty_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithPlanId(Guid.Empty);

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.PlanIdRequired);
    }

    #endregion

    #region PlanName Validation

    [Fact]
    public void PlanName_WhenEmpty_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithPlanName("");

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.PlanNameRequired);
    }

    [Fact]
    public void PlanName_WhenExceedsMaxLength_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithPlanName(new string('A', 101));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.PlanNameMaxLength);
    }

    #endregion

    #region CurrentPeriodStart Validation

    [Fact]
    public void CurrentPeriodStart_WhenEmpty_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithCurrentPeriodStart(default);
        subscription.WithCurrentPeriodEnd(new DateTimeOffset(2026, 2, 28, 23, 59, 59, TimeSpan.Zero));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.CurrentPeriodStartRequired);
    }

    #endregion

    #region CurrentPeriodEnd Validation

    [Fact]
    public void CurrentPeriodEnd_WhenEmpty_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithCurrentPeriodEnd(default);

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.CurrentPeriodEndRequired);
    }

    #endregion

    #region CurrentPeriodStart vs CurrentPeriodEnd Validation

    [Fact]
    public void CurrentPeriodStart_WhenGreaterThanCurrentPeriodEnd_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithCurrentPeriodStart(new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero));
        subscription.WithCurrentPeriodEnd(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.CurrentPeriodStartMustBeEarlier);
    }

    #endregion

    #region CancellationReason Validation

    [Fact]
    public void CancellationReason_WhenExceedsMaxLength_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithCancellationReason(new string('A', 501));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.CancellationReasonMaxLength);
    }

    #endregion

    #region ExternalSubscriptionId Validation

    [Fact]
    public void ExternalSubscriptionId_WhenExceedsMaxLength_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithExternalSubscriptionId(new string('A', 101));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.ExternalSubscriptionIdMaxLength);
    }

    #endregion

    #region ExternalCustomerId Validation

    [Fact]
    public void ExternalCustomerId_WhenExceedsMaxLength_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithExternalCustomerId(new string('A', 101));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.ExternalCustomerIdMaxLength);
    }

    #endregion

    #region Provider Validation

    [Fact]
    public void Provider_WhenExceedsMaxLength_ReturnsError()
    {
        var subscription = CreateValidSubscription();
        subscription.WithProvider(new string('A', 51));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.ProviderMaxLength);
    }

    #endregion

    #region Features Validation

    [Fact]
    public void Features_WhenEmpty_ReturnsError()
    {
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithPlanId(Guid.NewGuid())
            .WithPlanName("Plan Básico")
            .WithPrice(new Money(9.99m, Currency.EUR))
            .WithCurrentPeriodStart(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero))
            .WithCurrentPeriodEnd(new DateTimeOffset(2026, 2, 28, 23, 59, 59, TimeSpan.Zero));

        var result = _validator.Validate(subscription);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionValidationMessages.FeaturesRequired);
    }

    #endregion
}
