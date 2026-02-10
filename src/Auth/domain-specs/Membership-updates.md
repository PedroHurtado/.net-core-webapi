# Fudie — Membership: Modificación para Aplicaciones Externas

## Especificación de Cambios

**Stack:** .NET Core 8 / C# 12 / Firestore  
**Fecha:** Febrero 2026  
**Estado:** Modificación sobre especificación existente de Membership

---

## 1. Contexto del Cambio

Las aplicaciones externas (TPV, sistemas de gestión, integraciones de terceros) necesitan conectarse a la API de Fudie en nombre de un restaurante. En lugar de crear un sistema paralelo de gestión de acceso, se reutiliza el modelo de Membership existente añadiendo un nuevo rol: `ExternalApp`.

La justificación es directa: una aplicación externa es un miembro más del restaurante. El Owner la invita igual que invita a un camarero, le asigna permisos a través del rol, y puede revocar su acceso en cualquier momento. La diferencia es únicamente la experiencia post-aceptación y el mecanismo de autenticación contra la API.

---

## 2. Cambios en el Enum MembershipRole

### Antes

| Valor | Descripción |
|-------|-------------|
| Owner | Propietario del restaurante. Acceso completo. |
| Manager | Encargado. Gestiona operativa diaria. |
| Waiter | Camarero. Acceso limitado a operaciones del día a día. |

### Después

| Valor | Descripción |
|-------|-------------|
| Owner | Propietario del restaurante. Acceso completo. |
| Manager | Encargado. Gestiona operativa diaria. |
| Waiter | Camarero. Acceso limitado a operaciones del día a día. |
| ExternalApp | Aplicación externa. Acceso programático a la API del restaurante. |

---

## 3. Flujo de Invitación de Aplicación Externa

El flujo es idéntico al de cualquier otro miembro:

1. El **Owner** del restaurante invita desde su panel (sección "Equipo" o "Integraciones"). No distingue mecánicamente entre invitar un camarero o conectar una aplicación — introduce un email/teléfono y selecciona el rol.
2. La invitación se envía por **email, SMS u otro canal** al desarrollador de la aplicación externa.
3. El **desarrollador** acepta la invitación (se autentica con Google OAuth, igual que cualquier miembro).
4. Al aceptar, se crea el documento `Membership` con `Role = ExternalApp`.
5. El desarrollador accede a una pantalla específica del rol `ExternalApp` donde puede:
   - Ver y copiar su **API Key**.
   - Rotar (regenerar) la API Key.
   - Ver estadísticas de uso.

El Owner nunca ve ni gestiona la API Key directamente. Desde su panel puede:

- Ver que la aplicación está conectada (la Membership existe y está activa).
- Desactivar la aplicación (`Membership.Deactivate`).
- Eliminar la aplicación (`Membership.Delete`).

---

## 4. Autenticación: Dos Mecanismos, Mismo Backend

Con la adición de `ExternalApp`, la API de Fudie tiene dos tipos de consumidores autenticados:

| Consumidor | Mecanismo | Cómo llega al backend |
|------------|-----------|----------------------|
| Web app de Fudie (Owner, Manager, Waiter) | Cookie de sesión (opaca, HttpOnly) | Header `Cookie` automático del navegador |
| Aplicación externa (ExternalApp) | API Key | Header `Authorization: ApiKey {key}` o header custom `X-Api-Key` |

En ambos casos, el backend resuelve la identidad del llamante y consulta su Membership para determinar qué puede hacer. La lógica de autorización es la misma independientemente de cómo se autenticó.

---

## 5. Modelo de Datos: Nuevas Colecciones

### Colección: `api_keys`

```
api_keys/{apiKeyHash}              ← SHA-256 de la API Key (nunca en claro)
├── membershipId: Guid             ← referencia a la Membership del ExternalApp
├── restaurantId: Guid             ← denormalizado para consulta rápida
├── name: string                   ← nombre descriptivo ("TPV MiSoftware")
├── prefix: string                 ← primeros 8 caracteres de la key (para identificación visual)
├── isActive: boolean
├── createdAt: timestamp
├── lastUsedAt: timestamp
└── expiresAt: timestamp | null    ← null = no expira
```

**Notas:**

- La API Key se muestra **una sola vez** al desarrollador en el momento de generación. Fudie no almacena la key en claro, solo su hash SHA-256.
- El campo `prefix` permite al desarrollador identificar qué key está usando sin exponer el valor completo (ej: `fud_a3k9x7mp...`).
- El `apiKeyHash` como ID del documento permite búsqueda directa O(1) al validar una request: se hashea la key recibida y se busca el documento.

### Formato de la API Key

```
fud_{32-caracteres-aleatorios}
```

Ejemplo:

```
fud_a3K9mZ3pX7nR2wQ8vB4cD6eF1gH5jK
```

El prefijo `fud_` permite identificar visualmente que es una API Key de Fudie y facilita la detección en auditorías de seguridad (escaneo de repositorios, logs, etc.).

