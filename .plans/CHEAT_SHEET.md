# 🚀 Cheat Sheet - Desarrollo con IA

**Referencia rápida para desarrollar features completas en 30-60 minutos**

---

## 📋 Checklist Rápido

```
□ 1. Define dominio (domain-specs/[Entidad].md)
□ 2. Copia Prompt 1 → Genera dominio + tests
□ 3. Copia Prompt 2 → Genera persistencia
□ 4. Copia Prompt 3 → Genera Get por ID
□ 5. Copia Prompt 4 → Genera Get lista
□ 6. Copia Prompt 5 → Genera Create
□ 7. Copia Prompt 6 → Genera Update
□ 8. Copia Prompt 7 → Genera Delete (opcional)
□ 9. Copia Prompt 8 → Genera tests integración
□ 10. Ejecuta: .\validate-feature.ps1 -Entity "[Entidad]"
□ 11. Copia Prompt 9 → Revisa y optimiza
□ 12. Commit
```

---

## 🤖 Prompts ([AI_PROMPTS.md](./AI_PROMPTS.md))

### Prompt 1: Dominio + Tests
```
Variables: [Entidad], [entidad]
Genera: models/[Entidad].cs + [Entidad]Tests.cs
Valida: dotnet test
```

### Prompt 2: Persistencia
```
Genera: Configurations/[Entidad]Configuration.cs
Actualiza: ApplicationDbContext.cs
Valida: dotnet build
```

### Prompt 3-4: Queries
```
Genera: queries/Get[Entidad].cs + Get[Entidades].cs
Valida: Swagger
```

### Prompt 5-7: Commands
```
Genera: commands/Create[Entidad].cs + Update[Entidad].cs + Delete[Entidad].cs
Valida: Swagger
```

### Prompt 8: Tests Integración
```
Genera: IntegrationTests/Features/[Entidad]/*Tests.cs
Valida: dotnet test
```

### Prompt 9: Code Review
```
Revisa: Todo el código generado
Optimiza: Según SOLID y Clean Code
```

---

## 🛠️ Comandos Útiles

### Compilación
```bash
dotnet clean
dotnet build
```

### Tests
```bash
# Todos los tests
dotnet test

# Tests de una entidad
dotnet test --filter "FullyQualifiedName~[Entidad]"

# Tests unitarios
dotnet test tests/WebApi.UnitTests/

# Tests de integración
dotnet test tests/WebApi.IntegrationTests/
```

### Ejecución
```bash
# Ejecutar aplicación
dotnet run --project src/webapi

# Swagger
https://localhost:5001/swagger
```

### Validación
```bash
# Windows
.\validate-feature.ps1 -Entity "Pedido"

# Linux/Mac
./validate-feature.sh Pedido
```

---

## 📂 Estructura de Archivos

```
src/webapi/features/[entidad]/
├── models/[Entidad].cs                    ← Prompt 1
├── queries/
│   ├── Get[Entidad].cs                    ← Prompt 3
│   └── Get[Entidades].cs                  ← Prompt 4
└── commands/
    ├── Create[Entidad].cs                 ← Prompt 5
    ├── Update[Entidad].cs                 ← Prompt 6
    └── Delete[Entidad].cs                 ← Prompt 7

src/webapi/infrastructure/Configurations/
└── [Entidad]Configuration.cs              ← Prompt 2

tests/WebApi.UnitTests/Features/[Entidad]/
└── [Entidad]Tests.cs                      ← Prompt 1

tests/WebApi.IntegrationTests/Features/[Entidad]/
├── Create[Entidad]Tests.cs                ← Prompt 8
├── Get[Entidad]Tests.cs                   ← Prompt 8
├── Get[Entidades]Tests.cs                 ← Prompt 8
└── Update[Entidad]Tests.cs                ← Prompt 8

domain-specs/
└── [Entidad].md                           ← Manual
```

---

## 🎯 Patrones Clave

### Dominio
```csharp
public class [Entidad] : Entity
{
    public string Name { get; protected set; }
    public IReadOnlyCollection<Item> Items => _items.ToList().AsReadOnly();
    protected HashSet<Item> _items = [];
    
    protected [Entidad](Guid id, string name) : base(id) { }
    
    public static Result<[Entidad]> Create(Guid id, string name)
    {
        var entity = new [Entidad](id, name);
        var validation = ValidateEntity(entity, new [Entidad]Validator());
        return validation.IsFailure 
            ? Result<[Entidad]>.Failure(validation.Errors) 
            : Result<[Entidad]>.Success(entity);
    }
    
    protected class [Entidad]Validator : AbstractValidator<[Entidad]> { }
}
```

### Query
```csharp
public class Get[Entidad] : IFeatureModule
{
    public record Response(...);
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/[entidades]/{id:guid}", async (Guid id, IGet<[Entidad], Guid> repo) =>
        {
            var entity = await repo.Get(id);
            return Results.Ok(new Response(...));
        })
        .WithOpenApi()...;
    }
    
    [Injectable]
    public class Repository(IGetOrThrowAsync repo) : IGet<[Entidad], Guid> { }
}
```

