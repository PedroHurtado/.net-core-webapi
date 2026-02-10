namespace Auth.Features.Roles.Domain.TenantRoleAggregate;

/// <summary>
/// Represents a role within a tenant that defines permissions for its members.
/// </summary>
/// <remarks>
/// <para>Roles are the unit of permission assignment. Each role belongs to a single tenant
/// and defines groups, additional scopes, and excluded scopes that shape the JWT claims.</para>
/// <para>System roles (Owner, Manager, Waiter, ExternalApp, Customer) are seeded when a tenant
/// is created. Custom roles can be created, updated, and deleted by the Owner.</para>
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
    /// Gets a value indicating whether this is a system-defined role.
    /// </summary>
    /// <value><c>true</c> if the role was created by SeedSystemRoles; otherwise, <c>false</c>.</value>
    public bool IsSystem { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this role's name and description can be modified.
    /// </summary>
    /// <value><c>true</c> if the role allows editing; otherwise, <c>false</c> (Owner role).</value>
    public bool IsEditable { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this role can be deleted.
    /// </summary>
    /// <value><c>true</c> for custom roles; <c>false</c> for system roles.</value>
    public bool IsDeletable { get; protected set; }

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
