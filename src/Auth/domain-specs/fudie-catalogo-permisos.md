# Fudie — Catálogo de Permisos

## Especificación Técnica para Implementación

**Stack:** .NET Core 8 / C# 12 / Firestore  
**Fecha:** Febrero 2026  
**Estado:** Especificación para implementación con Claude Code

---

## 1. Visión General

Este documento define cómo se construye el catálogo de permisos de Fudie. El catálogo es la pieza que conecta los endpoints del sistema con los roles configurables por restaurante definidos en el modelo de autorización.

El principio fundamental es: **el desarrollador no escribe nada relacionado con permisos.** El scope de cada endpoint se genera automáticamente por reflexión a partir de la slice que lo define. Todo endpoint está cerrado por defecto. El único gesto explícito del desarrollador es abrir un endpoint con `AllowAnonymous()`.

---

## 2. Las Tres Perspectivas

### 2.1 Fudie como producto

Fudie controla el catálogo de permisos. Cuando se añade una funcionalidad nueva (una slice), el catálogo crece automáticamente. Fudie también precarga roles de sistema con agrupaciones sensatas que funcionan out of the box para la mayoría de restaurantes.

Fudie como organización opera a nivel de plataforma. Los empleados de Fudie se autentican en el mismo sistema de Auth — Fudie no es un tenant, es el propietario de la aplicación. Un super admin con user y password es el primer usuario, e invita al resto del equipo de Fudie con el mecanismo estándar. Los permisos de plataforma (alérgenos, planes, lectura de restaurantes activos) conviven en el mismo catálogo y bitfield que los permisos de restaurante.

### 2.2 El administrador del restaurante

El Owner piensa en personas y acciones: "mi encargado puede gestionar la carta, mi camarero solo puede verla, la app del TPV puede leer los menús para facturar". No piensa en endpoints ni en bitfields.

El Owner configura roles usando agrupaciones lógicas de permisos. Si necesita control más fino, puede bajar al nivel de scope atómico y quitar un permiso individual dentro de una agrupación.

### 2.3 El desarrollador de un microservicio

El desarrollador crea su slice. Punto. La reflexión descubre la clase, extrae el nombre como scope atómico, lee el verbo HTTP para la agrupación automática, y asigna el bit. El desarrollador no escribe strings de permisos, no decora con atributos, no piensa en seguridad.

---

## 3. Scopes Atómicos

### 3.1 Generación automática

Cada slice es una clase pública que implementa `IFeatureModule` y vive en su propio archivo. El sistema ya descubre estas clases por reflexión en `MapFeatures`. El nombre de la clase **es** el scope atómico.

```
CreateMenu          → scope atómico: CreateMenu
UpdateMenu          → scope atómico: UpdateMenu
ActivateMenu        → scope atómico: ActivateMenu
GetMenu             → scope atómico: GetMenu
AddMenuItemAllergen → scope atómico: AddMenuItemAllergen
```

Cada scope atómico tiene una posición fija en el bitfield. Un bit por clase. Las posiciones son inmutables — una vez asignada, no se reasigna jamás.

### 3.2 Todo cerrado por defecto

Al registrar las rutas, todos los endpoints requieren autenticación. El desarrollador solo interviene para abrir:

```csharp
public void AddRoutes(IEndpointRouteBuilder app)
{
    app.MapPost("/auth/login", Handler)
       .AllowAnonymous();
}
```

Si no hay `AllowAnonymous()`, el endpoint está protegido y su scope atómico participa en el sistema de permisos.

---

## 4. Agrupaciones

Las agrupaciones son conjuntos de scopes atómicos que el Owner ve al configurar roles. Hay dos tipos: automáticas y custom.

### 4.1 Agrupación automática por verbo HTTP

La reflexión lee el verbo HTTP de cada endpoint y genera dos agrupaciones por dominio:

- **`{dominio}:read`** — agrupa todos los endpoints `GET` del dominio.
- **`{dominio}:write`** — agrupa todos los endpoints `POST`, `PUT`, `DELETE` del dominio.

Ejemplo para el bounded context de Menus:

