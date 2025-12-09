namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

public record RemoveCategoryCommand(Guid CategoryId);

[Injectable]
public class RemoveCategory(
    IValidator<Menu> menuValidator
) : IModifyCommand<RemoveCategoryCommand, Menu>
{
    public Menu Execute(Menu menu, RemoveCategoryCommand command)
    {
        var category = menu.Categories.FirstOrDefault(c => c.Id == command.CategoryId);

        NotFoundGuard.ThrowIfNull(
            category,
            command.CategoryId           
        );

        ValidationGuard.ThrowIf(
            category!.Items.Count != 0,
            "No se puede eliminar una categoría con items",
            "CategoryId"
        );

        menu.Categories.Remove(category);
        menu.UpdatedAt = DateTime.UtcNow;

        return menuValidator.ValidateOrThrow(menu);
    }
}
