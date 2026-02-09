# Domain Specification: Session

---

## 1. Enums

No aplica.

---

## 2. Value Objects

No aplica. Los arrays de permisos (`Groups`, `AdditionalScopes`, `ExcludedScopes`) son colecciones primitivas, no value objects.

---

## 3. Entidades

No aplica. El agregado Session no tiene entidades hijas.

---

## 4. Aggregate: Session

### Contexto

La sesión vive en el servicio de Auth. Es el vínculo entre la cookie opaca del navegador y la identidad del usuario. No almacena JWT — el JWT efímero se genera por request a partir de los datos de la sesión.

La sesión contiene la información que el servicio de Auth necesita para construir el JWT efímero sin tener que cargar Membership ni Role en cada request. Los permisos del rol están denormalizados aquí para evitar una lectura extra a Firestore por request.

Cuando cambian los permisos de un rol, se destruyen las sesiones afectadas. El usuario re-autentica con un click de Google OAuth y la nueva sesión se crea con los permisos actualizados.

### Estructura

```
Session (Aggregate Root)
├─ Id: Guid                              ← valor de la cookie (GUID v4 criptográfico)
├─ UserId: string                        ← providerId del User
├─ TenantId: Guid?                       ← null hasta crear/seleccionar tenant
├─ RoleId: Guid?                         ← null hasta tener Membership
├─ Groups: string[]                      ← agrupaciones del rol (denormalizadas)
├─ AdditionalScopes: string[]            ← scopes individuales añadidos
├─ ExcludedScopes: string[]              ← scopes individuales excluidos
├─ IsOwner: bool                         ← bypass de permisos (flag owner: true en JWT)
├─ CreatedAt: DateTimeOffset
├─ LastActivityAt: DateTimeOffset
└─ ExpiresAt: DateTimeOffset             ← LastActivityAt + 30 días
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| UserId | string | protected set |
| TenantId | Guid? | protected set |
| RoleId | Guid? | protected set |
| Groups | string[] | protected set |
| AdditionalScopes | string[] | protected set |
| ExcludedScopes | string[] | protected set |
| IsOwner | bool | protected set |
| CreatedAt | DateTimeOffset | init |
| LastActivityAt | DateTimeOffset | protected set |
| ExpiresAt | DateTimeOffset | protected set |

#### Propiedades Calculadas

| Propiedad | Tipo | Fórmula |
|-----------|------|---------|
| IsExpired | bool | `DateTimeOffset.UtcNow > ExpiresAt` |
| HasTenantContext | bool | `TenantId != null` |

**Notas:**

- `Id` se genera con aleatoriedad criptográfica. Es el valor que viaja en la cookie `fudie_session`. No contiene información — es un puntero opaco al documento de Firestore.
- `UserId` es el `ProviderId` del agregado User. Identifica al usuario independientemente del proveedor de autenticación.
- `TenantId` es nullable. Es null en dos escenarios: (1) usuario recién autenticado que aún no ha creado/seleccionado tenant, (2) usuarios de plataforma Fudie que operan sin tenant.
- `RoleId` es nullable. Es null cuando no hay tenant activo. Siempre tiene valor cuando `TenantId` tiene valor.
- `Groups`, `AdditionalScopes`, `ExcludedScopes` están denormalizados desde el rol para evitar una lectura extra a Firestore por request. Se copian al crear la sesión o al establecer el contexto de tenant. Son arrays vacíos cuando no hay tenant activo.
- `IsOwner` — cuando es `true`, el JWT lleva `owner: true` y el microservicio hace bypass de validación de permisos. No lleva arrays de permisos.
- Sliding expiration: `LastActivityAt` se actualiza en cada request válida. `ExpiresAt` se recalcula como `LastActivityAt + 30 días`.

### Invariantes (Validator)

> Estas reglas se implementan en `SessionValidator : AbstractValidator<Session>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| UserId | NotEmpty | "UserId is required" |
| RoleId | NotNull when TenantId != null | "RoleId is required when TenantId is set" |
| RoleId | Null when TenantId == null | "RoleId must be null when TenantId is not set" |
| ExpiresAt | > CreatedAt | "ExpiresAt must be after CreatedAt" |
| ExpiresAt | > LastActivityAt | "ExpiresAt must be after LastActivityAt" |

