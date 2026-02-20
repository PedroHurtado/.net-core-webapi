# Tarea: Configurar secretos compartidos entre microservicios

## 1. Crear `Directory.Build.props` en la raíz de la solución (al lado del .sln)

```xml
<Project>
  <PropertyGroup>
    <UserSecretsId>569f94d2-6a9b-4b8c-b190-7525f739671c</UserSecretsId>
  </PropertyGroup>
</Project>
```

## 2. Quitar `UserSecretsId` del `.csproj` de Auth

Actualmente Auth tiene `<UserSecretsId>569f94d2-6a9b-4b8c-b190-7525f739671c</UserSecretsId>` en su `.csproj`. Eliminarlo. Lo hereda de `Directory.Build.props`.

## 3. Actualizar `docker-compose.yml`

Definir un volumen compartido a nivel de compose para user-secrets y referenciarlo en todos los servicios:

```yaml
volumes:
  user-secrets:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: ${USER_SECRETS_PATH:-~/.microsoft/usersecrets}

services:
  auth-service:
    volumes:
      - user-secrets:/home/appuser/.microsoft/usersecrets:ro

  customer-service:
    volumes:
      - user-secrets:/home/appuser/.microsoft/usersecrets:ro

  subscription-service:
    volumes:
      - user-secrets:/home/appuser/.microsoft/usersecrets:ro
```

## 4. Añadir nuevos secretos al almacén

Ejecutar desde la raíz de cualquier proyecto de la solución:

```bash
dotnet user-secrets set "Fudie:SeedKey" "{generar-guid}"
dotnet user-secrets set "Fudie:PlatformTenantId" "{generar-guid}"
dotnet user-secrets set "Fudie:InternalSecret" "{generar-guid}"
```

## Resultado esperado

- Todos los proyectos comparten el mismo `UserSecretsId` vía `Directory.Build.props`
- Todos los contenedores en Docker Compose leen los mismos secretos vía volumen compartido
- Un solo almacén de secretos en desarrollo
