# Domain Specification: Membership

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

---

## 2. Value Objects

*No hay value objects en este agregado. Todos los campos son tipos primitivos.*

---

## 3. Aggregate: Membership

### Estructura

```
Membership (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ UserId: string?
├─ Role: TenantRole
├─ IsActive: bool
├─ InvitationEmail: string
└─ InvitationStatus: InvitationStatus
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | init |
| UserId | string? | protected set |
| Role | TenantRole | protected set |
| IsActive | bool | protected set |
| InvitationEmail | string | init |
| InvitationStatus | InvitationStatus | protected set |

### Invariantes (Validator)

> Estas reglas se implementan en `MembershipValidator : AbstractValidator<Membership>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "TenantId is required" |
| Role | NotNull | "Role is required" |
| InvitationEmail | NotEmpty | "Invitation email is required" |
| InvitationEmail | MaxLength(254) | "Invitation email cannot exceed 254 characters" |
| InvitationEmail | ValidEmail | "Invitation email must be a valid email address" |
| InvitationStatus | IsInEnum | "Invitation status must be a valid status" |
| UserId | MaxLength(100) | "User ID cannot exceed 100 characters" |

---

## 4. Response

```csharp
public record MembershipResponse(
    Guid Id,
    Guid TenantId,
    string? UserId,
    Guid RoleId,
    bool IsActive,
    string InvitationEmail,
    string InvitationStatus
);
```

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
> - Membership.Create es la invitación — nace con InvitationStatus=Pending y UserId=null
> - AcceptInvitation va después (vincula UserId)
> - Las Queries van después
> - ChangeRole, Deactivate/Reactivate van después
> - Delete va al final

> **Tests de dominio**: Usar `TestableMembership` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableMembership` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 6.1 Membership.Create

> Crear es invitar. El membership nace con InvitationStatus=Pending y UserId=null. Se lanza el evento MembershipCreated que dispara el envío del email de invitación.

#### Event Storming
```
🟡[Owner] → 🔵(CreateMembership) → 🟤[[Membership]] → 🟠<MembershipCreated>
                                        │
                                  🟣{UniqueMembership}
                                  🟣{RoleExists}
                                  🟣{RoleIsHuman}
```

#### Input

| Campo | Tipo |
|-------|------|
| InvitationEmail | string |
| RoleId | Guid |

#### Inyecta
- `IValidator<Membership>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya existe membership con ese email en el tenant | 409 | ConflictGuard | "A membership with this email already exists in this tenant" |
| El rol no existe | 404 | NotFoundGuard | "Role not found" |
| El rol es ExternalApp | 409 | ConflictGuard | "Cannot assign ExternalApp role to a membership" |

