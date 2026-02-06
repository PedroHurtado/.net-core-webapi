namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents an image associated with a customer.
/// </summary>
/// <remarks>
/// <para>Each image has a unique identifier, URL, optional alt text, display order, and cover flag.</para>
/// <para>Only one image can be marked as cover at a time within a customer.</para>
/// </remarks>
public partial record CustomerImage(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsCover
);

public static class CustomerImageValidationMessages
{
    public const string IdRequired = "Image id is required";
    public const string UrlRequired = "Image URL is required";
    public const string UrlMaxLength = "Image URL cannot exceed 500 characters";
    public const string UrlFormat = "Image URL must be a valid URL";
    public const string AltTextMaxLength = "Alt text cannot exceed 200 characters";
    public const string DisplayOrderNonNegative = "Display order cannot be negative";
}

/// <summary>
/// Provides validation rules for the <see cref="CustomerImage"/> value object.
/// </summary>
public class CustomerImageValidator : AbstractValidator<CustomerImage>
{
    public CustomerImageValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(CustomerImageValidationMessages.IdRequired);

        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage(CustomerImageValidationMessages.UrlRequired);

        RuleFor(x => x.Url)
            .MaximumLength(500)
            .WithMessage(CustomerImageValidationMessages.UrlMaxLength);

        RuleFor(x => x.Url)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.Url))
            .WithMessage(CustomerImageValidationMessages.UrlFormat);

        RuleFor(x => x.AltText)
            .MaximumLength(200)
            .WithMessage(CustomerImageValidationMessages.AltTextMaxLength);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(CustomerImageValidationMessages.DisplayOrderNonNegative);
    }
}
