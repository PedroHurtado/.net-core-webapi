namespace Customer.Features.Menus.Domain.MenuAggregate;

public record AddCategoryCommand(
    string Name,
    string? Description = null,
    int DisplayOrder = 0
);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddCategory(
        MenuCategory.Create createCategory,
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<AddCategoryCommand, Menu>
    {
        public override Menu Execute(Menu menu, AddCategoryCommand command)
        {
            var duplicateName = menu._categories.Any(c => c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

            ConflictGuard.ThrowIf(
                duplicateName,
                "Ya existe una categoría con ese nombre"
            );

            var category = createCategory.Execute(new CreateCategoryCommand(
                command.Name,
                command.Description,
                command.DisplayOrder
            ));

            menu._categories.Add(category);
            menu.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