| Agrupación | Scopes atómicos que contiene |
|------------|------------------------------|
| `menu:read` | `GetMenu`, `GetMenus` |
| `menu:write` | `CreateMenu`, `UpdateMenu`, `ActivateMenu`, `DeactivateMenu`, `AddMenuCategory`, `UpdateMenuCategory`, `RemoveMenuCategory`, `AddItemToCategory`, `UpdateCategoryItem`, `RemoveItemFromCategory`, `SetMenuDepositPolicy`, `RemoveMenuDepositPolicy` |
| `menu-item:read` | `GetMenuItem`, `GetMenuItems` |
| `menu-item:write` | `CreateMenuItem`, `UpdateMenuItem`, `ActivateMenuItem`, `DeactivateMenuItem`, `MarkMenuItemAsAvailable`, `MarkMenuItemAsUnavailable`, `AddMenuItemAllergen`, `RemoveMenuItemAllergen`, `AddMenuItemPriceOption`, `UpdateMenuItemPriceOption`, `RemoveMenuItemPriceOption`, `SetMenuItemNutritionalInfo`, `RemoveMenuItemNutritionalInfo`, `SetMenuItemDepositOverride`, `RemoveMenuItemDepositOverride` |
| `allergen:read` | `GetAllergen`, `GetAllergens` |
| `allergen:write` | `CreateAllergen`, `UpdateAllergen` |

El dominio se infiere del agregado raíz al que pertenece la slice (por namespace o convención de carpetas).

### 4.2 Agrupación custom

Cuando la agrupación automática por verbo no es suficiente, el desarrollador puede etiquetar un endpoint con una agrupación custom. Misma sintaxis que `AllowAnonymous()`:

```csharp
public void AddRoutes(IEndpointRouteBuilder app)
{
    app.MapPut("/menu-items/{id}/deposit-override", Handler)
       .WithPermissionGroup("menu-item:deposit");
}
```

Esto permite crear agrupaciones transversales que cruzan la línea read/write. Por ejemplo, `menu-item:deposit` podría agrupar `SetMenuItemDepositOverride` y `RemoveMenuItemDepositOverride` — ambos son write, pero el Owner quiere controlarlos por separado porque tocan dinero.

Un endpoint puede pertenecer a la agrupación automática por verbo **y** a una agrupación custom simultáneamente. Las agrupaciones no son excluyentes — son vistas sobre el mismo bitfield.

---

## 5. Lo Que Ve el Owner

Al configurar un rol, el Owner ve las agrupaciones organizadas por dominio:

```
Menús
  ├── menu:read        ✓  (ver menús)
  └── menu:write       ✓  (gestionar menús)

Platos
  ├── menu-item:read   ✓  (ver platos)
  ├── menu-item:write  ✓  (gestionar platos)
  └── menu-item:deposit ☐  (políticas de fianza de platos)

Alérgenos
  ├── allergen:read    ✓  (ver alérgenos)
  └── allergen:write   ☐  (gestionar alérgenos)
```

Si el Owner quiere más control, expande una agrupación y ve los scopes atómicos individuales con sus descripciones:

```
menu-item:write  ✓  (gestionar platos)
  ├── CreateMenuItem             ✓  Crear un nuevo plato
  ├── UpdateMenuItem             ✓  Modificar datos de un plato
  ├── ActivateMenuItem           ☐  Activar un plato para que sea visible
  ├── DeactivateMenuItem         ✓  Desactivar un plato temporalmente
  ├── MarkMenuItemAsAvailable    ✓  Marcar plato como disponible hoy
  ├── MarkMenuItemAsUnavailable  ✓  Marcar plato como no disponible hoy
  ├── AddMenuItemAllergen        ✓  Añadir un alérgeno a un plato
  ├── RemoveMenuItemAllergen     ✓  Quitar un alérgeno de un plato
  ├── AddMenuItemPriceOption     ✓  Añadir una opción de precio
  ├── UpdateMenuItemPriceOption  ✓  Modificar una opción de precio
  └── RemoveMenuItemPriceOption  ✓  Eliminar una opción de precio
```

La agrupación es comodidad. El scope atómico es el control real.

---

## 6. Bitfield

