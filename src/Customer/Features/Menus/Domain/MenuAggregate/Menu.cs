namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Menu Aggregate Root - MicroDomain.
/// La lógica de negocio está en Commands separados (partial class).
/// </summary>
public partial class Menu : AggregateRoot
{
    // === Propiedades básicas ===
    public Guid RestaurantId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public bool IsActive { get; protected set; }
    public int DisplayOrder { get; protected set; }

    // === Vigencia ===
    public DateTime? EffectiveFrom { get; protected set; }
    public DateTime? EffectiveUntil { get; protected set; }

    // === Value Objects ===
    public DepositPolicy? DepositPolicy { get; protected set; }

    // === Auditoría ===
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    // === Colecciones ===
    protected HashSet<MenuCategory> _categories = [];
    public IReadOnlyCollection<MenuCategory> Categories => _categories.ToList().AsReadOnly();

    // === Constructores ===
    protected Menu() : base(Guid.Empty) { }

    public Menu(Guid id) : base(id) { }
}

/// <summary>
/// Validador para Menu.
/// </summary>
public class MenuValidator : AbstractValidator<Menu>
{
    public MenuValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El Id del menú es requerido");

        RuleFor(x => x.RestaurantId)
            .NotEmpty()
            .WithMessage("El Id del restaurante es requerido");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre del menú es requerido")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El orden de visualización debe ser mayor o igual a 0");

        // Validar fechas de vigencia
        RuleFor(x => x)
            .Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveUntil.HasValue
                       || x.EffectiveFrom < x.EffectiveUntil)
            .WithMessage("La fecha de inicio debe ser anterior a la fecha de fin")
            .WithName("EffectiveFrom");

        // Nota: DepositPolicy se valida en su factory method Create(), no aquí.
    }
}
