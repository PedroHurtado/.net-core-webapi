using Fudie.Domain;
using Fudie.DependencyInjection;

namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

/// <summary>
/// Comando para actualizar una categoría de un menú.
/// </summary>
public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder
);

/// <summary>
/// Actualiza una categoría existente de un menú.
/// </summary>
[Injectable]
public class UpdateCategory : IModifyCommand<UpdateCategoryCommand, Menu>
{
    public Result<Menu> Execute(Menu menu, UpdateCategoryCommand command)
    {
        var category = menu.Categories.FirstOrDefault(c => c.Id == command.CategoryId);
        
        if (category is null)
        {
            return Result<Menu>.Failure("Categoría no encontrada", "CategoryId");
        }
        
        if (menu.Categories.Any(c => c.Id != command.CategoryId 
            && c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Menu>.Failure("Ya existe una categoría con ese nombre", "Name");
        }

        category.Name = command.Name;
        category.Description = command.Description;
        category.DisplayOrder = command.DisplayOrder;
        

        var categoryValidation = Entity.ValidateEntity(category, new MenuCategoryValidator());
        if (categoryValidation.IsFailure)
        {
            return Result<Menu>.Failure(categoryValidation.Errors);
        }

        menu.UpdatedAt = DateTime.UtcNow;

        return Entity.ValidateEntity(menu, new MenuValidator());
    }
}