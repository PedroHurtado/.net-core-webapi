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
├─ User: User?
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
| User | User? | protected set |
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

---

## 4. Response

```csharp
public record MembershipResponse(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
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
> - Membership.Create es la invitación — nace con InvitationStatus=Pending y User=null
> - AcceptInvitation va después (vincula User)
> - Las Queries van después
> - ChangeRole, Activate/Deactivate van después
> - Delete va al final

> **Tests de dominio**: Usar `TestableMembership` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableMembership` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 6.1 Membership.Create

> Crear es invitar. El membership nace con InvitationStatus=Pending y User=null. Se lanza el evento MembershipCreated que dispara el envío del email de invitación.

#### Event Storming
```
🟡[Owner] → 🔵(CreateMembership) → 🟤[[Membership]] → 🟠<MembershipCreated>
                                        │
                                  🟣{UniqueEmail} ← slice
                                  🟣{RoleExists} ← slice
```

#### Input

| Campo | Tipo |
|-------|------|
| InvitationEmail | string |
| Role | TenantRole |

#### Inyecta
- `IValidator<Membership>`

#### Guards (dominio)

Ninguno. Las validaciones de datos las hace el Validator.

#### Guards (slice)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| El rol no existe | 404 | NotFoundGuard | "Role not found" |
| Ya existe membership con ese email en el tenant | 409 | ConflictGuard | "A membership with this email already exists in this tenant" |

#### Lógica
```csharp
var membership = new Membership(Guid.NewGuid())
{
    TenantId = command.TenantId,
    User = null,
    Role = command.Role,
    IsActive = true,
    InvitationEmail = command.InvitationEmail,
    InvitationStatus = InvitationStatus.Pending
};

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships

**Guards de slice**
```csharp
var role = await tenantRoleRepository.GetByIdAsync(request.RoleId);
NotFoundGuard.ThrowIfNull(role, request.RoleId);

var duplicate = await membershipRepository.ExistsByEmailAndTenantAsync(
    request.InvitationEmail, tenantId);
ConflictGuard.ThrowIf(duplicate, "A membership with this email already exists in this tenant");
```

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
- Input: InvitationEmail="maria@ejemplo.com", Role=Manager
- Resultado: Membership con InvitationStatus=Pending, User=null, IsActive=true

❌ Email vacío → ValidationException "Invitation email is required"
❌ Email inválido → ValidationException "Invitation email must be a valid email address"
❌ Role null → ValidationException "Role is required"

#### Tests Unitarios (Servicio)

✅ Verifica que el rol existe
✅ Verifica unicidad de email
✅ Añade el membership al repositorio
✅ Guarda los cambios
✅ Retorna Response mapeado correctamente
❌ Rol no existe → 404
❌ Email duplicado → 409

#### Tests Integración

✅ 201 Created → MembershipResponse con InvitationStatus=Pending
❌ 404 → Rol no encontrado
❌ 409 → Email duplicado
❌ 422 → Validación fallida

---

### 6.2 Membership.AcceptInvitation

> El invitado hace login con Google OAuth y accede al link de invitación. La slice lee el userId del JWT y el membershipId de la ruta.

#### Event Storming
```
🟡[Invitado] → 🔵(AcceptInvitation) → 🟤[[Membership]] → 🟠<InvitationAccepted>
                                           │
                                     🟣{IsPending}
```

#### Input

| Campo | Tipo |
|-------|------|
| User | User |

#### Inyecta
- `IValidator<Membership>`

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Invitación no está en Pending | 409 | ConflictGuard | "Invitation is not pending" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    membership.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

membership.User = command.User;
membership.InvitationStatus = InvitationStatus.Accepted;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/accept

> Endpoint público. El userId se obtiene del JWT.

```csharp
var userId = httpContext.User.GetUserId();
var membershipId = route.Get<Guid>("membershipId");
```

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Aceptar invitación pendiente
- Precondición: InvitationStatus=Pending, User=null
- Input: User=valid
- Resultado: InvitationStatus=Accepted, User vinculado

❌ Invitación ya aceptada → ConflictException
❌ Invitación cancelada → ConflictException

#### Tests Integración

✅ 200 OK → MembershipResponse con InvitationStatus=Accepted
❌ 404 → No encontrado
❌ 409 → No está pendiente

---

### 6.3 Membership.CancelInvitation

#### Event Storming
```
🟡[Owner] → 🔵(CancelInvitation) → 🟤[[Membership]] → 🟠<InvitationCancelled>
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
    membership.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

membership.InvitationStatus = InvitationStatus.Cancelled;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/cancel-invitation

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Cancelar invitación pendiente → InvitationStatus=Cancelled
❌ Invitación ya aceptada → ConflictException
❌ Invitación ya cancelada → ConflictException

#### Tests Integración

✅ 200 OK → MembershipResponse con InvitationStatus=Cancelled
❌ 404 → No encontrado
❌ 409 → No está pendiente

---

### 6.4 Membership.ResendInvitation

> Solo dispara el evento. No cambia estado.

#### Event Storming
```
🟡[Owner] → 🔵(ResendInvitation) → 🟤[[Membership]] → 🟠<InvitationResent>
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
    membership.InvitationStatus != InvitationStatus.Pending,
    "Invitation is not pending");

// No cambia estado — solo lanza evento InvitationResent
```

