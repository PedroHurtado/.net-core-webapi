# Guía de Migración: ProjectReference → NuGet Fudie

## Objetivo
Reemplazar las `ProjectReference` a `src/Fudie`, `src/Fudie.Security` y `src/Fudie.Generator` por el meta-package NuGet `Fudie`.

```
dotnet add package Fudie --version 1.0.3
```

## Microservicios migrados
- [x] Plan
- [ ] Customers
- [ ] Menus
- [ ] Schedules
- [ ] Subscriptions
- [ ] Auth (complejo)
- [ ] Gateway (complejo)

---

## Pasos por microservicio

### 1. csproj — Quitar ProjectReference, añadir PackageReference

**Quitar** las 3 referencias al monolito:
```xml
<!-- QUITAR -->
<ProjectReference Include="..\Fudie\Fudie.csproj" />
<ProjectReference Include="..\Fudie.Security\Fudie.Security.csproj" />
<ProjectReference Include="..\Fudie.Generator\Fudie.Generator.csproj"
    OutputItemType="Analyzer"
    ReferenceOutputAssembly="false" />
```

**Añadir** el meta-package:
```xml
<PackageReference Include="Fudie" Version="1.0.3" />
```

> El Generator viene como analyzer dentro del NuGet. Los archivos generados siguen visibles gracias a `EmitCompilerGeneratedFiles` + `CompilerGeneratedFilesOutputPath` que ya tenemos.

**Preservar** la referencia a Firestore (sigue siendo ProjectReference):
```xml
<ProjectReference Include="..\..\..\Firestore\src\Fudie.Firestore.EntityFrameworkCore\Fudie.Firestore.EntityFrameworkCore.csproj" />
```

### 2. GlobalUsings.cs — Limpiar usings de Fudie

**Quitar** los namespaces que ya vienen como implicit usings en los paquetes NuGet:
```csharp
// QUITAR - ya los traen los paquetes NuGet
global using Fudie.DependencyInjection;
global using Fudie.Domain;
global using Fudie.Features;        // eliminado del framework
global using Fudie.Http;
global using Fudie.Infrastructure;
global using Fudie.Validation;
global using Fudie.Security;
```

**Mantener** los usings de terceros (FluentValidation, EF Core, etc.) y los de Firestore:
```csharp
// MANTENER
global using FluentValidation;
global using FluentValidation.Results;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.DependencyInjection;
global using Fudie.Firestore.EntityFrameworkCore.Metadata.Builders;
global using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
```

### 3. Build y resolver errores

Compilar:
```bash
dotnet build src/{Microservicio}/{Microservicio}.csproj
```

Los errores típicos son en **Program.cs** (ver paso 4) y en **DbContext** (ver paso 5).

### 4. Program.cs — Cambios de API

| Antes (monolito) | Ahora (NuGet) |
|---|---|
| `AddFudieSecurity(opts => ...)` | `AddFudieJwksProvider()` |
| Bloque manual de StaticFiles + SwaggerUI (~20 líneas) | `app.UseFudieOpenApi()` |
| `app.UseFudieAuthorization()` + `app.MapFeatures()` + `app.MapCatalog()` | `app.MapFeatures(builder => builder.UseFudieAuthorization())` |

**Program.cs de referencia (Plan):**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<PlanDbContext>((sp, options) =>{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<PlanDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

builder.Services.AddFudieJwksProvider();

builder.Services.AddInjectables();

var app = builder.Build();

app.UseExceptionHandler();

app.UseFudieOpenApi();

app.MapFeatures(builder => builder.UseFudieAuthorization());

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
```

### 5. DbContext — Heredar de FudieDbContext

| Antes | Ahora |
|---|---|
| `DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork` | `FudieDbContext(options)` |
| Método manual `Query<T>()` | Eliminar (lo hereda de FudieDbContext) |

**Importante**: Añadir `base.OnModelCreating(modelBuilder)` al principio del `OnModelCreating` para que FudieDbContext ignore `DomainEvents` en entidades con `IHasDomainEvents`.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);  // <-- AÑADIR
    // ... resto de configuración
}
```

### 6. appsettings.json — Simplificar Security

**Antes:**
```json
"Fudie": {
    "Security": {
        // múltiples opciones de FudieSecurityOptions
    }
}
```

**Ahora:**
```json
"Security": {
    "JwksUrl": "http://localhost:5176/auth/jwks",
    "CacheRefreshMinutes": 60
}
```

### 7. Dockerfile — Quitar COPY de Fudie

**Quitar** las líneas que copian los proyectos Fudie (ya no son ProjectReference):
```dockerfile
# QUITAR (tanto en csproj copy como en source copy)
COPY ${PROJECT_DIR}/src/Fudie/Fudie.csproj webapi/src/Fudie/
COPY ${PROJECT_DIR}/src/Fudie.Generator/Fudie.Generator.csproj webapi/src/Fudie.Generator/
COPY ${PROJECT_DIR}/src/Fudie.Security/Fudie.Security.csproj webapi/src/Fudie.Security/
COPY ${PROJECT_DIR}/src/Fudie/ webapi/src/Fudie/
COPY ${PROJECT_DIR}/src/Fudie.Generator/ webapi/src/Fudie.Generator/
COPY ${PROJECT_DIR}/src/Fudie.Security/ webapi/src/Fudie.Security/
```

**Cambiar** rutas hardcodeadas a `${PROJECT_DIR}`:
```dockerfile
# Antes
COPY --from=build /src/webapi/src/{Micro}/OpenApi ./OpenApi
# Ahora
COPY --from=build /src/${PROJECT_DIR}/src/{Micro}/OpenApi ./OpenApi
```

### 8. OpenApi — Renombrar YAML

Renombrar el archivo YAML en la carpeta `OpenApi/` al nombre real del agregado (ej: `plan-api.yaml` → nombre del agregado).

`UseFudieOpenApi()` busca por defecto en la carpeta `OpenApi/` del ContentRootPath. Configurable via appsettings:
```json
"Fudie": {
    "OpenApi": {
        "Folder": "OpenApi",
        "RoutePrefix": "swagger"
    }
}
```

### 9. Tests unitarios

Mismo proceso: quitar ProjectReference a Fudie, añadir PackageReference al meta-package. Los tests deben compilar y pasar sin cambios en la lógica de test.

### 10. Tests de integración (si existen)

Verificar que el WebApplicationFactory funcione con las nuevas dependencias NuGet.

---

## Notas importantes

- **Auth y Gateway se dejan para el final** — tienen lógica de seguridad más compleja
- **Fudie.Firestore sigue como ProjectReference** — no se ha empaquetado como NuGet
- **Verificar arranque del microservicio** después de cada migración, no solo compilación
