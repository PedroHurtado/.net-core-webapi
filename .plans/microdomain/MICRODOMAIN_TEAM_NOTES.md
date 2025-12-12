# Patrón MicroDomain - Notas para el Equipo

**Audiencia**: Desarrolladores Junior

---

## 🎯 Objetivo

Máximo paralelismo en desarrollo. Cada desarrollador trabaja en un **comando completo** sin conflictos de merge. **Encapsulación real** con `protected set`.

---

## 📁 Estructura
```
Domain/[Aggregate]/
├── [Aggregate].cs              ← Aggregate Root (partial)
├── [Aggregate]_[Action].cs     ← Command anidado (partial)
├── Entities/
│   └── [Entity].cs             ← Entidades hijas
├── ValueObjects/
│   └── [ValueObject].cs        ← Objetos de valor
└── Enums/
    └── [Enum].cs               ← Enumeraciones

Tests/
├── Helpers/
│   └── Testables/
│       └── Testable[Entity].cs ← Helper para tests de validadores
└── Domain/
    ├── [Entity]ValidatorTests.cs
    └── [Entity]_[Action]Tests.cs
```

**Naming de archivos**: `Menu.cs`, `Menu_Create.cs`, `Menu_Update.cs`, `Menu_Activate.cs`

---

## 📋 Reglas por Componente

### Entity (incluye Aggregate Root)
```csharp
public partial class Foo : Entity
{
    protected Foo() { }                    // ← EF Core
    public Foo(Guid id) : base(id) { }     // ← Creación
    
    public string Name { get; protected set; }       // ← Encapsulado
    public HashSet<Bar> Bars { get; protected set; } = [];
}

public class FooValidator : AbstractValidator<Foo>
{
    public FooValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

**Reglas**:
- ✅ `partial class` para permitir comandos en archivos separados
- ✅ Dos constructores (protected + public con Guid)
- ✅ Propiedades `{ get; protected set; }` encapsuladas
- ✅ Colecciones: `HashSet<T>` con inicializador `= []`
- ✅ Validator en mismo archivo, clase separada
- ❌ NO lógica de negocio en la entidad

---

### ValueObject
```csharp
public record Baz
{
    public BazType Type { get; }
    public decimal Amount { get; }
    
    private Baz(BazType type, decimal amount)
    {
        Type = type;
        Amount = amount;
    }
    
    public static Baz Create(BazType type, decimal amount)
    {
        var instance = new Baz(type, amount);
        return new BazValidator().ValidateOrThrow(instance);
    }
}

public class BazValidator : AbstractValidator<Baz>
{
    public BazValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
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

### Command (Clase Anidada)
```csharp
// Archivo: Menu_Update.cs
namespace Customer.Features.Menus.Domain.MenuAggregate;

using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Validation;
using FluentValidation;

public record UpdateMenuCommand(
    string Name,
    string? Description,
    int DisplayOrder
);

public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<Menu> menuValidator
    ) : AbstractModifyCommand<UpdateMenuCommand, Menu>
    {
        protected override Menu Handle(Menu entity, UpdateMenuCommand command)
        {
            entity.Name = command.Name;           // ✅ Accede a protected set
            entity.Description = command.Description;
            entity.UpdatedAt = DateTime.UtcNow;

            return menuValidator.ValidateOrThrow(entity);
        }
    }
}
```

**Reglas**:
- ✅ Archivo separado con `partial class`
- ✅ Clase anidada dentro de la Entity
- ✅ `[Injectable(ServiceLifetime.Singleton)]` - comandos son stateless
- ✅ Hereda de `AbstractCreateCommand<,>`, `AbstractModifyCommand<,>` o `AbstractModifyCommand<>`
- ✅ Implementa `Handle()` con la lógica de negocio
- ✅ Acceso a `protected set` por ser clase anidada
- ✅ Inyectar `IValidator<T>` (no instanciar con `new`)
- ❌ NO usar `Result<T>`, `try-catch`, ni `new Validator()`

---

## 📦 Namespaces Requeridos

| Namespace | Proporciona |
|-----------|-------------|
| `Fudie.Domain` | `Entity`, `AggregateRoot`, `AbstractCreateCommand<,>`, `AbstractModifyCommand<,>`, `AbstractModifyCommand<>`, `ConflictException` |
| `Fudie.DependencyInjection` | `[Injectable]`, `ServiceLifetime` para registro automático en DI |
| `Fudie.Validation` | `ValidationGuard`, `ConflictGuard`, `NotFoundGuard`, `ValidateOrThrow()` |
| `FluentValidation` | `IValidator<T>` para inyectar validators |

---

## 🔌 Clases Base de Comandos

| Clase | Método a implementar | Uso |
|-------|---------------------|-----|
| `AbstractCreateCommand<TCmd, TEntity>` | `protected override TEntity Handle(TCmd command)` | Crear entidad nueva |
| `AbstractModifyCommand<TCmd, TEntity>` | `protected override TEntity Handle(TEntity entity, TCmd command)` | Modificar con datos |
| `AbstractModifyCommand<TEntity>` | `protected override TEntity Handle(TEntity entity)` | Modificar sin datos |

---

## 🔗 API Fluent

Los comandos exponen una API fluent type-safe. El compilador fuerza el orden correcto.

### Crear entidad
```csharp
var entity = await menuCreate
    .Create(command)
    .Save(uow.SaveChangesAsync)
    .GetEntity();
```

### Modificar con datos
```csharp
var entity = await menuUpdate
    .Find(repository.Get(id))
    .Execute(command)
    .Save(uow.SaveChangesAsync)
    .GetEntity();
```

### Modificar sin datos
```csharp
var entity = await menuActivate
    .Find(repository.Get(id))
    .Execute()
    .Save(uow.SaveChangesAsync)
    .GetEntity();
```

### Sin persistir (para tests)
```csharp
var entity = menuCreate
    .Create(command)
    .GetEntity();
```

**El compilador garantiza el orden**: No puedes llamar `Execute()` sin `Find()`, ni `Save()` sin `Execute()`.

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
        protected override Menu Handle(Menu entity, UpdateMenuCommand command)
        {
            entity.Name = command.Name;  // ✅ Compila - clase anidada
            return entity;
        }
    }
}

