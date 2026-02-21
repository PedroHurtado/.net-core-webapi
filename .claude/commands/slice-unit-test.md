Lee la especificación en src/$1/domain-specs/$2.md y la siguiente guía de estilo:

- .plans/templates/styles/slice/style-slice-test-unit.md

A partir de la especificación, genera los tests unitarios de las slices. Aplica la guía de estilo.

## Rutas de archivos

La ruta base es `src/$1.UnitTests/Features/$1/Api/`. Dentro de ella:

- **Tests de slices command** → `{Aggregate}Aggregate/Commands/{Action}{Aggregate}Tests.cs`
- **Tests de slices query** → `{Aggregate}Aggregate/Queries/{Action}{Aggregate}Tests.cs`

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