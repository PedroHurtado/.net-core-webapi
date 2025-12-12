# MicroDomain - Workflow de Implementación

**Prerrequisitos**: Domain Specification ✅ | OpenAPI Contract ✅ | Wireframe ✅

---

## 📋 Fase 1: Modelo de Dominio (Todo el equipo - ~2h)

### Orden estricto de implementación (TDD en todo)

```
① Enums              (sin tests)
      ↓
② ValueObjects       (TDD: factory + métodos)
      ↓
③ Entity hija        (TDD: validator + calculadas)
      ↓
④ Aggregate Root     (TDD: validator + calculadas)
```

### Checklist por componente

#### ① Enum
```csharp
public enum DepositType { PerPerson, PercentageOfBill, FixedAmount }
```
- [ ] Creado en `/Enums/`

#### ② ValueObject (TDD)

**Primero los tests:**
```csharp
public class DepositPolicyTests
{
    // Tests de factory
    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        var policy = DepositPolicy.Create(DepositType.PerPerson, 10m);
        policy.Amount.Should().Be(10m);
    }

    [Fact]
    public void Create_NegativeAmount_ShouldThrow()
    {
        var act = () => DepositPolicy.Create(DepositType.PerPerson, -5m);
        act.Should().Throw<ValidationException>();
    }

    // Tests de lógica de negocio
    [Fact]
    public void CalculateDeposit_PerPerson_ShouldMultiplyByGuests()
    {
        var policy = DepositPolicy.Create(DepositType.PerPerson, 10m);
        
        var deposit = policy.CalculateDeposit(guestCount: 4, estimatedBill: 100m);
        
        deposit.Should().Be(40m);  // 10 * 4
    }

    [Fact]
    public void CalculateDeposit_FixedAmount_ShouldReturnAmount()
    {
        var policy = DepositPolicy.Create(DepositType.FixedAmount, 50m);
        
        var deposit = policy.CalculateDeposit(guestCount: 10, estimatedBill: 500m);
        
        deposit.Should().Be(50m);  // Fijo, ignora guests y bill
    }
}
```

**Después la implementación:**
```csharp
public record DepositPolicy
{
    public DepositType DepositType { get; }
    public decimal Amount { get; }
    
    private DepositPolicy(...) { ... }
    
    public static DepositPolicy Create(...)
    {
        var instance = new DepositPolicy(...);
        return new DepositPolicyValidator().ValidateOrThrow(instance);
    }
    
    public decimal CalculateDeposit(int guestCount, decimal estimatedBill)
    {
        return DepositType switch
        {
            DepositType.PerPerson => Amount * guestCount,
            DepositType.FixedAmount => Amount,
            _ => 0m
        };
    }
}
```

- [ ] Tests de `Create()` válido/inválido escritos (red)
- [ ] Tests de métodos de negocio escritos (red)
- [ ] `record` implementado con factory + validator
- [ ] Tests pasan (green)

#### ③ Entity Hija (TDD)

**Primero tests de Validator + propiedades calculadas:**
```csharp
public class InvoiceLineTests
{
    private readonly InvoiceLineValidator _validator = new();

    // Tests de Validator
    [Fact]
    public void Quantity_Zero_ShouldFail()
    {
        var line = new TestableInvoiceLine()
            .WithQuantity(0);
        
        _validator.Validate(line).IsValid.Should().BeFalse();
    }

    // Tests de propiedades calculadas
    [Fact]
    public void LineTotal_ShouldBeQuantityTimesUnitPrice()
    {
        var line = new TestableInvoiceLine()
            .WithQuantity(3)
            .WithUnitPrice(25.50m);
        
        line.LineTotal.Should().Be(76.50m);  // 3 * 25.50
    }
}
```

**Después la Entity:**
```csharp
public partial class InvoiceLine : Entity
{
    protected InvoiceLine() { }
    public InvoiceLine(Guid id) : base(id) { }
    
    public Guid InvoiceId { get; init; }
    public int Quantity { get; protected set; }
    public decimal UnitPrice { get; protected set; }
    
    // Propiedad calculada
    public decimal LineTotal => Quantity * UnitPrice;
}
```

- [ ] Testable helper creado
- [ ] Tests de Validator escritos (red)
- [ ] Tests de propiedades calculadas escritos (red)
- [ ] Entity implementada
- [ ] Tests pasan (green)

#### ④ Aggregate Root (TDD)

**Primero tests de Validator + propiedades calculadas:**
```csharp
public class InvoiceTests
{
    private readonly InvoiceValidator _validator = new();

    // Tests de Validator
    [Fact]
    public void TaxRate_Negative_ShouldFail()
    {
        var invoice = new TestableInvoice()
            .WithTaxRate(-0.1m);
        
        _validator.Validate(invoice).IsValid.Should().BeFalse();
    }

    // Tests de propiedades calculadas
    [Fact]
    public void Subtotal_ShouldSumAllLineTotals()
    {
        var invoice = new TestableInvoice()
            .WithLine(quantity: 2, unitPrice: 10m)   // 20
            .WithLine(quantity: 1, unitPrice: 30m);  // 30
        
        invoice.Subtotal.Should().Be(50m);
    }

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
}
```

