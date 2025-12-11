namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

public record AddCategoryCommand(
    string Name,
    string? Description = null,
    int DisplayOrder = 0
);

[Injectable]
public class AddCategory(
    IValidator<MenuCategory> categoryValidator,
    IValidator<Menu> menuValidator
) : IModifyCommand<AddCategoryCommand, Menu>
{
    public Menu Execute(Menu menu, AddCategoryCommand command)
    {
        var category = new MenuCategory(Guid.NewGuid())
        {
            Name = command.Name,
            Description = command.Description,
            DisplayOrder = command.DisplayOrder,
            IsActive = true
        };

        categoryValidator.ValidateOrThrow(category);

        var duplicateName = menu.Categories.Any(c => c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

        ConflictGuard.ThrowIf(
            duplicateName,
            "Ya existe una categoría con ese nombre"            
        );

        menu.Categories.Add(category);
        menu.UpdatedAt = DateTime.UtcNow;
        

        return menuValidator.ValidateOrThrow(menu);
    }
}