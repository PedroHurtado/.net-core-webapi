namespace Customer.Features.Menus.Domain.MenuAggregate;

public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder
);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateCategory(
        MenuCategory.Update updateCategory,
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<UpdateCategoryCommand, Menu>
    {
        public override Menu Execute(Menu menu, UpdateCategoryCommand command)
        {
            var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

            NotFoundGuard.ThrowIfNull(category, command.CategoryId);

            var duplicateName = menu._categories.Any(c =>
                c.Id != command.CategoryId &&
                c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

            ConflictGuard.ThrowIf(
                duplicateName,
                "Ya existe una categoría con ese nombre"
            );

            updateCategory.Execute(category!, new UpdateCategoryDetailsCommand(
                command.Name,
                command.Description,
                command.DisplayOrder
            ));

            menu.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
