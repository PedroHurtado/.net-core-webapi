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
├─ Id: Guid                              ← valor de la cookie (GUID v7 criptográfico)
├─ UserId: Guid                          ← Id del agregado User
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
| UserId | Guid | protected set |
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

- `Id` se genera con `Guid.CreateVersion7()` — componente temporal + aleatoriedad criptográfica. Es el valor que viaja en la cookie `fudie_session`. No contiene información — es un puntero opaco al documento de Firestore.
- `UserId` es el `Id` del agregado User de Fudie. Cuando se crea la sesión, el usuario ya existe en el sistema.
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
    Guid UserId,
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

> ⚠️ **IMPORTANTE**: El orden de los comandos respeta las dependencias.
> - Las Queries (Get, List) van después de Create porque son necesarias para verificar persistencia
> - SetTenantContext y ClearTenantContext van después de Create porque dependen de que la sesión exista
> - Refresh va después de SetTenantContext porque se ejecuta en cada request autenticada
> - Destroy va al final porque es la operación terminal
> - ResolveAuth y GetJwks son slices del servicio de Auth que consumen los comandos de dominio

> **Tests de dominio**: Usar `TestableSession` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableSession` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 7.1 Session.Create

#### Event Storming
```
🟡[Auth Service] → 🔵(CreateSession) → 🟤[[Session]] → 🟠<SessionCreated>
```

#### Input

| Campo | Tipo | Default |
|-------|------|---------|
| UserId | Guid | |

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
- Este comando no tiene slice propia — se ejecuta internamente desde las slices de login (LoginWithGoogle, LoginWithPassword) del agregado User.

#### Tests Unitarios (Dominio)

✅ Crear sesión con datos válidos
- Input: UserId="user-001-guid"
- Resultado: Session creada con TenantId=null, RoleId=null, Groups=[], IsOwner=false, ExpiresAt=CreatedAt+30días

❌ UserId vacío
- Input: UserId=""
- Resultado: ValidationException "UserId is required"

---

### 7.2 GetSessionById

#### Event Storming
```
🟡[Auth Service] → 🔵(GetSessionById) → 🟤[[Session]] → 📊 Session
```

#### Query interna

No es un endpoint REST. Es una operación interna del servicio de Auth que se ejecuta en cada request autenticada (consumida por ResolveAuth — sección 7.9).

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

### 7.3 ListSessionsByRoleAndTenant

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

### 7.4 ListSessionsByUserId

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

### 7.5 Session.SetTenantContext

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

#### Slice: PUT /auth/sessions/{id}/tenant

**Request**
```csharp
public record SetTenantContextRequest(
    Guid TenantId
);
```

> La slice recibe solo el `TenantId`. Internamente carga la Membership (userId + tenantId), obtiene el rol, y extrae Groups/AdditionalScopes/ExcludedScopes/IsOwner para pasarlos al comando de dominio.

**Response**: 204 No Content

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

#### Tests Integración

✅ 204 No Content

❌ 404 → Session no encontrada

❌ 404 → Membership no encontrada para ese tenant

❌ 422 → Validación fallida

---

### 7.6 Session.ClearTenantContext

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

#### Slice: DELETE /auth/sessions/{id}/tenant

**Response**: 204 No Content

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

#### Tests Integración

✅ 204 No Content

❌ 404 → Session no encontrada

---

### 7.7 Session.Refresh

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
- Se ejecuta internamente dentro de ResolveAuth (sección 7.9) en cada request válida. No tiene slice propia.
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
- Se ejecuta en logout voluntario (slice Logout del agregado User), invalidación por cambio de permisos, desactivación de Membership, o eliminación de Membership.
- La cookie `fudie_session` se limpia en la respuesta HTTP cuando es un logout voluntario.
- Para invalidación masiva (cambio de permisos de un rol), se usa `ListSessionsByRoleAndTenant` seguido de `Destroy` para cada sesión.
- No tiene slice propia — se ejecuta internamente desde otras slices.

#### Tests Unitarios (Dominio)

✅ Destruir sesión existente
- Resultado: Documento eliminado de Firestore

---

### 7.9 ResolveAuth

> El gateway llama a este endpoint en cada request autenticada. Es el punto donde la cookie o API Key se convierte en JWT efímero.

#### Event Storming
```
🟡[Gateway] → 🔵(ResolveAuth) → 🟤[[Session]] → 🟠<AuthResolved>
                                      │
                            🟣{SessionExists}
                            🟣{SessionNotExpired}
