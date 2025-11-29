# Tarea 4: Compilación y Tests

## Objetivo
Compilar todos los proyectos y ejecutar tests para verificar que la migración fue exitosa.

---

## Estado
- [ ] No iniciado
- [ ] En progreso
- [ ] Completado
- [ ] Verificado

---

## Pasos de Compilación

### 1. Limpiar builds anteriores
```bash
cd "c:\Users\Home\Documents\Cursos 2025\11-noviembre\03-Asp .met core\webapi"
dotnet clean
```

### 2. Compilar proyecto Fudie
```bash
dotnet build src/Fudie/Fudie.csproj
```

**Resultado esperado:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 3. Compilar proyecto webapi
```bash
dotnet build src/webapi/webapi.csproj
```

**Resultado esperado:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 4. Compilar toda la solución
```bash
dotnet build webapi.sln
```

**Resultado esperado:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Ejecución de Tests

### 1. Ejecutar todos los tests
```bash
dotnet test
```

**Resultado esperado:**
```
Passed!  - Failed:     0, Passed:    XX, Skipped:     0, Total:    XX
```

### 2. Ejecutar tests con detalle
```bash
dotnet test --verbosity normal
```

### 3. Verificar cobertura (si está configurada)
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## Verificación de Funcionalidad

### 1. Ejecutar la aplicación
```bash
dotnet run --project src/webapi/webapi.csproj
```

**Verificar:**
- [ ] La aplicación inicia sin errores
- [ ] Swagger UI está disponible en `/swagger`
- [ ] Los endpoints responden correctamente

### 2. Probar endpoints clave

**Abrir en navegador:**
```
http://localhost:5000/swagger
```

**Verificar:**
- [ ] Swagger muestra todos los endpoints
- [ ] Los modelos de error (CustomProblemDetails) están documentados
- [ ] Las respuestas 400, 404, 422, 500 aparecen en la documentación

### 3. Probar manejo de errores

**Endpoint de prueba (si existe):**
```bash
# Probar error 404
curl http://localhost:5000/api/test/notfound

# Probar error de validación
curl -X POST http://localhost:5000/api/test/validate \
  -H "Content-Type: application/json" \
  -d '{"invalid": "data"}'
```

**Verificar:**
- [ ] Los errores devuelven formato `application/problem+json`
- [ ] Los errores incluyen `traceId` y `timestamp`
- [ ] Los errores de validación incluyen detalles específicos

---

## Checklist de Verificación

### Compilación
- [ ] `Fudie.csproj` compila sin errores
- [ ] `webapi.csproj` compila sin errores
- [ ] `webapi.sln` compila sin errores
- [ ] No hay warnings relacionados con namespaces

### Tests
- [ ] Todos los tests unitarios pasan
- [ ] Todos los tests de integración pasan
- [ ] No hay tests ignorados inesperadamente

### Funcionalidad
- [ ] La aplicación inicia correctamente
- [ ] Swagger UI funciona
- [ ] Dependency Injection funciona (servicios con `[Injectable]`)
- [ ] Manejo global de excepciones funciona
- [ ] Validaciones con FluentValidation funcionan
- [ ] Patrón Result funciona correctamente

---

## Problemas Comunes

### Error: "No se puede cargar el ensamblado Fudie"
**Solución:**
```bash
dotnet clean
dotnet restore
dotnet build
```

### Error: "Servicio no registrado en DI"
**Solución:** Verificar que `AddInjectables()` se llama en `Program.cs`:
```csharp
builder.Services.AddInjectables(typeof(Program).Assembly);
```

### Tests fallan por namespaces
**Solución:** Actualizar `using` statements en los archivos de test también:
```csharp
using Fudie;
using Fudie.Domain;
```

### Swagger no muestra CustomProblemDetails
**Solución:** Verificar que el filtro está registrado:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<GlobalErrorResponsesOperationFilter>();
});
```

---

## Comandos de Diagnóstico

### Ver dependencias del proyecto webapi:
```bash
dotnet list src/webapi/webapi.csproj reference
```

### Ver paquetes NuGet de Fudie:
```bash
dotnet list src/Fudie/Fudie.csproj package
```

### Ver información de build:
```bash
dotnet build src/webapi/webapi.csproj --verbosity detailed
```

---

## Métricas de Éxito

✅ **Compilación exitosa:**
- 0 errores
- 0 warnings críticos

✅ **Tests exitosos:**
- 100% de tests pasando
- Misma cobertura que antes de la migración

✅ **Funcionalidad verificada:**
- Aplicación inicia
- Todos los endpoints funcionan
- Manejo de errores funciona
- Inyección de dependencias funciona

---

## Notas
- Si hay errores, revisar las tareas anteriores
- Documentar cualquier comportamiento inesperado
- Guardar logs de errores para análisis
