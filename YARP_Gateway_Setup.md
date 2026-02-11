# Fudie — YARP Gateway + Docker Compose

## Contexto

YARP (Yet Another Reverse Proxy) actúa como API Gateway de Fudie. Es un proyecto .NET 8 que:

- Recibe todas las requests en un único punto de entrada
- Rutea por path a cada microservicio (`/menus/*` → menu-service, `/auth/*` → auth-service)
- Ejecuta un middleware custom que convierte cookie de sesión → JWT antes de reenviar
- Rate limiting, CORS y seguridad van aquí

Stack: .NET 8, C# 12, YARP (NuGet `Yarp.ReverseProxy`), Docker.

---

## 1. Proyecto YARP Gateway

### 1.1 Crear proyecto

```bash
dotnet new web -n Fudie.Gateway -f net8.0
cd Fudie.Gateway
dotnet add package Yarp.ReverseProxy
```

### 1.2 Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("default", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 100;
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.UseRateLimiter();

// Middleware cookie → JWT (ver sección 3)
// app.UseMiddleware<AuthResolveMiddleware>();

app.MapReverseProxy();

app.Run();
```

### 1.3 appsettings.json (estructura base)

```json
{
  "AllowedOrigins": ["http://localhost:4200"],
  "AuthService": {
    "ResolveUrl": "http://auth-service:5001/auth/resolve"
  },
  "ReverseProxy": {
    "Routes": {
      "auth-route": {
        "ClusterId": "auth-cluster",
        "Match": {
          "Path": "/auth/{**catch-all}"
        }
      },
      "menu-route": {
        "ClusterId": "menu-cluster",
        "RateLimiterPolicy": "default",
        "Match": {
          "Path": "/menus/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "auth-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://auth-service:5001"
          }
        }
      },
      "menu-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://menu-service:5002"
          }
        }
      }
    }
  }
}
```

### 1.4 appsettings.Development.json

Sobreescribe solo los destinos para desarrollo local. YARP hace merge con el base.

```json
{
  "AuthService": {
    "ResolveUrl": "http://localhost:5001/auth/resolve"
  },
  "ReverseProxy": {
    "Clusters": {
      "auth-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5001"
          }
        }
      },
      "menu-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5002"
          }
        }
      }
    }
  }
}
```

> **Escenario mixto**: Si un microservicio corre desde el IDE (ej: menu-service en `localhost:5002`) y el resto en Docker, el `appsettings.Development.json` apunta ese cluster a `localhost:5002` y los demás a los contenedores (`http://auth-service:5001`). YARP soporta hot reload — los cambios en el JSON se aplican sin reiniciar.

---

## 2. Docker

### 2.1 Dockerfile optimizado para microservicios .NET 8

Un único Dockerfile reutilizable. Usa multi-stage build con imagen AOT-ready mínima.

```dockerfile
# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copiar csproj y restaurar primero (cachea dependencias)
COPY *.csproj ./
RUN dotnet restore --runtime linux-musl-x64

# Copiar código y publicar
COPY . ./
RUN dotnet publish -c Release -o /app \
    --runtime linux-musl-x64 \
    --self-contained false \
    --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Seguridad: usuario no-root
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app .

# Puerto por defecto
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ASSEMBLY_NAME.dll"]
```

**Notas:**
- Reemplazar `ASSEMBLY_NAME.dll` por el nombre real del ensamblado (ej: `Fudie.MenuService.dll`)
- Imagen `alpine` pesa ~100MB vs ~210MB de la imagen base estándar
- `--self-contained false` usa el runtime compartido de la imagen base (más ligero)
- Usuario no-root por seguridad
- Si el microservicio tiene proyectos de shared library, ajustar el COPY y paths del csproj

### 2.2 docker-compose.yml

```yaml
services:
  gateway:
    build:
      context: ./src/Fudie.Gateway
      dockerfile: Dockerfile
    ports:
      - "8000:8080"
    volumes:
      - ./src/Fudie.Gateway/appsettings.json:/app/appsettings.json:ro
    depends_on:
      - auth-service
      - menu-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker

  auth-service:
    build:
      context: ./src/Fudie.AuthService
      dockerfile: Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker

  menu-service:
    build:
      context: ./src/Fudie.MenuService
      dockerfile: Dockerfile
    ports:
      - "5002:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker

  firestore-emulator:
    image: google/cloud-sdk:latest
    command: gcloud emulators firestore start --host-port=0.0.0.0:8086
    ports:
      - "8086:8086"
```

### 2.3 appsettings.Docker.json (para YARP dentro de Docker)

Cuando YARP corre dentro de Docker Compose, los servicios se resuelven por nombre de contenedor:

```json
{
  "AuthService": {
    "ResolveUrl": "http://auth-service:8080/auth/resolve"
  },
  "ReverseProxy": {
    "Clusters": {
      "auth-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://auth-service:8080"
          }
        }
      },
      "menu-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://menu-service:8080"
          }
        }
      }
    }
  }
}
```

---

## 3. Middleware Cookie → JWT (placeholder)

Este middleware intercepta las requests antes de que YARP las reenvíe. Llama a `POST /auth/resolve` del auth-service con los headers originales (incluida la cookie), recibe el JWT en el body, y lo inyecta como `Authorization: Bearer {token}`.

