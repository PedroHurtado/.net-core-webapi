# Patrón MicroDomain - Notas para el Equipo

**Audiencia**: Desarrolladores Junior

---

## 🎯 Objetivo

Máximo paralelismo en desarrollo. Cada desarrollador trabaja en un **comando completo** sin conflictos de merge. **Encapsulación real** con `protected set` e **inmutabilidad del Id** con `init`.

---

## 📁 Estructura
```
Features/[Feature]/
├── Domain/
│   └── [Aggregate]/
│       ├── [Aggregate].cs              ← Aggregate Root (partial)
│       ├── Commands/
│       │   ├── [Aggregate]_Create.cs   ← Command anidado (partial)
│       │   ├── [Aggregate]_Update.cs
│       │   └── [Aggregate]_[Action].cs
│       ├── Entities/
│       │   ├── [Entity].cs             ← Entidad hija (partial)
│       │   └── [Entity]_Create.cs      ← Command de entidad hija
│       ├── ValueObjects/
│       │   └── [ValueObject].cs
│       └── Enums/
│           └── [Enum].cs
│
├── Api/
│   ├── Commands/                       ← POST, PUT, DELETE
│   │   ├── Create[Aggregate].cs
│   │   ├── Update[Aggregate].cs
│   │   └── [Action].cs
│   └── Queries/                        ← GET
│       ├── Get[Aggregate].cs
│       └── Get[Aggregate]s.cs
│
└── Contracts/                          ← Solo si Response se comparte (3+ usos)
    └── [Aggregate]Response.cs

Tests/
└── [Feature]/
    ├── Helpers/
    │   └── Testables/
    │       └── Testable[Entity].cs
    └── Domain/
        ├── [Entity]ValidatorTests.cs
        └── [Entity]_[Action]Tests.cs
```

**Naming de archivos Domain**: `Menu.cs`, `Menu_Create.cs`, `Menu_Update.cs`
**Naming de archivos Api**: `CreateMenu.cs`, `UpdateMenu.cs`, `GetMenu.cs`

---

## 📋 Reglas por Componente

### Entity Base
```csharp
public abstract class Entity(Guid id)
{
    public Guid Id { get; init; } = id;  // ← init = inmutable después de creación
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

**Reglas clave**: 
- El `Id` usa `init`, no `protected set`. Una vez creada la entidad, su identidad **nunca cambia**.
- Solo los Aggregate Roots pueden emitir Domain Events.

---

### Aggregate Root
```csharp
public partial class Menu : AggregateRoot
{
    protected Menu() { }                    // ← EF Core
    public Menu(Guid id) : base(id) { }     // ← Creación
    
    public string Name { get; protected set; }       // ← Encapsulado
    public HashSet<MenuCategory> Categories { get; protected set; } = [];
}

