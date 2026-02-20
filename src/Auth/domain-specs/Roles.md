# Domain Specification: TenantRole

---

## 1. Enums

*No hay enums en este agregado.*

---

## 2. Value Objects

*No hay value objects en este agregado. Todos los campos son tipos primitivos o arrays de strings.*

---

## 3. Aggregate: TenantRole

### Estructura

```
TenantRole (Aggregate Root)
├─ Id: Guid
├─ TenantId: Guid
├─ Name: string
├─ Description: string
├─ IsOwner: bool
├─ Groups: IReadOnlyCollection<string>
├─ AdditionalScopes: IReadOnlyCollection<string>
└─ ExcludedScopes: IReadOnlyCollection<string>
```

#### Propiedades

| Propiedad | Tipo | Modificador |
|-----------|------|-------------|
| Id | Guid | init |
| TenantId | Guid | init |
| Name | string | protected set |
| Description | string | protected set |
| IsOwner | bool | protected set |

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

> Estas reglas se implementan en `TenantRoleValidator : AbstractValidator<TenantRole>`

| Propiedad | Regla | Mensaje |
|-----------|-------|---------|
| Id | NotEmpty | "Id is required" |
| TenantId | NotEmpty | "TenantId is required" |
| Name | NotEmpty | "Name is required" |
| Name | MaxLength(100) | "Name cannot exceed 100 characters" |
| Description | MaxLength(500) | "Description cannot exceed 500 characters" |

---

## 4. Response

```csharp
public record TenantRoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsOwner,
    IReadOnlyCollection<string> Groups,
    IReadOnlyCollection<string> AdditionalScopes,
    IReadOnlyCollection<string> ExcludedScopes
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
> - TenantRole.CreateOwnerRole se ejecuta al provisionar un tenant (Seed o Subscription)
> - TenantRole.Create es para todos los demás roles
> - Las Queries van después de Create
> - Update y UpdatePermissions van después
> - Delete va al final

> **Tests de dominio**: Usar `TestableTenantRole` para preparar estado previo. Usar `DomainFixture` para resolver comandos y validators. **NO encadenar comandos** para crear estado.
>
> **Tests de slice**: Usar `TestableTenantRole` para el estado que devuelve el repository mock. Usar `DomainFixture` para resolver el comando que la slice inyecta. Mock de `IRepository` e `IUnitOfWork`.

---

### 6.1 TenantRole.CreateOwnerRole

> Comando interno que crea el rol Owner de un tenant. Se ejecuta al provisionar un tenant (Seed de plataforma o compra de Subscription). Solo puede haber un Owner por tenant.

#### Event Storming
```
🟡[System] → 🔵(CreateOwnerRole) → 🟤[[TenantRole]] → 🟠<OwnerRoleCreated>
```

#### Input

| Campo | Tipo |
|-------|------|
| TenantId | Guid |

#### Inyecta
- `IValidator<TenantRole>`

#### Lógica
```csharp
var role = new TenantRole(Guid.NewGuid())
{
    TenantId = command.TenantId,
    Name = "Owner",
    Description = "Propietario. Acceso total al tenant.",
    IsOwner = true
};

return tenantRoleValidator.ValidateOrThrow(role);
```

#### Slice: Interno

> No expuesto como endpoint público. Se invoca desde Seed y desde el flujo de Subscription.

#### Tests Unitarios (Dominio)

✅ Crear rol Owner con datos válidos
- Input: TenantId=valid
- Resultado: TenantRole con Name="Owner", IsOwner=true, Groups=[], AdditionalScopes=[], ExcludedScopes=[]

---

### 6.2 TenantRole.Create

> Crea cualquier rol que no sea Owner. El Owner decide qué roles necesita su negocio.

#### Event Storming
```
🟡[Owner] → 🔵(CreateTenantRole) → 🟤[[TenantRole]] → 🟠<TenantRoleCreated>
                                        │
                                  🟣{UniqueName}
