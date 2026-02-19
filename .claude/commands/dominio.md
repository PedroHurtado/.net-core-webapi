Lee la especificación en src/$1/domain_specs/$2.md y las siguientes guías de estilo:

- .plans/templates/styles/enum/style-enum.md
- .plans/templates/styles/valueobject/style-valueobject.md
- .plans/templates/styles/entity/style-entity.md
- .plans/templates/styles/aggregate/style-aggregate.md

A partir de la especificación, genera TODOS los archivos del dominio siguiendo el orden de dependencias de la especificación. Aplica cada guía de estilo según el tipo de artefacto.

## Reglas

1. **NO leer archivos del proyecto** que no sean la especificación y las guías de estilo
2. **NO explorar** la estructura del proyecto
3. **NO lanzar subagentes**
4. **NO compilar ni ejecutar tests**
5. **Genera SOLO los archivos** — sin explicaciones, sin resúmenes, sin preguntas
6. **SI ALGO NO ESTÁ CLARO EN LA ESPECIFICACIÓN, PREGUNTA. NO ASUMAS NI INVENTES.**