### 6.1 Estructura

Cada scope atómico (nombre de clase) tiene una posición fija en el bitfield. El bitfield es un `byte[]` sin límite práctico. 1000 permisos son 125 bytes. No hay techo artificial.

Las posiciones son inmutables. Si se elimina una slice, su posición queda reservada y no se reutiliza. Los nuevos scopes se añaden en la siguiente posición libre.

### 6.2 Transporte en el JWT

El `byte[]` viaja en el claim `permissions` del JWT efímero codificado en Base64. 1000 bits son ~168 caracteres en Base64. Cabe sin problema en el token.

```json
{
  "jti": "uuid-único",
  "sub": "userId",
  "tid": "restaurantId",
  "permissions": "AQIDBAU...",
  "iat": 1738900800,
  "exp": 1738900830
}
```

### 6.3 Agrupaciones como máscaras

Cada agrupación (automática o custom) es una máscara de bits precalculada — el OR de las posiciones de los scopes atómicos que contiene. La máscara es un `byte[]` de la misma longitud que el bitfield del usuario.

Cuando el Owner activa `menu:write`, enciende todos los bits de la máscara. Cuando desactiva `ActivateMenuItem` individualmente, apaga ese bit concreto.

### 6.4 Validación en el microservicio

El microservicio recibe el bitfield en el JWT efímero. La librería de autorización conoce la posición del scope atómico del endpoint que se está ejecutando. La validación es una operación AND a nivel de bits sobre el byte correspondiente:

```csharp
int byteIndex = permissionPosition / 8;
int bitIndex = permissionPosition % 8;
bool hasPermission = (userPermissions[byteIndex] & (1 << bitIndex)) != 0;
```

---

## 7. Descripciones de los Scopes

### 7.1 Responsabilidad de producto, no del desarrollador

Las descripciones de los scopes son lo que el Owner ve cuando configura roles. Son texto para humanos: "Crear un nuevo plato", "Activar un plato para que sea visible", "Configurar política de fianza de un plato". La calidad de estas descripciones determina si el Owner entiende lo que está activando o desactivando.

El desarrollador no es la persona adecuada para escribirlas. Piensa en código, no en usabilidad. Si las descripciones las escribe el desarrollador, acabarán siendo técnicas, inconsistentes o incomprensibles para alguien que gestiona un restaurante.

Las descripciones son **responsabilidad del product owner**. Se definen en el domain specification de cada agregado, junto a la definición funcional de cada comando y query. El domain specification ya contiene la información necesaria — es la fuente de verdad natural.

### 7.2 Archivo de descripciones

Cada microservicio contiene un archivo de descripciones que mapea scopes atómicos y agrupaciones a texto legible en múltiples idiomas. Este archivo lo mantiene producto, no el desarrollador.

Las descripciones se definen en español en el domain specification durante la sesión de diseño. Claude Code genera el archivo con el español como base y traduce automáticamente al resto de idiomas necesarios.

```json
{
  "scopes": {
    "CreateMenu": {
      "es": "Crear un nuevo menú",
      "en": "Create a new menu"
    },
    "UpdateMenu": {
      "es": "Modificar datos de un menú",
      "en": "Update menu details"
    },
    "ActivateMenu": {
      "es": "Activar un menú para que sea visible al público",
      "en": "Activate a menu to make it publicly visible"
    },
    "DeactivateMenu": {
      "es": "Desactivar un menú temporalmente",
      "en": "Temporarily deactivate a menu"
    },
    "AddMenuCategory": {
      "es": "Añadir una categoría a un menú",
      "en": "Add a category to a menu"
    },
    "UpdateMenuCategory": {
      "es": "Modificar una categoría de un menú",
      "en": "Update a menu category"
    },
    "RemoveMenuCategory": {
      "es": "Eliminar una categoría de un menú",
      "en": "Remove a category from a menu"
    },
    "AddItemToCategory": {
      "es": "Añadir un plato a una categoría",
      "en": "Add a dish to a category"
    },
    "UpdateCategoryItem": {
      "es": "Modificar un plato dentro de una categoría",
      "en": "Update a dish within a category"
    },
    "RemoveItemFromCategory": {
      "es": "Quitar un plato de una categoría",
      "en": "Remove a dish from a category"
    },
    "SetMenuDepositPolicy": {
      "es": "Configurar la política de fianza del menú",
      "en": "Set the menu deposit policy"
    },
    "RemoveMenuDepositPolicy": {
      "es": "Eliminar la política de fianza del menú",
      "en": "Remove the menu deposit policy"
    },
    "GetMenu": {
      "es": "Ver un menú",
      "en": "View a menu"
    },
    "GetMenus": {
      "es": "Ver la lista de menús",
      "en": "View the menu list"
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
    "menu-item:deposit": {
      "es": "Políticas de fianza de platos",
      "en": "Dish deposit policies"
    }
  }
}
```

