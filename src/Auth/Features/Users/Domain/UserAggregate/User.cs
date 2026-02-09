namespace Auth.Features.Users.Domain.UserAggregate;

/// <summary>
/// Represents an authenticated user in the Fudie platform.
/// </summary>
/// <remarks>
/// <para>The User aggregate supports two authentication providers: Google OAuth and Local (password-based).</para>
/// <para>Users are not multi-tenant — a user can belong to multiple restaurants via Membership.</para>
/// </remarks>
public partial class User : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the identifier of the user in the external authentication provider.
    /// </summary>
    /// <value>The provider-specific user identifier (e.g., Google sub claim or "local|identifier").</value>
    public string ProviderId { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the authentication provider used to create this account.
    /// </summary>
    /// <value>The <see cref="Enums.AuthProvider"/> value indicating the authentication method.</value>
    public AuthProvider Provider { get; protected set; }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    /// <value>A globally unique email address, maximum 256 characters.</value>
    public string Email { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    /// <value>The full name, maximum 200 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    /// <value>An optional phone number, maximum 20 characters. May be completed before purchasing a subscription.</value>
    public string? Phone { get; protected set; }

    /// <summary>
    /// Gets the URL of the user's avatar image.
    /// </summary>
    /// <value>An optional URL from the OAuth provider, maximum 500 characters. Null for local users.</value>
    public string? AvatarUrl { get; protected set; }

    /// <summary>
    /// Gets the user's hashed password.
    /// </summary>
    /// <value>A <see cref="HashedPassword"/> instance for local users, or null for OAuth users.</value>
    public HashedPassword? Password { get; protected set; }

    /// <summary>
    /// Gets the date and time of the user's last login.
    /// </summary>
    /// <value>The UTC timestamp of the last login, or null if the user has never logged in.</value>
    public DateTime? LastLoginAt { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the user account is active.
    /// </summary>
    /// <value><c>true</c> if the account is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the user authenticates via an external OAuth provider.
    /// </summary>
    /// <value><c>true</c> if the provider is not Local; otherwise, <c>false</c>.</value>
    public bool IsOAuth => Provider != AuthProvider.Local;

    /// <summary>
    /// Gets a value indicating whether the user has a password set.
    /// </summary>
    /// <value><c>true</c> if the password is not null; otherwise, <c>false</c>.</value>
    public bool HasPassword => Password != null;

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected User() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public User(Guid id) : base(id) { }
}

public static class UserValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string ProviderIdRequired = "ProviderId is required";
    public const string ProviderNotValid = "Provider is not valid";
    public const string EmailRequired = "Email is required";
    public const string EmailMaxLength = "Email cannot exceed 256 characters";
    public const string EmailNotValid = "Email is not valid";
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 200 characters";
    public const string PhoneMaxLength = "Phone cannot exceed 20 characters";
    public const string AvatarUrlMaxLength = "AvatarUrl cannot exceed 500 characters";
    public const string PasswordRequiredForLocal = "Password is required for local users";
    public const string PasswordOnlyForLocal = "Password only applies to local users";
}

/// <summary>
/// Provides validation rules for the <see cref="User"/> aggregate root.
/// </summary>
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(UserValidationMessages.IdRequired);

        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage(UserValidationMessages.ProviderIdRequired);

        RuleFor(x => x.Provider)
            .IsInEnum()
            .WithMessage(UserValidationMessages.ProviderNotValid);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(UserValidationMessages.EmailRequired);

        RuleFor(x => x.Email)
            .MaximumLength(256)
            .WithMessage(UserValidationMessages.EmailMaxLength);

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage(UserValidationMessages.EmailNotValid);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(UserValidationMessages.NameRequired);

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage(UserValidationMessages.NameMaxLength);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage(UserValidationMessages.PhoneMaxLength);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500)
            .WithMessage(UserValidationMessages.AvatarUrlMaxLength);

        RuleFor(x => x.Password)
            .NotNull()
            .When(x => x.Provider == AuthProvider.Local)
            .WithMessage(UserValidationMessages.PasswordRequiredForLocal);

        RuleFor(x => x.Password)
            .Null()
            .When(x => x.Provider != AuthProvider.Local)
            .WithMessage(UserValidationMessages.PasswordOnlyForLocal);
    }
}
