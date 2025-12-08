using Fudie.Domain;
using Fudie.DependencyInjection;

namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

/// <summary>
/// Comando para eliminar una categoría de un menú.
/// </summary>
public record RemoveCategoryCommand(Guid CategoryId);

/// <summary>
/// Elimina una categoría de un menú existente.
/// </summary>
[Injectable]
public class RemoveCategory : IModifyCommand<RemoveCategoryCommand, Menu>
{
    public Result<Menu> Execute(Menu menu, RemoveCategoryCommand command)
    {
        var category = menu.Categories.FirstOrDefault(c => c.Id == command.CategoryId);
        if (category is null)
        {
            return Result<Menu>.Failure("Categoría no encontrada", "CategoryId");
        }

        if (category.Items.Any())
        {
            return Result<Menu>.Failure("No se puede eliminar una categoría con items", "CategoryId");
        }

        menu.Categories.Remove(category);
        menu.UpdatedAt = DateTime.UtcNow;

        return Entity.ValidateEntity(menu, new MenuValidator());
    }
}