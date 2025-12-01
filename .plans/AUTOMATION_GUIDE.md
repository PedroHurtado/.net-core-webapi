# 🤖 Guía de Automatización Completa para Desarrolladores Junior

**Objetivo**: Después de definir el dominio, generar automáticamente todo el código necesario mediante prompts estructurados a la IA.

---

## 📋 Índice

1. [Visión General del Flujo](#-visión-general-del-flujo)
2. [Paso 0: Preparación](#-paso-0-preparación)
3. [Paso 1: Definir el Dominio](#-paso-1-definir-el-dominio-manual)
4. [Paso 2: Generar Dominio + Tests](#-paso-2-generar-dominio--tests)
5. [Paso 3: Generar Persistencia](#️-paso-3-generar-persistencia)
6. [Paso 4: Generar Queries (Lectura)](#-paso-4-generar-queries-lectura)
7. [Paso 5: Generar Commands (Escritura)](#️-paso-5-generar-commands-escritura)
8. [Paso 6: Generar Tests de Integración](#-paso-6-generar-tests-de-integración)
9. [Paso 7: Validación Final](#-paso-7-validación-final)
10. [Checklist de Validación](#-checklist-de-validación)

---

## 🎯 Visión General del Flujo

```
┌─────────────────────────────────────────────────────────────┐
│  PASO 0: Preparación                                        │
│  - Crear carpeta de feature                                 │
│  - Revisar análisis del proyecto                            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  PASO 1: Definir Dominio (MANUAL)                           │
│  - Crear domain-specs/[Entidad].md                          │
│  - Event Storming + Example Mapping                         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  PASO 2: Generar Dominio + Tests (IA)                       │
│  - features/[entidad]/models/[Entidad].cs                   │
│  - tests/UnitTests/Features/[Entidad]/[Entidad]Tests.cs     │
│  ✅ Compilar y ejecutar tests                               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  PASO 3: Generar Persistencia (IA)                          │
│  - infrastructure/Configurations/[Entidad]Configuration.cs  │
│  - Actualizar ApplicationDbContext                          │
│  ✅ Compilar                                                │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  PASO 4: Generar Queries (IA)                               │
│  - features/[entidad]/queries/Get[Entidad].cs               │
│  - features/[entidad]/queries/Get[Entidades].cs             │
│  ✅ Compilar y probar en Swagger                            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  PASO 5: Generar Commands (IA)                              │
│  - features/[entidad]/commands/Create[Entidad].cs           │
│  - features/[entidad]/commands/Update[Entidad].cs           │
│  - features/[entidad]/commands/Delete[Entidad].cs           │
│  ✅ Compilar y probar en Swagger                            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  PASO 6: Generar Tests de Integración (IA)                  │
│  - tests/IntegrationTests/Features/[Entidad]/               │
│  ✅ Ejecutar todos los tests                                │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  PASO 7: Validación Final                                   │
│  - Ejecutar checklist completo                              │
│  - Code review                                              │
│  ✅ Feature completo                                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Paso 0: Preparación

### Antes de empezar, asegúrate de:

1. **Leer el análisis del proyecto**:
   - Lee: [ANALISIS_PROYECTO_FUDIE.md](./ANALISIS_PROYECTO_FUDIE.md)

2. **Revisar la guía de estilo**:
   - Lee: [style_guide_examples.md](./templates/style_guide_examples.md)

3. **Crear la estructura de carpetas**:
   ```bash
   # Reemplaza [entidad] con el nombre de tu entidad en minúsculas (ej: pedidos)
   mkdir -p src/webapi/features/[entidad]/models
   mkdir -p src/webapi/features/[entidad]/queries
   mkdir -p src/webapi/features/[entidad]/commands
   mkdir -p tests/WebApi.UnitTests/Features/[Entidad]
   mkdir -p tests/WebApi.IntegrationTests/Features/[Entidad]
   mkdir -p src/webapi/infrastructure/Configurations
   ```

4. **Crear carpeta de especificaciones de dominio** (si no existe):
   ```bash
   mkdir -p domain-specs
   ```

---

## 📝 Paso 1: Definir el Dominio (MANUAL)

**Este es el ÚNICO paso manual obligatorio.**

### 1.1 Crear archivo de especificación

Crea: `domain-specs/[Entidad].md`

Usa la plantilla: [domain_definition_template.md](./templates/domain_definition_template.md)

### 1.2 Completar la especificación

**Secciones obligatorias:**

1. **Estado y Estructura**
   - Propiedades con tipos y validaciones
   - Relaciones con otras entidades
   - Invariantes de negocio

2. **Event Storming**
   - Flujo temporal: Actor → Comando → Agregado → Evento
   - Constraints y reglas

3. **Example Mapping**
   - Ejemplos de éxito
   - Ejemplos de fallo
   - Casos edge

### 1.3 Ejemplo completo

Ver: `domain-specs/Pizza.md` (si existe) o crear uno siguiendo la plantilla.

---

## 🤖 Paso 2: Generar Dominio + Tests

### 2.1 Prompt para la IA

**Copia y pega este prompt** (reemplaza `[Entidad]` con tu entidad):

```
🎯 CONTEXTO:
Soy un desarrollador trabajando en un proyecto .NET 8 con Clean Architecture, DDD y Vertical Slices.

📂 ARCHIVOS DE REFERENCIA:
1. Análisis del proyecto: @[.plans/analisis-proyecto-fudie.md]
2. Guía de estilo: @[.plans/templates/style_guide_examples.md]
3. Definición de dominio: @[domain-specs/[Entidad].md]

🎯 TAREA:
Genera la clase de dominio y sus tests unitarios siguiendo EXACTAMENTE los patrones del proyecto.

📋 ENTREGABLES:

1. **Clase de Dominio**: `src/webapi/features/[entidad]/models/[Entidad].cs`
   - Hereda de `Entity` (Fudie.Domain)
   - Constructor protegido
   - Factory method `Create()` retornando `Result<[Entidad]>`
   - Métodos de comportamiento retornando `Result`
   - Validador anidado con FluentValidation
   - Colecciones encapsuladas (backing field + IReadOnlyCollection)

2. **Tests Unitarios**: `tests/WebApi.UnitTests/Features/[Entidad]/[Entidad]Tests.cs`
   - Clase de test con xUnit
   - Tests para factory method `Create()`
   - Tests para cada método de comportamiento
   - Tests mapeando 1:1 los ejemplos del Example Mapping
   - Usar FluentAssertions para asserts

⚠️ RESTRICCIONES:
- NO uses controladores, usa Minimal APIs
- NO uses excepciones para flujo de negocio, usa Result<T>
- Sigue EXACTAMENTE el estilo de Pizza.cs
- Todos los tests deben pasar
- El código debe compilar sin errores

📤 FORMATO DE RESPUESTA:
Proporciona el código completo de ambos archivos, listo para copiar y pegar.
```

### 2.2 Validación

Después de generar el código:

```bash
# Compilar
dotnet build

# Ejecutar tests unitarios
dotnet test tests/WebApi.UnitTests/Features/[Entidad]/[Entidad]Tests.cs
```

**✅ Criterio de éxito**: Todos los tests pasan, código compila sin errores.

---

## 🗄️ Paso 3: Generar Persistencia

### 3.1 Prompt para la IA

```
🎯 CONTEXTO:
Continúo con la implementación de [Entidad]. Ya tengo el dominio y sus tests.

📂 ARCHIVOS DE REFERENCIA:
1. Clase de dominio: @[src/webapi/features/[entidad]/models/[Entidad].cs]
2. ApplicationDbContext: @[src/webapi/infrastructure/ApplicationDbContext.cs]
3. Ejemplo de configuración: Busca archivos *Configuration.cs en infrastructure/Configurations/

🎯 TAREA:
Genera la configuración de persistencia para Entity Framework Core.

📋 ENTREGABLES:

1. **Configuración EF Core**: `src/webapi/infrastructure/Configurations/[Entidad]Configuration.cs`
   - Implementa `IEntityTypeConfiguration<[Entidad]>`
   - Configura tabla, clave primaria
   - Configura propiedades (Required, MaxLength según validaciones del dominio)
   - Configura relaciones (HasMany, WithMany, etc.)
   - Ignora propiedades calculadas (si las hay)

2. **Actualización de DbContext**: Indica qué líneas agregar a `ApplicationDbContext.cs`
   - Agregar `DbSet<[Entidad]>` property

⚠️ RESTRICCIONES:
- Las validaciones de MaxLength deben coincidir con las del validador de dominio
- Usa convenciones de EF Core cuando sea posible
- Para colecciones Many-to-Many, usa tabla de unión si es necesario

📤 FORMATO DE RESPUESTA:
1. Código completo de [Entidad]Configuration.cs
2. Líneas exactas para agregar a ApplicationDbContext.cs
```

### 3.2 Aplicar cambios

1. Crear archivo de configuración
2. Actualizar `ApplicationDbContext.cs`:
   ```csharp
   public required DbSet<[Entidad]> [Entidades] { get; set; }
   ```

### 3.3 Validación

```bash
# Compilar
dotnet build

# Verificar que no hay errores de configuración
dotnet run --project src/webapi
# Debe iniciar sin errores
```

**✅ Criterio de éxito**: Aplicación inicia correctamente, no hay errores de EF Core.

---

## 🔍 Paso 4: Generar Queries (Lectura)

### 4.1 Prompt para Get[Entidad] (por ID)

```
🎯 CONTEXTO:
Implemento la funcionalidad de lectura para [Entidad].

📂 ARCHIVOS DE REFERENCIA:
1. Clase de dominio: @[src/webapi/features/[entidad]/models/[Entidad].cs]
2. Ejemplo de query: @[src/webapi/features/pizzas/queries/GetPizza.cs]
3. Guía de estilo: @[.plans/templates/style_guide_examples.md]

🎯 TAREA:
Genera el endpoint de lectura para obtener un [Entidad] por ID.

📋 ENTREGABLES:

**Query**: `src/webapi/features/[entidad]/queries/Get[Entidad].cs`
- Implementa `IFeatureModule`
- Record `Response` con todas las propiedades necesarias
- Records anidados para relaciones (ej: IngredientResponse)
- Método `AddRoutes()` con endpoint GET /[entidades]/{id:guid}
- Repositorio adaptador implementando `IGet<[Entidad], Guid>`
- Usa `IGetOrThrowAsync` con `tracking: false` e `includeProperties` si hay relaciones
- Configuración OpenAPI completa con `.WithOpenApi()`, `.WithName()`, etc.
- Atributo `[Injectable]` en el repositorio

⚠️ RESTRICCIONES:
- Endpoint debe ser GET /[entidades]/{id:guid}
- Usa `tracking: false` para consultas de solo lectura
- Incluye propiedades relacionadas con `includeProperties`
- Response debe incluir TODAS las propiedades del dominio
- Usa `Results.Ok(response)`
- Documenta con `.Produces<Response>(200)` y `.Produces<CustomProblemDetails>(404)`

📤 FORMATO DE RESPUESTA:
Código completo del archivo Get[Entidad].cs
```

### 4.2 Prompt para Get[Entidades] (lista con paginación)

```
🎯 CONTEXTO:
Implemento el listado paginado de [Entidad].

📂 ARCHIVOS DE REFERENCIA:
1. Clase de dominio: @[src/webapi/features/[entidad]/models/[Entidad].cs]
2. Ejemplo de query: @[src/webapi/features/pizzas/queries/GetPizzas.cs]

🎯 TAREA:
Genera el endpoint de listado paginado.

📋 ENTREGABLES:

**Query**: `src/webapi/features/[entidad]/queries/Get[Entidades].cs`
- Implementa `IFeatureModule`
- Record `Query` con parámetros: `string? Name, int Page = 1, int Size = 25`
- Record `Response` (igual que en Get[Entidad])
- Interface `IService` con método `Handler(Query query)`
- Clase `Service` con `[Injectable]` que:
  - Inyecta `IQuery`
  - Usa `Query<[Entidad]>().Include()` para relaciones
  - Filtra por nombre (case-insensitive) si se proporciona
  - Ordena por nombre
  - Aplica paginación con Skip/Take
  - Proyecta a Response con Select
- Endpoint GET /[entidades] con `[AsParameters] Query query`
- Configuración OpenAPI con `.WithStandardOpenApi<List<Response>>()`

⚠️ RESTRICCIONES:
- Retorna `IQueryable<Response>` desde el servicio
- Filtrado debe ser case-insensitive
- Paginación por defecto: Page=1, Size=25
- Ordenamiento por nombre

📤 FORMATO DE RESPUESTA:
Código completo del archivo Get[Entidades].cs
```

### 4.3 Validación

```bash
# Compilar
dotnet build

# Ejecutar aplicación
dotnet run --project src/webapi

# Probar en Swagger
# Navega a https://localhost:5001/swagger
# Prueba GET /[entidades]/{id} y GET /[entidades]
```

**✅ Criterio de éxito**: Endpoints funcionan en Swagger, retornan datos correctamente.

---

## ✍️ Paso 5: Generar Commands (Escritura)

### 5.1 Prompt para Create[Entidad]

```
🎯 CONTEXTO:
Implemento la funcionalidad de creación de [Entidad].

📂 ARCHIVOS DE REFERENCIA:
1. Clase de dominio: @[src/webapi/features/[entidad]/models/[Entidad].cs]
2. Ejemplo de command: @[src/webapi/features/pizzas/commands/CreatePizza.cs]
3. Guía de estilo: @[.plans/templates/style_guide_examples.md]

🎯 TAREA:
Genera el endpoint de creación.

📋 ENTREGABLES:

**Command**: `src/webapi/features/[entidad]/commands/Create[Entidad].cs`
- Implementa `IFeatureModule`
- Record `Request` con propiedades necesarias para crear (sin ID)
- Record `Response` (igual que en queries)
- Interface `IService` con `HandlerAsync(Request request)`
- Clase `Service` con `[Injectable]` que:
  - Inyecta `IAdd<[Entidad]>`, `IUnitOfWork`
  - Si hay relaciones, inyecta `IGetOrThrowAsync` para lookup
  - Llama a `[Entidad].Create()` con `.ValueOrThrow()`
  - Si hay relaciones, las agrega con métodos del dominio usando `.SuccessOrThrow()`
  - Llama a `repository.Add(entity)`
  - Llama a `unitOfWork.SaveChangesAsync()`
  - Retorna Response mapeado
- Repositorio adaptador `Repository` con `[Injectable]` implementando `IAdd<[Entidad]>`
  - Usa `IRepository.Entry(entity).State = EntityState.Added`
- Endpoint POST /[entidades]
- Configuración OpenAPI con `.WithStandardOpenApi<Response>()` incluyendo:
  - successStatusCode: 201
  - additionalErrorCodes: [422, 404] si hay lookups

⚠️ RESTRICCIONES:
- Usa `Guid.NewGuid()` para generar ID
- Usa `.ValueOrThrow()` y `.SuccessOrThrow()` de ResultExtensions
- NO uses try-catch, las excepciones las maneja GlobalExceptionHandler
- Retorna `Results.Created("", response)`
- Valida existencia de entidades relacionadas con `GetOrThrowAsync`

📤 FORMATO DE RESPUESTA:
Código completo del archivo Create[Entidad].cs
```

### 5.2 Prompt para Update[Entidad]

```
🎯 CONTEXTO:
Implemento la funcionalidad de actualización de [Entidad].

📂 ARCHIVOS DE REFERENCIA:
1. Clase de dominio: @[src/webapi/features/[entidad]/models/[Entidad].cs]
2. Ejemplo de command: @[src/webapi/features/pizzas/commands/UpdatePizza.cs]

🎯 TAREA:
Genera el endpoint de actualización.

📋 ENTREGABLES:

**Command**: `src/webapi/features/[entidad]/commands/Update[Entidad].cs`
- Similar a Create pero:
  - Interface `IService` con `HandlerAsync(Guid id, Request request)`
  - Service inyecta `IUpdate<[Entidad], Guid>` en lugar de `IAdd`
  - Llama a `repository.Get(id)` para obtener entidad existente
  - Llama a método `Update()` del dominio con `.SuccessOrThrow()`
  - Si hay colecciones, sincroniza (elimina viejos, agrega nuevos)
  - NO llama a `Add()`, solo a `SaveChangesAsync()`
  - Repositorio implementa `IUpdate<[Entidad], Guid>`
- Endpoint PUT /[entidades]/{id:guid}
- Configuración OpenAPI con successStatusCode: 200

⚠️ RESTRICCIONES:
- El ID viene del route, no del body
- Usa el método `Update()` del dominio si existe
- Para colecciones, sincroniza correctamente (no solo agrega)
- Retorna `Results.Ok(response)`

📤 FORMATO DE RESPUESTA:
Código completo del archivo Update[Entidad].cs
```

### 5.3 Prompt para Delete[Entidad] (opcional)

```
🎯 CONTEXTO:
Implemento la funcionalidad de eliminación de [Entidad].

📂 ARCHIVOS DE REFERENCIA:
1. Clase de dominio: @[src/webapi/features/[entidad]/models/[Entidad].cs]
2. Patrón de repository: @[src/Fudie/Infrastructure/Repository.cs]

🎯 TAREA:
Genera el endpoint de eliminación.

📋 ENTREGABLES:

**Command**: `src/webapi/features/[entidad]/commands/Delete[Entidad].cs`
- Implementa `IFeatureModule`
- Interface `IService` con `HandlerAsync(Guid id)`
- Service inyecta `IRemove<[Entidad], Guid>` y `IUnitOfWork`
- Llama a `repository.Get(id)` para verificar existencia
- Llama a `repository.Remove(entity)`
- Llama a `unitOfWork.SaveChangesAsync()`
- Repositorio implementa `IRemove<[Entidad], Guid>`
  - Método `Remove()` usa `Entry(entity).State = EntityState.Deleted`
- Endpoint DELETE /[entidades]/{id:guid}
- Retorna `Results.NoContent()`

📤 FORMATO DE RESPUESTA:
Código completo del archivo Delete[Entidad].cs
```

### 5.4 Validación

```bash
# Compilar
dotnet build

# Ejecutar aplicación
dotnet run --project src/webapi

# Probar en Swagger
# POST /[entidades] - Crear
# PUT /[entidades]/{id} - Actualizar
# DELETE /[entidades]/{id} - Eliminar
```

**✅ Criterio de éxito**: Todos los endpoints funcionan, validaciones se ejecutan correctamente.

---

## 🧪 Paso 6: Generar Tests de Integración

### 6.1 Prompt para Tests de Integración

```
🎯 CONTEXTO:
Necesito tests de integración completos para [Entidad].

📂 ARCHIVOS DE REFERENCIA:
1. Commands y Queries: @[src/webapi/features/[entidad]/]
2. Ejemplo de tests: @[tests/WebApi.IntegrationTests/Features/Ingredients/]
3. Guía de estilo: @[.plans/templates/style_guide_examples.md]

🎯 TAREA:
Genera tests de integración para todos los endpoints.

📋 ENTREGABLES:

1. **Create[Entidad]Tests.cs**: `tests/WebApi.IntegrationTests/Features/[Entidad]/Create[Entidad]Tests.cs`
   - Clase con `IClassFixture<WebApplicationFactory<Program>>`
   - Constructor que configura InMemoryDatabase único
   - Test: `Create[Entidad]_WithValidData_ShouldReturnCreated()`
   - Test: `Create[Entidad]_WithInvalidData_ShouldReturnUnprocessableEntity()`
   - Test: `Create[Entidad]_WithNonExistentRelation_ShouldReturnNotFound()` (si aplica)

2. **Get[Entidad]Tests.cs**: Tests para GET por ID
   - Test: `Get[Entidad]_WithExistingId_ShouldReturnOk()`
   - Test: `Get[Entidad]_WithNonExistingId_ShouldReturnNotFound()`

3. **Get[Entidades]Tests.cs**: Tests para GET lista
   - Test: `Get[Entidades]_ShouldReturnOkWithList()`
   - Test: `Get[Entidades]_WithFilter_ShouldReturnFilteredResults()`
   - Test: `Get[Entidades]_WithPagination_ShouldReturnCorrectPage()`

4. **Update[Entidad]Tests.cs**: Tests para PUT
   - Test: `Update[Entidad]_WithValidData_ShouldReturnOk()`
   - Test: `Update[Entidad]_WithInvalidData_ShouldReturnUnprocessableEntity()`
   - Test: `Update[Entidad]_WithNonExistingId_ShouldReturnNotFound()`

⚠️ RESTRICCIONES:
- Usa `WebApplicationFactory<Program>`
- Configura InMemoryDatabase con nombre único: `"TestDatabase_" + Guid.NewGuid()`
- Re-registra interfaces de DbContext si es necesario
- Usa `PostAsJsonAsync`, `PutAsJsonAsync`, `DeleteAsync`
- Usa `ReadFromJsonAsync<T>()` para deserializar respuestas
- Usa FluentAssertions para asserts
- Cada test debe ser independiente (no compartir datos)

📤 FORMATO DE RESPUESTA:
Código completo de todos los archivos de test.
```

### 6.2 Validación

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar solo tests de integración de [Entidad]
dotnet test --filter "FullyQualifiedName~[Entidad]"

# Ver cobertura (opcional)
dotnet test /p:CollectCoverage=true
```

**✅ Criterio de éxito**: Todos los tests pasan (unitarios + integración).

---

## ✅ Paso 7: Validación Final

### 7.1 Checklist Completo

Ejecuta este checklist antes de considerar la feature completa:

```bash
# 1. Compilación limpia
dotnet clean
dotnet build
# ✅ Sin errores ni warnings

# 2. Todos los tests pasan
dotnet test
# ✅ 100% tests pasando

# 3. Aplicación inicia correctamente
dotnet run --project src/webapi
# ✅ Sin errores en consola

# 4. Swagger funciona
# Navega a https://localhost:5001/swagger
# ✅ Todos los endpoints visibles y documentados

# 5. Pruebas manuales en Swagger
# ✅ POST /[entidades] - Crear con datos válidos → 201
# ✅ POST /[entidades] - Crear con datos inválidos → 422
# ✅ GET /[entidades]/{id} - ID existente → 200
# ✅ GET /[entidades]/{id} - ID inexistente → 404
# ✅ GET /[entidades] - Lista → 200
# ✅ PUT /[entidades]/{id} - Actualizar → 200
# ✅ DELETE /[entidades]/{id} - Eliminar → 204

# 6. Verificar estructura de archivos
```

### 7.2 Estructura de Archivos Esperada

```
src/webapi/features/[entidad]/
├── models/
│   └── [Entidad].cs                          ✅
├── queries/
│   ├── Get[Entidad].cs                       ✅
│   └── Get[Entidades].cs                     ✅
└── commands/
    ├── Create[Entidad].cs                    ✅
    ├── Update[Entidad].cs                    ✅
    └── Delete[Entidad].cs                    ✅ (opcional)

src/webapi/infrastructure/Configurations/
└── [Entidad]Configuration.cs                 ✅

tests/WebApi.UnitTests/Features/[Entidad]/
└── [Entidad]Tests.cs                         ✅

tests/WebApi.IntegrationTests/Features/[Entidad]/
├── Create[Entidad]Tests.cs                   ✅
├── Get[Entidad]Tests.cs                      ✅
├── Get[Entidades]Tests.cs                    ✅
└── Update[Entidad]Tests.cs                   ✅

domain-specs/
└── [Entidad].md                              ✅
```

---

## 📊 Checklist de Validación

### ✅ Dominio
- [ ] Clase hereda de `Entity`
- [ ] Constructor protegido
- [ ] Factory method `Create()` retorna `Result<T>`
- [ ] Métodos de comportamiento retornan `Result`
- [ ] Validador anidado con FluentValidation
- [ ] Colecciones encapsuladas (backing field + IReadOnlyCollection)
- [ ] Propiedades calculadas son `get only`
- [ ] Tests unitarios cubren todos los casos del Example Mapping
- [ ] Todos los tests unitarios pasan

### ✅ Persistencia
- [ ] Configuración implementa `IEntityTypeConfiguration<T>`
- [ ] Tabla configurada con `ToTable()`
- [ ] Clave primaria configurada con `HasKey()`
- [ ] Propiedades configuradas (Required, MaxLength)
- [ ] Relaciones configuradas (HasMany, WithMany, etc.)
- [ ] DbSet agregado a ApplicationDbContext
- [ ] Aplicación inicia sin errores de EF Core

### ✅ Queries (Lectura)
- [ ] Get[Entidad] implementa `IFeatureModule`
- [ ] Endpoint GET /[entidades]/{id:guid}
- [ ] Usa `IGetOrThrowAsync` con `tracking: false`
- [ ] Incluye propiedades relacionadas con `includeProperties`
- [ ] Response incluye todas las propiedades
- [ ] Configuración OpenAPI completa
- [ ] Get[Entidades] tiene paginación (Page, Size)
- [ ] Filtrado por nombre (case-insensitive)
- [ ] Ordenamiento por nombre
- [ ] Endpoints funcionan en Swagger

### ✅ Commands (Escritura)
- [ ] Create[Entidad] implementa `IFeatureModule`
- [ ] Endpoint POST /[entidades]
- [ ] Usa `[Entidad].Create().ValueOrThrow()`
- [ ] Valida relaciones con `GetOrThrowAsync`
- [ ] Usa `IAdd<T>` y `IUnitOfWork`
- [ ] Retorna `Results.Created("", response)`
- [ ] Update[Entidad] usa `IUpdate<T, ID>`
- [ ] Endpoint PUT /[entidades]/{id:guid}
- [ ] Sincroniza colecciones correctamente
- [ ] Delete[Entidad] usa `IRemove<T, ID>` (si aplica)
- [ ] Endpoint DELETE /[entidades]/{id:guid}
- [ ] Retorna `Results.NoContent()`
- [ ] Todos los commands funcionan en Swagger

### ✅ Tests de Integración
- [ ] Usa `WebApplicationFactory<Program>`
- [ ] InMemoryDatabase con nombre único
- [ ] Tests para Create (válido, inválido, relación inexistente)
- [ ] Tests para Get por ID (existente, inexistente)
- [ ] Tests para Get lista (sin filtro, con filtro, paginación)
- [ ] Tests para Update (válido, inválido, inexistente)
- [ ] Tests para Delete (existente, inexistente)
- [ ] Todos los tests de integración pasan

### ✅ Calidad de Código
- [ ] Sin warnings de compilación
- [ ] Namespaces consistentes
- [ ] Nombres descriptivos
- [ ] Código formateado correctamente
- [ ] Sin código comentado
- [ ] Sin TODOs pendientes

---

## 🚀 Tips para Desarrolladores Junior

### 1. **Siempre sigue el orden de los pasos**
No saltes pasos. Cada paso valida el anterior.

### 2. **Compila después de cada paso**
No acumules cambios sin compilar.

### 3. **Lee los errores de compilación con atención**
La IA puede cometer errores. Aprende a identificarlos.

### 4. **Usa los ejemplos existentes como referencia**
Si tienes dudas, mira cómo está hecho en Pizza o Ingredient.

### 5. **Ejecuta los tests frecuentemente**
Los tests son tu red de seguridad.

### 6. **Pide ayuda si te atascas**
Si después de 3 intentos con la IA no funciona, pide ayuda a un senior.

### 7. **Documenta problemas encontrados**
Si encuentras un patrón que no funciona, documéntalo para mejorar las plantillas.

---

## 🎓 Recursos Adicionales

- **Análisis del Proyecto**: [ANALISIS_PROYECTO_FUDIE.md](./ANALISIS_PROYECTO_FUDIE.md)
- **Guía de Estilo**: [style_guide_examples.md](./templates/style_guide_examples.md)
- **Plantilla de Dominio**: [domain_definition_template.md](./templates/domain_definition_template.md)
- **Ejemplos Reales**: `src/webapi/features/pizzas/` y `src/webapi/features/ingredients/`

---

## 📞 Soporte

Si tienes problemas:

1. Revisa el checklist de validación
2. Compara con los ejemplos existentes (Pizza, Ingredient)
3. Revisa el análisis del proyecto
4. Pide ayuda en el canal de desarrollo

---

**¡Buena suerte! 🚀**
