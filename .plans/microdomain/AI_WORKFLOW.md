# Flujo de Trabajo con IA - MicroDomain

Guía para desarrolladores sobre cómo trabajar eficientemente con Claude en el patrón MicroDomain.

---

## Regla Principal

```
1 Sesión de Claude = 1 Agregado
```

**Nunca mezclar agregados en la misma conversación.**

---

## Inicio de Sesión

Copiar y pegar al iniciar:

```
Agregado: [NombreAgregado]
Spec: .plans/domains/[Nombre].md
Patrón: .plans/microdomain/MICRODOMAIN.md

Tarea: [descripción corta]
Referencia: [archivo similar existente]
```

**Ejemplo real:**

```
Agregado: Menu
Spec: .plans/domains/Menu.md
Patrón: .plans/microdomain/MICRODOMAIN.md

Tarea: Crear comando RemoveItem
Referencia: Menu_RemoveCategory.cs
```

---

## Comandos Cortos

| Intención | Prompt |
|-----------|--------|
| Crear entidad | `"Crea MenuItem según la spec"` |
| Crear comando | `"Comando AddItem similar a Menu_AddCategory.cs"` |
| Crear validador | `"Validador para MenuItem según spec"` |
| Crear ValueObject | `"ValueObject PriceOption según spec"` |
| Revisar código | `"Revisa este comando: [pegar código]"` |
| Corregir error | `"Error: [mensaje]. Archivo: [path]"` |

---

## Regla del Archivo Ancla

**Siempre referenciar un archivo existente como ejemplo:**

```
✅ "Crea RemoveItem similar a Menu_RemoveCategory.cs"
❌ "Crea un comando para remover items del menú"
```

Esto reduce explicaciones y mantiene consistencia.

---

## Qué NO Hacer

| Evitar | Hacer en su lugar |
|--------|-------------------|
| Pegar código largo para "contexto" | Dar path del archivo |
| Explicar el patrón MicroDomain | Ya está en MICRODOMAIN.md |
| Pedir múltiples features a la vez | Una por mensaje |
| Conversaciones largas de diseño | Usar spec como fuente de verdad |
| Mezclar agregados | Nueva sesión por agregado |

---

## Trabajo en Paralelo

### Niveles de Dependencia

```
┌─────────────────────────────────────────────────────────┐
│ NIVEL 0 - Sin dependencias (paralelo total)             │
├─────────────────────────────────────────────────────────┤
│ • Enums                                                 │
│ • ValueObjects (PriceOption, NutritionalInfo, etc.)     │
│ • Entity base (Menu.cs, MenuCategory.cs, MenuItem.cs)   │
│ • Validators                                            │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ NIVEL 1 - Depende de Nivel 0                            │
├─────────────────────────────────────────────────────────┤
│ • Commands Create (MenuCategory_Create, MenuItem_Create)│
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ NIVEL 2 - Depende de Nivel 1                            │
├─────────────────────────────────────────────────────────┤
│ • Commands en AggregateRoot (Menu_AddCategory, etc.)    │
│   (usan los Create commands como dependencia)           │
└─────────────────────────────────────────────────────────┘
```

### Ejemplo: 4 Devs en Paralelo

| Dev | Nivel 0 | Nivel 1-2 |
|-----|---------|-----------|
| A | MenuCategory + Validator | MenuCategory_Create, Menu_AddCategory, Menu_UpdateCategory, Menu_RemoveCategory |
| B | MenuItem + Validator | MenuItem_Create, Menu_AddItem, Menu_UpdateItem, Menu_RemoveItem |
| C | PriceOption, NutritionalInfo + Validators | MenuItem_SetPriceOptions, MenuItem_SetNutrition |
| D | DepositPolicy, ItemDepositOverride + Validators | Menu_SetDepositPolicy, MenuItem_SetDepositOverride |

### Sin Conflictos de Merge

Cada archivo es independiente:

```
Menu_AddCategory.cs    ← Solo Dev A
Menu_AddItem.cs        ← Solo Dev B
PriceOption.cs         ← Solo Dev C
```

---

## Flujo Recomendado

```
┌─────────────────────────────────────────────────────────┐
│ 1. Tech Lead define estado del agregado (Menu.cs)       │
│ 2. Tech Lead crea spec completa (.plans/domains/X.md)   │
│ 3. Devs en paralelo trabajan sus piezas asignadas       │
│ 4. Cada dev: nueva sesión Claude → genera → PR          │
│ 5. PRs independientes, merge sin conflictos             │
└─────────────────────────────────────────────────────────┘
```

---

## División por Rol

| Rol | Usa Claude para |
|-----|-----------------|
| Junior | Generar código siguiendo ejemplos |
| Mid | Revisar código, resolver errores |
| Senior | Diseñar specs, validar arquitectura |

---

## Optimizar Contexto

### Buenas Prácticas

1. **Sesiones cortas**: Genera 2-3 archivos, cierra, abre nueva
2. **Referencias, no código**: `"Lee Menu_AddCategory.cs"` en vez de pegar
3. **Spec como verdad**: Si algo no está en la spec, preguntar al Tech Lead
4. **Un tema por sesión**: No mezclar crear + revisar + refactorizar

### Señales de Cambiar Sesión

- Claude empieza a "olvidar" el patrón
- Respuestas inconsistentes con código anterior
- Más de 20 mensajes en la conversación

---

## Checklist Pre-PR

```
□ Código sigue patrón de archivo ancla
□ ValidationMessages como static class con const
□ [Injectable(ServiceLifetime.Singleton)] en commands
□ Guards correctos (NotFound/Conflict/Validation)
□ XML docs en clases públicas
□ Build compila sin errores
```

---

*¿Dudas sobre el flujo? Pregunta al Tech Lead antes de iniciar.*
