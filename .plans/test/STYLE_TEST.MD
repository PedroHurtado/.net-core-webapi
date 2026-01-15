# Estilo de Tests para Domain

## Enums

- Verificar el conteo total de valores (detectar cambios accidentales)
- `[Theory]` + `[InlineData]` para verificar nombres string esperados (persistencia como string)

## ValueObjects (Records)

- **Creación**: Tests para cada tipo/variante con sus parámetros requeridos
- **Validación**: `[Theory]` por cada regla, separando casos inválidos y válidos
- **Igualdad**: Verificar comportamiento de record (`Equals`, `==`, `GetHashCode`)
- **Comportamiento**: `[Theory]` para cada método de dominio cubriendo distintos escenarios

## Validators (FluentValidation)

- Helper `CreateUnvalidated()` con reflection para saltarse validación del factory
- `[Theory]` por cada propiedad/regla
- Verificar mensaje de error con `WithErrorMessage()`
- Separar tests inválidos y válidos
- Tests finales de entidades completas válidas

## Patrón General

- `[Theory]` + `[InlineData]` siempre que haya múltiples casos
- Incluir boundary values (0, -1, límites exactos)
- Agrupar por `#region` según aspecto testeado
- Naming: `Metodo_Condicion_ResultadoEsperado`