using FluentValidation;
using Fudie.Domain;
using Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;
using Customer.Features.Menus.Domain.MenuAggregate.Enums;

namespace Customer.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// MenuItem Entity - DTO simple para MicroDomain.
/// Pertenece a una MenuCategory dentro del agregado Menu.
/// </summary>
public class MenuItem : Entity
{
    // === Propiedades básicas ===
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    // === Alto riesgo / Pedido anticipado ===
    public bool IsHighRiskItem { get; set; }
    public bool RequiresAdvanceOrder { get; set; }
    
    // === Stock/Disponibilidad ===
    public bool IsAvailable { get; set; } = true;
    public bool IsAlwaysAvailable { get; set; } = true;
    
    // === Value Objects ===
    public ItemDepositOverride? DepositOverride { get; set; }
    public NutritionalInfo? NutritionalInfo { get; set; }
    
    // === Colecciones ===
    public HashSet<DayOfWeek> AvailableDays { get; set; } = [];
    public HashSet<PriceOption> PriceOptions { get; set; } = [];
    public HashSet<Guid> AllergenIds { get; set; } = [];
    public string? AllergenNotes { get; set; }

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
