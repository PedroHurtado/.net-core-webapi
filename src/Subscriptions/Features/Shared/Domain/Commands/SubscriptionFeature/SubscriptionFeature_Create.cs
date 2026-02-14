namespace Subscriptions.Features.Shared.Domain.ValueObjects;

public record CreateSubscriptionFeatureCommand(
    string Code,
    FeatureType Type,
    int? Limit
);

public partial record SubscriptionFeature
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<SubscriptionFeature> featureValidator
    ) : AbstractCreateCommand<CreateSubscriptionFeatureCommand, SubscriptionFeature>
    {
        public override SubscriptionFeature Execute(CreateSubscriptionFeatureCommand command)
        {
            var feature = new SubscriptionFeature(
                command.Code,
                command.Type,
                command.Limit);

            return featureValidator.ValidateOrThrow(feature);
        }
    }
}
