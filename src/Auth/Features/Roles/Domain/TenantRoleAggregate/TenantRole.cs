namespace Auth.Features.Roles.Domain.TenantRoleAggregate;

/// <summary>
/// Represents a role within a tenant that defines permissions for its members.
/// </summary>
/// <remarks>
/// <para>Roles are the unit of permission assignment. Each role belongs to a single tenant
/// and defines groups, additional scopes, and excluded scopes that shape the JWT claims.</para>
/// <para>The Owner role (IsOwner=true) is created when a tenant is provisioned and cannot be
/// edited or deleted. All other roles are created by the Owner and are fully manageable.</para>
/// </remarks>
public partial class TenantRole : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the identifier of the tenant this role belongs to.
    /// </summary>
    /// <value>The <see cref="Guid"/> of the owning tenant.</value>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Gets the display name of this role.
    /// </summary>
    /// <value>A unique name within the tenant, maximum 100 characters.</value>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the description of this role.
    /// </summary>
    /// <value>An optional description, maximum 500 characters.</value>
    public string Description { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this is the Owner role of the tenant.
    /// </summary>
    /// <value><c>true</c> if the role is the Owner; otherwise, <c>false</c>.</value>
    public bool IsOwner { get; protected set; }

    /// <summary>
    /// The internal collection of permission groups.
    /// </summary>
    protected HashSet<string> _groups = [];

    /// <summary>
    /// Gets the read-only collection of permission groups assigned to this role.
    /// </summary>
    /// <value>A read-only collection of group permission strings.</value>
    public IReadOnlyCollection<string> Groups => _groups.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of additional scopes.
    /// </summary>
    protected HashSet<string> _additionalScopes = [];

    /// <summary>
    /// Gets the read-only collection of individually added scopes.
    /// </summary>
    /// <value>A read-only collection of additional scope strings.</value>
    public IReadOnlyCollection<string> AdditionalScopes => _additionalScopes.ToList().AsReadOnly();

    /// <summary>
    /// The internal collection of excluded scopes.
    /// </summary>
    protected HashSet<string> _excludedScopes = [];

    /// <summary>
    /// Gets the read-only collection of individually excluded scopes.
    /// </summary>
    /// <value>A read-only collection of excluded scope strings.</value>
    public IReadOnlyCollection<string> ExcludedScopes => _excludedScopes.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance for ORM purposes.
    /// </summary>
    protected TenantRole() : base(Guid.Empty) { }

    /// <summary>
    /// Initializes a new instance with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public TenantRole(Guid id) : base(id) { }
}

public static class TenantRoleValidationMessages
{
    public const string IdRequired = "Id is required";
    public const string TenantIdRequired = "TenantId is required";
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 100 characters";
    public const string DescriptionMaxLength = "Description cannot exceed 500 characters";
}

/// <summary>
/// Provides validation rules for the <see cref="TenantRole"/> aggregate root.
/// </summary>
public class TenantRoleValidator : AbstractValidator<TenantRole>
{
    public TenantRoleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(TenantRoleValidationMessages.IdRequired);

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage(TenantRoleValidationMessages.TenantIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(TenantRoleValidationMessages.NameRequired);

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage(TenantRoleValidationMessages.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage(TenantRoleValidationMessages.DescriptionMaxLength);
    }
}
