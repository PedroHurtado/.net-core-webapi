# MicroDomain - Guía Completa

**Objetivo**: Máximo paralelismo en desarrollo. Cada desarrollador trabaja en un comando completo sin conflictos de merge.

**Prerrequisitos**: Domain Specification + OpenAPI Contract + Wireframe

---

## Estructura de Carpetas

```
Features/[Feature]/
├── Domain/[Aggregate]/
│   ├── [Aggregate].cs                    ← Aggregate Root (partial)
│   ├── Commands/
│   │   ├── [Aggregate]/
│   │   │   ├── [Aggregate]_Create.cs     ← Command anidado (partial)
│   │   │   ├── [Aggregate]_Update.cs
│   │   │   └── [Aggregate]_[Action].cs
│   │   └── [Entity]/
│   │       └── [Entity]_[Action].cs
│   ├── Entities/
│   │   └── [Entity].cs                   ← Entidad hija (partial)
│   ├── ValueObjects/
│   │   └── [ValueObject].cs
│   └── Enums/
│       └── [Enum].cs
│
├── Api/
│   ├── Commands/                         ← POST, PUT, DELETE
│   │   └── [Action][Aggregate].cs
│   └── Queries/                          ← GET
│       └── Get[Aggregate].cs
│
└── Contracts/                            ← Solo si Response se comparte (3+ usos)
    └── [Aggregate]Response.cs

Tests/[Feature]/
├── Helpers/Testables/
│   └── Testable[Entity].cs
└── Domain/
    ├── [Entity]ValidatorTests.cs
    └── [Entity]_[Action]Tests.cs
```

---

## Fases de Desarrollo

### Fase 1: Modelo de Dominio (Todo el equipo - TDD)

**Orden estricto**:
```
① Enum           (sin tests)
② ValueObject    → Test factory + Test métodos → Implementar
③ Entity hija    → Test validator + Test calculadas → Implementar
④ Aggregate      → Test validator + Test calculadas → Implementar
```

**PR + Code Review antes de Fase 2**

### Fase 2: Commands (En paralelo - TDD)

Cada desarrollador toma uno o más commands:
```
Dev A: MenuCategory_Create.cs + Tests
Dev B: Menu_Create.cs + Tests
Dev C: Menu_Update.cs + Tests
Dev D: Menu_AddCategory.cs + Tests
```

**Ciclo por command**: Test (red) → Implementar (green) → Refactor → PR

### Fase 3: Api (En paralelo)

Cada endpoint en su propio archivo con nested classes.

---

## Componentes del Dominio

### Entity Base
```csharp
public abstract class Entity(Guid id)
{
    public Guid Id { get; init; } = id;  // init = inmutable después de creación
}
```

### AggregateRoot Base
```csharp
public abstract class AggregateRoot(Guid id) : Entity(id)
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Aggregate Root
```csharp
public partial class Menu : AggregateRoot
{
    protected Menu() : base(Guid.Empty) { }        // EF Core
    public Menu(Guid id) : base(id) { }            // Creación

    public Guid RestaurantId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }

    protected HashSet<MenuCategory> _categories = [];
    public IReadOnlyCollection<MenuCategory> Categories => _categories.ToList().AsReadOnly();

    // Propiedades calculadas
    public int TotalItems => _categories.Sum(c => c.Items.Count);
}

public static class MenuValidationMessages
{
    public const string Required = "{PropertyName} is required";
    public const string MaxLength = "{PropertyName} cannot exceed {MaxLength} characters";
    public const string MinValue = "{PropertyName} must be greater than or equal to {ComparisonValue}";
}

public class MenuValidator : AbstractValidator<Menu>
{
    public MenuValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(MenuValidationMessages.Required);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(MenuValidationMessages.Required)
            .MaximumLength(100)
            .WithMessage(MenuValidationMessages.MaxLength);
    }
}
```

**Reglas**:
- `partial class` para comandos en archivos separados
- Hereda de `AggregateRoot`
- Dos constructores (protected + public con Guid)
- `Id` heredado con `init` (inmutable)
- Propiedades `{ get; protected set; }`
- Colecciones: `HashSet<T>` protegido + `IReadOnlyCollection<T>` público
- Validator + ValidationMessages en mismo archivo
- NO lógica de negocio en la entidad

### Entidad Hija
```csharp
public partial class MenuCategory : Entity
{
    protected MenuCategory() : base(Guid.Empty) { }
    public MenuCategory(Guid id) : base(id) { }

