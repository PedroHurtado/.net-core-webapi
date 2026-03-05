# Auth — Reglas de negocio

## Niveles de acceso

| Endpoint | Seguridad | Quién puede usarlo |
|----------|-----------|-------------------|
| `POST /auth/logout` | `RequireAuthenticated` | Cualquier usuario autenticado |
| `GET /auth/me` | `RequireAuthenticated` | Cualquier usuario autenticado |
| `PUT /auth/me` | `RequireAuthenticated` | Cualquier usuario autenticado (sus propios datos) |
| `POST /auth/me/change-password` | `RequirePlatform` | Solo el superadmin de Fudie |
| `GET /users?email=...` | `RequirePlatform` | Solo Fudie (plataforma) |
| `GET /users/{id}` | `RequirePlatform` | Solo Fudie (plataforma) |
| `POST /users/{id}/deactivate` | `RequirePlatform` | Solo Fudie (plataforma) |
| `POST /users/{id}/activate` | `RequirePlatform` | Solo Fudie (plataforma) |

## Reglas

### POST /auth/logout
- Lee la sesión activa desde la cookie `fudie_session`
- El repositorio NO debe usar `IGet<Session, Guid>` porque devuelve 404 con el ID en el mensaje (expone información)
- Usar un repositorio custom que devuelva `Session?` (nullable)
- Si la sesión no existe, lanzar un ConflictGuard de no autorizado (401)
- Elimina la sesión y la cookie del navegador

### GET /auth/me
- Devuelve datos del usuario en sesión + lista de tenantIds donde tiene membresía activa
- Los tenants se obtienen vía `IMembershipLookup.FindAllByUserId`
- El response (`MeResponse`) incluye solo `tenantId` por cada membership, sin rol
- Permite al frontend ofrecer cambio de tenant

### PUT /auth/me
- El usuario actualiza sus propios datos (nombre, teléfono)
- No recibe ID — opera sobre el usuario en sesión
- El email no se modifica (viene del proveedor OAuth o fue definido por script)

### POST /auth/me/change-password
- No recibe ID — opera sobre el usuario en sesión
- Solo el superadmin de Fudie (AuthProvider.Local) puede cambiar su contraseña
- Invalida todas las sesiones excepto la actual
- 409 si el usuario no es de tipo Local

### GET /users?email=...
- Permite a operarios de Fudie buscar un usuario por email
- Caso de uso: localizar un usuario para luego activarlo/desactivarlo por ID
- Solo plataforma

### GET /users/{id}
- Solo plataforma puede consultar usuarios por ID

### POST /users/{id}/deactivate
- Solo Fudie puede desactivar usuarios por mal uso de la plataforma
- Elimina todas las sesiones activas del usuario
- 409 si ya está inactivo

### POST /users/{id}/activate
- Solo Fudie puede reactivar usuarios
- 409 si ya está activo
