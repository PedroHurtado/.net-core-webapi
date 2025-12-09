using Fudie.Domain;
using Fudie.DependencyInjection;

namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

/// <summary>
/// Comando para añadir una categoría a un menú.
/// </summary>
public record AddCategoryCommand(
    string Name,
    string? Description = null,
    int DisplayOrder = 0
);

/// <summary>
/// Añade una nueva categoría a un menú existente.
/// </summary>
[Injectable]
public class AddCategory : IModifyCommand<AddCategoryCommand, Menu>
{
    public Result<Menu> Execute(Menu menu, AddCategoryCommand command)
    {
        var category = new MenuCategory(Guid.NewGuid())
        {
            Name = command.Name,
            Description = command.Description,
            DisplayOrder = command.DisplayOrder,
            IsActive = true
        };

        var categoryValidation = Entity.ValidateEntity(category, new MenuCategoryValidator());
        if (categoryValidation.IsFailure)
        {
            return Result<Menu>.Failure(categoryValidation.Errors);
        }
       
        if (menu.Categories.Any(c => c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Menu>.Failure("Ya existe una categoría con ese nombre", "Name");
        }

        menu.Categories.Add(category);
        menu.UpdatedAt = DateTime.UtcNow;

        return Entity.ValidateEntity(menu, new MenuValidator());
    }
}