---

## 5. Response

```csharp
public record SessionResponse(
    Guid Id,
    string UserId,
    Guid? TenantId,
    Guid? RoleId,
    string[] Groups,
    string[] AdditionalScopes,
    string[] ExcludedScopes,
    bool IsOwner,
    bool HasTenantContext,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastActivityAt,
    DateTimeOffset ExpiresAt
);
```

**Nota:** Este Response es interno del servicio de Auth. No se expone a los microservicios ni al frontend. El frontend solo ve la cookie opaca. Los microservicios solo ven el JWT efímero.

---

## 6. Event Storming — Leyenda

| Color | Elemento | Símbolo | Descripción |
|-------|----------|---------|-------------|
| 🟠 Naranja | Domain Event | `<EventName>` | Algo que ocurrió (pasado) |
| 🔵 Azul | Command | `(CommandName)` | Intención/Acción (imperativo) |
| 🟡 Amarillo | Actor | `[ActorName]` | Usuario o sistema que inicia |
| 🟣 Púrpura | Policy | `{PolicyName}` | Regla de negocio/Política |
| 🟤 Marrón | Aggregate | `[[AggregateName]]` | Entidad raíz del agregado |
| 🔴 Rojo | Hot Spot | `⚠️` | Dudas o conflictos pendientes |
| 🟢 Verde | Read Model | `📊` | Vista/Proyección de datos |
| ⚪ Blanco | External System | `⚡` | Sistema externo |

---

## 7. Comandos

> ⚠️ **IMPORTANTE**: La Session es un agregado de infraestructura del servicio de Auth. No tiene endpoints REST propios — los comandos se ejecutan internamente desde las slices de autenticación (LoginWithGoogle, LoginWithPassword, Logout) y desde operaciones del sistema (invalidación por cambio de permisos).
>
> **Tests de dominio**: Usar `TestableSession` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.

---

### 7.1 Session.Create

#### Event Storming
```
🟡[Auth Service] → 🔵(CreateSession) → 🟤[[Session]] → 🟠<SessionCreated>
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| UserId | string | |

#### Inyecta
- `IValidator<Session>`

#### Guards

Ninguno.

#### Lógica
```csharp
var now = DateTimeOffset.UtcNow;

var session = new Session(Guid.CreateVersion7())
{
    UserId = command.UserId,
    TenantId = null,
    RoleId = null,
    Groups = [],
    AdditionalScopes = [],
    ExcludedScopes = [],
    IsOwner = false,
    CreatedAt = now,
    LastActivityAt = now,
    ExpiresAt = now.AddDays(30)
};

return sessionValidator.ValidateOrThrow(session);
```

**Notas:**
- Se crea sin contexto de tenant. El usuario acaba de autenticarse y aún no ha seleccionado en qué tenant va a trabajar.
- `Guid.CreateVersion7()` genera un GUID v7 con componente temporal + aleatoriedad criptográfica.
- La cookie `fudie_session` se configura en la slice de login, no aquí. El comando de dominio solo crea el agregado.

#### Tests Unitarios (Dominio)

✅ Crear sesión con datos válidos
- Input: UserId="google-oauth2|123456789"
- Resultado: Session creada con TenantId=null, RoleId=null, Groups=[], IsOwner=false, ExpiresAt=CreatedAt+30días

❌ UserId vacío
- Input: UserId=""
- Resultado: ValidationException "UserId is required"

---

### 7.2 Session.SetTenantContext

#### Event Storming
```
🟡[Usuario] → 🔵(SetTenantContext) → 🟤[[Session]] → 🟠<TenantContextSet>
                                          │
                                🟣{MembershipExists}
