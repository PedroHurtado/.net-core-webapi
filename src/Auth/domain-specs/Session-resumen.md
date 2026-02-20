# Session — Resumen de Implementación

## Comandos de Dominio

| # | Comando | Sección | Slice propia? |
|---|---------|---------|---------------|
| 1 | **Session.Create** | 7.1 | No — interno desde login |
| 2 | **Session.SetTenantContext** | 7.5 | Si + usado internamente desde login |
| 3 | **Session.Refresh** | 7.7 | No — interno desde ResolveAuth y login |

## Slices (Endpoints REST)

| # | Método | Ruta | Comando | Response |
|---|--------|------|---------|----------|
| 1 | **PUT** | `/auth/sessions/tenant` | Session.SetTenantContext | 204 |
| 2 | **POST** | `/auth/resolve` | ResolveAuth (7.9) | 200 |
| 3 | **GET** | `/auth/jwks` | GetJwks (7.10) | 200 |

## Descartados

- **Session.ClearTenantContext** — No hay caso de uso. El usuario siempre pertenece a al menos una organización y cambia entre ellas con SetTenantContext.
- **Session.Destroy** — No es comando de dominio. Es un `DeleteAsync` en el repositorio, ejecutado por logout o expiración de cookie (30 días sin actividad).

## Pendiente de revisión

| Query | Sección | Descripción |
|-------|---------|-------------|
| **ListSessionsByRoleAndTenant** | 7.3 | Query interna para invalidación por cambio de permisos |
| **ListSessionsByUserId** | 7.4 | Query interna para desactivación/eliminación de Membership |