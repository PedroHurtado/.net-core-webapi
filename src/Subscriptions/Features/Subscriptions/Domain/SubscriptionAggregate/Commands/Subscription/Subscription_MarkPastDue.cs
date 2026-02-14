namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

public partial class Subscription
{
    [Injectable(ServiceLifetime.Singleton)]
    public class MarkPastDue(
        IValidator<Subscription> subscriptionValidator
    ) : AbstractModifyCommand<Subscription>
    {
        public override Subscription Execute(Subscription subscription)
        {
            ConflictGuard.ThrowIf(
                subscription.Status != SubscriptionStatus.Active && subscription.Status != SubscriptionStatus.Trial,
                "Subscription cannot be marked as past due from current status");

            subscription.Status = SubscriptionStatus.PastDue;

            return subscriptionValidator.ValidateOrThrow(subscription);
        }
    }
}
