using Fudie.Domain;
using Fudie.DependencyInjection;

namespace Customer.Features.Menus.Domain.MenuAggregate.Commands;

/// <summary>
/// Comando para eliminar la política de fianzas de un menú.
/// </summary>
public record RemoveDepositPolicyCommand;

/// <summary>
/// Elimina la política de fianzas de un menú existente.
/// </summary>
[Injectable]
public class RemoveDepositPolicy : IModifyCommand<RemoveDepositPolicyCommand, Menu>
{
    public Result<Menu> Execute(Menu menu, RemoveDepositPolicyCommand command)
    {
        menu.DepositPolicy = null;
        menu.UpdatedAt = DateTime.UtcNow;
        return Entity.ValidateEntity(menu, new MenuValidator());        
    }
}