---

## 6. Cambios en Comandos Existentes de Membership

### 6.1 Membership.Create — Sin cambios estructurales

El comando acepta `ExternalApp` como valor válido de `MembershipRole`. No requiere modificación de lógica.

Cuando el rol es `ExternalApp`, tras crear la Membership se genera automáticamente la API Key asociada.

### 6.2 Membership.ChangeRole — Nueva restricción

No se permite cambiar un rol humano (Owner/Manager/Waiter) a `ExternalApp` ni viceversa. Son naturalezas distintas.

```csharp
// Nueva validación
ConflictGuard.ThrowIf(
    (membership.Role == MembershipRole.ExternalApp && command.Role != MembershipRole.ExternalApp) ||
    (membership.Role != MembershipRole.ExternalApp && command.Role == MembershipRole.ExternalApp),
    "Cannot change between human and external app roles");
```

### 6.3 Membership.Deactivate — Efecto adicional

Cuando se desactiva una Membership con rol `ExternalApp`, la API Key asociada se marca como inactiva. Las requests con esa API Key son rechazadas inmediatamente.

### 6.4 Membership.Delete — Efecto adicional

Cuando se elimina una Membership con rol `ExternalApp`, la API Key asociada se elimina de Firestore.

### 6.5 Membership.Reactivate — Efecto adicional

Cuando se reactiva una Membership con rol `ExternalApp`, la API Key asociada se reactiva automáticamente.

---

## 7. Nuevos Comandos

### 7.1 ApiKey.Rotate

Regenera la API Key de una Membership `ExternalApp`. La key anterior queda inválida inmediatamente.

#### Event Storming

```
🟡[ExternalApp] → 🔵(RotateApiKey) → 🟤[[ApiKey]] → 🟠<ApiKeyRotated>
```

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Membership no es ExternalApp | 403 | ForbiddenGuard | "Only external apps can rotate API keys" |
| Membership no está activa | 409 | ConflictGuard | "Cannot rotate key for inactive membership" |

#### Lógica

1. Generar nueva API Key (`fud_` + 32 caracteres aleatorios criptográficos).
2. Hashear la nueva key (SHA-256).
3. Eliminar el documento `api_keys/{oldHash}`.
4. Crear nuevo documento `api_keys/{newHash}` con los mismos datos y nueva `createdAt`.
5. Devolver la nueva key en claro (única vez).

#### Slice: POST /memberships/{membershipId}/api-key/rotate

**Response**: 200 OK

```json
{
  "apiKey": "fud_a3K9mZ3pX7nR2wQ8vB4cD6eF1gH5jK",
  "prefix": "fud_a3K9",
  "message": "Store this key securely. It will not be shown again."
}
```

---

## 8. Flujo de Validación de API Key

```
App externa envía request con header X-Api-Key: fud_xxxxx
        │
        ▼
Backend hashea la key recibida (SHA-256)
        │
        ▼
Busca en Firestore: api_keys/{hash}
        │
        ▼
¿Documento existe? → Si no, 401 Unauthorized
        │
        ▼
¿isActive == true? → Si no, 401 Unauthorized
        │
        ▼
¿expiresAt > now (o null)? → Si expiró, 401 Unauthorized
        │
        ▼
Obtiene membershipId → Carga Membership
        │
        ▼
¿Membership.IsActive == true? → Si no, 401 Unauthorized
        │
        ▼
Actualiza lastUsedAt en api_keys
        │
        ▼
Request autorizada con contexto: restaurantId + role (ExternalApp)
```

---

## 9. Resumen de Cambios

| Elemento | Cambio |
|----------|--------|
| `MembershipRole` enum | Nuevo valor: `ExternalApp` |
| `Membership.ChangeRole` | Nueva restricción: no se permite cruce entre roles humanos y ExternalApp |
| `Membership.Deactivate` | Efecto adicional: desactiva API Key asociada |
| `Membership.Reactivate` | Efecto adicional: reactiva API Key asociada |
| `Membership.Delete` | Efecto adicional: elimina API Key asociada |
| Nueva colección | `api_keys/{apiKeyHash}` |
| Nuevo comando | `ApiKey.Rotate` |
| Nuevo endpoint | `POST /memberships/{membershipId}/api-key/rotate` |

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Qué permisos específicos tiene el rol ExternalApp? ¿Acceso de solo lectura o también escritura? | Pendiente: Se definirá con la matriz de permisos por rol |
| 2 | ¿Límite de API Keys por restaurante? | Pendiente |
| 3 | ¿Rate limiting específico por API Key? | Pendiente |
| 4 | ¿Necesidad de scopes (ej: solo lectura de reservas, solo menú)? | Pendiente: Puede ser V2 |
| 5 | ¿Webhook de notificaciones hacia la app externa? | Pendiente: Fuera de alcance actual |

---

**Fecha:** Febrero 2026  
**Contexto:** Derivado de sesión de diseño — Seguridad de administradores y aplicaciones externas
