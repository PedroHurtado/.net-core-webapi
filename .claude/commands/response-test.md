Lee la especificación en src/$1/domain-specs/$2.md y la siguiente guía de estilo:

- .plans/templates/styles/response/style-test-response.md

A partir de la especificación, genera los tests de los responses. Aplica la guía de estilo.

## Rutas de archivos

La ruta base es `src/$1.UnitTests/Features/$1/Api/`. Dentro de ella:

- **Tests de response** → `{Aggregate}Aggregate/{Aggregate}ResponseTests.cs`

Un solo archivo de test por agregado, con regiones por cada record.

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