# Domain Tests Style Guide

## Estructura

- Respetar la estructura de carpetas del dominio en los tests
- Helpers y métodos auxiliares van en el proyecto de tests
- Usar `GlobalUsings.cs` del dominio, no incluir `using` en archivos de test

## Estilo

- Sin documentación XML en tests (el test ES la documentación)
- Archivos pequeños y enfocados
- Solo tests que aporten valor real, no tests triviales

## Enums

- Verificar nombres string (se persisten como string)

## ValueObjects

- **Creación**: Casos representativos por tipo/variante
- **Validación**: `[Theory]` por regla con boundary values
- **Comportamiento**: Tests para métodos de dominio con escenarios reales

## Validators

- Helper `CreateUnvalidated()` con reflection en proyecto de tests
- `[Theory]` por regla, verificar `WithErrorMessage()`
- Separar casos inválidos y válidos

## Convenciones

- `[Theory]` + `[InlineData]` para múltiples casos
- Naming: `Metodo_Condicion_ResultadoEsperado`
- Agrupar con `#region` si es necesario