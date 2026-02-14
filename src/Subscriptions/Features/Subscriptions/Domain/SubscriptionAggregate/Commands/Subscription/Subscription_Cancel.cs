namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

public record CancelSubscriptionCommand(
    string? Reason
);

public partial class Subscription
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Cancel(
        IValidator<Subscription> subscriptionValidator
    ) : AbstractModifyCommand<CancelSubscriptionCommand, Subscription>
    {
        public override Subscription Execute(Subscription subscription, CancelSubscriptionCommand command)
        {
            ConflictGuard.ThrowIf(
                subscription.Status != SubscriptionStatus.Active &&
                subscription.Status != SubscriptionStatus.Trial &&
                subscription.Status != SubscriptionStatus.PastDue,
                "Subscription cannot be cancelled from current status");

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTimeOffset.UtcNow;
            subscription.CancellationReason = command.Reason;

            return subscriptionValidator.ValidateOrThrow(subscription);
        }
    }
}