```

#### Input

| Campo | Tipo |
|-------|------|
| TenantId | Guid |
| RoleId | Guid |
| Groups | string[] |
| AdditionalScopes | string[] |
| ExcludedScopes | string[] |
| IsOwner | bool |

#### Inyecta
- `IValidator<Session>`

#### Guards

Ninguno. La validación de que la Membership existe y está activa se hace en la slice que invoca este comando, no aquí.

#### Lógica
```csharp
session.TenantId = command.TenantId;
session.RoleId = command.RoleId;
session.Groups = command.Groups;
session.AdditionalScopes = command.AdditionalScopes;
session.ExcludedScopes = command.ExcludedScopes;
session.IsOwner = command.IsOwner;

return sessionValidator.ValidateOrThrow(session);
```

**Notas:**
- Este comando se ejecuta cuando el usuario selecciona un tenant (al crear un tenant nuevo, al seleccionar uno existente, o automáticamente si solo tiene uno).
- Los datos de permisos se copian directamente del rol de la Membership. No se consulta el catálogo de permisos — el Auth no lo conoce, solo copia lo que tiene el rol.
- Si `IsOwner` es `true`, los arrays de permisos se ignoran al construir el JWT — el Owner tiene bypass total.

#### Tests Unitarios (Dominio)

> Estado previo: `TestableSession` sin contexto de tenant.

✅ Establecer contexto de tenant
- Precondición: Session con TenantId=null
- Input: TenantId=valid, RoleId=valid, Groups=["menu:read"], IsOwner=false
- Resultado: Session con TenantId, RoleId y Groups actualizados

✅ Establecer contexto como Owner
- Precondición: Session con TenantId=null
- Input: TenantId=valid, RoleId=valid, Groups=[], IsOwner=true
- Resultado: Session con IsOwner=true, Groups vacío (no se necesitan)

✅ Cambiar de tenant (usuario con múltiples Memberships)
- Precondición: Session con TenantId=tenant1
- Input: TenantId=tenant2, RoleId=otherRole, Groups=["reservation:read"]
- Resultado: Session con nuevo TenantId, RoleId y Groups

❌ TenantId con RoleId null
- Input: TenantId=valid, RoleId=null
- Resultado: ValidationException "RoleId is required when TenantId is set"

---

### 7.3 Session.ClearTenantContext

#### Event Storming
```
🟡[Usuario] → 🔵(ClearTenantContext) → 🟤[[Session]] → 🟠<TenantContextCleared>
```

#### Input

Ninguno

#### Inyecta
- `IValidator<Session>`

#### Guards

Ninguno.

#### Lógica
```csharp
session.TenantId = null;
session.RoleId = null;
session.Groups = [];
session.AdditionalScopes = [];
session.ExcludedScopes = [];
session.IsOwner = false;

return sessionValidator.ValidateOrThrow(session);
```

**Notas:**
- Se usa cuando el usuario quiere volver al estado "sin tenant" para seleccionar otro, o cuando se desactiva su Membership del tenant actual.

#### Tests Unitarios (Dominio)

> Estado previo: `TestableSession` con contexto de tenant.

✅ Limpiar contexto de tenant
- Precondición: Session con TenantId=valid, RoleId=valid, Groups=["menu:read"]
- Resultado: Session con TenantId=null, RoleId=null, Groups=[], IsOwner=false

✅ Limpiar contexto ya limpio (idempotente)
- Precondición: Session con TenantId=null
- Resultado: Sin cambios

---

### 7.4 Session.Refresh

#### Event Storming
```
🟡[Auth Service] → 🔵(RefreshSession) → 🟤[[Session]] → 🟠<SessionRefreshed>
```

#### Input

Ninguno

#### Inyecta
- `IValidator<Session>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Session expirada | 401 | UnauthorizedGuard | "Session expired" |

#### Lógica
```csharp
UnauthorizedGuard.ThrowIf(session.IsExpired, "Session expired");

var now = DateTimeOffset.UtcNow;
session.LastActivityAt = now;
session.ExpiresAt = now.AddDays(30);

return sessionValidator.ValidateOrThrow(session);
```