#### Lógica
```csharp
var role = await tenantRoleRepository.GetByIdAsync(command.RoleId);

NotFoundGuard.ThrowIfNull(role, command.RoleId);

ConflictGuard.ThrowIf(
    role!.Name == "ExternalApp",
    "Cannot assign ExternalApp role to a membership");

var duplicate = await membershipRepository.ExistsByEmailAndTenantAsync(
    command.InvitationEmail, command.TenantId);

ConflictGuard.ThrowIf(duplicate, "A membership with this email already exists in this tenant");

var membership = new Membership(Guid.NewGuid())
{
    TenantId = command.TenantId,
    UserId = null,
    RoleId = command.RoleId,
    IsActive = true,
    InvitationEmail = command.InvitationEmail,
    InvitationStatus = InvitationStatus.Pending
};

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships

**Request**
```csharp
public record CreateMembershipRequest(
    string InvitationEmail,
    Guid RoleId
);
```

**Response**: 201 Created → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Crear membership con datos válidos
- Input: InvitationEmail="maria@ejemplo.com", RoleId=valid (Manager)
- Resultado: Membership con InvitationStatus=Pending, UserId=null, IsActive=true

✅ Crear membership con rol custom
- Input: RoleId=valid (Sommelier)
- Resultado: Membership creado

❌ Email vacío
- Input: InvitationEmail=""
- Resultado: ValidationException "Invitation email is required"

❌ Email inválido
- Input: InvitationEmail="no-es-email"
- Resultado: ValidationException "Invitation email must be a valid email address"

❌ RoleId vacío
- Input: RoleId=Guid.Empty
- Resultado: ValidationException "RoleId is required"

❌ Rol no existe
- Input: RoleId=inexistente
- Resultado: KeyNotFoundException "Role not found"

❌ Rol es ExternalApp
- Input: RoleId=valid (ExternalApp)
- Resultado: ConflictException "Cannot assign ExternalApp role to a membership"

❌ Membership duplicado (mismo email + tenant)
- Precondición: Ya existe membership con email="maria@ejemplo.com" en el tenant
- Input: InvitationEmail="maria@ejemplo.com"
- Resultado: ConflictException "A membership with this email already exists in this tenant"

#### Tests Unitarios (Servicio)

✅ Verifica que el rol existe
- Verifica que tenantRoleRepository.GetByIdAsync es llamado con roleId

✅ Verifica unicidad antes de crear
- Verifica que membershipRepository.ExistsByEmailAndTenantAsync es llamado

✅ Añade el membership al repositorio
- Verifica que repository.Add es llamado con el membership creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del membership

#### Tests Integración

✅ 201 Created → MembershipResponse con InvitationStatus=Pending

❌ 404 → Rol no encontrado

❌ 409 → Duplicado o rol ExternalApp

❌ 422 → Validación fallida

---

### 6.2 Membership.AcceptInvitation

> El invitado hace login con Google OAuth. El sistema vincula su userId al membership y cambia InvitationStatus a Accepted.

#### Event Storming
```
🟡[Invitado] → 🔵(AcceptInvitation) → 🟤[[Membership]] → 🟠<InvitationAccepted>
                                           │
                                     🟣{IsPending}
```

#### Input

| Campo | Tipo |
|-------|------|
| UserId | string |

#### Inyecta
- `IValidator<Membership>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Invitación no está en Pending | 409 | ConflictGuard | "Invitation is not pending" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    membership.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

membership.UserId = command.UserId;
membership.InvitationStatus = InvitationStatus.Accepted;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/accept

**Request**

> El UserId se obtiene del JWT (Google OAuth). No se envía en el body.

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Aceptar invitación pendiente
- Precondición: Membership con InvitationStatus=Pending, UserId=null
- Input: UserId="google-oauth2|123456789"
- Resultado: Membership con InvitationStatus=Accepted, UserId="google-oauth2|123456789"

❌ Invitación ya aceptada
- Precondición: Membership con InvitationStatus=Accepted
- Resultado: ConflictException "Invitation is not pending"

❌ Invitación cancelada
- Precondición: Membership con InvitationStatus=Cancelled
- Resultado: ConflictException "Invitation is not pending"

#### Tests Integración

✅ 200 OK → MembershipResponse con InvitationStatus=Accepted

❌ 404 → No encontrado

❌ 409 → No está pendiente

---

### 6.3 Membership.CancelInvitation

> El Owner cancela una invitación pendiente.

#### Event Storming
```
🟡[Owner] → 🔵(CancelInvitation) → 🟤[[Membership]] → 🟠<InvitationCancelled>
                                        │
                                  🟣{IsPending}
```

#### Input

*Sin input adicional*

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Invitación no está en Pending | 409 | ConflictGuard | "Invitation is not pending" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    membership.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

membership.InvitationStatus = InvitationStatus.Cancelled;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/cancel-invitation

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Cancelar invitación pendiente
- Precondición: Membership con InvitationStatus=Pending
- Resultado: Membership con InvitationStatus=Cancelled

❌ Invitación ya aceptada
- Precondición: Membership con InvitationStatus=Accepted
- Resultado: ConflictException "Invitation is not pending"

❌ Invitación ya cancelada
- Precondición: Membership con InvitationStatus=Cancelled
- Resultado: ConflictException "Invitation is not pending"

#### Tests Integración

✅ 200 OK → MembershipResponse con InvitationStatus=Cancelled

❌ 404 → No encontrado

❌ 409 → No está pendiente

---

### 6.4 Membership.ResendInvitation

> El Owner reenvía la invitación. Solo dispara el evento para que se envíe el email de nuevo. No cambia estado.