public class MenuValidator : AbstractValidator<Menu>
{
    public MenuValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

**Reglas**:
- ✅ `partial class` para permitir comandos en archivos separados
- ✅ Hereda de `AggregateRoot` (no de Entity directamente)
- ✅ Dos constructores (protected + public con Guid)
- ✅ `Id` heredado con `init` (inmutable)
- ✅ Propiedades `{ get; protected set; }` encapsuladas
- ✅ Colecciones: `HashSet<T>` con inicializador `= []`
- ✅ Validator en mismo archivo, clase separada
- ✅ Acceso a `AddDomainEvent()` para publicar eventos
- ❌ NO lógica de negocio en la entidad

---

### Entidad Hija (también partial)
```csharp
// MenuCategory.cs
public partial class MenuCategory : Entity
{
    protected MenuCategory() { }                    // ← EF Core
    public MenuCategory(Guid id) : base(id) { }     // ← Creación
    
    public Guid MenuId { get; init; }               // ← init = relación inmutable
    public string Name { get; protected set; }
    public int DisplayOrder { get; protected set; }
}

public class MenuCategoryValidator : AbstractValidator<MenuCategory>
{
    public MenuCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
```

```csharp
// MenuCategory_Create.cs
public record CreateCategoryCommand(Guid MenuId, string Name, int DisplayOrder);

public partial class MenuCategory
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(IValidator<MenuCategory> validator) 
        : AbstractCreateCommand<CreateCategoryCommand, MenuCategory>
    {
        public override MenuCategory Handle(CreateCategoryCommand command)
        {
            var category = new MenuCategory(Guid.NewGuid())
            {
                MenuId = command.MenuId,
                Name = command.Name,
                DisplayOrder = command.DisplayOrder
            };
            return validator.ValidateOrThrow(category);
        }
    }
}
```

**Reglas**:
- ✅ Entidades hijas también usan `partial class`
- ✅ `MenuId` con `init` (relación inmutable)
- ✅ Command de creación en archivo separado
- ✅ Se inyecta en commands del agregado padre

---

### ValueObject
```csharp
public record Price
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Price Create(decimal amount, string currency)
    {
        var instance = new Price(amount, currency);
        return new PriceValidator().ValidateOrThrow(instance);
    }
}

public class PriceValidator : AbstractValidator<Price>
{
    public PriceValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
```

**Reglas**:
- ✅ `record` con `{ get; }` (inmutable)
- ✅ Constructor `private`
- ✅ Factory `Create()` retorna `T` (no Result)
- ✅ Validación con `ValidateOrThrow()`
- ❌ NO constructor público

---

### Command del Agregado (Clase Anidada)
```csharp
// Archivo: Menu_Update.cs
public record UpdateMenuCommand(string Name, string? Description, int DisplayOrder);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(IValidator<Menu> menuValidator) 
        : AbstractModifyCommand<UpdateMenuCommand, Menu>
    {
        public override Menu Handle(Menu entity, UpdateMenuCommand command)
        {
            entity.Name = command.Name;           // ✅ Accede a protected set
            entity.Description = command.Description;
            entity.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(entity);
        }
    }
}
```

---

### Command que Compone Entidades Hijas
```csharp
// Archivo: Menu_AddCategory.cs
public record AddCategoryCommand(string Name, int DisplayOrder);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AddCategory(
        MenuCategory.Create createCategory,  // ← Inyecta command de entidad hija
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<AddCategoryCommand, Menu>
    {
        public override Menu Handle(Menu menu, AddCategoryCommand command)
        {
            var category = createCategory.Handle(
                new CreateCategoryCommand(menu.Id, command.Name, command.DisplayOrder)
            );
            
            menu.Categories.Add(category);
            return menuValidator.ValidateOrThrow(menu);
        }
    }
}
```

**Reglas Commands**:
- ✅ Archivo separado con `partial class`
- ✅ Clase anidada dentro de la Entity
- ✅ `[Injectable(ServiceLifetime.Singleton)]` - comandos son stateless
- ✅ Hereda de `AbstractCreateCommand<,>` o `AbstractModifyCommand<,>`
- ✅ Implementa `Handle()` con la lógica de negocio
- ✅ Acceso a `protected set` por ser clase anidada
- ✅ Inyectar validators y commands hijos (no instanciar con `new`)
- ❌ NO usar `Result<T>`, `try-catch`, ni `new Validator()`

---

## 📦 Namespaces Requeridos

| Namespace | Proporciona |
|-----------|-------------|
| `Fudie.Domain` | `Entity`, `AggregateRoot`, `AbstractCreateCommand<,>`, `AbstractModifyCommand<,>`, `ConflictException` |
| `Fudie.DependencyInjection` | `[Injectable]`, `ServiceLifetime` para registro automático en DI |
| `Fudie.Validation` | `ValidationGuard`, `ConflictGuard`, `NotFoundGuard`, `ValidateOrThrow()` |
| `FluentValidation` | `IValidator<T>` para inyectar validators |

---

## 🔌 Clases Base de Comandos

| Clase | Método a implementar | Uso |
|-------|---------------------|-----|
| `AbstractCreateCommand<TCmd, TEntity>` | `TEntity Handle(TCmd command)` | Crear entidad nueva |
| `AbstractModifyCommand<TCmd, TEntity>` | `TEntity Handle(TEntity entity, TCmd command)` | Modificar con datos |
| `AbstractModifyCommand<TEntity>` | `TEntity Handle(TEntity entity)` | Modificar sin datos |

---

## 🔒 Encapsulación Real

Las clases anidadas en C# tienen acceso a miembros `protected` de la clase contenedora:
```csharp
// Menu.cs
public partial class Menu : Entity
{
    public string Name { get; protected set; }  // ← protected set
}

// Menu_Update.cs
public partial class Menu
{
    public class Update : AbstractModifyCommand<UpdateMenuCommand, Menu>
    {
        public override Menu Handle(Menu entity, UpdateMenuCommand command)
        {
            entity.Name = command.Name;  // ✅ Compila - clase anidada
            return entity;
        }
    }
}

// En cualquier otra clase
menu.Name = "Hack";  // ❌ No compila - protected set
```

---

## 🔄 Flujo en Handler (Imperativo)
```csharp
app.MapPost("/menus", async (
    CreateMenuRequest request,
    Menu.Create menuCreate,
    IRepository<Menu> repo,
    IUnitOfWork uow) =>
{
    var command = new CreateMenuCommand(request.Name, request.Description, request.DisplayOrder);
    
    var menu = menuCreate.Handle(command);
    
    await repo.AddAsync(menu);
    await uow.SaveChangesAsync();
    
    return Results.Created($"/menus/{menu.Id}", MapToResponse(menu));
});

app.MapPut("/menus/{id}", async (
    Guid id,
    UpdateMenuRequest request,
    Menu.Update menuUpdate,
    IRepository<Menu> repo,
    IUnitOfWork uow) =>
{
    var menu = await repo.GetAsync(id);
    NotFoundGuard.ThrowIfNull(menu, id);
    
    var command = new UpdateMenuCommand(request.Name, request.Description, request.DisplayOrder);
    
    var updated = menuUpdate.Handle(menu, command);
    
    await uow.SaveChangesAsync();
    
    return Results.Ok(MapToResponse(updated));
});

app.MapPost("/menus/{id}/categories", async (
    Guid id,
    AddCategoryRequest request,
    Menu.AddCategory addCategory,
    IRepository<Menu> repo,
    IUnitOfWork uow) =>
{
    var menu = await repo.GetAsync(id);
    NotFoundGuard.ThrowIfNull(menu, id);
    
    var command = new AddCategoryCommand(request.Name, request.DisplayOrder);
    
    var updated = addCategory.Handle(menu, command);
    
    await uow.SaveChangesAsync();
    
    return Results.Ok(MapToResponse(updated));
});
```

**Nota**: `GlobalExceptionHandler` convierte las excepciones en respuestas HTTP apropiadas.

---

## 🧪 Testing

### Estructura de Tests
```
Tests/
├── Helpers/
│   └── Testables/
│       ├── TestableMenu.cs
│       └── TestableMenuCategory.cs
└── Domain/
    ├── MenuValidatorTests.cs
    ├── MenuCategoryValidatorTests.cs
    ├── Menu_CreateTests.cs
    ├── Menu_UpdateTests.cs
    └── Menu_AddCategoryTests.cs
```

### Testable Helpers

Para testear validadores de forma aislada, crear clases que heredan de la Entity con FluentInterface. Solo implementar métodos `With*` para propiedades que están en el validador.
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
}
```

### Tests de Validador
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

### Tests de Commands
```csharp
public class Menu_CreateTests
{
    [Fact]
    public void Create_ValidCommand_ShouldReturnMenu()
    {
        var command = new CreateMenuCommand("Menú del día", "Descripción", 1);
        var createMenu = new Menu.Create(new MenuValidator());
        
        var menu = createMenu.Handle(command);
        
        menu.Name.Should().Be("Menú del día");
        menu.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_EmptyName_ShouldThrow()
    {
        var command = new CreateMenuCommand("", null, 0);
        var createMenu = new Menu.Create(new MenuValidator());
        
        var act = () => createMenu.Handle(command);
        
        act.Should().Throw<ValidationException>();
    }
}

public class Menu_AddCategoryTests
{
    [Fact]
    public void AddCategory_ValidCommand_ShouldAddToCollection()
    {
        var menu = new Menu.Create(new MenuValidator())
            .Handle(new CreateMenuCommand("Menú", null, 1));
        
        var addCategory = new Menu.AddCategory(
            new MenuCategory.Create(new MenuCategoryValidator()),
            new MenuValidator()
        );
        
        var updated = addCategory.Handle(menu, new AddCategoryCommand("Entrantes", 1));
        
        updated.Categories.Should().HaveCount(1);
        updated.Categories.First().Name.Should().Be("Entrantes");
        updated.Categories.First().MenuId.Should().Be(menu.Id);
    }
}
```

### Orden de Tests

1. **Fase 1**: Tests de Validadores (con Testable helpers)
2. **Fase 2**: Tests de Commands (en paralelo, cada dev sus commands)

Esto garantiza independencia de equipos: el validador funciona antes de que existan los commands.

---

## 🛡️ Herramientas de Validación

| Herramienta | HTTP | Cuándo usar |
|-------------|------|-------------|
| `IValidator<T>.ValidateOrThrow(entity)` | 422 | Validación estructural (formato, rangos, requeridos) |
| `ValidationGuard.ThrowIf(condition, message, property)` | 422 | Reglas de negocio que invalidan los datos |
| `ConflictGuard.ThrowIf(condition, message)` | 409 | Conflictos con estado actual (duplicados, transiciones inválidas) |
| `NotFoundGuard.ThrowIfNull(entity)` | 404 | Entidad no existe (detecta nombre automáticamente) |
| `NotFoundGuard.ThrowIfNull(entity, id)` | 404 | Entidad no existe (incluye Id en mensaje) |

### Criterio 422 vs 409

| Pregunta | Código |
|----------|--------|
| ¿El dato en sí mismo es inválido? (formato, rango, vacío) | 422 |
| ¿El dato es válido pero choca con algo que ya existe? | 409 |

---

## 🔄 Flujo del Comando

1. **Buscar** entidades relacionadas con `NotFoundGuard.ThrowIfNull()`
2. **Crear** entidad/value object con datos del command
3. **Validar estructuralmente** con `validator.ValidateOrThrow()`
4. **Validar conflictos** con `ConflictGuard.ThrowIf()`
5. **Modificar** estado del agregado
6. **Retornar** agregado validado con `aggregateValidator.ValidateOrThrow()`

---

## ⚡ Orden de Desarrollo

### Fase 1: Modelo de Dominio (TODO EL EQUIPO)
```
1. Enums           (0 dependencias)
2. ValueObjects    (solo enums)
3. Entity hoja     (sin hijos, partial class)
4. Entity padre    (con colecciones, partial class)
5. Aggregate Root  (partial class)
6. Testable helpers
7. Tests de Validators
```

**Objetivo**: 
- Validar Domain Specification contra código real
- Todo el equipo conoce el dominio antes de desarrollar
- Detectar errores de diseño temprano
- Validadores testeados y funcionando

**Entregable**: PR con modelo completo + tests de validadores → Code Review conjunto

---

### Fase 2: Commands (EN PARALELO)
```
8. Commands de entidades hijas  (ej: MenuCategory.Create)
9. Commands del agregado        (cada dev toma uno o más)
10. Tests de Commands
```

Cada comando en su propio archivo `[Entity]_[Action].cs` como `partial class`.

**Requisito**: Fase 1 completada y aprobada

---

**Cada paso**: Código + Test → Validar → Siguiente

---

## ❌ Errores Comunes

| Error | Corrección |
|-------|------------|
| Lógica en Entity | Mover a Command |
| `protected set` en Id | Usar `init` (inmutable) |
| Constructor público en ValueObject | Usar factory `Create()` |
| Usar `Result<T>` | Lanzar excepciones con Guards |
| Instanciar validator con `new` en comando | Inyectar `IValidator<T>` |
| Usar `try-catch` para validaciones | Usar Guards apropiados |
| Duplicado → ValidationGuard (422) | Usar ConflictGuard (409) |
| `List<T>` en colecciones | Usar `HashSet<T>` |
| `[Injectable]` sin Singleton | Usar `[Injectable(ServiceLifetime.Singleton)]` |
| Command fuera de la Entity | Usar `partial class` + clase anidada |
| `{ get; set; }` público | Usar `{ get; protected set; }` |
| `new` en Testable helper | Usar métodos `With*` que asignan a la propiedad base |
| Entidad hija sin `partial` | También usar `partial class` |
| Instanciar entidad hija directamente | Inyectar y usar su Command.Create |

---

## 📝 Checklist antes de PR

### Fase 1: Modelo
- [ ] Entity es `partial class`
- [ ] Entity tiene dos constructores
- [ ] `Id` usa `init` (no `protected set`)
- [ ] Propiedades usan `{ get; protected set; }`
- [ ] Relaciones inmutables (MenuId) usan `init`
- [ ] Validator en mismo archivo que Entity
- [ ] ValueObject usa factory `Create()` con `ValidateOrThrow()`
- [ ] Testable helper creado con métodos `With*`
- [ ] Tests de Validator pasan

### Fase 2: Commands
- [ ] Command es clase anidada en `partial class`
- [ ] Command tiene `[Injectable(ServiceLifetime.Singleton)]`
- [ ] Command hereda de `AbstractCreateCommand<,>` o `AbstractModifyCommand<,>`
- [ ] Command implementa `Handle()` con lógica de negocio
- [ ] Command inyecta validators y commands hijos (no usa `new`)
- [ ] Command usa Guards apropiados (404/409/422)
- [ ] Command retorna entidad validada
- [ ] Tests de Command cubren éxito y fallo
- [ ] Archivo nombrado `[Entity]_[Action].cs`

---

## 🎯 Beneficio Final

**Un comando = Un archivo = Un desarrollador = Cero conflictos = Encapsulación real**

---

*¿Dudas? Pregunta antes de implementar.*
