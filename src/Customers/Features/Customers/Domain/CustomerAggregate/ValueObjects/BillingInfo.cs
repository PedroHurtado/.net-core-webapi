namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents the billing information for a customer.
/// </summary>
/// <remarks>
/// <para>Contains the business name, tax identifier, and billing address.</para>
/// <para>The billing address can differ from the customer's physical address.</para>
/// </remarks>
public partial record BillingInfo(
    string BusinessName,
    string TaxId,
    Address BillingAddress
);

public static class BillingInfoValidationMessages
{
    public const string BusinessNameRequired = "Business name is required";
    public const string BusinessNameMaxLength = "Business name cannot exceed 200 characters";
    public const string TaxIdRequired = "Tax ID is required";
    public const string TaxIdMaxLength = "Tax ID cannot exceed 50 characters";
    public const string BillingAddressRequired = "Billing address is required";
}

/// <summary>
/// Provides validation rules for the <see cref="BillingInfo"/> value object.
/// </summary>
public class BillingInfoValidator : AbstractValidator<BillingInfo>
{
    public BillingInfoValidator()
    {
        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .WithMessage(BillingInfoValidationMessages.BusinessNameRequired);

        RuleFor(x => x.BusinessName)
            .MaximumLength(200)
            .WithMessage(BillingInfoValidationMessages.BusinessNameMaxLength);

        RuleFor(x => x.TaxId)
            .NotEmpty()
            .WithMessage(BillingInfoValidationMessages.TaxIdRequired);

        RuleFor(x => x.TaxId)
            .MaximumLength(50)
            .WithMessage(BillingInfoValidationMessages.TaxIdMaxLength);

        RuleFor(x => x.BillingAddress)
            .NotNull()
            .WithMessage(BillingInfoValidationMessages.BillingAddressRequired);
    }
}
