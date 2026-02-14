namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

public partial class Subscription
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Expire(
        IValidator<Subscription> subscriptionValidator
    ) : AbstractModifyCommand<Subscription>
    {
        public override Subscription Execute(Subscription subscription)
        {
            ConflictGuard.ThrowIf(
                subscription.Status != SubscriptionStatus.Cancelled &&
                subscription.Status != SubscriptionStatus.PastDue,
                "Subscription cannot be expired from current status");

            subscription.Status = SubscriptionStatus.Expired;

            return subscriptionValidator.ValidateOrThrow(subscription);
        }
    }
}
