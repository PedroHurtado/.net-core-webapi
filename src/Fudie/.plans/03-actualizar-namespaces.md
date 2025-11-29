# Tarea 3: Actualizar Namespaces en el Proyecto webapi

## Objetivo
Actualizar todos los `using` statements en el proyecto `webapi` para usar los nuevos namespaces de `Fudie` en lugar de `webapi.common`.

---

## Estado
- [ ] No iniciado
- [ ] En progreso
- [x] Completado
- [x] Verificado

---

## Mapeo de Namespaces

| Namespace Antiguo | Namespace Nuevo |
|-------------------|-----------------|
| `using webapi.common;` | `using Fudie;` |
| `using webapi.common.dependencyinjection;` | `using Fudie.DependencyInjection;` |
| `using webapi.common.domain;` | `using Fudie.Domain;` |
| `using webapi.common.infrastructure;` | `using Fudie.Infrastructure;` |
| `using webapi.common.openapi;` | `using Fudie.OpenApi;` |

---

## Archivos a Actualizar

### 1. Program.cs
**Ubicación:** `src/webapi/Program.cs`

**Buscar y reemplazar:**
```csharp
// ANTES:
using webapi.common;
using webapi.common.dependencyinjection;

// DESPUÉS:
using Fudie;
using Fudie.DependencyInjection;
```

---

### 2. Archivos en features/
**Ubicación:** `src/webapi/features/**/*.cs`

Buscar en todos los archivos de features y reemplazar:

```bash
# Comando para encontrar archivos que usan webapi.common
grep -r "using webapi.common" src/webapi/features/
```

**Reemplazos comunes:**
- `using webapi.common;` → `using Fudie;`
- `using webapi.common.domain;` → `using Fudie.Domain;`
- `using webapi.common.infrastructure;` → `using Fudie.Infrastructure;`

---

### 3. Archivos en infrastructure/
**Ubicación:** `src/webapi/infrastructure/**/*.cs`

```bash
# Comando para encontrar archivos
grep -r "using webapi.common" src/webapi/infrastructure/
```

**Reemplazos esperados:**
- `using webapi.common.domain;` → `using Fudie.Domain;`
- `using webapi.common.infrastructure;` → `using Fudie.Infrastructure;`

---

## Comandos de Búsqueda y Reemplazo

### Buscar todos los archivos afectados:
```bash
cd "c:\Users\Home\Documents\Cursos 2025\11-noviembre\03-Asp .met core\webapi"
grep -r "webapi\.common" src/webapi/ --include="*.cs" | grep -v "bin\|obj"
```

### Reemplazo automático (usar con precaución):
```bash
# Reemplazar en todos los archivos .cs
find src/webapi -name "*.cs" -type f -exec sed -i 's/using webapi\.common;/using Fudie;/g' {} +
find src/webapi -name "*.cs" -type f -exec sed -i 's/using webapi\.common\.dependencyinjection;/using Fudie.DependencyInjection;/g' {} +
find src/webapi -name "*.cs" -type f -exec sed -i 's/using webapi\.common\.domain;/using Fudie.Domain;/g' {} +
find src/webapi -name "*.cs" -type f -exec sed -i 's/using webapi\.common\.infrastructure;/using Fudie.Infrastructure;/g' {} +
find src/webapi -name "*.cs" -type f -exec sed -i 's/using webapi\.common\.openapi;/using Fudie.OpenApi;/g' {} +
```

---

## Verificación

### 1. Verificar que no queden referencias antiguas:
```bash
grep -r "webapi\.common" src/webapi/ --include="*.cs" | grep -v "bin\|obj"
```

**Resultado esperado:** Sin resultados (o solo en comentarios)

### 2. Compilar el proyecto:
```bash
dotnet build src/webapi/webapi.csproj
```

**Resultado esperado:** Compilación exitosa sin errores de namespace

### 3. Verificar imports en archivos clave:
```bash
# Ver imports de Program.cs
head -20 src/webapi/Program.cs | grep "using"
```

---

## Checklist de Archivos Críticos

- [x] `src/webapi/Program.cs`
- [x] Todos los archivos en `src/webapi/features/`
- [x] Todos los archivos en `src/webapi/infrastructure/`
- [x] Verificar que no hay errores de compilación
- [x] Verificar que no quedan referencias a `webapi.common`

---

## Problemas Comunes

### Error: "El tipo o namespace 'common' no existe"
**Solución:** Aún hay archivos sin actualizar. Usar el comando de búsqueda para encontrarlos.

### Error: "Referencia ambigua"
**Solución:** Puede haber conflicto de nombres. Usar namespace completo:
```csharp
Fudie.Result result = ...
```

### Clases no encontradas después del cambio
**Solución:** Verificar que la referencia a `Fudie.csproj` esté en `webapi.csproj`

---

## Notas
- Usar búsqueda global en el IDE para encontrar todas las referencias
- Revisar especialmente archivos que implementan `IFeatureModule`
- Los archivos en `common/` no deben tocarse (se eliminarán después)
