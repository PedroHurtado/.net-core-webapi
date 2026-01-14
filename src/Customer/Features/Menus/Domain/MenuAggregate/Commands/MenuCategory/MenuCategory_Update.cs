namespace Customer.Features.Menus.Domain.MenuAggregate.Entities;

public record UpdateCategoryDetailsCommand(
    string Name,
    string? Description,
    int DisplayOrder
);

public partial class MenuCategory
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<MenuCategory> categoryValidator
    ) : AbstractModifyCommand<UpdateCategoryDetailsCommand, MenuCategory>
    {
        public override MenuCategory Execute(MenuCategory category, UpdateCategoryDetailsCommand command)
        {
            category.Name = command.Name;
            category.Description = command.Description;
            category.DisplayOrder = command.DisplayOrder;

            return categoryValidator.ValidateOrThrow(category);
        }
    }
}
