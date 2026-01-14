# MicroDomain - Cheat Sheet 📋

## Fase 1: Modelo (Juntos - TDD)
```
① Enum (sin tests)
② ValueObject    → Test factory + Test métodos → Implementar
③ Entity hija    → Test validator + Test calculadas → Implementar  
④ Aggregate      → Test validator + Test calculadas → Implementar
```

| Componente | Clave | Tests |
|------------|-------|-------|
| Entity | `partial` + 2 ctors + `protected set` | Validator + Calculadas |
| Aggregate | Hereda `AggregateRoot` | Validator + Calculadas |
| ValueObject | `record` + `private` ctor + `Create()` | Factory + Métodos |
| Id / FK | `{ get; init; }` (inmutable) | — |

**⏸️ PR + Code Review antes de Fase 2**

---

## 🧮 Propiedades Calculadas (TDD)

```csharp
// Test PRIMERO
[Fact]
public void Total_ShouldIncludeTax()
{
    var invoice = new TestableInvoice()
        .WithTaxRate(0.21m)
        .WithLine(quantity: 1, unitPrice: 100m);
    
    invoice.Total.Should().Be(121m);
}

// Implementación DESPUÉS
public decimal Subtotal => Lines.Sum(l => l.LineTotal);
public decimal TaxAmount => Subtotal * TaxRate;
public decimal Total => Subtotal + TaxAmount;
```

---

## Fase 2: Commands (Paralelo - TDD)

```
① Test (red) → ② Command (green) → ③ Refactor → ④ PR
```

**Nunca avanzar sin test pasando.**

```csharp
// Domain/MenuAggregate/Commands/Menu_Update.cs
public partial class Menu
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(IValidator<Menu> validator) 
        : AbstractModifyCommand<UpdateMenuCommand, Menu>
    {
        public override Menu Handle(Menu entity, UpdateMenuCommand cmd)
        {
            entity.Name = cmd.Name;
            return validator.ValidateOrThrow(entity);
        }
    }
}
```

| Base Class | Signature |
|------------|-----------|
| `AbstractCreateCommand<TCmd, T>` | `Handle(TCmd cmd) → T` |
| `AbstractModifyCommand<TCmd, T>` | `Handle(T entity, TCmd cmd) → T` |
| `AbstractModifyCommand<T>` | `Handle(T entity) → T` |

---

## Fase 3: Api

```csharp
// Api/Commands/CreateMenu.cs
public static class CreateMenu
{
    public record Request(string Name, string? Description);
    public record Response(Guid Id, string Name);
    
    public class Handler(Menu.Create menuCreate, IRepository<Menu> repo, IUnitOfWork uow)
    {
        public async Task<Response> Handle(Request request) { ... }
    }
    
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/menus", async (Request req, Handler h) => ...);
    }
}
```

| Carpeta | Contiene |
|---------|----------|
| `Api/Commands/` | POST, PUT, DELETE |
| `Api/Queries/` | GET |
| `Contracts/` | Response compartido (3+ usos) |

---

## Guards

| HTTP | Guard | Uso |
|------|-------|-----|
| 404 | `NotFoundGuard.ThrowIfNull(e, id)` | No existe |
| 409 | `ConflictGuard.ThrowIf(cond, msg)` | Duplicado |
| 422 | `ValidationGuard.ThrowIf(cond, msg, prop)` | Regla negocio |
| 422 | `validator.ValidateOrThrow(entity)` | Estructural |

---

## ❌ No hacer

- Lógica en Entity → **Command**
- `new Validator()` en Command → **Inyectar**
- `Result<T>` → **Exceptions + Guards**
- `List<T>` → **`HashSet<T>`**
- `{ get; set; }` → **`{ get; protected set; }`**
- Command fuera de Entity → **Nested class**

---

## ✅ Checklist rápido (TDD)

```
□ Test éxito escrito (red)
□ Test fallo escrito (red)
□ partial class
□ [Injectable(ServiceLifetime.Singleton)]
□ Hereda AbstractCommand correcto
□ Inyecta validator
□ Retorna validator.ValidateOrThrow(entity)
□ Tests pasan (green)
□ Archivo en: Domain/[Aggregate]/Commands/[Entity]_[Action].cs
```

---

## 📁 Estructura

```
Features/[Feature]/
├── Domain/[Aggregate]/
│   ├── [Aggregate].cs
│   ├── Commands/
│   │   ├── [Aggregate]/
│   │   │   └── [Aggregate]_[Action].cs
│   │   └── [Entity]/
│   │       └── [Entity]_[Action].cs
│   ├── Entities/
│   ├── ValueObjects/
│   └── Enums/
├── Api/
│   ├── Commands/
│   └── Queries/
└── Contracts/  ← Solo si se comparte
```
