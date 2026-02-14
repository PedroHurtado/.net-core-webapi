namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

public record ActivateSubscriptionCommand(
    string? ExternalSubscriptionId,
    string? ExternalCustomerId,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd
);

public partial class Subscription
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<Subscription> subscriptionValidator
    ) : AbstractModifyCommand<ActivateSubscriptionCommand, Subscription>
    {
        public override Subscription Execute(Subscription subscription, ActivateSubscriptionCommand command)
        {
            ConflictGuard.ThrowIf(
                subscription.Status != SubscriptionStatus.Trial && subscription.Status != SubscriptionStatus.PastDue,
                "Subscription cannot be activated from current status");

            subscription.Status = SubscriptionStatus.Active;
            subscription.CurrentPeriodStart = command.CurrentPeriodStart;
            subscription.CurrentPeriodEnd = command.CurrentPeriodEnd;
            subscription.TrialEndsAt = null;

            if (command.ExternalSubscriptionId != null)
                subscription.ExternalSubscriptionId = command.ExternalSubscriptionId;

            if (command.ExternalCustomerId != null)
                subscription.ExternalCustomerId = command.ExternalCustomerId;

            return subscriptionValidator.ValidateOrThrow(subscription);
        }
    }
}
