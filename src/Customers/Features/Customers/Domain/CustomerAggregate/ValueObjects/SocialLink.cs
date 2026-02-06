namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents a social media link for a customer.
/// </summary>
/// <remarks>
/// <para>Associates a social media platform name with its URL.</para>
/// <para>Supported platforms include Facebook, Instagram, TripAdvisor, Google, etc.</para>
/// </remarks>
public partial record SocialLink(
    string Platform,
    string Url
);

public static class SocialLinkValidationMessages
{
    public const string PlatformRequired = "Platform is required";
    public const string PlatformMaxLength = "Platform cannot exceed 50 characters";
    public const string UrlRequired = "URL is required";
    public const string UrlMaxLength = "URL cannot exceed 500 characters";
    public const string UrlFormat = "URL must be a valid URL";
}

/// <summary>
/// Provides validation rules for the <see cref="SocialLink"/> value object.
/// </summary>
public class SocialLinkValidator : AbstractValidator<SocialLink>
{
    public SocialLinkValidator()
    {
        RuleFor(x => x.Platform)
            .NotEmpty()
            .WithMessage(SocialLinkValidationMessages.PlatformRequired);

        RuleFor(x => x.Platform)
            .MaximumLength(50)
            .WithMessage(SocialLinkValidationMessages.PlatformMaxLength);

        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage(SocialLinkValidationMessages.UrlRequired);

        RuleFor(x => x.Url)
            .MaximumLength(500)
            .WithMessage(SocialLinkValidationMessages.UrlMaxLength);

        RuleFor(x => x.Url)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.Url))
            .WithMessage(SocialLinkValidationMessages.UrlFormat);
    }
}