**Notas:**
- Se ejecuta en cada request válida. El middleware del servicio de Auth carga la sesión, ejecuta Refresh, y luego construye el JWT efímero.
- La cookie se renueva con la nueva fecha de `Expires` en la respuesta HTTP.

#### Tests Unitarios (Dominio)

> Estado previo: `TestableSession` con diferentes estados de expiración.

✅ Refrescar sesión activa
- Precondición: Session con ExpiresAt futuro
- Resultado: LastActivityAt y ExpiresAt actualizados a now + 30 días

❌ Sesión expirada
- Precondición: Session con ExpiresAt en el pasado
- Resultado: UnauthorizedException "Session expired"

---

### 7.5 GetSessionById

#### Event Storming
```
🟡[Auth Service] → 🔵(GetSessionById) → 🟤[[Session]] → 📊 Session
```

#### Query interna

No es un endpoint REST. Es una operación interna del servicio de Auth que se ejecuta en cada request autenticada.

```csharp
var session = await sessionRepository.GetByIdAsync(sessionId);
// sessionId viene del valor de la cookie fudie_session
```

**Notas:**
- Si el documento no existe en Firestore → 401 Unauthorized.
- Si la sesión está expirada → se elimina el documento y se devuelve 401.

#### Tests Unitarios (Servicio)

✅ Obtiene sesión existente por Id
- Verifica que repository.GetByIdAsync es llamado con el sessionId de la cookie

✅ Retorna null cuando no existe
- Verifica que se devuelve null y se responde 401

---

### 7.6 ListSessionsByRoleAndTenant

#### Event Storming
```
🟡[Auth Service] → 🔵(ListSessionsByRoleAndTenant) → 🟤[[Session]] → 📊 Session[]
```

#### Query interna

Se ejecuta cuando cambian los permisos de un rol. Busca todas las sesiones activas con ese `RoleId` y `TenantId` para destruirlas.

```csharp
var sessions = await sessionRepository.ListByRoleAndTenantAsync(roleId, tenantId);
```

#### Tests Unitarios (Servicio)

✅ Retorna sesiones que coinciden con RoleId y TenantId
- Verifica que filtra correctamente

✅ Retorna lista vacía si no hay sesiones con ese rol
- Verifica que no falla

---

### 7.7 ListSessionsByUserId

#### Event Storming
```
🟡[Auth Service] → 🔵(ListSessionsByUserId) → 🟤[[Session]] → 📊 Session[]
```

#### Query interna

Se ejecuta cuando se desactiva o elimina una Membership. Busca todas las sesiones del usuario para destruir las del tenant afectado.

```csharp
var sessions = await sessionRepository.ListByUserIdAsync(userId);
```

#### Tests Unitarios (Servicio)

✅ Retorna sesiones del usuario

✅ Retorna lista vacía si el usuario no tiene sesiones activas

---

### 7.8 Session.Destroy

#### Event Storming
```
🟡[Usuario/Sistema] → 🔵(DestroySession) → 🟤[[Session]] → 🟠<SessionDestroyed>
```

#### Input

Ninguno (opera sobre la sesión cargada)

#### Guards

Ninguno.

#### Lógica

Eliminación física del documento en Firestore. No es un soft delete — la sesión se borra.

```csharp
await sessionRepository.DeleteAsync(session.Id);
```

**Notas:**
- Se ejecuta en logout voluntario, invalidación por cambio de permisos, desactivación de Membership, o eliminación de Membership.
- La cookie `fudie_session` se limpia en la respuesta HTTP cuando es un logout voluntario.
- Para invalidación masiva (cambio de permisos de un rol), se usa `ListSessionsByRoleAndTenant` seguido de `Destroy` para cada sesión.

#### Tests Unitarios (Dominio)

✅ Destruir sesión existente
- Resultado: Documento eliminado de Firestore

---

## 8. JWT Efímero — Construcción desde la Session

> Esta sección documenta cómo el servicio de Auth construye el JWT efímero a partir de los datos de la sesión. No es un comando del agregado — es lógica de infraestructura que consume los datos de la sesión.

### Construcción

