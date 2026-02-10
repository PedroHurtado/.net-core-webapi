# Fudie – Catálogo de Permisos: Propuesta de Estructura

**Fecha:** Febrero 2026  
**Estado:** Borrador para revisión

---

## 1. Catálogo que expone cada microservicio en `GET /catalog`

```json
{
  "serviceId": "menu-service",
  "scopes": [
    {
      "scope": "menu-service:CreateMenu",
      "httpVerb": "POST",
      "domain": "menu"
    },
    {
      "scope": "menu-service:UpdateMenu",
      "httpVerb": "PUT",
      "domain": "menu"
    },
    {
      "scope": "menu-service:GetMenu",
      "httpVerb": "GET",
      "domain": "menu"
    },
    {
      "scope": "menu-service:ActivateMenu",
      "httpVerb": "POST",
      "domain": "menu"
    },
    {
      "scope": "menu-service:AddMenuCategory",
      "httpVerb": "POST",
      "domain": "menu"
    },
    {
      "scope": "menu-service:SetMenuDepositPolicy",
      "httpVerb": "PUT",
      "domain": "menu",
      "customGroup": "menu:deposit"
    },
    {
      "scope": "menu-service:RemoveMenuDepositPolicy",
      "httpVerb": "DELETE",
      "domain": "menu",
      "customGroup": "menu:deposit"
    }
  ],
  "autoGroups": {
    "menu:read": [
      "menu-service:GetMenu"
    ],
    "menu:write": [
      "menu-service:CreateMenu",
      "menu-service:UpdateMenu",
      "menu-service:ActivateMenu",
      "menu-service:AddMenuCategory",
      "menu-service:SetMenuDepositPolicy",
      "menu-service:RemoveMenuDepositPolicy"
    ]
  },
  "customGroups": {
    "menu:deposit": [
      "menu-service:SetMenuDepositPolicy",
      "menu-service:RemoveMenuDepositPolicy"
    ]
  },
  "descriptions": {
    "scopes": {
      "menu-service:CreateMenu": {
        "es": "Crear un nuevo menú",
        "en": "Create a new menu"
      },
      "menu-service:UpdateMenu": {
        "es": "Modificar datos de un menú",
        "en": "Update menu details"
      },
      "menu-service:GetMenu": {
        "es": "Ver un menú",
        "en": "View a menu"
      },
      "menu-service:ActivateMenu": {
        "es": "Activar un menú para que sea visible al público",
        "en": "Activate a menu to make it publicly visible"
      },
      "menu-service:AddMenuCategory": {
        "es": "Añadir una categoría a un menú",
        "en": "Add a category to a menu"
      },
      "menu-service:SetMenuDepositPolicy": {
        "es": "Configurar la política de fianza del menú",
        "en": "Set the menu deposit policy"
      },
      "menu-service:RemoveMenuDepositPolicy": {
        "es": "Eliminar la política de fianza del menú",
        "en": "Remove the menu deposit policy"
      }
    },
    "groups": {
      "menu:read": {
        "es": "Ver menús",
        "en": "View menus"
      },
      "menu:write": {
        "es": "Gestionar menús",
        "en": "Manage menus"
      },
      "menu:deposit": {
        "es": "Políticas de fianza de menús",
        "en": "Menu deposit policies"
      }
    }
  }
}
```

---

## 2. Estructura del rol en Firestore del Auth

```json
{
  "name": "Encargado",
  "restaurantId": "xxx",
  "groups": ["menu:read", "menu:write"],
  "additionalScopes": ["reservation-service:CancelReservation"],
  "excludedScopes": ["menu-service:SetMenuDepositPolicy"]
}
```

| Campo | Descripción |
|-------|-------------|
| `groups` | Agrupaciones activadas por el Owner |
| `additionalScopes` | Scopes individuales añadidos fuera de agrupaciones |
| `excludedScopes` | Scopes individuales quitados de una agrupación activada |

---

## 3. JWT

```json
{
  "sub": "userId",
  "tid": "restaurantId",
  "groups": ["menu:read", "menu:write"],
  "add": ["reservation-service:CancelReservation"],
  "exc": ["menu-service:SetMenuDepositPolicy"]
}
```

---

## 4. Validación en el microservicio

```csharp
var scope = $"{serviceId}:{sliceClassName}";
var group = autoGroupForThisScope; // "menu:write"

bool inGroup = jwt.Groups.Contains(group);
bool excluded = jwt.Excluded.Contains(scope);
bool additional = jwt.Additional.Contains(scope);

bool hasPermission = (inGroup && !excluded) || additional;
```

---

## 5. Decisiones tomadas

| Decisión | Detalle |
|----------|---------|
| Scope | `{ServiceId}:{ClassName}` — ServiceId del appsettings.json + nombre de la clase |
| Transporte en JWT | Array de strings — descartado bitfield |
| Owner | Flag `owner: true` — no lleva array de permisos |
| Catálogo | Cada microservicio expone `GET /catalog` — público, inmutable entre deploys |
| Auth y catálogo | El Auth no conoce el catálogo — solo copia los permisos del rol al JWT |
| Permisos nuevos | Las agrupaciones resuelven que un permiso nuevo se incluya automáticamente |
| Roles | Almacenan agrupaciones + excepciones (additional/excluded) |
