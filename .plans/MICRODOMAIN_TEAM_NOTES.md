# Patrón MicroDomain - Notas para el Equipo

**Audiencia**: Desarrolladores Junior

---

## 🎯 Objetivo

Máximo paralelismo en desarrollo. Cada desarrollador trabaja en un **comando completo** sin conflictos de merge.

---

## 📁 Estructura

```
Domain/[Aggregate]/
├── [Aggregate].cs              ← Aggregate Root
├── Entities/
│   └── [Entity].cs             ← Entidades hijas
├── ValueObjects/
│   └── [ValueObject].cs        ← Objetos de valor
├── Enums/
│   └── [Enum].cs               ← Enumeraciones
└── Commands/
    └── [Aggregate]_[Action].cs ← Lógica de negocio
```

**Naming de archivos**: `Foo_Create.cs`, `FooBar_Add.cs` (ordenación alfabética por agregado)

---

## 📋 Reglas por Componente

### Entity (incluye Aggregate Root)

```csharp
public class Foo : Entity
{
    protected Foo() { }                    // ← EF Core
    public Foo(Guid id) : base(id) { }     // ← Creación
    
    public string Name { get; set; }       // ← Público, mutable
    public HashSet<Bar> Bars { get; set; } = [];
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
- ✅ Dos constructores (protected + public con Guid)
- ✅ Propiedades `{ get; set; }` públicas
- ✅ Colecciones: `HashSet<T>` con inicializador `= []`
- ✅ Validator en mismo archivo, clase separada
- ❌ NO lógica de negocio en la entidad

---

### ValueObject

```csharp
public record Baz
{
    public BazType Type { get; init; }
    public decimal Amount { get; init; }
    
    private Baz() { }
    
    public static Result<Baz> Create(BazType type, decimal amount)
    {
        var instance = new Baz { Type = type, Amount = amount };
        // Validación...
        return Result<Baz>.Success(instance);
    }
}
```

**Reglas**:
- ✅ `record` con `{ get; init; }`
- ✅ Constructor `private`
- ✅ Factory `Create()` retorna `Result<T>`
- ✅ Validación dentro del factory
- ❌ NO constructor público

---

### Command

```csharp
public record UpdateFooCommand(
    string Name,
    string? Description,
    int DisplayOrder
);

[Injectable]
public class UpdateFoo : IModifyCommand<UpdateFooCommand, Foo>
{
    public Result<Foo> Execute(Foo entity, UpdateFooCommand command)
    {
        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        return Entity.ValidateEntity(entity, new FooValidator());
    }
}
```

**Reglas**:
- ✅ Record para datos de entrada
- ✅ `[Injectable]` en la clase
- ✅ Implementa `ICreateCommand<TCmd, TEntity>` o `IModifyCommand<TCmd, TEntity>`
- ✅ Retorna `Result<TEntity>`
- ✅ Valida al final con `Entity.ValidateEntity()`
- ❌ NO dependencias externas (stateless)

---

## 🔌 Interfaces

| Interfaz | Uso | Firma |
|----------|-----|-------|
| `ICreateCommand<TCmd, TEntity>` | Crear entidad nueva | `Execute(TCmd command)` |
| `IModifyCommand<TCmd, TEntity>` | Modificar existente | `Execute(TEntity entity, TCmd command)` |

---

## 🔄 Flujo en Handler

```csharp
public class UpdateFooHandler(UpdateFoo updateFoo, IUnitOfWork uow)
{
    public async Task<Result<Response>> Handle(Guid id, UpdateFooCommand cmd)
    {
        var entity = await uow.GetAsync<Foo>(id);    // 1. Obtener
        var result = updateFoo.Execute(entity, cmd); // 2. Ejecutar comando
        if (result.IsFailure) return result.Errors;  // 3. Validar
        await uow.SaveChangesAsync();                // 4. Persistir
        return MapToResponse(entity);                // 5. Responder
    }
}
```

---

## ⚡ Orden de Desarrollo

### Fase 1: Modelo de Dominio (TODO EL EQUIPO)

```
1. Enums           (0 dependencias)
2. ValueObjects    (solo enums)
3. Entity hoja     (sin hijos)
4. Entity padre    (con colecciones)
5. Aggregate Root  
```

**Objetivo**: 
- Validar Domain Specification contra código real
- Todo el equipo conoce el dominio antes de desarrollar
- Detectar errores de diseño temprano

**Entregable**: PR con modelo completo + tests → Code Review conjunto

---

### Fase 2: Commands (EN PARALELO)

```
6. Commands        (cada dev toma uno o más)
```

**Requisito**: Fase 1 completada y aprobada

---

**Cada paso**: Código + Test → Validar → Siguiente

---

## ❌ Errores Comunes

| Error | Corrección |
|-------|------------|
| Lógica en Entity | Mover a Command |
| Constructor público en ValueObject | Usar factory `Create()` |
| Inyectar interfaz genérica | Inyectar clase concreta |
| Olvidar `Entity.ValidateEntity()` al final | Siempre validar antes de retornar |
| `List<T>` en colecciones | Usar `HashSet<T>` |

---

## 📝 Checklist antes de PR

- [ ] Entity tiene dos constructores
- [ ] Validator en mismo archivo que Entity
- [ ] ValueObject usa factory `Create()`
- [ ] Command tiene `[Injectable]`
- [ ] Command valida al final
- [ ] Tests cubren casos éxito y fallo
- [ ] Archivo nombrado `[Aggregate]_[Action].cs`

---

## 🎯 Beneficio Final

**Un comando = Un archivo = Un desarrollador = Cero conflictos**

---

*¿Dudas? Pregunta antes de implementar.*
