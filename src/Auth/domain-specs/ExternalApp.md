# Domain Specification: ExternalApp

---

## 1. Enums

### InvitationStatus

```csharp
public enum InvitationStatus
{
    Pending,
    Accepted,
    Cancelled
}
```

> Mismo enum que Membership. Se define en un namespace compartido.

---

## 2. Value Objects

*No hay value objects en este agregado. Todos los campos son tipos primitivos.*

---

## 3. Aggregate: ExternalApp

### Estructura

```
ExternalApp (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ User: User?
├─ Name: string
├─ IsActive: bool
├─ InvitationEmail: string
├─ InvitationStatus: InvitationStatus
├─ ApiKeyHash: string?
├─ ApiKeyPrefix: string?
├─ ApiKeyExpiresAt: DateTime?
├─ Groups: IReadOnlyCollection<string>
├─ AdditionalScopes: IReadOnlyCollection<string>
└─ ExcludedScopes: IReadOnlyCollection<string>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | init |
| User | User? | protected set |
| Name | string | protected set |
| IsActive | bool | protected set |
| InvitationEmail | string | init |
| InvitationStatus | InvitationStatus | protected set |
| ApiKeyHash | string? | protected set |
| ApiKeyPrefix | string? | protected set |
| ApiKeyExpiresAt | DateTime? | protected set |

#### Colecciones

```csharp
protected HashSet<string> _groups = [];
public IReadOnlyCollection<string> Groups => _groups.ToList().AsReadOnly();

protected HashSet<string> _additionalScopes = [];
public IReadOnlyCollection<string> AdditionalScopes => _additionalScopes.ToList().AsReadOnly();

protected HashSet<string> _excludedScopes = [];
public IReadOnlyCollection<string> ExcludedScopes => _excludedScopes.ToList().AsReadOnly();
```

### Invariantes (Validator)

> Estas reglas se implementan en `ExternalAppValidator : AbstractValidator<ExternalApp>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "TenantId is required" |
| Name | NotEmpty | "Name is required" |
| Name | MaxLength(150) | "Name cannot exceed 150 characters" |
| InvitationEmail | NotEmpty | "Invitation email is required" |
| InvitationEmail | MaxLength(254) | "Invitation email cannot exceed 254 characters" |
| InvitationEmail | ValidEmail | "Invitation email must be a valid email address" |
| InvitationStatus | IsInEnum | "Invitation status must be a valid status" |
| ApiKeyPrefix | MaxLength(8) | "API key prefix cannot exceed 8 characters" |

---

## 4. Response

```csharp
public record ExternalAppResponse(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    string Name,
    bool IsActive,
    string InvitationEmail,
    string InvitationStatus,
    string? ApiKeyPrefix,
    DateTime? ApiKeyExpiresAt,
    IReadOnlyCollection<string> Groups,
    IReadOnlyCollection<string> AdditionalScopes,
    IReadOnlyCollection<string> ExcludedScopes
);
```

> `ApiKeyHash` nunca se expone en el Response.

---

## 5. Event Storming - Leyenda

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

## 6. Comandos

> ⚠️ **IMPORTANTE**: El orden de los comandos respeta las dependencias.
> - ExternalApp.Create es la invitación — nace con InvitationStatus=Pending, User=null y sin API Key
> - AcceptInvitation va después (vincula User y genera API Key)
> - Las Queries van después
> - Update, UpdatePermissions van después
> - RotateApiKey va después (solo el developer)
> - Activate/Deactivate van después
> - Delete va al final

> **Tests de dominio**: Usar `TestableExternalApp` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableExternalApp` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 6.1 ExternalApp.Create

> Crear es invitar al desarrollador. Nace con InvitationStatus=Pending, User=null y sin API Key.

#### Event Storming
```
🟡[Owner] → 🔵(CreateExternalApp) → 🟤[[ExternalApp]] → 🟠<ExternalAppCreated>
                                         │
                                   🟣{UniqueEmail} ← slice
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| InvitationEmail | string |

#### Inyecta
- `IValidator<ExternalApp>`

#### Guards (dominio)

Ninguno. Las validaciones de datos las hace el Validator.

#### Guards (slice)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya existe external app con ese email en el tenant | 409 | ConflictGuard | "An external app with this email already exists in this tenant" |

