# Prompts: Domain Spec → OpenAPI (2 pasos)

---

## Paso 1: Extraer Endpoints

```
Desde el Domain Spec adjunto, extrae la tabla de endpoints.

## Mapeo

| Domain | Verbo |
|--------|-------|
| 🔵 `(Create[X])` | POST |
| 🔵 `(Update[X])` | PUT (completo) o PATCH (parcial) |
| 🔵 `(Delete[X])` | DELETE |
| 🔵 `(Add[Child])` | POST |
| 🔵 `([Verb][X])` | POST (acción) |
| 📊 `[X]View` | GET |
| 📊 `[X]ListView` | GET |

## Errores (de 🟣 Policies y 🔴 Errors)

- Validación fallida → 422
- No encontrado → 404
- Duplicado/Conflicto → 409
- Sin autorización → 401/403

## Reglas paths

- kebab-case, plural: `/menu-items`
- Nested: `/menus/{menuId}/categories`

## Output

| Origen | Verbo | Path | Query Params | Errores |
|--------|-------|------|--------------|---------|
```

---

## Paso 2: Generar OpenAPI

```
Genera OpenAPI 3.1 (YAML) usando:
- Domain Spec adjunto (schemas, validaciones)
- Tabla de endpoints validada (abajo)

## Tabla validada

[PEGAR TABLA DEL PASO 1]

## Reglas

- Schemas: PascalCase (`CreateMenuRequest`, `MenuResponse`)
- Validaciones del dominio → `required`, `maxLength`, `minimum`
- IDs: `uuid`, Dinero: `decimal`

## Errores: CustomProblemDetails

    CustomProblemDetails:
      type: object
      properties:
        type:
          type: string
          default: "about:blank"
        title:
          type: string
        status:
          type: integer
        detail:
          type: string
        instance:
          type: string
        extensions:
          type: object
          additionalProperties: true

## Output

YAML completo listo para guardar.
```
