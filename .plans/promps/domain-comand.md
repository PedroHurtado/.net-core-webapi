# Comandos MicroDomain

## Namespaces requeridos

| Namespace | Proporciona |
|-----------|-------------|
| `Fudie.Domain` | `Entity`, `AggregateRoot`, `ICreateCommand<,>`, `IModifyCommand<,>`, `IModifyCommand<>`, `ConflictException` |
| `Fudie.DependencyInjection` | `[Injectable]` para registro automático en DI |
| `Fudie.Validation` | `ValidationGuard`, `ConflictGuard`, `NotFoundGuard`, `ValidateOrThrow()` extension |
| `FluentValidation` | `IValidator<T>` para inyectar validators |

## Interfaces de comandos

| Interface | Firma | Cuándo usar |
|-----------|-------|-------------|
| `ICreateCommand<TCommand, TEntity>` | `TEntity Execute(TCommand command)` | Crear un nuevo agregado desde cero |
| `IModifyCommand<TCommand, TEntity>` | `TEntity Execute(TEntity entity, TCommand command)` | Modificar un agregado existente |
| `IModifyCommand<TEntity>` | `TEntity Execute(TEntity entity)` | Modificar un agregado existente sin comando |

## Herramientas de validación

| Herramienta | HTTP | Cuándo usar |
|-------------|------|-------------|
| `IValidator<T>.ValidateOrThrow(entity)` | 422 | Validación estructural (formato, rangos, requeridos) |
| `ValidationGuard.ThrowIf(condition, message, property)` | 422 | Reglas de negocio que invalidan los datos |
| `ConflictGuard.ThrowIf(condition, message)` | 409 | Conflictos con estado actual (duplicados, transiciones inválidas) |
| `NotFoundGuard.ThrowIfNull(entity)` | 404 | Entidad no existe (detecta nombre automáticamente) |
| `NotFoundGuard.ThrowIfNull(entity, id)` | 404 | Entidad no existe (incluye Id en mensaje) |

## Criterio 422 vs 409

| Pregunta | Código |
|----------|--------|
| ¿El dato en sí mismo es inválido? (formato, rango, vacío) | 422 |
| ¿El dato es válido pero choca con algo que ya existe? | 409 |

## Flujo del comando

1. **Buscar** entidades relacionadas con `NotFoundGuard.ThrowIfNull()`
2. **Crear** entidad/value object con datos del command
3. **Validar estructuralmente** con `validator.ValidateOrThrow()`
4. **Validar conflictos** con `ConflictGuard.ThrowIf()`
5. **Modificar** estado del agregado
6. **Retornar** agregado validado con `aggregateValidator.ValidateOrThrow()`

## Estructura

- Command: `record` con parámetros de entrada
- Clase: `[Injectable]`, inyecta `IValidator<T>` necesarios, implementa `ICreateCommand<,>`, `IModifyCommand<,>` o `IModifyCommand<>`
- Método: `TEntity Execute(...)` según la interface

## Reglas

- NO usar `Result<T>`, `try-catch`, ni instanciar validators con `new`
- Siempre inyectar validators
- Siempre retornar el agregado validado