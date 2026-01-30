# Estilo: Comandos de Value Object

## Guards Disponibles

| Guard | HTTP | Uso |
|-------|------|-----|
| `ValidationGuard.ThrowIf(condition, message, propertyName)` | 422 | Datos inválidos |
| `ConflictGuard.ThrowIf(condition, message)` | 409 | Conflictos de negocio |
| `NotFoundGuard.ThrowIfNull(entity, message)` | 404 | Recurso no encontrado |

---

## Comando Create

```csharp
namespace {Project}.Features.{Feature}.Domain.{Aggregate}Aggregate.ValueObjects;

public record Create{ValueObject}Command(
    {Type} {Param1},
    {Type} {Param2},
    {Type} {Param3} = {default}
);

public partial record {ValueObject}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<{ValueObject}> {valueObject}Validator
    ) : AbstractCreateCommand<Create{ValueObject}Command, {ValueObject}>
    {
        public override {ValueObject} Execute(Create{ValueObject}Command command)
        {
            var {valueObject} = new {ValueObject}(
                command.{Param1},
                command.{Param2},
                command.{Param3});

            return {valueObject}Validator.ValidateOrThrow({valueObject});
        }
    }
}
```

---

## Composición de Comandos

Cuando un VO contiene otro VO, **inyectar y usar su comando**:

```csharp
public partial record Money
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        Currency.Create currencyCreate,  // Inyectar comando del VO hijo
        IValidator<Money> moneyValidator
    ) : AbstractCreateCommand<CreateMoneyCommand, Money>
    {
        public override Money Execute(CreateMoneyCommand command)
        {
            // Usar comando, NO new Currency(...)
            var currency = currencyCreate.Execute(new CreateCurrencyCommand(
                command.CurrencyCode));

            var money = new Money(command.Amount, currency);

            return moneyValidator.ValidateOrThrow(money);
        }
    }
}
```

---

## Reglas

- Archivo separado: `{ValueObject}_Create.cs`
- Record del comando **fuera** de la clase parcial
- Clase `Create` **dentro** de `partial record {ValueObject}`
- Hereda de `AbstractCreateCommand<TCommand, TResult>`
- Atributo `[Injectable(ServiceLifetime.Singleton)]`
- Inyecta `IValidator<{ValueObject}>`
- Para VOs anidados: inyectar `{OtroVO}.Create`

---

## ⛔ PROHIBIDO

- **NO usar `new {ValueObject}(...)`** fuera del comando Create
- **NO crear métodos estáticos** (`New`, `From`, `Create`)
- **NO crear factory methods**
- **NO hacer constructor `public`**
