namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

public record CreateSubscriptionCommand(
    Guid TenantId,
    Guid PlanId,
    string PlanName,
    BillingPeriod BillingPeriod,
    Money Price,
    IReadOnlyCollection<SubscriptionFeature> Features,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    DateTimeOffset? TrialEndsAt,
    SubscriptionStatus Status
);

public partial class Subscription
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Subscription> subscriptionValidator
    ) : AbstractCreateCommand<CreateSubscriptionCommand, Subscription>
    {
        public override Subscription Execute(CreateSubscriptionCommand command)
        {
            var subscription = new Subscription(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                PlanId = command.PlanId,
                PlanName = command.PlanName,
                Status = command.Status,
                BillingPeriod = command.BillingPeriod,
                Price = command.Price,
                CurrentPeriodStart = command.CurrentPeriodStart,
                CurrentPeriodEnd = command.CurrentPeriodEnd,
                TrialEndsAt = command.TrialEndsAt
            };

            foreach (var feature in command.Features)
                subscription._features.Add(feature);

            return subscriptionValidator.ValidateOrThrow(subscription);
        }
    }
}
