Lee la especificación en src/$1/domain-specs/$2.md y las siguientes guías de estilo:

- .plans/templates/styles/aggregate/style-testable-agregate.md
- .plans/templates/styles/enum/style-test-enum.md
- .plans/templates/styles/valueobject/style-test-valueobject.md
- .plans/templates/styles/valueobject/style-test-validator.md
- .plans/templates/styles/aggregate/style-test-aggregate.md

A partir de la especificación, genera los testables y los tests del dominio. Aplica cada guía de estilo según el tipo de artefacto.

## Rutas de archivos

### Testables (Helpers)
La ruta base es `src/$1.UnitTests/Helpers/`. Un archivo por cada value object, entity y aggregate.

### Tests
La ruta base es `src/$1.UnitTests/Features/$1/Domain/`. Dentro de ella:

- **Tests de enums** → `{Aggregate}AggregateTests/EnumsTests/{Enum}Tests.cs`
- **Tests de value objects** → `{Aggregate}AggregateTests/ValueObjectsTests/{VO}Tests.cs`
- **Tests de validators de VO** → `{Aggregate}AggregateTests/ValueObjectsTests/{VO}ValidatorTests.cs`
- **Tests de aggregate** → `{Aggregate}AggregateTests/{Aggregate}Tests.cs`
- **Tests de validator de aggregate** → `{Aggregate}AggregateTests/{Aggregate}ValidatorTests.cs`
- **Tests de entity** → `{Aggregate}AggregateTests/{Entity}Tests.cs`
- **Tests de validator de entity** → `{Aggregate}AggregateTests/{Entity}ValidatorTests.cs`
- **Tests de artefactos Shared** → `SharedTests/{tipo}Tests/{Artefacto}Tests.cs`

**IMPORTANTE**: Cada artefacto que tenga validator genera DOS archivos de test separados: uno para el DTO/record/class (`{Artefacto}Tests.cs`) y otro para el validator (`{Artefacto}ValidatorTests.cs`). Nunca mezclarlos en el mismo archivo.

## Alcance

Genera SOLO testables y tests para los que se proporciona guía de estilo arriba. Si la especificación define otros tipos de tests, **ignóralos sin preguntar**.

## Reglas

1. **NO leer archivos del proyecto** que no sean la especificación y las guías de estilo
2. **NO explorar** la estructura del proyecto
3. **NO lanzar subagentes**
4. **NO compilar ni ejecutar tests**
5. **NO hacer preguntas con popup (AskUserQuestion)**
6. **NO asumir ni inventar** soluciones a problemas detectados

## Generación

Genera SOLO los archivos — sin explicaciones, sin resúmenes, sin preguntas.