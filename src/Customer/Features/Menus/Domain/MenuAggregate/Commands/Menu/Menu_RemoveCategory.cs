namespace Customer.Features.Menus.Domain.MenuAggregate;

public record RemoveCategoryCommand(Guid CategoryId);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RemoveCategory(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<RemoveCategoryCommand, Menu>
    {
        public override Menu Execute(Menu menu, RemoveCategoryCommand command)
        {
            var category = menu._categories.FirstOrDefault(c => c.Id == command.CategoryId);

            NotFoundGuard.ThrowIfNull(category, command.CategoryId);

            ValidationGuard.ThrowIf(
                category!.Items.Count != 0,
                "No se puede eliminar una categoría con items",
                "CategoryId"
            );

            menu._categories.Remove(category);
            menu.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
