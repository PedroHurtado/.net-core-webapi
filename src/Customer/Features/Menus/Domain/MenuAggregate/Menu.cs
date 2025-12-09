using FluentValidation;
using Fudie.Domain;

namespace Customer.Features.Menus.Domain.MenuAggregate;

/// <summary>
/// Menu Aggregate Root - DTO simple para MicroDomain.
/// La lógica de negocio está en Commands separados.
/// </summary>
public class Menu : AggregateRoot
{
    // === Propiedades básicas ===
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    
    // === Vigencia ===
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveUntil { get; set; }
    
    // === Value Objects ===
    public DepositPolicy? DepositPolicy { get; set; }
    
    // === Auditoría ===
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // === Colecciones ===
    public HashSet<MenuCategory> Categories { get; set; } = [];

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