    public string Name { get; protected set; } = string.Empty;
    public int DisplayOrder { get; protected set; }

    protected HashSet<MenuItem> _items = [];
    public IReadOnlyCollection<MenuItem> Items => _items.ToList().AsReadOnly();
}

public static class MenuCategoryValidationMessages
{
    public const string Required = "{PropertyName} is required";
    public const string MaxLength = "{PropertyName} cannot exceed {MaxLength} characters";
    public const string MinValue = "{PropertyName} must be greater than or equal to {ComparisonValue}";
}

public class MenuCategoryValidator : AbstractValidator<MenuCategory>
{
    public MenuCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(MenuCategoryValidationMessages.Required);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(MenuCategoryValidationMessages.Required)
            .MaximumLength(100)
            .WithMessage(MenuCategoryValidationMessages.MaxLength);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MenuCategoryValidationMessages.MinValue);
    }
}
```

**Reglas**:
- También `partial class`
- FKs inmutables usan `init` (ej: `MenuId { get; init; }`)
- Command de creación en archivo separado
- Se inyecta en commands del agregado padre

### ValueObject
```csharp
public record DepositPolicy
{
    public DepositType DepositType { get; }
    public decimal Amount { get; }
    public decimal? Percentage { get; }

    private DepositPolicy(DepositType depositType, decimal amount, decimal? percentage)
    {
        DepositType = depositType;
        Amount = amount;
        Percentage = percentage;
    }

    public static DepositPolicy Create(DepositType depositType, decimal amount, decimal? percentage = null)
    {
        var instance = new DepositPolicy(depositType, amount, percentage);
        return new DepositPolicyValidator().ValidateOrThrow(instance);
    }

    // Métodos de negocio
    public decimal CalculateDeposit(int guestCount, decimal estimatedBill)
    {
        return DepositType switch
        {
            DepositType.PerPerson => Amount * guestCount,
            DepositType.PercentageOfBill => estimatedBill * (Percentage!.Value / 100m),
            DepositType.FixedAmount => Amount,
            _ => 0m
        };
    }
}

public static class DepositPolicyValidationMessages
{
    public const string GreaterThanZero = "{PropertyName} must be greater than zero";
    public const string PercentageRange = "{PropertyName} must be between {From} and {To}";
}

public class DepositPolicyValidator : AbstractValidator<DepositPolicy>
{
    public DepositPolicyValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(DepositPolicyValidationMessages.GreaterThanZero);

        RuleFor(x => x.Percentage)
            .InclusiveBetween(1, 100)
            .When(x => x.Percentage.HasValue)
            .WithMessage(DepositPolicyValidationMessages.PercentageRange);
    }
}
```

**Reglas**:
- `record` con `{ get; }` (inmutable)
- Constructor `private`
- Factory `Create()` retorna `T` (no Result)
- Validación con `ValidateOrThrow()`
- Métodos de negocio permitidos
- NO constructor público

---

## Commands

### Clases Base

| Clase | Método | Uso |
|-------|--------|-----|
| `AbstractCreateCommand<TCmd, TEntity>` | `TEntity Execute(TCmd command)` | Crear entidad nueva |
| `AbstractModifyCommand<TCmd, TEntity>` | `TEntity Execute(TEntity entity, TCmd command)` | Modificar con datos |
| `AbstractModifyCommand<TEntity>` | `TEntity Execute(TEntity entity)` | Modificar sin datos |

### CreateCommand
```csharp
// Menu_Create.cs
public record CreateMenuCommand(string Name, string? Description, int DisplayOrder);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(IValidator<Menu> validator)
        : AbstractCreateCommand<CreateMenuCommand, Menu>
    {
        public override Menu Execute(CreateMenuCommand command)
        {
            var menu = new Menu(Guid.NewGuid())
            {
                Name = command.Name,
                Description = command.Description,
                DisplayOrder = command.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            return validator.ValidateOrThrow(menu);
        }
    }
}
```

### ModifyCommand (con datos)
```csharp
// Menu_Update.cs
public record UpdateMenuCommand(string Name, string? Description, int DisplayOrder);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(IValidator<Menu> menuValidator)
        : AbstractModifyCommand<UpdateMenuCommand, Menu>
    {
        public override Menu Execute(Menu entity, UpdateMenuCommand command)
        {
            entity.Name = command.Name;
            entity.Description = command.Description;
            entity.DisplayOrder = command.DisplayOrder;
            entity.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(entity);
        }
    }
}
```

### ModifyCommand (sin datos)
```csharp
// Menu_Activate.cs
public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(IValidator<Menu> validator)
        : AbstractModifyCommand<Menu>
    {
        public override Menu Execute(Menu entity)
        {
            ConflictGuard.ThrowIf(entity.IsActive, "Menu already active");

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;

            return validator.ValidateOrThrow(entity);
        }
    }
}
```

### Command con composición (inyecta command hijo)
```csharp
// Menu_AddCategory.cs
public record AddCategoryCommand(string Name, string? Description = null, int DisplayOrder = 0);

