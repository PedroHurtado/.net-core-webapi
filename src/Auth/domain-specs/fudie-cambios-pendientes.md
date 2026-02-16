# Fudie — Cambios pendientes

## 1. Membership: Owner como Membership sin invitación

Membership actual exige `InvitationEmail` (required) e `InvitationStatus` (required). El Owner no se crea por invitación. Ambos campos pasan a nullable. Validator: si uno es null, ambos deben ser null. Cuando son null, es un Owner creado por sistema. Cuando tienen valor, es un miembro invitado.

Protección del Owner: `Membership.Delete` y `Membership.Deactivate` deben comprobar si el TenantRole asociado tiene `IsDeletable: false`. Si lo tiene, 409. `Membership.ChangeRole` del Owner debe comprobar `IsEditable: false`. Si lo tiene, 409.

## 2. Membership: documento fudie-membership-modificacion.md obsoleto

Este documento habla de un enum `MembershipRole` con Owner/Manager/Waiter/ExternalApp. Ya no existe ese enum. La Membership referencia un `RoleId` que apunta a TenantRole. El documento entero está desactualizado y contradice la spec actual de Membership y TenantRole.

## 3. ExternalApp: agregado separado, no Membership

ExternalApp.md define un agregado independiente con sus propios campos de invitación, permisos (Groups/AdditionalScopes/ExcludedScopes) y API Key. Esto contradice fudie-membership-modificacion.md que dice que ExternalApp es una Membership con rol. Hay que decidir cuál es la fuente de verdad. Si ExternalApp es agregado propio, el documento de modificación de Membership sobra. Si es Membership, ExternalApp.md sobra.

## 4. Extensiones de `IEndpointRouteBuilder`

Cuatro caminos para cualquier endpoint:

- `.AllowAnonymous()` → pasa cualquiera (ya existe)
- `.RequireAuthenticated()` → nuevo, solo necesita `sub` válido, sin `tid`
- `.RequirePlatform()` → nuevo, combinable con scopes y `.WithPermissionGroup()`, exige tenant de plataforma
- Sin nada → cerrado por defecto, requiere `tid` + permisos del tenant

## 5. Middleware: orden de evaluación

1. `AllowAnonymous()` → pasa
2. ¿JWT válido? → si no, 401
3. `.RequireAuthenticated()` → tiene `sub`, pasa. No exige `tid`
4. `.RequirePlatform()` → `tid` debe ser el de plataforma, si no 403
5. `owner: true` → bypass de permisos dentro de su tenant
6. Validación de scopes (groups, additional, excluded)

## 6. Identificación del tenant de plataforma

ID de configuración en `appsettings` o variable de entorno. No hay flag en Customer. El middleware compara el `tid` del JWT contra ese ID.

## 7. Session Hot Spot 2: corrección

Dice: "TenantId null para usuarios de plataforma." Incorrecto. El equipo de Fudie opera como un tenant más con Memberships, roles y permisos. Tiene `TenantId` (el de plataforma). El hot spot se elimina.

## 8. Flujo post-creación de Customer

Al crear un Customer (por seed o por suscripción), se ejecuta automáticamente: SeedSystemRoles (5 TenantRole) + Membership del User como Owner (sin invitación, vinculada al TenantRole Owner).

## 9. Seed de plataforma

Endpoint protegido en Auth con una clave de activación: un GUID configurado como variable de entorno (`FUDIE_SEED_KEY`). El endpoint comprueba que el request lleva esa clave en un header. Si no la lleva o no coincide, 401.

Si ya existe el super admin, primero destruye todo (Membership Owner, TenantRoles, Customer, User) y vuelve a crear desde cero. Un solo endpoint, idempotente con reset. 

En producción se configura el GUID como variable de entorno, se ejecuta el seed, se elimina la variable. En desarrollo se deja fijo en `appsettings.Development.json` para ejecutarlo tantas veces como haga falta.

Flujo: crea User Local → con JWT de ese User (solo `sub`), llama a `POST /customers` (marcado con `.RequireAuthenticated()`). El flujo del punto 8 se ejecuta automáticamente.

## 10. Contradicción bitfield vs arrays de strings

fudie-modelo-autorizacion.md usa bitfield (`permissions: 13`, `byte[]`, operaciones AND). fudie-catalogo-permisos.md también usa bitfield. Pero Catalog.md y fudie-catalogo-propuesta.md usan arrays de strings (`groups`, `add`, `exc`). Session.md y TenantRole usan arrays de strings. La decisión tomada en Catalog.md dice explícitamente: "Transporte en JWT — Array de strings — descartado bitfield." Hay que eliminar toda referencia a bitfield de fudie-modelo-autorizacion.md y fudie-catalogo-permisos.md.

## 11. User.md: "creados por script" → incorrecto

Varias notas dicen que los usuarios Local se crean "por script". El seed es un endpoint, no un script. Actualizar las referencias.

## 12. Configuración: `appsettings`

Dos valores nuevos:

- `Fudie:PlatformTenantId` — GUID fijo, permanente, se decide antes del seed. Va en todos los microservicios (el middleware de `.RequirePlatform()` lo necesita).
- `Fudie:SeedKey` — GUID, solo en Auth. Se elimina de producción tras el primer seed. En desarrollo se queda fijo.