```

#### Input

No tiene input explícito. Lee los headers originales del cliente reenviados por el gateway:
- Cookie `fudie_session` → para administradores web
- Header `X-Api-Key` → para aplicaciones externas

#### Inyecta
- `ISessionRepository`
- `Session.Refresh`
- Servicio de firma JWT (clave privada ES256)

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| No hay cookie ni API Key | 401 | UnauthorizedGuard | "Authentication required" |
| Cookie → sesión no existe | 401 | UnauthorizedGuard | "Invalid session" |
| Cookie → sesión expirada | 401 | UnauthorizedGuard | "Session expired" |
| API Key → hash no existe en api_keys | 401 | UnauthorizedGuard | "Invalid API key" |
| API Key → api_key no activa | 401 | UnauthorizedGuard | "API key is inactive" |
| API Key → Membership no activa | 401 | UnauthorizedGuard | "Membership is inactive" |

#### Lógica
```
¿Tiene cookie fudie_session?
    │
    ├─ Sí → GetSessionById(sessionId)
    │         │
    │         ├─ ¿Existe? → No → 401
    │         ├─ ¿Expirada? → Sí → Destroy + 401
    │         └─ Session.Refresh() → sliding expiration
    │                │
    │                ▼
    │         Construye JWT efímero desde datos de la sesión
    │
    └─ ¿Tiene header X-Api-Key?
              │
              ├─ Sí → Hash SHA-256 → busca en api_keys/{hash}
              │         │
              │         ├─ ¿Existe y activa? → No → 401
              │         └─ Carga Membership → obtiene permisos del rol
              │                │
              │                ▼
              │         Construye JWT efímero desde datos de la Membership
              │
              └─ Ninguno → 401

Firma JWT con clave privada ES256 (vida útil: 45 segundos)
        │
        ▼
