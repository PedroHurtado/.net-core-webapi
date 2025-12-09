# Comandos MicroDomain

## Namespaces requeridos

| Namespace | Proporciona |
|-----------|-------------|
| `Fudie.Domain` | `Entity`, `AggregateRoot`, `ICreateCommand<,>`, `IModifyCommand<,>` |
| `Fudie.DependencyInjection` | `[Injectable]` para registro automático en DI |
| `Fudie.Validation` | `ValidationGuard`, `ValidateOrThrow()` extension |
| `FluentValidation` | `IValidator<T>` para inyectar validators |

## Interfaces de comandos

| Interface | Firma | Cuándo usar |
|-----------|-------|-------------|
| `ICreateCommand<TCommand, TEntity>` | `TEntity Execute(TCommand command)` | Crear un nuevo agregado desde cero |
| `IModifyCommand<TCommand, TEntity>` | `TEntity Execute(TEntity entity, TCommand command)` | Modificar un agregado existente |
| `IModifyCommand<TEntity>`           | `TEntity Execute(TEntity entity)` | Modificar un agregado existente y no necesita comando|

## Herramientas de validación

| Herramienta | Cuándo usar |
|-------------|-------------|
| `IValidator<T>.ValidateOrThrow(entity)` | Validación estructural de una entidad o value object (formato, rangos, requeridos) |
| `ValidationGuard.ThrowIf(condition, message, property)` | Invariantes de negocio del agregado (duplicados, límites, estados inválidos) |

## Flujo del comando

1. **Crear** entidad/value object con datos del command
2. **Validar estructuralmente** la entidad creada con `validator.ValidateOrThrow()`
3. **Validar invariantes** del agregado con `ValidationGuard.ThrowIf()`
4. **Modificar** estado del agregado
5. **Retornar** agregado validado con `aggregateValidator.ValidateOrThrow()`

## Estructura

- Command: `record` con parámetros de entrada
- Clase: `[Injectable]`, inyecta `IValidator<T>` necesarios, implementa `ICreateCommand<,>` o `IModifyCommand<,>`o IModifyCommand<>
- Método: `TEntity Execute(...)` según la interface

## Reglas

- NO usar `Result<T>`, `try-catch`, ni instanciar validators con `new`
- Siempre inyectar validators
- Siempre retornar el agregado validado