**Después el Aggregate:**
```csharp
public partial class Invoice : AggregateRoot
{
    protected Invoice() { }
    public Invoice(Guid id) : base(id) { }
    
    public decimal TaxRate { get; protected set; }
    public HashSet<InvoiceLine> Lines { get; protected set; } = [];
    
    // Propiedades calculadas
    public decimal Subtotal => Lines.Sum(l => l.LineTotal);
    public decimal TaxAmount => Subtotal * TaxRate;
    public decimal Total => Subtotal + TaxAmount;
}
```

- [ ] Testable helper creado (con `WithLine()` para facilitar)
- [ ] Tests de Validator escritos (red)
- [ ] Tests de propiedades calculadas escritos (red)
- [ ] Aggregate implementado
- [ ] Tests pasan (green)

### 🚦 Checkpoint Fase 1
```
□ Todos los Enums creados
□ Todos los ValueObjects con factory + validator
□ Todas las Entities con partial + dos constructores
□ Aggregate Root hereda de AggregateRoot
□ Testable helpers creados
□ Tests de Validators pasan (green)
□ PR creado → Code Review conjunto
```

**⏸️ STOP - No avanzar hasta PR aprobado**

---

## ⚡ Fase 2: Commands (En paralelo - cada dev)

### Asignación de trabajo
```
┌───────────────────────────────────────────────────────────────┐
│  Dev A: MenuCategory_Create.cs    + MenuCategory_CreateTests  │
│  Dev B: Menu_Create.cs            + Menu_CreateTests          │
│  Dev C: Menu_Update.cs            + Menu_UpdateTests          │
│  Dev D: Menu_AddCategory.cs       + Menu_AddCategoryTests     │
└───────────────────────────────────────────────────────────────┘
```

### ⚠️ Regla de oro: TDD por Command
```
① Escribir Test (red)
② Implementar Command (green)  
③ Refactor si necesario
④ PR individual
```

**Nunca avanzar al siguiente command sin test pasando.**

### Template de Command

#### ⚠️ Primero el Test (TDD)
```csharp
public class Menu_CreateTests
{
    [Fact]
    public void Create_ValidCommand_ShouldReturnMenu()
    {
        var cmd = new CreateMenuCommand("Menú", null, 1);
        var create = new Menu.Create(new MenuValidator());
        
        var menu = create.Handle(cmd);
        
        menu.Name.Should().Be("Menú");
        menu.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_EmptyName_ShouldThrow()
    {
        var cmd = new CreateMenuCommand("", null, 0);
        var create = new Menu.Create(new MenuValidator());
        
        var act = () => create.Handle(cmd);
        
        act.Should().Throw<ValidationException>();
    }
}
```

#### CreateCommand (entidad nueva)
```csharp
// Menu_Create.cs
public record CreateMenuCommand(string Name, string? Description, int DisplayOrder);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(IValidator<Menu> validator) 
        : AbstractCreateCommand<CreateMenuCommand, Menu>
    {
        public override Menu Handle(CreateMenuCommand cmd)
        {
            var menu = new Menu(Guid.NewGuid())
            {
                Name = cmd.Name,
                Description = cmd.Description,
                DisplayOrder = cmd.DisplayOrder
            };
            return validator.ValidateOrThrow(menu);
        }
    }
}
```

#### ModifyCommand (con datos)
```csharp
// Menu_Update.cs
public record UpdateMenuCommand(string Name, string? Description, int DisplayOrder);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(IValidator<Menu> validator) 
        : AbstractModifyCommand<UpdateMenuCommand, Menu>
    {
        public override Menu Handle(Menu entity, UpdateMenuCommand cmd)
        {
            entity.Name = cmd.Name;
            entity.Description = cmd.Description;
            entity.DisplayOrder = cmd.DisplayOrder;
            return validator.ValidateOrThrow(entity);
        }
    }
}
```

#### ModifyCommand (sin datos)
```csharp
// Menu_Activate.cs
public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(IValidator<Menu> validator) 
        : AbstractModifyCommand<Menu>
    {
        public override Menu Handle(Menu entity)
        {
            ConflictGuard.ThrowIf(entity.IsActive, "Menu already active");
            entity.IsActive = true;
            return validator.ValidateOrThrow(entity);
        }
    }
}
```

#### Command con composición (inyecta command hijo)
```csharp
// Menu_AddCategory.cs
public record AddCategoryCommand(string Name, int DisplayOrder);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddCategory(
        MenuCategory.Create createCategory,
        IValidator<Menu> validator
    ) : AbstractModifyCommand<AddCategoryCommand, Menu>
    {
        public override Menu Handle(Menu menu, AddCategoryCommand cmd)
        {
            ConflictGuard.ThrowIf(
                menu.Categories.Any(c => c.Name == cmd.Name),
                $"Category '{cmd.Name}' already exists");
            
            var category = createCategory.Handle(
                new CreateCategoryCommand(menu.Id, cmd.Name, cmd.DisplayOrder)
            );
            
            menu.Categories.Add(category);
            return validator.ValidateOrThrow(menu);
        }
    }
}
```