```csharp
// Si no hay contexto de tenant
if (session.TenantId is null)
{
    return new JwtPayload
    {
        Sub = session.UserId,
        Iat = now,
        Exp = now + TimeSpan.FromSeconds(45)
    };
}

// Si es Owner → bypass de permisos
if (session.IsOwner)
{
    return new JwtPayload
    {
        Sub = session.UserId,
        Tid = session.TenantId,
        Owner = true,
        Iat = now,
        Exp = now + TimeSpan.FromSeconds(45)
    };
}

// Usuario normal con tenant
return new JwtPayload
{
    Sub = session.UserId,
    Tid = session.TenantId,
    Groups = session.Groups,
    Add = session.AdditionalScopes,
    Exc = session.ExcludedScopes,
    Iat = now,
    Exp = now + TimeSpan.FromSeconds(45)
};
```

### JWT resultante — Sin tenant

```json
{
  "sub": "google-oauth2|123456789",
  "iat": 1738900800,
  "exp": 1738900845
}
```

### JWT resultante — Owner

```json
{
  "sub": "google-oauth2|123456789",
  "tid": "tenant-guid",
  "owner": true,
  "iat": 1738900800,
  "exp": 1738900845
}
```

### JWT resultante — Usuario con permisos

```json
{
  "sub": "google-oauth2|123456789",
  "tid": "tenant-guid",
  "groups": ["menu:read", "menu:write"],
  "add": ["reservation-service:CancelReservation"],
  "exc": ["menu-service:SetMenuDepositPolicy"],
  "iat": 1738900800,
  "exp": 1738900845
}
```

**Notas:**
- El JWT se firma con clave privada ES256. Solo el servicio de Auth tiene la clave privada.
- Vida útil: 45 segundos. Nace, viaja al microservicio, se valida, muere.
- Los microservicios validan la firma con clave pública descargada de `/.well-known/jwks.json` del servicio de Auth.
- Clock skew recomendado: 5 segundos en `TokenValidationParameters`.

---

## 9. Flujos de Uso

### 9.1 Login (Google OAuth o Password)

```
Usuario se autentica
        │
        ▼
Slice de login resuelve/crea User
        │
        ▼
Session.Create(userId)
        │
        ▼
Session sin tenant → cookie fudie_session={sessionId}
        │
        ▼
Frontend muestra selector de tenant (o auto-selecciona si solo tiene uno)
        │
        ▼
Session.SetTenantContext(tenantId, roleId, groups, add, exc, isOwner)
```

### 9.2 Request autenticada (cada request)

```
Cookie fudie_session={sessionId}
        │
        ▼
GetSessionById(sessionId)
        │
        ▼
¿Existe? → No → 401
        │
        ▼
Session.Refresh() → actualiza sliding expiration
        │
        ▼
Construye JWT efímero desde datos de la sesión
        │
        ▼
Firma JWT con clave privada ES256
        │
        ▼
Reenvía request + JWT al microservicio destino
```

### 9.3 Cambio de permisos de un rol

```
Owner modifica permisos de un rol
        │
        ▼
ListSessionsByRoleAndTenant(roleId, tenantId)
        │
        ▼
Para cada sesión → Session.Destroy()
        │
        ▼
Usuarios afectados → siguiente request → 401 → re-login (un click)
        │
        ▼
Nueva sesión con permisos actualizados
```

### 9.4 Desactivación de Membership

```
Owner desactiva Membership de un usuario
        │
        ▼
ListSessionsByUserId(userId)
        │
        ▼
Filtrar sesiones con TenantId del tenant afectado
        │
        ▼
Session.Destroy() para cada una
```

### 9.5 Crear tenant (comprar suscripción)

```
Usuario autenticado sin tenant
        │
        ▼
POST /tenants → crea tenant + Membership Owner
        │
        ▼
Session.SetTenantContext(tenantId, roleId, groups=[], add=[], exc=[], isOwner=true)
        │
        ▼
Siguiente request → JWT con owner: true y tid
```

---

## 10. Descripciones de Permisos

