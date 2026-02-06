namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents a culture/locale identifier.
/// </summary>
/// <remarks>
/// <para>Follows the standard format 'xx-XX' (language-REGION).</para>
/// <para>Examples: es-ES, en-GB, fr-FR.</para>
/// </remarks>
public partial record CultureCode(
    string Code
);

public static class CultureCodeValidationMessages
{
    public const string CodeRequired = "Culture code is required";
    public const string CodeFormat = "Culture code must follow format 'xx-XX' (e.g. es-ES)";
}

/// <summary>
/// Provides validation rules for the <see cref="CultureCode"/> value object.
/// </summary>
public class CultureCodeValidator : AbstractValidator<CultureCode>
{
    public CultureCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(CultureCodeValidationMessages.CodeRequired);

        RuleFor(x => x.Code)
            .Matches(@"^[a-z]{2}-[A-Z]{2}$")
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage(CultureCodeValidationMessages.CodeFormat);
    }
}
