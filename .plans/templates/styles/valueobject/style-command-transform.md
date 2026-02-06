# Estilo: Comandos Transform de Value Object

## Propósito

Los comandos Transform se usan para crear una **nueva instancia** de un Value Object a partir de una existente, aplicando cambios mediante la expresión `with`. Separan claramente la **creación** (Create) de la **transformación** (Transform) del VO.

---

## Clases Base

```csharp
public abstract class AbstractTransformCommand<TCommand, TValueObject>
    where TCommand : class
    where TValueObject : class
{
    public abstract TValueObject Execute(TValueObject current, TCommand command);
}

public abstract class AbstractTransformCommand<TValueObject>
    where TValueObject : class
{
    public abstract TValueObject Execute(TValueObject current);
}
```

- **Con comando**: Cuando la transformación necesita datos externos
- **Sin comando**: Cuando la transformación se basa solo en el estado actual del VO

---

## Transform con Comando

```csharp
namespace {Project}.Features.{Feature}.Domain.{Aggregate}Aggregate.ValueObjects;

public record {Action}{ValueObject}Command(
    {Type} {Param1}
);

public partial record {ValueObject}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class {Action}(
        IValidator<{ValueObject}> {valueObject}Validator
    ) : AbstractTransformCommand<{Action}{ValueObject}Command, {ValueObject}>
    {
        public override {ValueObject} Execute({ValueObject} current, {Action}{ValueObject}Command command)
        {
            var updated = current with { {Property} = command.{Param1} };

            return {valueObject}Validator.ValidateOrThrow(updated);
        }
    }
}
```

### Ejemplo: Cambiar importe de un precio

```csharp
public record ChangeAmountCommand(decimal NewAmount);

public partial record Price
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ChangeAmount(
        IValidator<Price> priceValidator
    ) : AbstractTransformCommand<ChangeAmountCommand, Price>
    {
        public override Price Execute(Price current, ChangeAmountCommand command)
        {
            var updated = current with { Amount = command.NewAmount };

            return priceValidator.ValidateOrThrow(updated);
        }
    }
}
```

---

## Transform sin Comando

```csharp
public partial record {ValueObject}
{
    [Injectable(ServiceLifetime.Singleton)]
    public class {Action}(
        IValidator<{ValueObject}> {valueObject}Validator
    ) : AbstractTransformCommand<{ValueObject}>
    {
        public override {ValueObject} Execute({ValueObject} current)
        {
            var updated = current with { {Property} = {expression} };

            return {valueObject}Validator.ValidateOrThrow(updated);
        }
    }
}
```

### Ejemplo: Aplicar descuento fijo

```csharp
public partial record Price
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ApplyHalfDiscount(
        IValidator<Price> priceValidator
    ) : AbstractTransformCommand<Price>
    {
        public override Price Execute(Price current)
        {
            var updated = current with { Amount = current.Amount * 0.5m };

            return priceValidator.ValidateOrThrow(updated);
        }
    }
}
```

---

## Composición con VOs Anidados

Cuando la transformación afecta a un VO hijo, inyectar su comando correspondiente:

```csharp
public record ChangeCurrencyCommand(string CurrencyCode);

public partial record Money
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ChangeCurrency(
        Currency.Create currencyCreate,
        IValidator<Money> moneyValidator
    ) : AbstractTransformCommand<ChangeCurrencyCommand, Money>
    {
        public override Money Execute(Money current, ChangeCurrencyCommand command)
        {
            var currency = currencyCreate.Execute(new CreateCurrencyCommand(
                command.CurrencyCode));

            var updated = current with { Currency = currency };

            return moneyValidator.ValidateOrThrow(updated);
        }
    }
}
```

---

## Reglas

- Archivo separado: `{ValueObject}_{Action}.cs`
- Record del comando (si lo hay) **fuera** de la clase parcial
- Clase `{Action}` **dentro** de `partial record {ValueObject}`
- Hereda de `AbstractTransformCommand<TCommand, TValueObject>` o `AbstractTransformCommand<TValueObject>`
- Atributo `[Injectable(ServiceLifetime.Singleton)]`
- Inyecta `IValidator<{ValueObject}>`
- Usar expresión `with` para crear la nueva instancia
- El nombre de la clase usa **semántica de dominio** (`ChangeAmount`, `ApplyDiscount`, `Activate`)
- Para VOs anidados: inyectar el comando Create o Transform del hijo

---

## ⛔ PROHIBIDO

- **NO mutar el VO** → Siempre devolver nueva instancia con `with`
- **NO crear métodos `WithX`** directamente en el record
- **NO hacer transformaciones fuera del patrón de comandos**
