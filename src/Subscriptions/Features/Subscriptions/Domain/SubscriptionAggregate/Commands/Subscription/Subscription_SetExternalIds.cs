namespace Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;

public record SetExternalIdsCommand(
    string ExternalSubscriptionId,
    string ExternalCustomerId,
    string Provider
);

public partial class Subscription
{
    [Injectable(ServiceLifetime.Singleton)]
    public class SetExternalIds(
        IValidator<Subscription> subscriptionValidator
    ) : AbstractModifyCommand<SetExternalIdsCommand, Subscription>
    {
        public override Subscription Execute(Subscription subscription, SetExternalIdsCommand command)
        {
            subscription.ExternalSubscriptionId = command.ExternalSubscriptionId;
            subscription.ExternalCustomerId = command.ExternalCustomerId;
            subscription.Provider = command.Provider;

            return subscriptionValidator.ValidateOrThrow(subscription);
        }
    }
}
