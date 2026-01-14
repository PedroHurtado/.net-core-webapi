namespace Customer.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// MenuItem Entity - MicroDomain.
/// Pertenece a una MenuCategory dentro del agregado Menu.
/// </summary>
public partial class MenuItem : Entity
{
    // === Propiedades básicas ===
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public string? ImageUrl { get; protected set; }
    public int DisplayOrder { get; protected set; }
    public bool IsActive { get; protected set; }

    // === Alto riesgo / Pedido anticipado ===
    public bool IsHighRiskItem { get; protected set; }
    public bool RequiresAdvanceOrder { get; protected set; }
    public int? MinimumAdvanceOrderQuantity { get; protected set; }

    // === Stock/Disponibilidad ===
    public bool IsAvailable { get; protected set; } = true;
    public bool IsAlwaysAvailable { get; protected set; } = true;

    // === Value Objects ===
    public ItemDepositOverride? DepositOverride { get; protected set; }
    public NutritionalInfo? NutritionalInfo { get; protected set; }

    // === Colecciones ===
    protected HashSet<DayOfWeek> _availableDays = [];
    public IReadOnlyCollection<DayOfWeek> AvailableDays => _availableDays.ToList().AsReadOnly();

    protected HashSet<PriceOption> _priceOptions = [];
    public IReadOnlyCollection<PriceOption> PriceOptions => _priceOptions.ToList().AsReadOnly();

    protected HashSet<Guid> _allergenIds = [];
    public IReadOnlyCollection<Guid> AllergenIds => _allergenIds.ToList().AsReadOnly();

    public string? AllergenNotes { get; protected set; }

    // === Constructores ===
    protected MenuItem() : base(Guid.Empty) { }

    public MenuItem(Guid id) : base(id) { }
}

/// <summary>
/// Validador para MenuItem.
/// </summary>
public class MenuItemValidator : AbstractValidator<MenuItem>
{
    public MenuItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El Id del item es requerido");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre del item es requerido")
            .MaximumLength(150)
            .WithMessage("El nombre no puede exceder 150 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La descripción no puede exceder 1000 caracteres");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .WithMessage("La URL de imagen no puede exceder 500 caracteres")
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("La URL de imagen no es válida");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El orden de visualización debe ser mayor o igual a 0");

        // Si requiere pedido anticipado, debe ser item de alto riesgo
        RuleFor(x => x)
            .Must(x => !x.RequiresAdvanceOrder || x.IsHighRiskItem)
            .WithMessage("Si RequiresAdvanceOrder es true, IsHighRiskItem también debe serlo")
            .WithName("RequiresAdvanceOrder");

        // MinimumAdvanceOrderQuantity debe estar en rango válido
        RuleFor(x => x.MinimumAdvanceOrderQuantity)
            .InclusiveBetween(1, 100)
            .When(x => x.MinimumAdvanceOrderQuantity.HasValue)
            .WithMessage("La cantidad mínima debe estar entre 1 y 100");

        // Si MinimumAdvanceOrderQuantity tiene valor, RequiresAdvanceOrder debe ser true
        RuleFor(x => x)
            .Must(x => !x.MinimumAdvanceOrderQuantity.HasValue || x.RequiresAdvanceOrder)
            .WithMessage("Si MinimumAdvanceOrderQuantity tiene valor, RequiresAdvanceOrder debe ser true")
            .WithName("MinimumAdvanceOrderQuantity");

        // Si no está siempre disponible, debe tener días configurados
        RuleFor(x => x.AvailableDays)
            .NotEmpty()
            .When(x => !x.IsAlwaysAvailable)
            .WithMessage("Debe especificar los días disponibles si el item no está siempre disponible");

        // Debe tener al menos una opción de precio
        RuleFor(x => x.PriceOptions)
            .NotEmpty()
            .WithMessage("El item debe tener al menos una opción de precio");

        // Nota: Los Value Objects (PriceOption, DepositOverride, NutritionalInfo)
        // se validan en su factory method Create(), no aquí.

        RuleFor(x => x.AllergenNotes)
            .MaximumLength(500)
            .WithMessage("Las notas de alérgenos no pueden exceder 500 caracteres");
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