### 🚦 Checkpoint por Command (TDD)
```
□ Test de éxito escrito (red)
□ Test de fallo escrito (red)
□ Archivo nombrado [Entity]_[Action].cs
□ partial class + clase anidada
□ [Injectable(ServiceLifetime.Singleton)]
□ Hereda de AbstractCommand correcto
□ Inyecta validators (no new)
□ Usa Guards apropiados (404/409/422)
□ Retorna entidad validada
□ Tests pasan (green)
□ PR individual creado
```

---

## 🔌 Fase 3: Api (En paralelo)

### Estructura del archivo Api (nested classes)

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
            var cmd = new CreateMenuCommand(request.Name, request.Description, request.DisplayOrder);
            var menu = menuCreate.Handle(cmd);
            
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
namespace Customer.Features.Menus.Api.Commands;

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
            
            var cmd = new UpdateMenuCommand(request.Name, request.Description, request.DisplayOrder);
            menuUpdate.Handle(menu, cmd);
            
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
namespace Customer.Features.Menus.Api.Queries;

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
// Program.cs o extensión
app.MapMenuEndpoints();

// MenuEndpoints.cs
public static class MenuEndpoints
{
    public static void MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        // Commands
        CreateMenu.MapEndpoint(app);
        UpdateMenu.MapEndpoint(app);
        AddCategory.MapEndpoint(app);
        
        // Queries
        GetMenu.MapEndpoint(app);
        GetMenus.MapEndpoint(app);
    }
}
```

### Cuándo mover Response a Contracts/

```
Contracts/
└── MenuResponse.cs   ← Solo cuando se repite en múltiples endpoints
```

Decisión de equipo: si el mismo Response se usa en 3+ endpoints, mover a Contracts.

### 🚦 Checkpoint por Api endpoint
```
□ Archivo en Api/Commands/ o Api/Queries/
□ static class con nested Request, Response, Handler
□ Handler inyecta Domain Commands (no lógica de negocio)
□ MapEndpoint() registra la ruta
□ Request local al archivo (a menos que se comparta)
□ Response local al archivo (mover a Contracts si se repite 3+)
□ Guards aplicados (404 antes de ejecutar)
□ Códigos HTTP correctos (201/200/404/409/422)
```

---

## 🛡️ Referencia Rápida: Guards

| Situación | Guard | HTTP |
|-----------|-------|------|
| Entidad no encontrada | `NotFoundGuard.ThrowIfNull(entity, id)` | 404 |
| Duplicado / Conflicto | `ConflictGuard.ThrowIf(condition, msg)` | 409 |
| Regla de negocio | `ValidationGuard.ThrowIf(condition, msg, prop)` | 422 |
| Validación estructural | `validator.ValidateOrThrow(entity)` | 422 |

---

## 📁 Estructura Final

```
Features/
└── Menus/
    ├── Domain/
    │   └── MenuAggregate/
    │       ├── Menu.cs
    │       ├── Commands/
    │       │   ├── Menu_Create.cs
    │       │   ├── Menu_Update.cs
    │       │   └── Menu_AddCategory.cs
    │       ├── Entities/
    │       │   ├── MenuCategory.cs
    │       │   └── MenuCategory_Create.cs
    │       ├── ValueObjects/
    │       │   └── Price.cs
    │       └── Enums/
    │           └── DepositType.cs
    │
    ├── Api/
    │   ├── Commands/
    │   │   ├── CreateMenu.cs
    │   │   ├── UpdateMenu.cs
    │   │   └── AddCategory.cs
    │   └── Queries/
    │       ├── GetMenu.cs
    │       ├── GetMenus.cs
    │       └── GetMenuCategories.cs
    │
    └── Contracts/                 ← Solo si se comparte entre endpoints
        └── MenuResponse.cs

Tests/
└── Menus/
    ├── Helpers/
    │   └── Testables/
    │       ├── TestableMenu.cs
    │       └── TestableMenuCategory.cs
    └── Domain/
        ├── MenuValidatorTests.cs
        ├── Menu_CreateTests.cs
        └── Menu_UpdateTests.cs
```

---

## ⏱️ Tiempos Estimados

| Fase | Duración | Modalidad |
|------|----------|-----------|
| Fase 1: Modelo + Tests Validators | ~2h | Todo el equipo |
| Code Review | ~30min | Todo el equipo |
| Fase 2: Commands + Tests (TDD) | ~1-2h | Paralelo |
| Fase 3: Api (Commands + Queries) | ~1h | Paralelo |

**Total por agregado**: ~4-5h con 4 devs en paralelo

---

*Referencia: MICRODOMAIN_TEAM_NOTES.md para detalles*
