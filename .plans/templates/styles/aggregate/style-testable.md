# Estilo: Testable (Helpers)

## Propósito

Los `Testable` exponen constructores protegidos y setters para poder crear instancias en tests sin pasar por los comandos de dominio.

**Regla de oro**: Los `Testable` solo se usan para tests de la propia entidad/VO, **nunca para tests de comandos**.

---

## Testable para Value Object

```csharp
namespace {Project}.UnitTests.Helpers;

public record Testable{ValueObject} : {ValueObject}
{
    public Testable{ValueObject}({Type} {param1}, {Type} {param2}, {Type} {param3} = {default})
        : base({param1}, {param2}, {param3}) { }
}
```

---

## Testable para Aggregate/Entity

```csharp
namespace {Project}.UnitTests.Helpers;

public class Testable{Aggregate} : {Aggregate}
{
    public Testable{Aggregate}(Guid id) : base(id) { }

    public void Set{Property}({Type} {property}) => {Property} = {property};
    
    public new void Add{Item}({ItemType} {item}) => _{collection}.Add({item});
}
```

---

## Reglas

- Namespace: `{Project}.UnitTests.Helpers`
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- Value Objects: `record` que hereda del VO
- Aggregates/Entities: `class` que hereda del agregado/entidad
- Exponer `Set{Property}` para cada propiedad `protected set`
- Exponer `Add{Item}` con `new` para colecciones (accede al backing field)