#### Event Storming
```
🟡[Owner] → 🔵(ResendInvitation) → 🟤[[Membership]] → 🟠<InvitationResent>
                                        │
                                  🟣{IsPending}
```

#### Input

*Sin input adicional*

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Invitación no está en Pending | 409 | ConflictGuard | "Invitation is not pending" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    membership.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

// No cambia estado — solo lanza evento InvitationResent
```

#### Slice: POST /memberships/{membershipId}/resend-invitation

**Response**: 200 OK

#### Tests Unitarios (Dominio)

✅ Reenviar invitación pendiente
- Precondición: Membership con InvitationStatus=Pending
- Resultado: Sin cambio de estado, evento lanzado

❌ Invitación no pendiente
- Precondición: Membership con InvitationStatus=Accepted
- Resultado: ConflictException "Invitation is not pending"

#### Tests Integración

✅ 200 OK

❌ 404 → No encontrado

❌ 409 → No está pendiente

---

### 6.5 ListMemberships

**Slice**: GET /memberships

> El tenantId se obtiene del JWT.

**Response**: 200 OK → `MembershipResponse[]`

#### Tests Unitarios (Servicio)

✅ Obtiene memberships del tenantId del JWT
- Verifica que repository.GetByTenantIdAsync es llamado con el tenantId del token

✅ Retorna lista mapeada correctamente
- Verifica que el Response contiene todos los memberships del tenant

#### Tests Integración

✅ 200 OK → Lista de MembershipResponse

✅ 200 OK → Lista vacía si no hay memberships

---

### 6.6 GetMembership

**Slice**: GET /memberships/{membershipId}

**Response**: 200 OK → `MembershipResponse`

#### Tests Integración

✅ 200 OK → MembershipResponse

❌ 404 → No encontrada

---

### 6.7 Membership.ChangeRole

#### Event Storming
```
🟡[Owner] → 🔵(ChangeRole) → 🟤[[Membership]] → 🟠<MembershipRoleChanged>
                                   │
                             🟣{RoleExists}
                             🟣{RoleIsHuman}
                             🟣{NotOwnerRole}
```

#### Input

| Campo | Tipo |
|-------|------|
| RoleId | Guid |

#### Inyecta
- `IValidator<Membership>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Mismo rol actual | 409 | ConflictGuard | "Membership already has this role" |
| El rol no existe | 404 | NotFoundGuard | "Role not found" |
| El rol es ExternalApp | 409 | ConflictGuard | "Cannot assign ExternalApp role to a membership" |
| El membership actual es Owner y es el único | 409 | ConflictGuard | "Cannot change role of the only owner" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    membership.RoleId == command.RoleId,
    "Membership already has this role");

var newRole = await tenantRoleRepository.GetByIdAsync(command.RoleId);

NotFoundGuard.ThrowIfNull(newRole, command.RoleId);

ConflictGuard.ThrowIf(
    newRole!.Name == "ExternalApp",
    "Cannot assign ExternalApp role to a membership");

// Si el membership actual tiene rol Owner, verificar que no es el único
var currentRole = await tenantRoleRepository.GetByIdAsync(membership.RoleId);
if (currentRole!.Name == "Owner")
{
    var ownerCount = await membershipRepository.CountByRoleIdAsync(membership.RoleId);
    ConflictGuard.ThrowIf(ownerCount <= 1, "Cannot change role of the only owner");
}

membership.RoleId = command.RoleId;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: PUT /memberships/{membershipId}/role

