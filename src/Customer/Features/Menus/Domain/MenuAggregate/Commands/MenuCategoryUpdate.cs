namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder
);

[Injectable(ServiceLifetime.Singleton)]
public class UpdateCategory(
    IValidator<MenuCategory> categoryValidator,
    IValidator<Menu> menuValidator
) : AbstractModifyCommand<UpdateCategoryCommand, Menu>
{
    public override Menu Execute(Menu menu, UpdateCategoryCommand command)
    {
        var category = menu.Categories.FirstOrDefault(c => c.Id == command.CategoryId);

        ValidationGuard.ThrowIf(
            category is null,
            "Categoría no encontrada",
            "CategoryId"
        );

        var duplicateName = menu.Categories.Any(c =>
            c.Id != command.CategoryId &&
            c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

        ValidationGuard.ThrowIf(
            duplicateName,
            "Ya existe una categoría con ese nombre",
            "Name"
        );

        category!.Name = command.Name;
        category.Description = command.Description;
        category.DisplayOrder = command.DisplayOrder;

        categoryValidator.ValidateOrThrow(category);

        menu.UpdatedAt = DateTime.UtcNow;

        return menuValidator.ValidateOrThrow(menu);
    }
}