public static class AddCategoryValidationMessages
{
    public const string CategoryNameAlreadyExists = "A category with this name already exists";
}

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
            var duplicateName = menu._categories.Any(c =>
                c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

            ConflictGuard.ThrowIf(
                duplicateName,
                AddCategoryValidationMessages.CategoryNameAlreadyExists
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
```

**Reglas Commands**:
- Archivo separado: `[Entity]_[Action].cs`
- `partial class` + clase anidada
- `[Injectable(ServiceLifetime.Singleton)]` - commands son stateless
- Inyectar validators y commands hijos (NO `new`)
- Acceso a `protected set` por ser clase anidada
- NO usar `Result<T>`, `try-catch`

---

## Guards

| HTTP | Guard | Uso |
|------|-------|-----|
| 404 | `NotFoundGuard.ThrowIfNull(entity, id)` | Entidad no existe |
| 409 | `ConflictGuard.ThrowIf(condition, msg)` | Duplicado / Conflicto |
| 422 | `ValidationGuard.ThrowIf(condition, msg, prop)` | Regla de negocio |
| 422 | `validator.ValidateOrThrow(entity)` | Validación estructural |

### Criterio 422 vs 409

| Pregunta | Código |
|----------|--------|
| ¿El dato en sí mismo es inválido? (formato, rango, vacío) | 422 |
| ¿El dato es válido pero choca con algo que ya existe? | 409 |

---

## Mensajes de Validación

Usar constantes con placeholders de FluentValidation:

```csharp
public static class MenuValidationMessages
{
    // Genéricos (reutilizables)
    public const string Required = "{PropertyName} is required";
    public const string MaxLength = "{PropertyName} cannot exceed {MaxLength} characters";
    public const string MinValue = "{PropertyName} must be greater than or equal to {ComparisonValue}";
    public const string GreaterThanZero = "{PropertyName} must be greater than zero";
    public const string Range = "{PropertyName} must be between {From} and {To}";
    public const string CannotBeNegative = "{PropertyName} cannot be negative";

    // Específicos (reglas de negocio)
    public const string StartDateMustBeEarlierThanEndDate = "Start date must be earlier than end date";
}
```

**Placeholders disponibles**:
- `{PropertyName}` - Nombre de la propiedad
- `{MaxLength}` - Longitud máxima
- `{MinLength}` - Longitud mínima
- `{ComparisonValue}` - Valor de comparación
- `{From}` / `{To}` - Rango

---

## Api

### Estructura del archivo (nested classes)
```csharp
// Api/Commands/CreateMenu.cs
namespace Customer.Features.Menus.Api.Commands;

public static class CreateMenu
{
    public record Request(string Name, string? Description, int DisplayOrder);

    public record Response(Guid Id, string Name);

    public class Handler(Menu.Create menuCreate, IRepository<Menu> repo, IUnitOfWork uow)
    {
        public async Task<Response> Handle(Request request)
        {
            var command = new CreateMenuCommand(request.Name, request.Description, request.DisplayOrder);
            var menu = menuCreate.Execute(command);

            await repo.AddAsync(menu);
            await uow.SaveChangesAsync();

            return new Response(menu.Id, menu.Name);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/menus", async (Request request, Handler handler) =>
        {
            var response = await handler.Handle(request);
            return Results.Created($"/menus/{response.Id}", response);
        });
    }
}
```

```csharp
// Api/Commands/UpdateMenu.cs
public static class UpdateMenu
{
    public record Request(string Name, string? Description, int DisplayOrder);

    public record Response(Guid Id, string Name);

    public class Handler(Menu.Update menuUpdate, IRepository<Menu> repo, IUnitOfWork uow)
    {
        public async Task<Response> Handle(Guid id, Request request)
        {
            var menu = await repo.GetAsync(id);
            NotFoundGuard.ThrowIfNull(menu, id);

            var command = new UpdateMenuCommand(request.Name, request.Description, request.DisplayOrder);
            menuUpdate.Execute(menu, command);

            await uow.SaveChangesAsync();

            return new Response(menu.Id, menu.Name);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/menus/{id:guid}", async (Guid id, Request request, Handler handler) =>
        {
            var response = await handler.Handle(id, request);
            return Results.Ok(response);
        });
    }
}
```

```csharp
// Api/Queries/GetMenu.cs
public static class GetMenu
{
    public record Response(Guid Id, string Name, List<CategoryDto> Categories);
    public record CategoryDto(Guid Id, string Name, int DisplayOrder);

    public class Handler(IRepository<Menu> repo)
    {
        public async Task<Response?> Handle(Guid id)
        {
            var menu = await repo.GetAsync(id);
            if (menu is null) return null;

            return new Response(
                menu.Id,
                menu.Name,
                menu.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.DisplayOrder)).ToList()
            );
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/menus/{id:guid}", async (Guid id, Handler handler) =>
        {
            var response = await handler.Handle(id);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });
    }
}
```

### Registro de endpoints
```csharp
// MenuEndpoints.cs
public static class MenuEndpoints
{
    public static void MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        CreateMenu.MapEndpoint(app);
        UpdateMenu.MapEndpoint(app);
        AddCategory.MapEndpoint(app);
        GetMenu.MapEndpoint(app);
        GetMenus.MapEndpoint(app);
    }
}