```

#### Input

| Campo | Tipo |
|-------|------|
| TenantId | Guid |
| Name | string |
| Description | string |

#### Inyecta
- `IValidator<TenantRole>`

#### Guards (en slice)

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Ya existe un rol con ese nombre en el tenant | 409 | ConflictGuard | "A role with this name already exists" |

#### Lógica
```csharp
var role = new TenantRole(Guid.NewGuid())
{
    TenantId = command.TenantId,
    Name = command.Name,
    Description = command.Description,
    IsOwner = false
};

return tenantRoleValidator.ValidateOrThrow(role);
```

#### Slice: POST /tenant-roles

**Request**
```csharp
public record CreateTenantRoleRequest(
    string Name,
    string Description
);
```

**Response**: 201 Created → `TenantRoleResponse`

#### Tests Unitarios (Dominio)

✅ Crear rol con datos válidos
- Input: Name="Sommelier", Description="Carta de vinos"
- Resultado: TenantRole con IsOwner=false, Groups=[]

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

#### Tests Unitarios (Servicio)

✅ Verifica unicidad antes de crear
- Verifica que se comprueba si ya existe un rol con ese nombre

✅ Llama a TenantRole.Create con los parámetros correctos
- Verifica que se invoca tenantRoleCreate.Execute con el command correcto

✅ Añade el rol al repositorio
- Verifica que repository.Add es llamado con el rol creado

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del rol

#### Tests Integración

✅ 201 Created → TenantRoleResponse con IsOwner=false

❌ 409 → Nombre duplicado

❌ 422 → Validación fallida

---

### 6.3 GetTenantRole

#### Event Storming
```
🟡[Owner] → 🔵(GetTenantRole) → 🟤[[TenantRole]] → 📊 TenantRoleResponse
```

#### Slice: GET /tenant-roles/{id}

**Response**: 200 OK → `TenantRoleResponse`

#### Tests Unitarios (Servicio)

✅ Obtiene el rol del repositorio con el id correcto
- Verifica que repository.GetByIdAsync es llamado con el id

✅ Retorna Response mapeado correctamente
- Verifica que el Response contiene los datos del rol

#### Tests Integración

✅ 200 OK → TenantRoleResponse

❌ 404 → No encontrado

---

### 6.4 ListTenantRoles

#### Event Storming
```
🟡[Owner] → 🔵(ListTenantRoles) → 🟤[[TenantRole]] → 📊 TenantRoleResponse[]
```

#### Slice: GET /tenant-roles

> El tenantId se obtiene del JWT.

**Response**: 200 OK → `TenantRoleResponse[]`

#### Tests Unitarios (Servicio)

✅ Obtiene roles del tenantId del JWT
- Verifica que repository.GetByTenantIdAsync es llamado con el tenantId del token

✅ Retorna lista mapeada correctamente
- Verifica que el Response contiene todos los roles del tenant

#### Tests Integración

✅ 200 OK → Lista de TenantRoleResponse

✅ 200 OK → Lista vacía si no hay roles

---

### 6.5 TenantRole.Update

> Solo permite cambiar Name y Description. No permite modificar el rol Owner (IsOwner=true). El check de nombre duplicado vive en la slice.

#### Event Storming
```
🟡[Owner] → 🔵(UpdateTenantRole) → 🟤[[TenantRole]] → 🟠<TenantRoleUpdated>
                                        │
                                  🟣{IsOwner}
                                  🟣{UniqueName}
```

#### Input

| Campo | Tipo |
|-------|------|
| Name | string |
| Description | string |

#### Inyecta
- `IValidator<TenantRole>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Rol es Owner | 409 | ConflictGuard | "This role cannot be edited" |
| Nombre duplicado en el tenant (otro rol) | 409 | ConflictGuard (en slice) | "A role with this name already exists" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(role.IsOwner, "This role cannot be edited");

role.Name = command.Name;
role.Description = command.Description;

