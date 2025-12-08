using Fudie.Domain;
using Fudie.DependencyInjection;

namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;



/// <summary>
/// Elimina la política de fianzas de un menú existente.
/// </summary>
[Injectable]
public class RemoveDepositPolicy : IModifyCommand<Menu>
{
    public Result<Menu> Execute(Menu menu)
    {
        menu.DepositPolicy = null;
        menu.UpdatedAt = DateTime.UtcNow;
        return Entity.ValidateEntity(menu, new MenuValidator());        
    }
}