#### Lógica
```csharp
var externalApp = new ExternalApp(Guid.NewGuid())
{
    TenantId = command.TenantId,
    User = null,
    Name = command.Name,
    IsActive = true,
    InvitationEmail = command.InvitationEmail,
    InvitationStatus = InvitationStatus.Pending,
    ApiKeyHash = null,
    ApiKeyPrefix = null,
    ApiKeyExpiresAt = null
};

return externalAppValidator.ValidateOrThrow(externalApp);
```

#### Slice: POST /external-apps

**Guards de slice**
```csharp
var duplicate = await externalAppRepository.ExistsByEmailAndTenantAsync(
    request.InvitationEmail, tenantId);
ConflictGuard.ThrowIf(duplicate, "An external app with this email already exists in this tenant");
```

**Request**
```csharp
public record CreateExternalAppRequest(
    string Name,
    string InvitationEmail
);
```

**Response**: 201 Created → `ExternalAppResponse`

#### Tests Unitarios (Dominio)

✅ Crear external app con datos válidos
- Input: Name="TPV MiSoftware", InvitationEmail="dev@misoftware.com"
- Resultado: InvitationStatus=Pending, User=null, ApiKeyHash=null, IsActive=true

❌ Name vacío → ValidationException
❌ Email vacío → ValidationException
❌ Email inválido → ValidationException

#### Tests Unitarios (Servicio)

✅ Verifica unicidad de email
✅ Añade la external app al repositorio
✅ Guarda los cambios
✅ Retorna Response mapeado correctamente
❌ Email duplicado → 409

#### Tests Integración

✅ 201 Created → ExternalAppResponse con InvitationStatus=Pending
❌ 409 → Email duplicado
❌ 422 → Validación fallida

---

### 6.2 ExternalApp.AcceptInvitation

> El desarrollador hace login con Google OAuth. Se vincula su User y se genera la API Key.

#### Event Storming
```
🟡[Developer] → 🔵(AcceptInvitation) → 🟤[[ExternalApp]] → 🟠<ExternalAppInvitationAccepted>
                                            │
                                      🟣{IsPending}
```

#### Input

| Campo | Tipo |
|-------|------|
| User | User |

#### Inyecta
- `IValidator<ExternalApp>`

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Invitación no está en Pending | 409 | ConflictGuard | "Invitation is not pending" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    externalApp.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

externalApp.User = command.User;
externalApp.InvitationStatus = InvitationStatus.Accepted;

var apiKey = $"fud_{CryptoRandom.GenerateString(32)}";
externalApp.ApiKeyHash = SHA256.HashString(apiKey);
externalApp.ApiKeyPrefix = apiKey[..8];

return (externalAppValidator.ValidateOrThrow(externalApp), apiKey);
```

> La API Key en claro se devuelve **una sola vez** en el Response. Fudie solo almacena el hash.

#### Slice: POST /external-apps/{externalAppId}/accept

> Endpoint público. El userId se obtiene del JWT.

```csharp
var userId = httpContext.User.GetUserId();
var externalAppId = route.Get<Guid>("externalAppId");
```

**Response**: 200 OK

```json
{
  "apiKey": "fud_a3K9mZ3pX7nR2wQ8vB4cD6eF1gH5jK",
  "prefix": "fud_a3K9",
  "message": "Store this key securely. It will not be shown again."
}
```

#### Tests Unitarios (Dominio)

✅ Aceptar invitación pendiente
- Precondición: InvitationStatus=Pending, User=null
- Input: User=valid
- Resultado: InvitationStatus=Accepted, User vinculado, ApiKeyHash generado, ApiKeyPrefix generado

❌ Invitación ya aceptada → ConflictException
❌ Invitación cancelada → ConflictException

#### Tests Integración

✅ 200 OK → Response con apiKey en claro (única vez)
❌ 404 → No encontrada
❌ 409 → No está pendiente

---

### 6.3 ExternalApp.CancelInvitation

#### Event Storming
```
🟡[Owner] → 🔵(CancelInvitation) → 🟤[[ExternalApp]] → 🟠<ExternalAppInvitationCancelled>
                                        │
                                  🟣{IsPending}
```

#### Input

*Sin input adicional*

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Invitación no está en Pending | 409 | ConflictGuard | "Invitation is not pending" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    externalApp.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

externalApp.InvitationStatus = InvitationStatus.Cancelled;

return externalAppValidator.ValidateOrThrow(externalApp);
```

#### Slice: POST /external-apps/{externalAppId}/cancel-invitation

**Response**: 200 OK → `ExternalAppResponse`

#### Tests Unitarios (Dominio)