return tenantRoleValidator.ValidateOrThrow(role);
```

#### Slice: PUT /tenant-roles/{id}

**Request**
```csharp
public record UpdateTenantRoleRequest(
    string Name,
    string Description
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar nombre y descripción de rol normal
- Precondición: TenantRole con IsOwner=false
- Input: Name="Jefe de Sala", Description="Coordina servicio"
- Resultado: TenantRole actualizado

❌ Rol Owner no se puede editar
- Precondición: TenantRole con IsOwner=true
- Resultado: ConflictException "This role cannot be edited"

❌ Name vacío
- Input: Name=""
- Resultado: ValidationException "Name is required"

#### Tests Integración

✅ 204 No Content

❌ 404 → No encontrado

❌ 409 → Es Owner o nombre duplicado

❌ 422 → Validación fallida

---

### 6.6 TenantRole.UpdatePermissions

> Actualiza groups, additionalScopes y excludedScopes. No permite modificar el rol Owner (IsOwner=true). Al guardar se invalidan las sesiones de los memberships con este rol.

#### Event Storming
```
🟡[Owner] → 🔵(UpdatePermissions) → 🟤[[TenantRole]] → 🟠<TenantRolePermissionsUpdated>
                                         │
                                   🟣{IsOwner}
```

#### Input

| Campo | Tipo |
|-------|------|
| Groups | List\<string\> |
| AdditionalScopes | List\<string\> |
| ExcludedScopes | List\<string\> |

#### Inyecta
- `IValidator<TenantRole>`

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Rol es Owner | 409 | ConflictGuard | "This role cannot be edited" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(role.IsOwner, "This role cannot be edited");

role._groups = [.. command.Groups];
role._additionalScopes = [.. command.AdditionalScopes];
role._excludedScopes = [.. command.ExcludedScopes];

return tenantRoleValidator.ValidateOrThrow(role);
```

#### Slice: PUT /tenant-roles/{id}/permissions

**Request**
```csharp
public record UpdatePermissionsRequest(
    List<string> Groups,
    List<string> AdditionalScopes,
    List<string> ExcludedScopes
);
```

**Response**: 204 No Content

#### Tests Unitarios (Dominio)

✅ Actualizar permisos de rol normal
- Precondición: TenantRole con IsOwner=false, Groups=[]
- Input: Groups=["menu:read", "menu:write"], AdditionalScopes=[], ExcludedScopes=[]
- Resultado: TenantRole con Groups=["menu:read", "menu:write"]

✅ Actualizar con exclusiones
- Input: Groups=["menu:write"], ExcludedScopes=["menu-svc:SetMenuDepositPolicy"]
- Resultado: TenantRole con exclusión aplicada

✅ Actualizar con scopes adicionales
- Input: AdditionalScopes=["res-svc:CancelReservation"]
- Resultado: TenantRole con scope adicional

❌ Rol Owner no se puede editar
- Precondición: TenantRole con IsOwner=true
- Resultado: ConflictException "This role cannot be edited"

#### Tests Unitarios (Servicio)

✅ Obtiene el rol del repositorio
- Verifica que repository.GetByIdAsync es llamado con roleId

✅ Llama a TenantRole.UpdatePermissions
- Verifica que se invoca con el command correcto

✅ Guarda los cambios
- Verifica que unitOfWork.SaveChangesAsync es llamado

#### Tests Integración

✅ 204 No Content

❌ 404 → No encontrado

❌ 409 → Es Owner

---

### 6.7 Delete (Slice)

> No es un comando de dominio. La lógica vive en la slice. No permite eliminar el rol Owner. No permite eliminar si hay memberships asignados.

#### Event Storming
```
🟡[Owner] → 🔵(DeleteTenantRole) → 🟤[[TenantRole]] → 🟠<TenantRoleDeleted>
                                        │
                                  🟣{IsOwner}
                                  🟣{NoMemberships}
```

#### Input

*Sin input adicional*

#### Guards

| Condición | HTTP | Guard | Mensaje |
|-----------|------|-------|---------|
| Rol es Owner | 409 | ConflictGuard | "This role cannot be deleted" |
| Existen memberships con este rol | 409 | ConflictGuard | "Cannot delete a role that has members assigned" |

#### Lógica
```csharp
ConflictGuard.ThrowIf(role.IsOwner, "This role cannot be deleted");

var hasMemberships = await membershipRepository.ExistsByRoleIdAsync(role.Id);

ConflictGuard.ThrowIf(hasMemberships, "Cannot delete a role that has members assigned");

repository.Remove(role);
```

#### Slice: DELETE /tenant-roles/{id}

**Response**: 204 No Content

#### Tests Unitarios (Servicio)

✅ Eliminar rol normal sin memberships
- Precondición: TenantRole con IsOwner=false, sin memberships
- Resultado: Rol eliminado

❌ Rol Owner no se puede eliminar
- Precondición: TenantRole con IsOwner=true
- Resultado: ConflictException "This role cannot be deleted"

❌ Rol con memberships asignados
- Precondición: TenantRole con IsOwner=false, tiene memberships
- Resultado: ConflictException "Cannot delete a role that has members assigned"

#### Tests Integración

✅ 204 No Content

❌ 404 → No encontrado

❌ 409 → Es Owner o tiene memberships

---

## 7. Descripciones de Permisos

> Las descripciones son **responsabilidad de producto**. Se definen en español durante la sesión de diseño. Claude Code genera el archivo de descripciones del microservicio con el español como base y traduce automáticamente al resto de idiomas necesarios.
>
> Deben ser claras, concisas y comprensibles para alguien sin conocimientos técnicos — es lo que el administrador ve cuando configura roles.

### Scopes atómicos

| Scope (nombre de clase) | Descripción (es) |
|--------------------------|-------------------|
| `CreateTenantRole` | Crear un nuevo rol personalizado |
| `GetTenantRole` | Ver los detalles de un rol |
| `ListTenantRoles` | Ver la lista de roles |
| `UpdateTenantRole` | Modificar nombre y descripción de un rol |
| `UpdatePermissions` | Configurar los permisos de un rol |
| `DeleteTenantRole` | Eliminar un rol personalizado |

---

## 8. Resumen de Endpoints (Orden de Implementación)

| # | Método | Ruta | Comando/Query | Response |
|---|--------|------|---------------|----------|
| 1 | POST | /tenant-roles | TenantRole.Create | 201 → `TenantRoleResponse` |
| 2 | GET | /tenant-roles/{id} | GetTenantRole | 200 → `TenantRoleResponse` |
| 3 | GET | /tenant-roles | ListTenantRoles | 200 → `TenantRoleResponse[]` |
| 4 | PUT | /tenant-roles/{id} | TenantRole.Update | 204 |
| 5 | PUT | /tenant-roles/{id}/permissions | TenantRole.UpdatePermissions | 204 |
| 6 | DELETE | /tenant-roles/{id} | Delete (slice) | 204 |

---

## 9. Persistencia (Firestore)

### Colección

`/tenant_roles/{tenantRoleId}`

### Configuración DbContext

```csharp
modelBuilder.Entity<TenantRole>(entity =>
{
    // QueryFilter: multi-tenancy by TenantId
    entity.HasQueryFilter(x => x.TenantId == tenantId);

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
  "id": "role-001-guid",
  "tenantId": "tenant-001-guid",
  "name": "Manager",
  "description": "Encargado. Gestiona operativa diaria.",
  "isOwner": false,
  "groups": ["menu:read", "menu:write", "reservation:read", "reservation:write"],
  "additionalScopes": ["res-svc:CancelReservation"],
  "excludedScopes": ["menu-svc:SetMenuDepositPolicy", "menu-svc:RemoveMenuDepositPolicy"]
}
```

---

## 10. Hot Spots ⚠️

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ¿Se necesita validar que los groups y scopes existen en algún catálogo al asignarlos? | Pendiente: El Auth no conoce el catálogo — solo copia permisos al JWT |
| 2 | ¿Se necesita auditoría de cambios en permisos de roles? | Pendiente |
| 3 | ¿Se debe invalidar sesiones al actualizar permisos de un rol? | Decidido: Sí — el wireframe lo muestra explícitamente |

---

**Fecha**: 2026-02-18
**Autor**: Equipo Fudie
