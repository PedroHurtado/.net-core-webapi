namespace Customer.Features.Menus.Domain.MenuAggregate.Entities;

public record CreateCategoryCommand(
    string Name,
    string? Description = null,
    int DisplayOrder = 0
);

public partial class MenuCategory
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<MenuCategory> categoryValidator
    ) : AbstractCreateCommand<CreateCategoryCommand, MenuCategory>
    {
        public override MenuCategory Execute(CreateCategoryCommand command)
        {
            var category = new MenuCategory(Guid.NewGuid())
            {
                Name = command.Name,
                Description = command.Description,
                DisplayOrder = command.DisplayOrder,
                IsActive = true
            };

            return categoryValidator.ValidateOrThrow(category);
        }
    }
}
