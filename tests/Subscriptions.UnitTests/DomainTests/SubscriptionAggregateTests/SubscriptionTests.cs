namespace Subscriptions.UnitTests.DomainTests.SubscriptionAggregateTests;

public class SubscriptionTests
{
    [Fact]
    public void Subscription_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var price = new Money(9.99m, Currency.EUR);
        var feature = new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100);
        var periodStart = DateTimeOffset.UtcNow;
        var periodEnd = periodStart.AddMonths(1);

        // Act
        var subscription = new TestableSubscription(id)
            .WithTenantId(tenantId)
            .WithPlanId(planId)
            .WithPlanName("Plan Básico")
            .WithStatus(SubscriptionStatus.Active)
            .WithBillingPeriod(BillingPeriod.Monthly)
            .WithPrice(price)
            .WithCurrentPeriodStart(periodStart)
            .WithCurrentPeriodEnd(periodEnd)
            .WithFeature(feature);

        // Assert
        subscription.Id.Should().Be(id);
        subscription.TenantId.Should().Be(tenantId);
        subscription.PlanId.Should().Be(planId);
        subscription.PlanName.Should().Be("Plan Básico");
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        subscription.Price.Should().Be(price);
        subscription.CurrentPeriodStart.Should().Be(periodStart);
        subscription.CurrentPeriodEnd.Should().Be(periodEnd);
        subscription.Features.Should().ContainSingle();
    }

    #region IsInTrial

    [Fact]
    public void IsInTrial_WithTrialStatusAndFutureTrialEnd_ShouldReturnTrue()
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Trial)
            .WithTrialEndsAt(DateTimeOffset.UtcNow.AddDays(14));

        // Assert
        subscription.IsInTrial.Should().BeTrue();
    }

    [Fact]
    public void IsInTrial_WithActiveStatus_ShouldReturnFalse()
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active);

        // Assert
        subscription.IsInTrial.Should().BeFalse();
    }

    [Fact]
    public void IsInTrial_WithTrialStatusAndPastTrialEnd_ShouldReturnFalse()
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Trial)
            .WithTrialEndsAt(DateTimeOffset.UtcNow.AddDays(-1));

        // Assert
        subscription.IsInTrial.Should().BeFalse();
    }

    #endregion

    #region IsUsable

    [Theory]
    [InlineData(SubscriptionStatus.Trial)]
    [InlineData(SubscriptionStatus.Active)]
    [InlineData(SubscriptionStatus.PastDue)]
    [InlineData(SubscriptionStatus.Cancelled)]
    public void IsUsable_WithUsableStatus_ShouldReturnTrue(SubscriptionStatus status)
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithStatus(status);

        // Assert
        subscription.IsUsable.Should().BeTrue();
    }

    [Fact]
    public void IsUsable_WithExpiredStatus_ShouldReturnFalse()
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Expired);

        // Assert
        subscription.IsUsable.Should().BeFalse();
    }

    #endregion

    #region HasExternalProvider

    [Fact]
    public void HasExternalProvider_WithExternalSubscriptionId_ShouldReturnTrue()
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid())
            .WithExternalSubscriptionId("sub_1OXbFJ2eZvKYlo2C5004ogvj");

        // Assert
        subscription.HasExternalProvider.Should().BeTrue();
    }

    [Fact]
    public void HasExternalProvider_WithoutExternalSubscriptionId_ShouldReturnFalse()
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid());

        // Assert
        subscription.HasExternalProvider.Should().BeFalse();
    }

    #endregion

    #region Features Collection

    [Fact]
    public void Features_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid());
        var feature = new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100);

        // Act
        subscription.WithFeature(feature);

        // Assert
        subscription.Features.Should().BeAssignableTo<IReadOnlyCollection<SubscriptionFeature>>();
        subscription.Features.Should().ContainSingle();
    }

    [Fact]
    public void Subscription_WithMultipleFeatures_ShouldContainAllFeatures()
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid());
        var feature1 = new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100);
        var feature2 = new SubscriptionFeature("PRIORITY_SUPPORT", FeatureType.Boolean, null);

        // Act
        subscription.WithFeature(feature1);
        subscription.WithFeature(feature2);

        // Assert
        subscription.Features.Should().HaveCount(2);
        subscription.Features.Should().Contain(x => x.Code == "RESERVATIONS_MONTHLY");
        subscription.Features.Should().Contain(x => x.Code == "PRIORITY_SUPPORT");
    }

    #endregion

    #region Enum Properties

    [Theory]
    [InlineData(SubscriptionStatus.Trial)]
    [InlineData(SubscriptionStatus.Active)]
    [InlineData(SubscriptionStatus.PastDue)]
    [InlineData(SubscriptionStatus.Cancelled)]
    [InlineData(SubscriptionStatus.Expired)]
    public void Subscription_WithStatus_ShouldSetCorrectly(SubscriptionStatus value)
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid());

        // Act
        subscription.WithStatus(value);

        // Assert
        subscription.Status.Should().Be(value);
    }

    [Theory]
    [InlineData(BillingPeriod.Monthly)]
    [InlineData(BillingPeriod.Quarterly)]
    [InlineData(BillingPeriod.Semester)]
    [InlineData(BillingPeriod.Yearly)]
    public void Subscription_WithBillingPeriod_ShouldSetCorrectly(BillingPeriod value)
    {
        // Arrange
        var subscription = new TestableSubscription(Guid.NewGuid());

        // Act
        subscription.WithBillingPeriod(value);

        // Assert
        subscription.BillingPeriod.Should().Be(value);
    }

    #endregion
}
