# Estilo: Testable (Helpers)

## Propósito

Los `Testable` exponen constructores protegidos y setters para poder crear instancias en tests sin pasar por los comandos de dominio.

**Regla de oro**: Los `Testable` solo se usan para preparar estado inicial en tests, **nunca para sustituir lógica de dominio**.

---

## Testable para Record (Value Object)

Los records son inmutables, el Testable simplemente expone un constructor público.
```csharp
namespace {Project}.UnitTests.Helpers;

public record Testable{Record}(
    {Type} {Param1},
    {Type} {Param2},
    {Type} {Param3} = {default}
) : {Record}({Param1}, {Param2}, {Param3});
```

---

## Testable para Aggregate/Entity

Métodos `With` fluent para propiedades y colecciones protegidas.
```csharp
namespace {Project}.UnitTests.Helpers;

public class Testable{Aggregate} : {Aggregate}
{
    public Testable{Aggregate}(Guid id) : base(id) { }

    public Testable{Aggregate} With{Property}({Type} value) { {Property} = value; return this; }

    public Testable{Aggregate} With{Item}({ItemType} item) { _{collection}.Add(item); return this; }
}
```

---

## Reglas

- Namespace: `{Project}.UnitTests.Helpers`
- **No `using`** → Van en `GlobalUsings.cs`
- **No XML docs**
- **No `new`** → Nunca ocultar métodos de dominio
- Records: `record` que hereda del record base, constructor con parámetros directos
- Aggregates/Entities: `class` con métodos `With` fluent que devuelven `this`
- Prefijo `With` para todo: propiedades (`With{Property}`) y colecciones (`With{Item}`)
- Acceso directo a backing fields heredados, sin `new`, sin ocultar lógica de dominio