namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents the contact information for a customer.
/// </summary>
/// <remarks>
/// <para>Phone is mandatory; email and website URL are optional.</para>
/// <para>When provided, email and website URL are validated for correct format.</para>
/// </remarks>
public partial record ContactInfo(
    string Phone,
    string? Email,
    string? WebsiteUrl
);

public static class ContactInfoValidationMessages
{
    public const string PhoneRequired = "Phone is required";
    public const string PhoneMaxLength = "Phone cannot exceed 20 characters";
    public const string EmailMaxLength = "Email cannot exceed 150 characters";
    public const string EmailFormat = "Email must be a valid email address";
    public const string WebsiteUrlMaxLength = "Website URL cannot exceed 500 characters";
    public const string WebsiteUrlFormat = "Website URL must be a valid URL";
}

/// <summary>
/// Provides validation rules for the <see cref="ContactInfo"/> value object.
/// </summary>
public class ContactInfoValidator : AbstractValidator<ContactInfo>
{
    public ContactInfoValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage(ContactInfoValidationMessages.PhoneRequired);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage(ContactInfoValidationMessages.PhoneMaxLength);

        RuleFor(x => x.Email)
            .MaximumLength(150)
            .WithMessage(ContactInfoValidationMessages.EmailMaxLength);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage(ContactInfoValidationMessages.EmailFormat);

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500)
            .WithMessage(ContactInfoValidationMessages.WebsiteUrlMaxLength);

        RuleFor(x => x.WebsiteUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
            .WithMessage(ContactInfoValidationMessages.WebsiteUrlFormat);
    }
}
