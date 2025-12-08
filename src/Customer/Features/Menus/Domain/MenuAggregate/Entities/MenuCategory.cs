using FluentValidation;
using Fudie.Domain;

namespace Customer.Features.Menus.Domain.MenuAggregate.Entities;

/// <summary>
/// MenuCategory Entity - DTO simple para MicroDomain.
/// Pertenece al agregado Menu.
/// </summary>
public class MenuCategory : Entity
{
    // === Propiedades básicas ===
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    // === Colecciones ===
    public HashSet<MenuItem> Items { get; set; } = [];

    // === Constructores ===
    protected MenuCategory() : base(Guid.Empty) { }
    
    public MenuCategory(Guid id) : base(id) { }
}

/// <summary>
/// Validador para MenuCategory.
/// </summary>
public class MenuCategoryValidator : AbstractValidator<MenuCategory>
{
    public MenuCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El Id de la categoría es requerido");
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la categoría es requerido")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");
        
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("La descripción no puede exceder 500 caracteres");
        
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El orden de visualización debe ser mayor o igual a 0");
    }
}
