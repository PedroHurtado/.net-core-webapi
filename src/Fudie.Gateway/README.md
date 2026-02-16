# Fudie Gateway

Reverse proxy basado en YARP que enruta las peticiones a los microservicios.

## Configuración de clusters

El archivo `appsettings.json` define los clusters con las direcciones de cada microservicio. Por defecto apuntan a los contenedores Docker:

| Cluster                | Dirección por defecto            |
|------------------------|----------------------------------|
| `auth-cluster`         | `http://auth-service:8080`       |
| `menus-cluster`        | `http://menus-service:8080`      |
| `plan-cluster`         | `http://plan-service:8080`       |
| `schedules-cluster`    | `http://schedules-service:8080`  |
| `customers-cluster`    | `http://customers-service:8080`  |
| `subscriptions-cluster`| `http://subscriptions-service:8080` |

## Debug local: redirigir un servicio al localhost

Para depurar un servicio ejecutándolo en local (desde el IDE), cambia su dirección en `appsettings.json` usando `host.docker.internal` que resuelve al host desde dentro del contenedor.

Por ejemplo, para depurar **Auth** en local (puerto 5001):

```json
"auth-cluster": {
  "Destinations": {
    "destination1": {
      "Address": "http://host.docker.internal:5001"
    }
  }
}
```

### Pasos

1. Arranca el servicio en local desde el IDE (ej: `dotnet run` en Auth, puerto 5001).
2. Edita `appsettings.json` y cambia la dirección del cluster correspondiente a `http://host.docker.internal:{puerto_local}`.
3. Guarda el archivo. **No es necesario reiniciar la gateway** — YARP recarga la configuración automáticamente (`reloadOnChange: true`).
4. Las peticiones al gateway ahora llegarán a tu servicio local.

### Restaurar

Para volver al contenedor, restaura la dirección original (ej: `http://auth-service:8080`) y guarda. YARP recargará de nuevo.

## Notas

- El `appsettings.json` está montado como bind mount (`appsettings.json:ro`), por lo que los cambios en el archivo local se reflejan dentro del contenedor.
- No uses variables de entorno (`ReverseProxy__Clusters__*`) en `docker-compose.yml` para los clusters, ya que sobreescriben el JSON e impiden la configuración dinámica.
- `host.docker.internal` solo funciona en Docker Desktop (Windows/Mac). En Linux puede requerir `--add-host=host.docker.internal:host-gateway` en docker-compose.
