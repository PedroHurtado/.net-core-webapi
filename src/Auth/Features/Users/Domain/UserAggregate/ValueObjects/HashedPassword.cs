namespace Auth.Features.Users.Domain.UserAggregate.ValueObjects;

/// <summary>
/// Represents a securely hashed password with its associated salt.
/// </summary>
/// <remarks>
/// <para>Only used for users with <see cref="Enums.AuthProvider.Local"/> (super administrators).</para>
/// <para>The plain password is never stored — only the hash and salt are persisted.</para>
/// </remarks>
public partial record HashedPassword(
    string Hash,
    string Salt
);

public static class HashedPasswordValidationMessages
{
    public const string HashRequired = "Hash is required";
    public const string SaltRequired = "Salt is required";
}

/// <summary>
/// Provides validation rules for the <see cref="HashedPassword"/> value object.
/// </summary>
public class HashedPasswordValidator : AbstractValidator<HashedPassword>
{
    public HashedPasswordValidator()
    {
        RuleFor(x => x.Hash)
            .NotEmpty()
            .WithMessage(HashedPasswordValidationMessages.HashRequired);

        RuleFor(x => x.Salt)
            .NotEmpty()
            .WithMessage(HashedPasswordValidationMessages.SaltRequired);
    }
}