**Request**
```csharp
public record ChangeRoleRequest(Guid RoleId);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Cambiar de Manager a Waiter
- Precondición: Membership con RoleId=Manager
- Input: RoleId=Waiter
- Resultado: Membership con RoleId=Waiter

✅ Cambiar a rol custom
- Input: RoleId=Sommelier
- Resultado: Membership con RoleId=Sommelier

✅ Cambiar Owner a Manager (hay más owners)
- Precondición: Membership con RoleId=Owner, existen 2 memberships con ese rol
- Input: RoleId=Manager
- Resultado: Membership con RoleId=Manager

❌ Mismo rol
- Precondición: Membership con RoleId=Manager
- Input: RoleId=Manager
- Resultado: ConflictException "Membership already has this role"

❌ Rol no existe
- Input: RoleId=inexistente
- Resultado: KeyNotFoundException "Role not found"

❌ Rol es ExternalApp
- Input: RoleId=ExternalApp
- Resultado: ConflictException "Cannot assign ExternalApp role to a membership"

❌ Único Owner intenta cambiar rol
- Precondición: Membership con RoleId=Owner, es el único
- Input: RoleId=Manager
- Resultado: ConflictException "Cannot change role of the only owner"

#### Tests Integración

✅ 204 No Content

❌ 404 → Membership o rol no encontrado

❌ 409 → Mismo rol, rol ExternalApp o único owner

---

### 6.8 Membership.Deactivate

#### Event Storming
```
🟡[Owner] → 🔵(DeactivateMembership) → 🟤[[Membership]] → 🟠<MembershipDeactivated>
```

#### Input

*Sin input adicional*

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactiva | 409 | ConflictGuard | "Membership is already inactive" |
| Es el único Owner activo | 409 | ConflictGuard | "Cannot deactivate the only active owner" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!membership.IsActive, "Membership is already inactive");

var currentRole = await tenantRoleRepository.GetByIdAsync(membership.RoleId);
if (currentRole!.Name == "Owner")
{
    var activeOwnerCount = await membershipRepository.CountActiveByRoleIdAsync(membership.RoleId);
    ConflictGuard.ThrowIf(activeOwnerCount <= 1, "Cannot deactivate the only active owner");
}

membership.IsActive = false;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/deactivate

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Desactivar membership activa (Waiter)
- Precondición: Membership con IsActive=true, Role=Waiter
- Resultado: Membership con IsActive=false

✅ Desactivar membership activa (Manager)
- Precondición: Membership con IsActive=true, Role=Manager
- Resultado: Membership con IsActive=false

✅ Desactivar Owner (hay más owners activos)
- Precondición: Membership con Role=Owner, IsActive=true, hay 2 owners activos
- Resultado: Membership con IsActive=false

❌ Membership ya inactiva
- Precondición: Membership con IsActive=false
- Resultado: ConflictException "Membership is already inactive"

❌ Único Owner activo
- Precondición: Membership con Role=Owner, es el único owner activo
- Resultado: ConflictException "Cannot deactivate the only active owner"

#### Tests Integración

✅ 200 OK → MembershipResponse con IsActive=false

❌ 404 → No encontrada

❌ 409 → Ya inactiva o único owner

---

### 6.9 Membership.Reactivate

#### Event Storming
```
🟡[Owner] → 🔵(ReactivateMembership) → 🟤[[Membership]] → 🟠<MembershipReactivated>
```

#### Input

*Sin input adicional*

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activa | 409 | ConflictGuard | "Membership is already active" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(membership.IsActive, "Membership is already active");

membership.IsActive = true;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/reactivate

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Reactivar membership inactiva
- Precondición: Membership con IsActive=false
- Resultado: Membership con IsActive=true

❌ Membership ya activa
- Precondición: Membership con IsActive=true
- Resultado: ConflictException "Membership is already active"

#### Tests Integración

✅ 200 OK → MembershipResponse con IsActive=true

❌ 404 → No encontrada

❌ 409 → Ya activa

---

### 6.10 Membership.Delete

#### Event Storming
```
🟡[Owner] → 🔵(DeleteMembership) → 🟤[[Membership]] → 🟠<MembershipDeleted>
                                        │
                                  🟣{NotOnlyOwner}
```

#### Input

*Sin input adicional*

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Es el único Owner | 409 | ConflictGuard | "Cannot delete the only owner" |

#### Lógica
```csharp
var currentRole = await tenantRoleRepository.GetByIdAsync(membership.RoleId);
if (currentRole!.Name == "Owner")
{
    var ownerCount = await membershipRepository.CountByRoleIdAsync(membership.RoleId);
    ConflictGuard.ThrowIf(ownerCount <= 1, "Cannot delete the only owner");
}