✅ Cancelar invitación pendiente → InvitationStatus=Cancelled
❌ Invitación no pendiente → ConflictException

#### Tests Integración

✅ 200 OK → ExternalAppResponse con InvitationStatus=Cancelled
❌ 404 → No encontrada
❌ 409 → No está pendiente

---

### 6.4 ExternalApp.ResendInvitation

> Solo dispara el evento. No cambia estado.

#### Event Storming
```
🟡[Owner] → 🔵(ResendInvitation) → 🟤[[ExternalApp]] → 🟠<ExternalAppInvitationResent>
                                        │
                                  🟣{IsPending}
```

#### Input

*Sin input adicional*

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Invitación no está en Pending | 409 | ConflictGuard | "Invitation is not pending" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    externalApp.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

// No cambia estado — solo lanza evento ExternalAppInvitationResent
```

#### Slice: POST /external-apps/{externalAppId}/resend-invitation

**Response**: 200 OK

#### Tests Unitarios (Dominio)

✅ Reenviar invitación pendiente → evento lanzado
❌ Invitación no pendiente → ConflictException

#### Tests Integración

✅ 200 OK
❌ 404 → No encontrada
❌ 409 → No está pendiente

---

### 6.5 ListExternalApps

#### Slice: GET /external-apps

> El tenantId se obtiene del JWT.

**Response**: 200 OK → `ExternalAppResponse[]`

#### Tests Unitarios (Servicio)

✅ Obtiene external apps del tenantId del JWT
✅ Retorna lista mapeada correctamente

#### Tests Integración

✅ 200 OK → Lista de ExternalAppResponse
✅ 200 OK → Lista vacía si no hay external apps

---

### 6.6 GetExternalApp

#### Slice: GET /external-apps/{externalAppId}

**Response**: 200 OK → `ExternalAppResponse`

#### Tests Integración

✅ 200 OK → ExternalAppResponse
❌ 404 → No encontrada

---

### 6.7 ExternalApp.Update

> Actualiza el nombre de la aplicación externa.

#### Event Storming
```
🟡[Owner] → 🔵(UpdateExternalApp) → 🟤[[ExternalApp]] → 🟠<ExternalAppUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |

#### Inyecta
- `IValidator<ExternalApp>`

#### Guards (dominio)

Ninguno.

#### Lógica
```csharp
externalApp.Name = command.Name;

return externalAppValidator.ValidateOrThrow(externalApp);
```

#### Slice: PUT /external-apps/{externalAppId}

**Request**
```csharp
public record UpdateExternalAppRequest(
    string Name
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar nombre
❌ Name vacío → ValidationException

#### Tests Integración

✅ 204 No Content
❌ 404 → No encontrada
❌ 422 → Validación fallida

---

### 6.8 ExternalApp.UpdatePermissions

> El Owner configura los permisos de la aplicación externa. Permisos individuales, no compartidos con otras apps.

#### Event Storming
```
🟡[Owner] → 🔵(UpdatePermissions) → 🟤[[ExternalApp]] → 🟠<ExternalAppPermissionsUpdated>
```

#### Input

| Campo | Tipo |
|-------|------|
| Groups | List\<string\> |
| AdditionalScopes | List\<string\> |
| ExcludedScopes | List\<string\> |

#### Inyecta
- `IValidator<ExternalApp>`

#### Guards (dominio)

Ninguno.

#### Lógica
```csharp
externalApp._groups = command.Groups.ToHashSet();
externalApp._additionalScopes = command.AdditionalScopes.ToHashSet();
externalApp._excludedScopes = command.ExcludedScopes.ToHashSet();

return externalAppValidator.ValidateOrThrow(externalApp);
```

#### Slice: PUT /external-apps/{externalAppId}/permissions

**Request**
```csharp
public record UpdateExternalAppPermissionsRequest(
    List<string> Groups,
    List<string> AdditionalScopes,
    List<string> ExcludedScopes
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar permisos
✅ Actualizar con exclusiones
✅ Sin duplicados en colecciones (HashSet)

#### Tests Integración

✅ 204 No Content
❌ 404 → No encontrada

---

### 6.9 ExternalApp.RotateApiKey

> El desarrollador regenera su API Key. La key anterior queda inválida inmediatamente.

#### Event Storming
```
🟡[Developer] → 🔵(RotateApiKey) → 🟤[[ExternalApp]] → 🟠<ApiKeyRotated>
                                        │
                                  🟣{IsAccepted}
                                  🟣{IsActive}
```

#### Input

*Sin input adicional*

#### Inyecta
- `IValidator<ExternalApp>`

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Invitación no está aceptada | 409 | ConflictGuard | "External app invitation has not been accepted" |
| External app no está activa | 409 | ConflictGuard | "Cannot rotate key for inactive external app" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    externalApp.InvitationStatus != InvitationStatus.Accepted,
    "External app invitation has not been accepted");