Devuelve JWT al gateway
```

**Construcción del JWT efímero:**

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

**JWT resultante — Sin tenant:**

```json
{
  "sub": "user-001-guid",
  "iat": 1738900800,
  "exp": 1738900845
}
```

**JWT resultante — Owner:**

```json
{
  "sub": "user-001-guid",
  "tid": "tenant-guid",
  "owner": true,
  "iat": 1738900800,
  "exp": 1738900845
}
```

**JWT resultante — Usuario con permisos:**

```json
{
  "sub": "user-001-guid",
  "tid": "tenant-guid",
  "groups": ["menu:read", "menu:write"],
  "add": ["reservation-service:CancelReservation"],
  "exc": ["menu-service:SetMenuDepositPolicy"],
  "iat": 1738900800,
  "exp": 1738900845
}
```

**Notas sobre el JWT efímero:**
- Se firma con clave privada ES256. Solo el servicio de Auth tiene la clave privada.
- Vida útil: 45 segundos. Nace, viaja al microservicio, se valida, muere.
- No se almacena en ningún sitio. No hay documento en Firestore. No hay caché.
- Los microservicios validan la firma con clave pública descargada del endpoint JWKS (sección 7.10).
- Clock skew recomendado: 5 segundos en `TokenValidationParameters`.

#### Slice: POST /auth/resolve

> Endpoint **interno**. Solo lo llama el gateway. No está expuesto al exterior.

**Request**

El gateway reenvía los headers originales del cliente. No hay body.

**Response**: 200 OK

```json
{
  "token": "eyJhbGciOiJFUzI1NiIs..."
}
```

**Response de error**: 401 Unauthorized

**Notas:**
- El gateway recibe el JWT y lo inyecta como header `Authorization: Bearer {token}` al reenviar la request al microservicio destino.
- Este endpoint no pasa por la librería de permisos — es el propio servicio de Auth resolviendo la autenticación.

#### Tests Unitarios (Servicio)

> Estado previo: `TestableSession` con diferentes configuraciones.

✅ Resolve con cookie válida — sesión con tenant
- Precondición: Session con TenantId, Groups=["menu:read"]
- Resultado: JWT con sub, tid, groups

✅ Resolve con cookie válida — sesión sin tenant
- Precondición: Session con TenantId=null
- Resultado: JWT con sub, sin tid ni permisos

✅ Resolve con cookie válida — sesión Owner
- Precondición: Session con IsOwner=true
- Resultado: JWT con sub, tid, owner=true, sin arrays de permisos

✅ Resolve con API Key válida
- Precondición: api_keys/{hash} existe y activa, Membership activa
- Resultado: JWT con sub, tid, permisos del rol de la Membership

❌ Sin cookie ni API Key
- Resultado: 401 "Authentication required"

❌ Cookie con sesión inexistente
- Resultado: 401 "Invalid session"

❌ Cookie con sesión expirada
- Resultado: 401 "Session expired", sesión destruida

❌ API Key inválida
- Resultado: 401 "Invalid API key"

❌ API Key con Membership inactiva
- Resultado: 401 "Membership is inactive"

#### Tests Integración

✅ 200 OK → `{ "token": "eyJ..." }` con cookie válida

✅ 200 OK → `{ "token": "eyJ..." }` con API Key válida

❌ 401 → Sin credenciales

❌ 401 → Cookie inválida

❌ 401 → API Key inválida

---

### 7.10 GetJwks

> Endpoint estándar JWKS. Los microservicios descargan la clave pública de aquí al arrancar para validar las firmas de los JWT efímeros.

#### Event Storming
```
🟡[Microservicio] → 🔵(GetJwks) → 📊 JwksResponse
```

#### Input

Ninguno.

#### Guards

Ninguno.

#### Slice: GET /auth/jwks

> Endpoint **público**. `.AllowAnonymous()`.

**Response**: 200 OK

```json
{
  "keys": [
    {
      "kty": "EC",
      "crv": "P-256",
      "x": "...",
      "y": "...",
      "kid": "auth-key-001",
      "use": "sig",
      "alg": "ES256"
    }
  ]
}
```

**Notas:**
- `kid` (Key ID) permite rotación de claves: se publica la clave nueva junto a la anterior, los microservicios validan contra ambas, y cuando ya no hay JWTs en vuelo con la clave vieja se retira.
- Los microservicios configuran `TokenValidationParameters` con `IssuerSigningKeyResolver` apuntando a esta URL.
- Se cachea al arrancar y se refresca periódicamente (ej: cada hora) sin reiniciar el microservicio.
- Clock skew recomendado: 5 segundos en `TokenValidationParameters.ClockSkew`.

#### Tests Integración

✅ 200 OK → JWKS con al menos una clave ES256

✅ La clave pública puede verificar un JWT firmado por el servicio de Auth

---

## 8. Descripciones de Permisos

> Las descripciones son **responsabilidad de producto**. Se definen en español durante la sesión de diseño. Claude Code genera el archivo de descripciones del microservicio con el español como base y traduce automáticamente al resto de idiomas necesarios.
>
> Deben ser claras, concisas y comprensibles para alguien sin conocimientos técnicos — es lo que el administrador del restaurante ve cuando configura roles.

### Scopes atómicos

| Scope (nombre de clase) | Descripción (es) |
|--------------------------|-------------------|
| `SetTenantContext` | Seleccionar el restaurante activo en la sesión |
| `ClearTenantContext` | Desconectarse del restaurante activo |

> `ResolveAuth` y `GetJwks` no generan scopes atómicos — son endpoints de infraestructura (`AllowAnonymous` / internos del gateway).
>
> `Session.Create`, `Session.Refresh`, `Session.Destroy`, `GetSessionById`, `ListSessionsByRoleAndTenant` y `ListSessionsByUserId` no generan scopes atómicos — son operaciones internas del servicio de Auth que no tienen endpoint REST propio.

### Agrupaciones custom

No aplica.

> Las agrupaciones automáticas (`session:read` y `session:write`) no se definen aquí — se generan por reflexión a partir del verbo HTTP.

---

## 9. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | PUT | /auth/sessions/{id}/tenant | Session.SetTenantContext | 204 |
| 2 | DELETE | /auth/sessions/{id}/tenant | Session.ClearTenantContext | 204 |
| 3 | POST | /auth/resolve | ResolveAuth | 200 → `{ "token": "..." }` |
| 4 | GET | /auth/jwks | GetJwks | 200 → JWKS |

> Los comandos Session.Create, Session.Refresh y Session.Destroy no tienen endpoint propio — se ejecutan internamente desde las slices de login/logout del agregado User y desde operaciones del sistema (invalidación por cambio de permisos).
>
> Las queries GetSessionById, ListSessionsByRoleAndTenant y ListSessionsByUserId son operaciones internas del repositorio.

---

## 10. Persistencia (Firestore)

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

### Documento Ejemplo — Sin tenant

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "userId": "user-001-guid",
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
  "userId": "user-001-guid",
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
  "userId": "user-002-guid",
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

## 11. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | Limpieza de sesiones expiradas | Decidido: TTL policy nativa de Firestore sobre el campo `expiresAt`. Firestore elimina automáticamente los documentos expirados (dentro de 24h). Sin Cloud Functions ni limpieza manual. |
| 2 | Usuario de plataforma Fudie (superadmin y equipo) — ¿necesita TenantId? | Decidido: TenantId null. Los permisos de plataforma se resuelven por Groups sin `tid` en el JWT. |
| 3 | Auto-selección de tenant cuando el usuario solo tiene una Membership | Decidido: sí. Si solo tiene una Membership, se ejecuta SetTenantContext automáticamente sin paso de UI. |

---

**Fecha**: 2026-02-09  
**Autor**: Equipo Fudie
