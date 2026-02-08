# Domain Specification: Membership

---

## 1. Enums

### 1.1 MembershipRole

| Valor | Descripción |
|-------|-------------|
| Owner | Propietario del restaurante. Acceso completo. |
| Manager | Encargado. Gestiona operativa diaria. |
| Waiter | Camarero. Acceso limitado a operaciones del día a día. |

> ⚠️ **Hot Spot #1**: La granularidad de permisos por rol se definirá cuando se aborde la seguridad de la aplicación.

---

## 2. Value Objects

*No hay value objects en este agregado. Todos los campos son tipos primitivos.*

---

## 3. Aggregate: Membership

### Estructura

```
Membership (Aggregate Root)
├─ Id: Guid
├─ UserId: string
├─ RestaurantId: Guid
├─ RestaurantName: string
├─ Role: MembershipRole
├─ IsActive: bool
├─ CreatedAt: DateTime
└─ UpdatedAt: DateTime
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| UserId | string | init |
| RestaurantId | Guid | init |
| RestaurantName | string | protected set |
| Role | MembershipRole | protected set |
| IsActive | bool | protected set |
| CreatedAt | DateTime | init |
| UpdatedAt | DateTime | protected set |

### Validaciones

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| UserId | NotEmpty | "User ID is required" |
| UserId | Max(100) | "User ID cannot exceed 100 characters" |
| RestaurantId | NotEmpty | "Restaurant ID is required" |
| RestaurantName | NotEmpty | "Restaurant name is required" |
| RestaurantName | Max(150) | "Restaurant name cannot exceed 150 characters" |
| Role | IsInEnum | "Role must be a valid membership role" |

---

## 4. Response

```csharp
public record MembershipResponse(
    Guid Id,
    string UserId,
    Guid RestaurantId,
    string RestaurantName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record MembershipSummaryResponse(
    Guid RestaurantId,
    string RestaurantName,
    string Role
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
> - Membership.Create se invoca desde Subscription Service (post-compra) o por invitación
> - Las Queries van después de Create
> - ChangeRole y Deactivate/Reactivate van después
> - Delete va al final

---

### 6.1 Membership.Create

#### Event Storming
```
🟡[System/Owner] → 🔵(CreateMembership) → 🟤[[Membership]] → 🟠<MembershipCreated>
                                               │
                                     🟣{UniqueMembership}
                                     ⚠️{InvitationPermissions}
```

#### Input

| Campo | Tipo |
|-------|------|
| UserId | string |
| RestaurantId | Guid |
| RestaurantName | string |
| Role | MembershipRole |

#### Inyecta
- `IValidator<Membership>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya existe membership userId+restaurantId | 409 | ConflictGuard | "User already has a membership in this restaurant" |

> ⚠️ **Hot Spot #2**: Guards adicionales sobre quién puede invitar a quién se definirán cuando se aborde la seguridad.

#### Lógica
```csharp
await ConflictGuard.ThrowIfAsync(
    await membershipRepository.ExistsAsync(command.UserId, command.RestaurantId),
    "User already has a membership in this restaurant");

var membership = new Membership(Guid.NewGuid())
{
    UserId = command.UserId,
    RestaurantId = command.RestaurantId,
    RestaurantName = command.RestaurantName,
    Role = command.Role,
    IsActive = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships

**Request**
```csharp
public record CreateMembershipRequest(
    string UserId,
    Guid RestaurantId,
    string RestaurantName,
    string Role
);
```

**Response**: 201 Created → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Crear membership con datos válidos
- Input: UserId="google-123", RestaurantId=Guid, RestaurantName="El Bar del Juanjo", Role=Owner
- Resultado: Membership creada con IsActive=true, CreatedAt y UpdatedAt establecidos

✅ Crear membership con rol Manager
- Input: Role=Manager
- Resultado: Membership creada con Role=Manager

✅ Crear membership con rol Waiter
- Input: Role=Waiter
- Resultado: Membership creada con Role=Waiter

❌ UserId vacío
- Input: UserId=""
- Resultado: ValidationException "User ID is required"

❌ RestaurantId vacío
- Input: RestaurantId=Guid.Empty
- Resultado: ValidationException "Restaurant ID is required"

❌ RestaurantName vacío
- Input: RestaurantName=""
- Resultado: ValidationException "Restaurant name is required"

❌ Role inválido
- Input: Role="InvalidRole"
- Resultado: ValidationException "Role must be a valid membership role"

❌ Membership duplicada
- Precondición: Ya existe membership con UserId="google-123" y RestaurantId=X
- Input: Mismos UserId y RestaurantId
- Resultado: ConflictException "User already has a membership in this restaurant"

#### Tests Unitarios (Servicio)

✅ Verifica unicidad antes de crear
- Verifica que membershipRepository.ExistsAsync es llamado con userId y restaurantId

✅ Llama a Membership.Create con los parámetros correctos
- Verifica que se invoca membershipCreate.Execute con el command correcto

✅ Añade la membership al repositorio
- Verifica que repository.Add es llamado con la membership creada

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos de la membership

#### Tests Integración

✅ 201 Created → MembershipResponse con IsActive=true

❌ 409 → Membership duplicada

❌ 422 → Validación fallida

---

### 6.2 Membership.ChangeRole

#### Event Storming
```
🟡[Owner/Manager] → 🔵(ChangeRole) → 🟤[[Membership]] → 🟠<MembershipRoleChanged>
                                           │
                                 ⚠️{RoleChangePermissions}
```

#### Input

| Campo | Tipo |
|-------|------|
| Role | MembershipRole |

#### Inyecta
- `IValidator<Membership>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Es el único Owner | 409 | ConflictGuard | "Cannot change role of the only owner" |
| Mismo rol actual | 409 | ConflictGuard | "Membership already has this role" |

> ⚠️ **Hot Spot #3**: Guards sobre quién puede cambiar rol a quién se definirán cuando se aborde la seguridad.

#### Lógica
```csharp
ConflictGuard.ThrowIf(
    membership.Role == command.Role,
    "Membership already has this role");

// Si es Owner, verificar que no es el único
if (membership.Role == MembershipRole.Owner)
{
    var ownerCount = await membershipRepository.CountByRestaurantAndRoleAsync(
        membership.RestaurantId, 
        MembershipRole.Owner);
    
    ConflictGuard.ThrowIf(ownerCount <= 1, "Cannot change role of the only owner");
}

membership.Role = command.Role;
membership.UpdatedAt = DateTime.UtcNow;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: PUT /memberships/{membershipId}/role

**Request**
```csharp
public record ChangeRoleRequest(string Role);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Cambiar rol de Manager a Waiter
- Precondición: Membership con Role=Manager
- Input: Role=Waiter
- Resultado: Membership con Role=Waiter, UpdatedAt actualizado

✅ Cambiar rol de Waiter a Manager
- Precondición: Membership con Role=Waiter
- Input: Role=Manager
- Resultado: Membership con Role=Manager

✅ Cambiar rol de Owner a Manager (hay más owners)
- Precondición: Membership con Role=Owner, existen 2 owners en el restaurante
- Input: Role=Manager
- Resultado: Membership con Role=Manager

❌ Mismo rol
- Precondición: Membership con Role=Manager
- Input: Role=Manager
- Resultado: ConflictException "Membership already has this role"

❌ Único Owner intenta cambiar rol
- Precondición: Membership con Role=Owner, es el único owner
- Input: Role=Manager
- Resultado: ConflictException "Cannot change role of the only owner"

#### Tests Unitarios (Servicio)

✅ Obtiene la membership del repositorio
- Verifica que repository.GetByIdAsync es llamado con membershipId

✅ Llama a Membership.ChangeRole
- Verifica que se invoca membershipChangeRole.Execute

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → Membership no encontrada

❌ 409 → Mismo rol o único owner

---

### 6.3 Membership.Deactivate

#### Event Storming
```
🟡[Owner/Manager] → 🔵(DeactivateMembership) → 🟤[[Membership]] → 🟠<MembershipDeactivated>
                                                     │
                                           ⚠️{DeactivationPermissions}
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

if (membership.Role == MembershipRole.Owner)
{
    var activeOwnerCount = await membershipRepository.CountActiveByRestaurantAndRoleAsync(
        membership.RestaurantId,
        MembershipRole.Owner);
    
    ConflictGuard.ThrowIf(activeOwnerCount <= 1, "Cannot deactivate the only active owner");
}

membership.IsActive = false;
membership.UpdatedAt = DateTime.UtcNow;

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

### 6.4 Membership.Reactivate

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
membership.UpdatedAt = DateTime.UtcNow;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: POST /memberships/{membershipId}/reactivate

**Response**: 200 OK → `MembershipResponse`

#### Tests Unitarios (Dominio)

✅ Reactivar membership inactiva
- Precondición: Membership con IsActive=false
- Resultado: Membership con IsActive=true, UpdatedAt actualizado

❌ Membership ya activa
- Precondición: Membership con IsActive=true
- Resultado: ConflictException "Membership is already active"

#### Tests Integración

✅ 200 OK → MembershipResponse con IsActive=true

❌ 404 → No encontrada

❌ 409 → Ya activa

---

### 6.5 Membership.UpdateRestaurantName

> Este comando se invoca internamente cuando el restaurante cambia de nombre (evento RestaurantUpdated).

#### Event Storming
```
🟠<RestaurantUpdated> → 🟣{SyncRestaurantName} → 🔵(UpdateRestaurantName) → 🟤[[Membership]]
```

#### Input

| Campo | Tipo |
|-------|------|
| RestaurantName | string |

#### Lógica
```csharp
membership.RestaurantName = command.RestaurantName;
membership.UpdatedAt = DateTime.UtcNow;

return membershipValidator.ValidateOrThrow(membership);
```

#### Slice: Interno (NotificationHandler)

> No expuesto como endpoint público. Se ejecuta como reacción al evento RestaurantUpdated.

#### Tests Unitarios (Dominio)

✅ Actualizar nombre del restaurante
- Precondición: Membership con RestaurantName="Nombre Antiguo"
- Input: RestaurantName="Nombre Nuevo"
- Resultado: Membership con RestaurantName="Nombre Nuevo"

---

### 6.6 Membership.Delete

#### Event Storming
```
🟡[Owner] → 🔵(DeleteMembership) → 🟤[[Membership]] → 🟠<MembershipDeleted>
                                         │
                               ⚠️{DeletePermissions}
```

#### Input

*Sin input adicional*

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Es el único Owner | 409 | ConflictGuard | "Cannot delete the only owner" |

#### Lógica
```csharp
if (membership.Role == MembershipRole.Owner)
{
    var ownerCount = await membershipRepository.CountByRestaurantAndRoleAsync(
        membership.RestaurantId,
        MembershipRole.Owner);
    
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

## 7. Queries

### 7.1 GetMembershipsByUser

**Slice**: GET /users/{userId}/memberships

> ⚠️ **Nota de seguridad**: El userId se obtiene del JWT. La ruta es solo semántica — el backend ignora el parámetro de ruta y usa el claim del token.

**Response**: 200 OK → `MembershipSummaryResponse[]`

```csharp
// Solo devuelve memberships activas
var memberships = await membershipRepository
    .GetActiveByUserIdAsync(userId)
    .Select(m => new MembershipSummaryResponse(
        m.RestaurantId,
        m.RestaurantName,
        m.Role.ToString()))
    .ToListAsync();
```

#### Tests Unitarios (Servicio)

✅ Obtiene memberships del userId del JWT
- Verifica que repository.GetActiveByUserIdAsync es llamado con el userId del token

✅ Solo devuelve memberships activas
- Verifica que no se incluyen memberships con IsActive=false

✅ Retorna lista mapeada correctamente
- Verifica que el Response contiene RestaurantId, RestaurantName y Role

#### Tests Integración

✅ 200 OK → Lista de MembershipSummaryResponse

✅ 200 OK → Lista vacía si no tiene memberships

---

### 7.2 GetMembership

**Slice**: GET /memberships/{membershipId}

**Response**: 200 OK → `MembershipResponse`

#### Tests Integración

✅ 200 OK → MembershipResponse

❌ 404 → No encontrada

---

### 7.3 GetMembershipsByRestaurant

**Slice**: GET /restaurant/memberships

> El restaurantId se obtiene del JWT (tenantId).

**Response**: 200 OK → `MembershipResponse[]`

#### Tests Unitarios (Servicio)

✅ Obtiene memberships del tenantId del JWT
- Verifica que repository.GetByRestaurantIdAsync es llamado con el tenantId del token

#### Tests Integración

✅ 200 OK → Lista de MembershipResponse

---

### 7.4 ValidateMembership

> Query interna para validar el cambio de tenant.

**Slice**: Interno (usado por /auth/tenant-token)

```csharp
// Valida que existe membership activa para userId + restaurantId
var membership = await membershipRepository
    .GetActiveByUserAndRestaurantAsync(userId, restaurantId);

return membership != null
    ? new MembershipSummaryResponse(membership.RestaurantId, membership.RestaurantName, membership.Role.ToString())
    : null;
```

---

## 8. Resumen de Endpoints

> **Convención de seguridad**: Los endpoints que operan sobre el usuario autenticado obtienen el userId del JWT. Los endpoints dentro del contexto de tenant obtienen el restaurantId del JWT.

| # | Método | Ruta | Comando/Query | Response | Notas |
|---|--------|------|---------------|----------|-------|
| 1 | POST | /memberships | Membership.Create | 201 → `MembershipResponse` | Sistema/Invitación |
| 2 | GET | /users/{userId}/memberships | GetMembershipsByUser | 200 → `MembershipSummaryResponse[]` | userId del JWT |
| 3 | GET | /memberships/{membershipId} | GetMembership | 200 → `MembershipResponse` | |
| 4 | GET | /restaurant/memberships | GetMembershipsByRestaurant | 200 → `MembershipResponse[]` | tenantId del JWT |
| 5 | PUT | /memberships/{membershipId}/role | Membership.ChangeRole | 204 | |
| 6 | POST | /memberships/{membershipId}/deactivate | Membership.Deactivate | 200 → `MembershipResponse` | |
| 7 | POST | /memberships/{membershipId}/reactivate | Membership.Reactivate | 200 → `MembershipResponse` | |
| 8 | DELETE | /memberships/{membershipId} | Membership.Delete | 204 | |

---

## 9. Persistencia (Firestore)

### Colección

`/memberships/{membershipId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<Membership>(entity =>
{
    // Composite index: UserId + RestaurantId (para unicidad y búsqueda)
    entity.HasIndex(m => new { m.UserId, m.RestaurantId }).IsUnique();

    // Index: UserId (para GetMembershipsByUser)
    entity.HasIndex(m => m.UserId);

    // Index: RestaurantId (para GetMembershipsByRestaurant)
    entity.HasIndex(m => m.RestaurantId);

    // Enum: Role se guarda como string
    entity.Property(m => m.Role)
        .HasConversion<string>();
});
```

### Documento Ejemplo

```json
{
  "id": "mem-001-guid",
  "userId": "google-oauth2|123456789",
  "restaurantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "restaurantName": "El Bar del Juanjo",
  "role": "Owner",
  "isActive": true,
  "createdAt": "2025-02-05T10:30:00Z",
  "updatedAt": "2025-02-05T10:30:00Z"
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Qué permisos específicos tiene cada rol (Owner, Manager, Waiter)? | Pendiente: Se definirá cuando se aborde seguridad |
| 2 | ¿Quién puede invitar nuevos miembros? ¿Owner a todos, o jerarquía (Owner→Manager→Waiter)? | Pendiente: Se definirá cuando se aborde seguridad |
| 3 | ¿Quién puede cambiar el rol de quién? | Pendiente: Se definirá cuando se aborde seguridad |
| 4 | ¿Quién puede desactivar/eliminar a quién? | Pendiente: Se definirá cuando se aborde seguridad |
| 5 | ¿Se necesita flujo de invitación por email con aceptación? | Pendiente: Puede añadirse en V2 |
| 6 | ¿Se necesita historial/auditoría de cambios en memberships? | Pendiente |
| 7 | ¿Transferencia de ownership entre usuarios? | Pendiente: Puede ser un comando específico en V2 |

---

**Fecha**: 2025-02-05
**Autor**: Equipo Fudie