ConflictGuard.ThrowIf(
    !externalApp.IsActive,
    "Cannot rotate key for inactive external app");

var apiKey = $"fud_{CryptoRandom.GenerateString(32)}";
externalApp.ApiKeyHash = SHA256.HashString(apiKey);
externalApp.ApiKeyPrefix = apiKey[..8];

return (externalAppValidator.ValidateOrThrow(externalApp), apiKey);
```

#### Slice: POST /external-apps/{externalAppId}/rotate-api-key

**Response**: 200 OK

```json
{
  "apiKey": "fud_b7R2nW5qY9mT4xK8vC3dF6eG1hJ5kL",
  "prefix": "fud_b7R2",
  "message": "Store this key securely. It will not be shown again."
}
```

#### Tests Unitarios (Dominio)

✅ Rotar API Key de external app activa y aceptada
❌ Invitación no aceptada → ConflictException
❌ External app inactiva → ConflictException

#### Tests Integración

✅ 200 OK → Response con nueva apiKey en claro
❌ 404 → No encontrada
❌ 409 → No aceptada o inactiva

---

### 6.10 ExternalApp.Deactivate

> Al desactivar, la API Key queda inválida. Las requests con esa key son rechazadas.

#### Event Storming
```
🟡[Owner] → 🔵(DeactivateExternalApp) → 🟤[[ExternalApp]] → 🟠<ExternalAppDeactivated>
```

#### Input

*Sin input adicional*

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactiva | 409 | ConflictGuard | "External app is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!externalApp.IsActive, "External app is already inactive");

externalApp.IsActive = false;

return externalAppValidator.ValidateOrThrow(externalApp);
```

#### Slice: POST /external-apps/{externalAppId}/deactivate

**Response**: 200 OK → `ExternalAppResponse`

#### Tests Unitarios (Dominio)

✅ Desactivar external app activa → IsActive=false
❌ Ya inactiva → ConflictException

#### Tests Integración

✅ 200 OK → ExternalAppResponse con IsActive=false
❌ 404 → No encontrada
❌ 409 → Ya inactiva

---

### 6.11 ExternalApp.Activate

> Al activar, la API Key vuelve a ser válida. No se genera nueva key.

#### Event Storming
```
🟡[Owner] → 🔵(ActivateExternalApp) → 🟤[[ExternalApp]] → 🟠<ExternalAppActivated>
```

#### Input

*Sin input adicional*

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activa | 409 | ConflictGuard | "External app is already active" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(externalApp.IsActive, "External app is already active");

externalApp.IsActive = true;