No aplica. La Session no tiene endpoints REST públicos — es un agregado interno del servicio de Auth. No genera scopes atómicos en el catálogo de permisos.

---

## 11. Resumen de Operaciones (Orden de Implementación)

| # | Operación | Tipo | Trigger | Resultado |
|---|-----------|------|---------|-----------|
| 1 | Session.Create | Comando | Login exitoso | Session sin tenant |
| 2 | GetSessionById | Query | Cada request | Session o 401 |
| 3 | Session.Refresh | Comando | Cada request válida | Sliding expiration |
| 4 | Session.SetTenantContext | Comando | Crear/seleccionar tenant | Session con permisos |
| 5 | Session.ClearTenantContext | Comando | Cambiar de tenant | Session sin permisos |
| 6 | ListSessionsByRoleAndTenant | Query | Cambio de permisos de rol | Sessions a destruir |
| 7 | ListSessionsByUserId | Query | Desactivar/eliminar Membership | Sessions a destruir |
| 8 | Session.Destroy | Comando | Logout / invalidación | Documento eliminado |

---

## 12. Persistencia (Firestore)

### Colección

`/sessions/{sessionId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<Session>(entity =>
{
    // Sin QueryFilter de TenantId — las sesiones se buscan por Id, UserId, o RoleId+TenantId

    // Ignore: propiedades computed
    entity.Ignore(x => x.IsExpired);
    entity.Ignore(x => x.HasTenantContext);
});
```

### Índices

| Campo(s) | Tipo | Justificación |
|----------|------|---------------|
| `UserId` | Simple | Buscar sesiones de un usuario para invalidación |
| `RoleId` + `TenantId` | Composite | Buscar sesiones por rol y tenant para invalidación masiva |

### Documento Ejemplo — Sin tenant

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "userId": "google-oauth2|123456789",
  "tenantId": null,
  "roleId": null,
  "groups": [],
  "additionalScopes": [],
  "excludedScopes": [],
  "isOwner": false,
  "createdAt": "2026-02-09T10:00:00Z",
  "lastActivityAt": "2026-02-09T10:00:00Z",
  "expiresAt": "2026-03-11T10:00:00Z"
}
```

### Documento Ejemplo — Owner con tenant

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "userId": "google-oauth2|123456789",
  "tenantId": "tenant-001-guid",
  "roleId": "owner-role-guid",
  "groups": [],
  "additionalScopes": [],
  "excludedScopes": [],
  "isOwner": true,
  "createdAt": "2026-02-09T10:00:00Z",
  "lastActivityAt": "2026-02-09T14:30:00Z",
  "expiresAt": "2026-03-11T14:30:00Z"
}
```

### Documento Ejemplo — Manager con permisos

```json
{
  "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "userId": "google-oauth2|987654321",
  "tenantId": "tenant-001-guid",
  "roleId": "manager-role-guid",
  "groups": ["menu:read", "menu:write", "reservation:read", "reservation:write"],
  "additionalScopes": [],
  "excludedScopes": ["menu-service:SetMenuDepositPolicy"],
  "isOwner": false,
  "createdAt": "2026-02-09T08:00:00Z",
  "lastActivityAt": "2026-02-09T15:45:00Z",
  "expiresAt": "2026-03-11T15:45:00Z"
}
```

---

## 13. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | Limpieza de sesiones expiradas | Decidido: TTL policy nativa de Firestore sobre el campo `expiresAt`. Firestore elimina automáticamente los documentos expirados (dentro de 24h). Sin Cloud Functions ni limpieza manual. |
| 2 | Usuario de plataforma Fudie (superadmin y equipo) — ¿necesita TenantId? | Decidido: TenantId null. Los permisos de plataforma se resuelven por Groups sin `tid` en el JWT. |
| 3 | Auto-selección de tenant cuando el usuario solo tiene una Membership | Decidido: sí. Si solo tiene una Membership, se ejecuta SetTenantContext automáticamente sin paso de UI. |

---

**Fecha**: 2026-02-09  
**Autor**: Equipo Fudie
