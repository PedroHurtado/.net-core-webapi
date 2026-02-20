Lee la especificación en src/$1/domain-specs/$2.md y las siguientes guías de estilo:

- .plans/templates/styles/enum/style-enum.md
- .plans/templates/styles/valueobject/style-valueobject.md
- .plans/templates/styles/entity/style-entity.md
- .plans/templates/styles/aggregate/style-aggregate.md

A partir de la especificación, genera los archivos del dominio siguiendo el orden de dependencias de la especificación. Aplica cada guía de estilo según el tipo de artefacto.

## Rutas de archivos

La ruta base es `src/$1/Features/$1/Domain/`. Dentro de ella:

- **Artefactos del agregado** → `{Aggregate}Aggregate/{tipo}/` (ej: `MenuAggregate/Enums/`, `MenuAggregate/ValueObjects/`, `MenuAggregate/Entities/`)
- **El aggregate root** → `{Aggregate}Aggregate/{Aggregate}.cs` (ej: `MenuAggregate/Menu.cs`)
- **Artefactos marcados como (Shared)** → `Shared/{tipo}/` (ej: `Shared/Enums/`, `Shared/ValueObjects/`)

Los Shared se crean PRIMERO porque son dependencias de varios agregados. Si este skill ya se ejecutó para otro agregado del mismo módulo, los Shared ya existen — **no los recrees y no leas la carpeta Shared para verificar; confía en que están creados**.

## Alcance

Genera SOLO los artefactos para los que se proporciona guía de estilo arriba (enums, value objects, entities, aggregates). Si la especificación define otros tipos de artefactos (commands, responses, queries, slices, etc.) que no tienen guía de estilo, **ignóralos sin preguntar**.

## Reglas

1. **NO leer archivos del proyecto** que no sean la especificación y las guías de estilo
2. **NO explorar** la estructura del proyecto
3. **NO lanzar subagentes**
4. **NO compilar ni ejecutar tests**
5. **NO hacer preguntas con popup (AskUserQuestion)**
6. **NO asumir ni inventar** soluciones a problemas detectados

## Fase 1: Validación de la especificación

Antes de generar cualquier archivo, analiza TODA la especificación buscando:

- **Contradicciones**: invariantes que se contradicen con los tests unitarios definidos (ej: invariante dice `Amount > 0` incondicional pero un test exitoso usa `Amount=0`)
- **Ambigüedades**: tipos o comportamientos que pueden interpretarse de más de una forma
- **Incompatibilidades con guías de estilo**: algo en la spec que no encaja con las reglas de la guía

Si encuentras problemas:
1. **NO generes ningún archivo**
2. Escribe un resumen con TODOS los problemas encontrados, con este formato por cada uno:

```
### ❌ [Artefacto] — Descripción corta

**Ubicación**: Sección X de la spec
**Problema**: Explicación concreta de la contradicción/ambigüedad
**Detalle**:
- La invariante dice: "..."
- Pero el test dice: "..."
**Sugerencia**: Posible resolución (sin aplicarla)
```

3. **Párarte** — El usuario corregirá la spec y relanzará el skill

Si NO hay problemas, pasa directamente a la Fase 2.

## Fase 2: Generación de archivos

Genera SOLO los archivos — sin explicaciones, sin resúmenes, sin preguntas.