// Program.cs
app.MapMenuEndpoints();
```

---

## Testing

### Testable Helpers

Para testear validators de forma aislada. Solo implementar `With*` para propiedades en el validator.

```csharp
// Tests/Helpers/Testables/TestableMenu.cs
public class TestableMenu : Menu
{
    public TestableMenu() : base(Guid.NewGuid()) { }

    public TestableMenu WithName(string name)
    {
        Name = name;
        return this;
    }

    public TestableMenu WithDisplayOrder(int order)
    {
        DisplayOrder = order;
        return this;
    }

    public TestableMenu WithCategory(MenuCategory category)
    {
        _categories.Add(category);
        return this;
    }
}
```

### Tests de Validator
```csharp
public class MenuValidatorTests
{
    private readonly MenuValidator _validator = new();

    [Fact]
    public void Name_Empty_ShouldFail()
    {
        var menu = new TestableMenu()
            .WithName("");

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Name_Valid_ShouldPass()
    {
        var menu = new TestableMenu()
            .WithName("Menú del día");

        var result = _validator.Validate(menu);

        result.IsValid.Should().BeTrue();
    }
}
```

### Tests de Propiedades Calculadas
```csharp
[Fact]
public void Total_ShouldIncludeTax()
{
    var invoice = new TestableInvoice()
        .WithTaxRate(0.21m)
        .WithLine(quantity: 1, unitPrice: 100m);

    invoice.Subtotal.Should().Be(100m);
    invoice.TaxAmount.Should().Be(21m);
    invoice.Total.Should().Be(121m);
}
```

### Tests de Commands
```csharp
public class Menu_CreateTests
{
    [Fact]
    public void Create_ValidCommand_ShouldReturnMenu()
    {
        var command = new CreateMenuCommand("Menú del día", "Descripción", 1);
        var createMenu = new Menu.Create(new MenuValidator());

        var menu = createMenu.Execute(command);

        menu.Name.Should().Be("Menú del día");
        menu.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_EmptyName_ShouldThrow()
    {
        var command = new CreateMenuCommand("", null, 0);
        var createMenu = new Menu.Create(new MenuValidator());

        var act = () => createMenu.Execute(command);

        act.Should().Throw<ValidationException>();
    }
}

public class Menu_AddCategoryTests
{
    [Fact]
    public void AddCategory_ValidCommand_ShouldAddToCollection()
    {
        var menu = new Menu.Create(new MenuValidator())
            .Execute(new CreateMenuCommand("Menú", null, 1));

        var addCategory = new Menu.AddCategory(
            new MenuCategory.Create(new MenuCategoryValidator()),
            new MenuValidator()
        );

        var updated = addCategory.Execute(menu, new AddCategoryCommand("Entrantes", null, 1));

        updated.Categories.Should().HaveCount(1);
        updated.Categories.First().Name.Should().Be("Entrantes");
    }
}
```

---

## Namespaces Requeridos

| Namespace | Proporciona |
|-----------|-------------|
| `Fudie.Domain` | `Entity`, `AggregateRoot`, `AbstractCreateCommand<,>`, `AbstractModifyCommand<,>` |
| `Fudie.DependencyInjection` | `[Injectable]` |
| `Fudie.Validation` | `ValidationGuard`, `ConflictGuard`, `NotFoundGuard`, `ValidateOrThrow()` |
| `FluentValidation` | `AbstractValidator<T>`, `IValidator<T>` |
| `Microsoft.Extensions.DependencyInjection` | `ServiceLifetime` |

---

## Errores Comunes

| Error | Corrección |
|-------|------------|
| Lógica en Entity | Mover a Command |
| `protected set` en Id | Usar `init` (inmutable) |
| Constructor público en ValueObject | Usar factory `Create()` |
| Usar `Result<T>` | Lanzar excepciones con Guards |
| `new Validator()` en Command | Inyectar `IValidator<T>` |
| `try-catch` para validaciones | Usar Guards apropiados |
| Duplicado → ValidationGuard (422) | Usar ConflictGuard (409) |
| `List<T>` en colecciones | Usar `HashSet<T>` |
| `[Injectable]` sin Singleton | `[Injectable(ServiceLifetime.Singleton)]` |
| Command fuera de Entity | `partial class` + clase anidada |
| `{ get; set; }` público | `{ get; protected set; }` |
| Instanciar entidad hija directamente | Inyectar y usar su Command.Create |

---

## Checklists

### Fase 1: Modelo
- [ ] Entity es `partial class`
- [ ] Entity tiene dos constructores (protected + public)
- [ ] `Id` usa `init` (no `protected set`)
- [ ] Propiedades usan `{ get; protected set; }`
- [ ] Relaciones inmutables (FKs) usan `init`
- [ ] Colecciones: `HashSet<T>` protegido + `IReadOnlyCollection<T>` público
- [ ] ValidationMessages + Validator en mismo archivo
- [ ] ValueObject usa factory `Create()` con `ValidateOrThrow()`
- [ ] Testable helper creado con métodos `With*`
- [ ] Tests de Validator pasan

### Fase 2: Commands
- [ ] Test éxito escrito (red)
- [ ] Test fallo escrito (red)
- [ ] Archivo nombrado `[Entity]_[Action].cs`
- [ ] `partial class` + clase anidada
- [ ] `[Injectable(ServiceLifetime.Singleton)]`
- [ ] Hereda de AbstractCommand correcto
- [ ] Inyecta validators y commands hijos (no `new`)
- [ ] Usa Guards apropiados (404/409/422)
- [ ] Retorna entidad validada
- [ ] Tests pasan (green)

### Fase 3: Api
- [ ] Archivo en `Api/Commands/` o `Api/Queries/`
- [ ] `static class` con nested Request, Response, Handler
- [ ] Handler inyecta Domain Commands
- [ ] `MapEndpoint()` registra la ruta
- [ ] Guards aplicados (404 antes de ejecutar)
- [ ] Códigos HTTP correctos (201/200/404/409/422)

---

## Resumen

**Un comando = Un archivo = Un desarrollador = Cero conflictos = Encapsulación real**

```
① Test (red) → ② Implementar (green) → ③ Refactor → ④ PR
```

**Nunca avanzar sin test pasando.**

---

*¿Dudas? Pregunta antes de implementar.*
