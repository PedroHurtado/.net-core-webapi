namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

public record CreateSocialLinkCommand(
    string Platform,
    string Url
);

public partial record SocialLink
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<SocialLink> socialLinkValidator
    ) : AbstractCreateCommand<CreateSocialLinkCommand, SocialLink>
    {
        public override SocialLink Execute(CreateSocialLinkCommand command)
        {
            var socialLink = new SocialLink(
                command.Platform,
                command.Url);

            return socialLinkValidator.ValidateOrThrow(socialLink);
        }
    }
}
