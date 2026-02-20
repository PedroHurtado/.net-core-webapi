Lee la especificación en src/$1/domain-specs/$2.md y las siguientes guías de estilo:

- .plans/templates/styles/valueobject/style-test-command.md
- .plans/templates/styles/aggregate/style-test-command.md

A partir de la especificación, genera los tests de los comandos del dominio. Aplica cada guía de estilo según el tipo de artefacto.

## Rutas de archivos

La ruta base es `src/$1.UnitTests/Features/$1/Domain/`. Dentro de ella:

- **Tests de comandos VO** → `{Aggregate}AggregateTests/CommandsTests/{VO}_{Action}Tests.cs`
- **Tests de comandos aggregate** → `{Aggregate}AggregateTests/Commands/{Aggregate}Tests/{Aggregate}{Action}Tests.cs`

### DomainFixture
Si no existe, genera `src/$1.UnitTests/Helpers/DomainFixture.cs` según la guía de test de comandos VO.

## Reglas

1. **NO leer archivos del proyecto** que no sean la especificación y las guías de estilo
2. **NO explorar** la estructura del proyecto
3. **NO lanzar subagentes**
4. **NO compilar ni ejecutar tests**
5. **NO hacer preguntas con popup (AskUserQuestion)**
6. **NO asumir ni inventar** soluciones a problemas detectados
7. **SI ALGO NO ESTÁ CLARO EN LA ESPECIFICACIÓN, PREGUNTA. NO ASUMAS NI INVENTES.**

## Generación

Genera SOLO los archivos — sin explicaciones, sin resúmenes, sin preguntas.