### 7.3 Registro automático en el servicio de Auth

Cuando un microservicio arranca:

1. La reflexión descubre las slices (scopes atómicos, verbos HTTP, agrupaciones custom).
2. Lee el archivo de descripciones.
3. Envía todo al servicio de Auth por un **endpoint interno** que solo pueden invocar los microservicios.
4. El servicio de Auth registra el catálogo: scope atómico + descripción + agrupaciones + posición en el bitfield.

No hay base de datos extra para las descripciones. No hay intervención manual. No hay deploy separado. El microservicio arranca, se registra, y el servicio de Auth tiene el catálogo actualizado.

Si un scope atómico no tiene descripción en el archivo, el servicio de Auth lo registra igualmente con el nombre de la clase como fallback. Esto permite que el sistema funcione aunque producto aún no haya completado todas las descripciones.

---

## 8. Registro Automático del Catálogo

El catálogo de permisos se construye en dos partes que convergen en el servicio de Auth:

**En cada microservicio (al arrancar):**

1. La reflexión descubre todas las clases `IFeatureModule`.
2. Para cada clase, extrae el nombre (scope atómico) y el verbo HTTP (para agrupación automática).
3. Lee la metadata de `WithPermissionGroup` si existe (agrupación custom).
4. Lee el archivo de descripciones.
5. Envía el registro al servicio de Auth por endpoint interno.

**En el servicio de Auth (al recibir el registro):**

1. Asigna posición en el bitfield si es un scope nuevo.
2. Calcula las máscaras de las agrupaciones.
3. Asocia las descripciones a cada scope y agrupación.

El catálogo resultante es el contrato compartido entre el servicio de Auth (que construye el bitfield del JWT y expone las descripciones al frontend) y la librería de los microservicios (que valida contra el bit del endpoint).

---

## 9. Resumen

| Concepto | Decisión |
|----------|----------|
| Scope atómico | Nombre de la clase de la slice. Un bit por clase. |
| Generación | Automática por reflexión. El desarrollador no escribe nada. |
| Cerrado por defecto | Todo endpoint requiere autenticación. Solo `AllowAnonymous()` abre. |
| Agrupación automática | `{dominio}:read` (GET) y `{dominio}:write` (POST/PUT/DELETE) |
| Agrupación custom | `.WithPermissionGroup("dominio:aspecto")` para casos especiales |
| Convivencia | Un endpoint pertenece a su agrupación automática y opcionalmente a una custom. No son excluyentes. |
| Descripciones | Responsabilidad de producto. Definidas en español en el domain specification. Claude Code genera el archivo multi-idioma del microservicio. |
| Registro | Automático al arrancar. Cada microservicio se registra en el servicio de Auth por endpoint interno. |
| Lo que ve el Owner | Agrupaciones por dominio con descripciones legibles, con posibilidad de expandir al nivel atómico. |
| Bitfield | `byte[]` sin límite práctico. Posiciones inmutables. Viaja en Base64 en el JWT. |
| Permisos de plataforma | Mismo catálogo, mismo bitfield. Fudie es el propietario, no un tenant. |
| Validación | AND a nivel de bits en el microservicio. Sin consulta a Firestore. |

---

**Fecha:** Febrero 2026  
**Contexto:** Derivado de sesión de diseño — Catálogo de permisos, scopes atómicos, agrupaciones y bitfield