### Command
```csharp
public class Create[Entidad] : IFeatureModule
{
    public record Request(...);
    public record Response(...);
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/[entidades]", async (IService service, Request request) =>
        {
            var response = await service.HandlerAsync(request);
            return Results.Created("", response);
        })
        .WithStandardOpenApi<Response>()...;
    }
    
    public interface IService { Task<Response> HandlerAsync(Request request); }
    
    [Injectable]
    public class Service(IAdd<[Entidad]> repo, IUnitOfWork uow) : IService
    {
        public async Task<Response> HandlerAsync(Request request)
        {
            var entity = [Entidad].Create(...).ValueOrThrow();
            repo.Add(entity);
            await uow.SaveChangesAsync();
            return new Response(...);
        }
    }
    
    [Injectable]
    public class Repository(IRepository repo) : IAdd<[Entidad]>
    {
        public void Add([Entidad] entity)
        {
            repo.Entry(entity).State = EntityState.Added;
        }
    }
}
```

---

## ✅ Validación Rápida

### Después de Cada Prompt

```bash
# Compila?
dotnet build
# ✅ Sin errores

# Tests pasan?
dotnet test
# ✅ 100% pasando
```

### Antes de Commit

```bash
# Validación completa
.\validate-feature.ps1 -Entity "[Entidad]"
# ✅ Todos los checks verdes
```

---

## 🚨 Troubleshooting

### No compila
```
1. Verifica namespaces
2. Compara con Pizza.cs
3. Revisa using statements
```

### Tests fallan
```
1. Lee el mensaje de error
2. Verifica Example Mapping
3. Compara con PizzaTests.cs
```

### Endpoints no aparecen en Swagger
```
1. Verifica que implementa IFeatureModule
2. Verifica AddRoutes()
3. Reinicia la aplicación
```

### Validación falla
```
1. Lee qué archivo falta
2. Ejecuta comando manualmente
3. Corrige el problema
4. Vuelve a validar
```

---

## 📊 Métricas de Éxito

| Aspecto | Objetivo |
|---------|----------|
| Tiempo | 30-60 min |
| Compilación | 0 errores |
| Tests | 100% pasando |
| Endpoints | Funcionan en Swagger |
| Validaciones | Retornan 422 |
| Errores 404 | Cuando no existe |

---

## 🎓 Niveles

### Junior (60-90 min)
```
□ Lee [AUTOMATION_GUIDE.md](./AUTOMATION_GUIDE.md) completo
□ Sigue paso a paso
□ Valida después de cada paso
```

### Mid (45-60 min)
```
□ Lee [WORKFLOW.md](./WORKFLOW.md) detallado
□ Usa prompts directamente
□ Valida al final
```

### Senior (30-45 min)
```
□ Lee [WORKFLOW.md](./WORKFLOW.md) rápido
□ Prompts directos
□ Validación final
```

---

## 📚 Documentos Clave

| Documento | Cuándo Usar |
|-----------|-------------|
| [README.md](./README.md) | Primera vez |
| [WORKFLOW.md](./WORKFLOW.md) | Siempre |
| [AI_PROMPTS.md](./AI_PROMPTS.md) | Cada paso |
| [AUTOMATION_GUIDE.md](./AUTOMATION_GUIDE.md) | Dudas |
| [ANALISIS_PROYECTO_FUDIE.md](./ANALISIS_PROYECTO_FUDIE.md) | Referencia |

---

## 🔗 Links Rápidos

```
Swagger:     https://localhost:5001/swagger
Health:      https://localhost:5001/health
Docs:        .plans/README.md
Prompts:     .plans/AI_PROMPTS.md
Workflow:    .plans/WORKFLOW.md
```

---

## 💡 Tips

1. **Siempre adjunta archivos** con `@` en prompts
2. **Reemplaza todas las variables** `[Entidad]`, `[entidad]`
3. **Compila después de cada prompt**
4. **No acumules cambios sin validar**
5. **Usa el script de validación** antes de commit
6. **Compara con Pizza** cuando tengas dudas
7. **Pide ayuda después de 3 intentos**

---

## ⏱️ Timeline Típico

```
00:00 - 00:20  Define dominio
00:20 - 00:25  Prompt 1 (Dominio + Tests)
00:25 - 00:28  Prompt 2 (Persistencia)
00:28 - 00:33  Prompts 3-4 (Queries)
00:33 - 00:43  Prompts 5-7 (Commands)
00:43 - 00:48  Prompt 8 (Tests Integración)
00:48 - 00:50  Validación automática
00:50 - 00:55  Prompt 9 (Revisión)
00:55 - 01:00  Pruebas y commit

Total: 1 hora
```

---

## 🎯 Objetivo

**Desarrollar features completas en 30-60 minutos con:**
- ✅ Código estandarizado
- ✅ Tests completos
- ✅ Validación automática
- ✅ Documentación actualizada

---

**¡Imprime esto y tenlo a mano! 📌**
