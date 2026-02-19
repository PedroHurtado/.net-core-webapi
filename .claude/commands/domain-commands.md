Lee la especificación en src/$1/domain-specs/$2.md y las siguientes guías de estilo:

- .plans/templates/styles/valueobject/style-command-create.md
- .plans/templates/styles/valueobject/style-command-transform.md
- .plans/templates/styles/aggregate/style-command-aggregate.md

A partir de la especificación, genera los comandos del dominio. Aplica cada guía de estilo según el tipo de artefacto.

## Rutas de archivos

La ruta base es `src/$1/Features/$1/Domain/`. Dentro de ella:

- **Comandos de VO** → `{Aggregate}Aggregate/Commands/{VO}/{VO}_{Action}.cs`
- **Comandos de entity** → `{Aggregate}Aggregate/Commands/{Entity}/{Entity}_{Action}.cs`
- **Comandos de aggregate** → `{Aggregate}Aggregate/Commands/{Aggregate}/{Aggregate}_{Action}.cs`
- **Comandos de Shared** → `Shared/Commands/{VO}/{VO}_{Action}.cs`

## Principio fundamental del dominio

Los comandos del dominio son **lógica pura**. El dominio recibe datos a través de command records, muta o crea estado mediante comandos, valida con validators inyectados y protege invariantes con Guards.

**El dominio NUNCA accede a infraestructura.** Si la especificación menciona consultas, búsquedas en base de datos, o cualquier acceso a repositorios, eso **NO va en los comandos del dominio** — va en los slices. Los comandos del dominio solo trabajan con los datos que reciben por parámetro.

## Alcance

Genera SOLO comandos del dominio. Si la especificación define queries, responses, slices u otros artefactos, **ignóralos sin preguntar**.

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