membershipRepository.Delete(membership);
```

#### Slice: DELETE /memberships/{membershipId}

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Eliminar membership de Waiter
- Precondición: Membership con Role=Waiter
- Resultado: Membership eliminada

✅ Eliminar membership de Manager
- Precondición: Membership con Role=Manager
- Resultado: Membership eliminada

✅ Eliminar Owner (hay más owners)
- Precondición: Membership con Role=Owner, hay 2 owners
- Resultado: Membership eliminada

❌ Único Owner
- Precondición: Membership con Role=Owner, es el único
- Resultado: ConflictException "Cannot delete the only owner"

#### Tests Integración

✅ 204 No Content

❌ 404 → No encontrada

❌ 409 → Único owner

---

## 7. Descripciones de Permisos

> Las descripciones son **responsabilidad de producto**. Se definen en español durante la sesión de diseño. Claude Code genera el archivo de descripciones del microservicio con el español como base y traduce automáticamente al resto de idiomas necesarios.
>
> Deben ser claras, concisas y comprensibles para alguien sin conocimientos técnicos — es lo que el administrador ve cuando configura roles.

### Scopes atómicos

| Scope (nombre de clase) | Descripción (es) |
|--------------------------|-------------------|
| `CreateMembership` | Invitar a un nuevo miembro al equipo |
| `ListMemberships` | Ver la lista de miembros del equipo |
| `GetMembership` | Ver los detalles de un miembro |
| `ChangeRole` | Cambiar el rol de un miembro |
| `DeactivateMembership` | Desactivar temporalmente a un miembro |
| `ReactivateMembership` | Reactivar a un miembro desactivado |
| `DeleteMembership` | Eliminar a un miembro del equipo |
| `CancelInvitation` | Cancelar una invitación pendiente |
| `ResendInvitation` | Reenviar una invitación pendiente |

---

## 8. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /memberships | Membership.Create | 201 → `MembershipResponse` |
| 2 | POST | /memberships/{membershipId}/accept | Membership.AcceptInvitation | 200 → `MembershipResponse` |
| 3 | POST | /memberships/{membershipId}/cancel-invitation | Membership.CancelInvitation | 200 → `MembershipResponse` |
| 4 | POST | /memberships/{membershipId}/resend-invitation | Membership.ResendInvitation | 200 |
| 5 | GET | /memberships | ListMemberships | 200 → `MembershipResponse[]` |
| 6 | GET | /memberships/{membershipId} | GetMembership | 200 → `MembershipResponse` |
| 7 | PUT | /memberships/{membershipId}/role | Membership.ChangeRole | 204 |
| 8 | POST | /memberships/{membershipId}/deactivate | Membership.Deactivate | 200 → `MembershipResponse` |
| 9 | POST | /memberships/{membershipId}/reactivate | Membership.Reactivate | 200 → `MembershipResponse` |
| 10 | DELETE | /memberships/{membershipId} | Membership.Delete | 204 |

---

## 9. Persistencia (Firestore)

### Colección

`/memberships/{membershipId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<Membership>(entity =>
{
    // QueryFilter: multi-tenancy by TenantId
    entity.HasQueryFilter(x => x.TenantId == tenantId);

    // Reference: Role → TenantRole
    entity.Reference(x => x.Role);
});
```

### Documento Ejemplo

```json
{
  "id": "mem-001-guid",
  "tenantId": "tenant-001-guid",
  "userId": null,
  "role": "tenant_roles/role-manager-guid",
  "isActive": true,
  "invitationEmail": "maria@ejemplo.com",
  "invitationStatus": "Pending"
}
```

```json
{
  "id": "mem-002-guid",
  "tenantId": "tenant-001-guid",
  "userId": "google-oauth2|123456789",
  "role": "tenant_roles/role-waiter-guid",
  "isActive": true,
  "invitationEmail": "ana@ejemplo.com",
  "invitationStatus": "Accepted"
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Se necesita flujo de expiración de invitaciones? | Pendiente: Puede añadirse en V2 |
| 2 | ¿Se necesita historial/auditoría de cambios en memberships? | Pendiente |
| 3 | ¿Transferencia de ownership entre usuarios? | Pendiente: Puede ser un comando específico en V2 |
| 4 | ¿Se debe invalidar sesión al cambiar de rol? | Decidido: Sí — el wireframe lo muestra explícitamente |
| 5 | ¿El Owner puede cambiar el rol de otro Owner? | Pendiente: Se definirá con la matriz de permisos |

---

**Fecha**: 2026-02-10
**Autor**: Equipo Fudie
