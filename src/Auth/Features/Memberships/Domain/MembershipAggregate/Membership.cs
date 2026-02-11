namespace Auth.Features.Memberships.Domain.MembershipAggregate;

/// <summary>
/// Represents a membership that links a user to a tenant with a specific role.
/// </summary>
/// <remarks>
/// <para>Domain commands are implemented as partial class methods in separate files.</para>
/// <para>A membership starts as an invitation (Pending status, no user) and transitions
/// to Accepted when the invitee joins via OAuth.</para>
/// </remarks>
public partial class Membership : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the identifier of the tenant this membership belongs to.
    /// </summary>
    /// <value>The <see cref="Guid"/> of the owning tenant. Set during creation.</value>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Gets the user associated with this membership.
    /// </summary>
    /// <value>The user linked to this membership, or <c>null</c> if the invitation has not been accepted yet.</value>
    public User? User { get; protected set; }

    /// <summary>
    /// Gets the role assigned to this membership.
    /// </summary>
    /// <value>The <see cref="TenantRole"/> that defines the permissions for this member.</value>
    public TenantRole Role { get; protected set; } = default!;

    /// <summary>
    /// Gets a value indicating whether this membership is active.
    /// </summary>
    /// <value><c>true</c> if the membership is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Gets the email address the invitation was sent to.
    /// </summary>
    /// <value>The email address of the invitee, maximum 254 characters.</value>
    public string InvitationEmail { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the current status of the invitation.
    /// </summary>
    /// <value>The <see cref="InvitationStatus"/> indicating whether the invitation is pending, accepted, or cancelled.</value>
    public InvitationStatus InvitationStatus { get; protected set; }

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected Membership() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public Membership(Guid id) : base(id) { }
}

public static class MembershipValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string TenantIdRequired = "TenantId is required";
    public const string RoleRequired = "Role is required";
    public const string InvitationEmailRequired = "Invitation email is required";
    public const string InvitationEmailMaxLength = "Invitation email cannot exceed 254 characters";
    public const string InvitationEmailInvalidFormat = "Invitation email must be a valid email address";
    public const string InvitationStatusInvalid = "Invitation status must be a valid status";
}

/// <summary>
/// Provides validation rules for the <see cref="Membership"/> aggregate root.
/// </summary>
public class MembershipValidator : AbstractValidator<Membership>
{
    public MembershipValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(MembershipValidationMessages.IdRequired);

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage(MembershipValidationMessages.TenantIdRequired);

        RuleFor(x => x.Role)
            .NotNull()
            .WithMessage(MembershipValidationMessages.RoleRequired);

        RuleFor(x => x.InvitationEmail)
            .NotEmpty()
            .WithMessage(MembershipValidationMessages.InvitationEmailRequired);

        RuleFor(x => x.InvitationEmail)
            .MaximumLength(254)
            .WithMessage(MembershipValidationMessages.InvitationEmailMaxLength);

        RuleFor(x => x.InvitationEmail)
            .EmailAddress()
            .WithMessage(MembershipValidationMessages.InvitationEmailInvalidFormat);

        RuleFor(x => x.InvitationStatus)
            .IsInEnum()
            .WithMessage(MembershipValidationMessages.InvitationStatusInvalid);
    }
}