#### Slice: POST /memberships/{membershipId}/resend-invitation

**Response**: 200 OK

#### Tests Unitarios (Dominio)

✅ Reenviar invitación pendiente → evento lanzado
❌ Invitación no pendiente → ConflictException

#### Tests Integración

✅ 200 OK
❌ 404 → No encontrado
❌ 409 → No está pendiente

---

### 6.5 ListMemberships

#### Slice: GET /memberships

> El tenantId se obtiene del JWT.

**Response**: 200 OK → `MembershipResponse[]`

#### Tests Unitarios (Servicio)

✅ Obtiene memberships del tenantId del JWT
✅ Retorna lista mapeada correctamente

#### Tests Integración

✅ 200 OK → Lista de MembershipResponse
✅ 200 OK → Lista vacía si no hay memberships

---

### 6.6 GetMembership

#### Slice: GET /memberships/{membershipId}

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
                             🟣{RoleExists} ← slice
```

#### Input

| Campo | Tipo |
|-------|------|
| Role | TenantRole |

#### Inyecta
- `IValidator<Membership>`

#### Guards (dominio)

Ninguno.

#### Guards (slice)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| El rol no existe | 404 | NotFoundGuard | "Role not found" |

#### Lógica
```csharp
membership.Role = command.Role;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: PUT /memberships/{membershipId}/role

**Guards de slice**
```csharp
var role = await tenantRoleRepository.GetByIdAsync(request.RoleId);
NotFoundGuard.ThrowIfNull(role, request.RoleId);
```

**Request**
```csharp
public record ChangeRoleRequest(Guid RoleId);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Cambiar de Manager a Waiter
✅ Cambiar a rol custom

#### Tests Unitarios (Servicio)

❌ Rol no existe → 404

#### Tests Integración

✅ 204 No Content
❌ 404 → Membership o rol no encontrado

---

### 6.8 Membership.Deactivate

#### Event Storming
```
🟡[Owner] → 🔵(DeactivateMembership) → 🟤[[Membership]] → 🟠<MembershipDeactivated>
                                            │
                                      🟣{IsActive}
```

#### Input

*Sin input adicional*

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está inactivo | 409 | ConflictGuard | "Membership is already inactive" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(!membership.IsActive, "Membership is already inactive");

membership.IsActive = false;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/deactivate

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Desactivar membership activo → IsActive=false
❌ Ya inactivo → ConflictException

#### Tests Integración

✅ 200 OK → MembershipResponse con IsActive=false
❌ 404 → No encontrada
❌ 409 → Ya inactiva

---

### 6.9 Membership.Activate

#### Event Storming
```
🟡[Owner] → 🔵(ActivateMembership) → 🟤[[Membership]] → 🟠<MembershipActivated>
                                          │
                                    🟣{IsInactive}
```

#### Input

*Sin input adicional*

#### Guards (dominio)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya está activo | 409 | ConflictGuard | "Membership is already active" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(membership.IsActive, "Membership is already active");

membership.IsActive = true;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/activate

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Activar membership inactivo → IsActive=true
❌ Ya activo → ConflictException

#### Tests Integración

✅ 200 OK → MembershipResponse con IsActive=true
❌ 404 → No encontrada
❌ 409 → Ya activa

---

### 6.10 Membership.Delete

#### Event Storming
```
🟡[Owner] → 🔵(DeleteMembership) → 🟤[[Membership]] → 🟠<MembershipDeleted>
```

#### Input

*Sin input adicional*

#### Guards (dominio)

Ninguno.

#### Lógica
```csharp
membershipRepository.Delete(membership);
```

#### Slice: DELETE /memberships/{membershipId}

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
| `CreateMembership` | Invitar a un nuevo miembro al equipo |
| `ListMemberships` | Ver la lista de miembros del equipo |
| `GetMembership` | Ver los detalles de un miembro |
| `ChangeRole` | Cambiar el rol de un miembro |
| `DeactivateMembership` | Desactivar temporalmente a un miembro |
| `ActivateMembership` | Reactivar a un miembro desactivado |
| `DeleteMembership` | Eliminar a un miembro del equipo |
| `CancelInvitation` | Cancelar una invitación pendiente |
| `ResendInvitation` | Reenviar una invitación pendiente |

> `AcceptInvitation` no genera scope — es un endpoint público que ejecuta el invitado.

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
| 9 | POST | /memberships/{membershipId}/activate | Membership.Activate | 200 → `MembershipResponse` |
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

    // Reference: User → User (nullable hasta que acepta la invitación)
    entity.Reference(x => x.User);

    // Reference: Role → TenantRole
    entity.Reference(x => x.Role);
});
```

### Documento Ejemplo

```json
{
  "id": "mem-001-guid",
  "tenantId": "tenant-001-guid",
  "user": null,
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
  "user": "users/user-001-guid",
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
| 3 | ¿Se debe invalidar sesión al cambiar de rol? | Decidido: Sí — el wireframe lo muestra explícitamente |

---

**Fecha**: 2026-02-11
**Autor**: Equipo Fudie
