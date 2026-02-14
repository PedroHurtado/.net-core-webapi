namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

public record RenewSubscriptionCommand(
    DateTimeOffset NewPeriodStart,
    DateTimeOffset NewPeriodEnd
);

public partial class Subscription
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Renew(
        IValidator<Subscription> subscriptionValidator
    ) : AbstractModifyCommand<RenewSubscriptionCommand, Subscription>
    {
        public override Subscription Execute(Subscription subscription, RenewSubscriptionCommand command)
        {
            ConflictGuard.ThrowIf(
                subscription.Status != SubscriptionStatus.Active,
                "Only active subscriptions can be renewed");

            subscription.CurrentPeriodStart = command.NewPeriodStart;
            subscription.CurrentPeriodEnd = command.NewPeriodEnd;

            return subscriptionValidator.ValidateOrThrow(subscription);
        }
    }
}