return externalAppValidator.ValidateOrThrow(externalApp);
```

#### Slice: POST /external-apps/{externalAppId}/activate

**Response**: 200 OK → `ExternalAppResponse`

#### Tests Unitarios (Dominio)

✅ Activar external app inactiva → IsActive=true
❌ Ya activa → ConflictException

#### Tests Integración

✅ 200 OK → ExternalAppResponse con IsActive=true
❌ 404 → No encontrada
❌ 409 → Ya activa

---

### 6.12 ExternalApp.Delete

#### Event Storming
```
🟡[Owner] → 🔵(DeleteExternalApp) → 🟤[[ExternalApp]] → 🟠<ExternalAppDeleted>
```

#### Input

*Sin input adicional*

#### Guards (dominio)

Ninguno.

#### Lógica
```csharp
externalAppRepository.Delete(externalApp);
```

#### Slice: DELETE /external-apps/{externalAppId}

**Response**: 204 No Content

#### Tests Integración

✅ 204 No Content
❌ 404 → No encontrada

---

## 7. Descripciones de Permisos

> Las descripciones son **responsabilidad de producto**. Se definen en español durante la sesión de diseño. Claude Code genera el archivo de descripciones del microservicio con el español como base y traduce automáticamente al resto de idiomas necesarios.
>
> Deben ser claras, concisas y comprensibles para alguien sin conocimientos técnicos — es lo que el administrador ve cuando configura roles.

### Scopes atómicos

| Scope (nombre de clase) | Descripción (es) |
|--------------------------|-------------------|
| `CreateExternalApp` | Conectar una nueva aplicación externa |
| `ListExternalApps` | Ver la lista de aplicaciones conectadas |
| `GetExternalApp` | Ver los detalles de una aplicación conectada |
| `UpdateExternalApp` | Modificar el nombre de una aplicación externa |
| `UpdateExternalAppPermissions` | Configurar los permisos de una aplicación externa |
| `CancelExternalAppInvitation` | Cancelar una invitación pendiente de aplicación externa |
| `ResendExternalAppInvitation` | Reenviar una invitación pendiente de aplicación externa |
| `DeactivateExternalApp` | Desactivar una aplicación externa |
| `ActivateExternalApp` | Reactivar una aplicación externa desactivada |
| `DeleteExternalApp` | Eliminar una aplicación externa |

> `AcceptInvitation` y `RotateApiKey` no generan scope — los ejecuta el desarrollador, no el Owner.

---

## 8. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /external-apps | ExternalApp.Create | 201 → `ExternalAppResponse` |
| 2 | POST | /external-apps/{id}/accept | ExternalApp.AcceptInvitation | 200 → ApiKey |
| 3 | POST | /external-apps/{id}/cancel-invitation | ExternalApp.CancelInvitation | 200 → `ExternalAppResponse` |
| 4 | POST | /external-apps/{id}/resend-invitation | ExternalApp.ResendInvitation | 200 |
| 5 | GET | /external-apps | ListExternalApps | 200 → `ExternalAppResponse[]` |
| 6 | GET | /external-apps/{id} | GetExternalApp | 200 → `ExternalAppResponse` |
| 7 | PUT | /external-apps/{id} | ExternalApp.Update | 204 |
| 8 | PUT | /external-apps/{id}/permissions | ExternalApp.UpdatePermissions | 204 |
| 9 | POST | /external-apps/{id}/rotate-api-key | ExternalApp.RotateApiKey | 200 → ApiKey |
| 10 | POST | /external-apps/{id}/deactivate | ExternalApp.Deactivate | 200 → `ExternalAppResponse` |
| 11 | POST | /external-apps/{id}/activate | ExternalApp.Activate | 200 → `ExternalAppResponse` |
| 12 | DELETE | /external-apps/{id} | ExternalApp.Delete | 204 |

---

## 9. Persistencia (Firestore)

### Colección

`/external_apps/{externalAppId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<ExternalApp>(entity =>
{
    // QueryFilter: multi-tenancy by TenantId
    entity.HasQueryFilter(x => x.TenantId == tenantId);

    // Reference: User → User (nullable)
    entity.Reference(x => x.User);

    // Backing fields: HashSet<string> para evitar duplicados
    entity.Property(x => x.Groups)
        .HasBackingField("_groups");

    entity.Property(x => x.AdditionalScopes)
        .HasBackingField("_additionalScopes");

    entity.Property(x => x.ExcludedScopes)
        .HasBackingField("_excludedScopes");
});
```

### Documento Ejemplo

```json
{
  "id": "ext-001-guid",
  "tenantId": "tenant-001-guid",
  "user": null,
  "name": "TPV MiSoftware",
  "isActive": true,
  "invitationEmail": "dev@misoftware.com",
  "invitationStatus": "Pending",
  "apiKeyHash": null,
  "apiKeyPrefix": null,
  "apiKeyExpiresAt": null,
  "groups": [],
  "additionalScopes": [],
  "excludedScopes": []
}
```

```json
{
  "id": "ext-002-guid",
  "tenantId": "tenant-001-guid",
  "user": "users/user-dev-guid",
  "name": "TPV MiSoftware",
  "isActive": true,
  "invitationEmail": "dev@misoftware.com",
  "invitationStatus": "Accepted",
  "apiKeyHash": "a1b2c3d4e5f6...sha256hash",
  "apiKeyPrefix": "fud_a3K9",
  "apiKeyExpiresAt": null,
  "groups": ["menu:read", "reservation:read"],
  "additionalScopes": [],
  "excludedScopes": []
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Límite de external apps por tenant? | Pendiente |
| 2 | ¿Rate limiting específico por API Key? | Pendiente |
| 3 | ¿Se necesita campo lastUsedAt para la API Key? | Pendiente |
| 4 | ¿Expiración automática de API Keys? | Pendiente |
| 5 | ¿Webhook de notificaciones hacia la app externa? | Pendiente: Fuera de alcance actual |

---

**Fecha**: 2026-02-11
**Autor**: Equipo Fudie