```csharp
public class AuthResolveMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpClient _httpClient;
    private readonly string _resolveUrl;

    public AuthResolveMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _httpClient = httpClientFactory.CreateClient();
        _resolveUrl = configuration["AuthService:ResolveUrl"]!;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Rutas públicas que no necesitan auth
        if (context.Request.Path.StartsWithSegments("/auth"))
        {
            await _next(context);
            return;
        }

        // Reenviar cookie al auth-service
        var request = new HttpRequestMessage(HttpMethod.Post, _resolveUrl);
        if (context.Request.Headers.TryGetValue("Cookie", out var cookie))
        {
            request.Headers.Add("Cookie", cookie.ToString());
        }

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            context.Response.StatusCode = 401;
            return;
        }

        // Leer JWT del body y inyectar como header
        var jwt = await response.Content.ReadAsStringAsync();
        context.Request.Headers["Authorization"] = $"Bearer {jwt}";

        await _next(context);
    }
}
```

> **NOTA**: Este es un placeholder. La implementación final depende de cómo el auth-service devuelve el JWT (estructura del body, content-type, etc). Adaptar según la especificación del agregado Session.

---

## 4. Escenarios de desarrollo

### Todo en Docker (servicios finalizados)

```bash
docker compose up
```

Todo entra por `localhost:8000`. YARP rutea internamente.

### Un servicio en desarrollo desde el IDE

1. El servicio nuevo se arranca desde Rider/VS en su puerto (ej: `localhost:5003`)
2. Se monta el `appsettings.json` de YARP como volumen
3. Se edita el archivo añadiendo la nueva ruta y cluster apuntando a `host.docker.internal:5003`
4. YARP recarga automáticamente (hot reload)

Ejemplo — añadir reservation-service en desarrollo:

```json
{
  "ReverseProxy": {
    "Routes": {
      "reservation-route": {
        "ClusterId": "reservation-cluster",
        "Match": {
          "Path": "/reservations/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "reservation-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://host.docker.internal:5003"
          }
        }
      }
    }
  }
}
```

### Servicio finalizado → mover a Docker

1. Crear Dockerfile para el servicio (copiar el template de la sección 2.1)
2. Añadir al `docker-compose.yml`
3. Cambiar el destino en YARP de `host.docker.internal:5003` a `reservation-service:8080`

---

## 5. Producción (Cloud Run)

- **YARP Gateway** → Cloud Run (autoescalado)
- **Cada microservicio** → Cloud Run independiente (autoescalado)
- **Load Balancer**  delante del gateway para dominio custom + SSL + Cloud Armor
- Los clusters en producción apuntan a las URLs internas de Cloud Run
- El mismo middleware cookie → JWT funciona igual


## 6. Observaciones de Claude Code

El documento está bien estructurado y cubre los puntos clave. Mis observaciones:

Lo bueno:

La separación de appsettings por entorno (Development, Docker, base) es correcta — YARP hace merge automático y eso simplifica la configuración.
El Dockerfile multi-stage con Alpine + usuario no-root es una buena práctica.
El flujo de desarrollo progresivo (IDE → Docker → Cloud Run) está bien pensado para un equipo que va construyendo microservicios incrementalmente.
Hot reload de YARP para añadir rutas sin reiniciar es un detalle práctico importante.
Puntos a revisar:

Puertos inconsistentes — En docker-compose.yml los servicios exponen el puerto interno 8080, pero en appsettings.json (base) los clusters apuntan a :5001 y :5002. El base debería apuntar a :8080 (el puerto real dentro del contenedor) o ser consistente con un solo entorno. El appsettings.Docker.json lo corrige, pero el base queda confuso.

IHttpClientFactory en el middleware — El middleware recibe IHttpClientFactory por constructor, pero los middlewares en ASP.NET se resuelven como singletons. Inyectar IHttpClientFactory ahí está bien, pero el HttpClient se crea una sola vez en el constructor (_httpClient = httpClientFactory.CreateClient()). Debería crearse en cada InvokeAsync para evitar problemas de DNS stale y respetar el ciclo de vida que IHttpClientFactory gestiona.

Rate limiting solo en menu-route — Solo menu-route tiene RateLimiterPolicy: "default". Si la intención es proteger todos los endpoints, falta en auth-route. Si es intencional (auth sin rate limit), vale la pena documentar por qué.

Sin health checks — YARP soporta health checks activos/pasivos para los clusters. En producción con Cloud Run convendría añadirlos para detectar instancias caídas.

Sin PathPrefix stripping — Las rutas llegan al microservicio con el prefijo (/menus/..., /auth/...). Si los controllers de los microservicios ya esperan esos prefijos, perfecto. Si no, necesitarás transforms para quitar el prefijo antes de reenviar.

Firestore emulator — Está en el compose pero no se referencia desde ningún servicio con FIRESTORE_EMULATOR_HOST. Los microservicios que lo usen necesitan esa variable de entorno.

En resumen: Es una buena base de referencia para arrancar. Los puntos 1 y 2 son bugs reales que hay que corregir antes de implementar. El resto son mejoras para cuando se acerque producción.
