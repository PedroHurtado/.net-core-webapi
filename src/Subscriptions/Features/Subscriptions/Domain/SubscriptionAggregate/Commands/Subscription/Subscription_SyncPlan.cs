namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

public record SyncPlanCommand(
    string PlanName,
    BillingPeriod BillingPeriod,
    Money Price,
    IReadOnlyCollection<SubscriptionFeature> Features
);

public partial class Subscription
{
    [Injectable(ServiceLifetime.Singleton)]
    public class SyncPlan(
        IValidator<Subscription> subscriptionValidator
    ) : AbstractModifyCommand<SyncPlanCommand, Subscription>
    {
        public override Subscription Execute(Subscription subscription, SyncPlanCommand command)
        {
            subscription.PlanName = command.PlanName;
            subscription.BillingPeriod = command.BillingPeriod;
            subscription.Price = command.Price;

            subscription._features.Clear();
            foreach (var feature in command.Features)
                subscription._features.Add(feature);

            return subscriptionValidator.ValidateOrThrow(subscription);
        }
    }
}