// En Handler (fuera de Menu)
menu.Name = "Hack";  // ❌ No compila - protected set
```

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
    ├── Menu_CreateTests.cs
    └── Menu_UpdateTests.cs
```

### Testable Helpers

Para testear validadores de forma aislada, crear clases que heredan de la Entity con FluentInterface. Solo implementar métodos `With*` para propiedades que están en el validador.
```csharp
// Tests/Helpers/Testables/TestableMenu.cs
public class TestableMenu : Menu
{
    public TestableMenu() : base(Guid.NewGuid()) { }
    
    // Solo propiedades validadas en MenuValidator
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
        
        var menu = createMenu
            .Create(command)
            .GetEntity();
        
        menu.Name.Should().Be("Menú del día");
    }

    [Fact]
    public void Create_EmptyName_ShouldThrow()
    {
        var command = new CreateMenuCommand("", null, 0);
        var createMenu = new Menu.Create(new MenuValidator());
        
        var act = () => createMenu.Create(command).GetEntity();
        
        act.Should().Throw<ValidationException>();
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

## 🔄 Flujo en Handler
```csharp
app.MapPut("/menus/{id}", async (
    Guid id,
    MenuUpdateRequest request,
    Menu.Update menuUpdate,
    IGet<Menu, Guid> repo,
    IUnitOfWork uow,
    ILogger<Program> logger) =>
{
    var cmd = new UpdateMenuCommand(request.Name, request.Description, request.DisplayOrder);
    
    var updated = await menuUpdate
        .Find(repo.Get(id))
        .Execute(cmd)
        .Save(uow.SaveChangesAsync)
        .GetEntity();
    
    logger.LogInformation("Updated Menu {Id}", updated.Id);
    return Results.Ok(MapToResponse(updated));
});
```

**Nota**: `GlobalExceptionHandler` convierte las excepciones en respuestas HTTP apropiadas.

---

## ⚡ Orden de Desarrollo

### Fase 1: Modelo de Dominio (TODO EL EQUIPO)
```
1. Enums           (0 dependencias)
2. ValueObjects    (solo enums)
3. Entity hoja     (sin hijos)
4. Entity padre    (con colecciones)
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
8. Commands        (cada dev toma uno o más)
9. Tests de Commands
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

---

## 📝 Checklist antes de PR

### Fase 1: Modelo
- [ ] Entity es `partial class`
- [ ] Entity tiene dos constructores
- [ ] Entity usa `{ get; protected set; }`
- [ ] Validator en mismo archivo que Entity
- [ ] ValueObject usa factory `Create()` con `ValidateOrThrow()`
- [ ] Testable helper creado con métodos `With*`
- [ ] Tests de Validator pasan

### Fase 2: Commands
- [ ] Command es clase anidada en `partial class`
- [ ] Command tiene `[Injectable(ServiceLifetime.Singleton)]`
- [ ] Command hereda de `AbstractCreateCommand<,>` o `AbstractModifyCommand<>`
- [ ] Command implementa `Handle()` con lógica de negocio
- [ ] Command inyecta validators (no usa `new`)
- [ ] Command usa Guards apropiados (404/409/422)
- [ ] Command retorna entidad validada
- [ ] Handler usa API fluent completa
- [ ] Tests de Command cubren éxito y fallo
- [ ] Archivo nombrado `[Entity]_[Action].cs`

---

## 🎯 Beneficio Final

**Un comando = Un archivo = Un desarrollador = Cero conflictos = Encapsulación real**

---

*¿Dudas? Pregunta antes de implementar.*