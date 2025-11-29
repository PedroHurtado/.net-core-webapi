# Fudie

**Biblioteca de clases reutilizable para APIs ASP.NET Core**

---

## 📖 Descripción

Fudie es una biblioteca .NET que proporciona funcionalidad común y reutilizable para aplicaciones web ASP.NET Core, incluyendo:

- 🎯 **Patrón Result** - Manejo funcional de resultados y errores
- 🔧 **Inyección de Dependencias** - Sistema de registro automático con atributos
- 🏗️ **Domain-Driven Design** - Clases base para entidades de dominio
- 📦 **Repository Pattern** - Interfaces para acceso a datos
- 🚨 **Manejo Global de Excepciones** - Handler centralizado de errores
- 📝 **OpenAPI/Swagger** - Extensiones para documentación automática

---

## 🚀 Instalación

### Como Referencia de Proyecto

```xml
<ItemGroup>
  <ProjectReference Include="..\Fudie\Fudie.csproj" />
</ItemGroup>
```

---

## 📦 Dependencias

- **.NET 8.0**
- **FluentValidation** 12.1.0
- **Microsoft.AspNetCore.*** (Http, Diagnostics, OpenApi)
- **Swashbuckle.AspNetCore.SwaggerGen** 6.4.0
- **Microsoft.EntityFrameworkCore** 8.0.0

---

## 📂 Estructura

```
Fudie/
├── Fudie                          (Namespace raíz)
│   ├── Result.cs                  - Patrón Result
│   ├── ResultExtensions.cs        - Extensiones para Result
│   ├── GlobalExceptionHandler.cs  - Manejo global de excepciones
│   ├── IFeatureModule.cs          - Interface para módulos de features
│   └── RouteExtension.cs          - Extensiones de routing
│
├── Fudie.DependencyInjection
│   ├── Injectable.cs              - Atributo para DI automática
│   └── InjectionExtension.cs      - Extensiones de IServiceCollection
│
├── Fudie.Domain
│   └── Entity.cs                  - Clase base para entidades
│
├── Fudie.Infrastructure
│   └── Repository.cs              - Interfaces de repositorio
│
└── Fudie.OpenApi
    ├── CustomProblemDetails.cs    - Modelo de errores RFC 7807
    └── EndPointExtensions.cs      - Extensiones para endpoints
```

---

## 🎯 Uso

### 1. Patrón Result

```csharp
using Fudie;

public Result<User> GetUser(Guid id)
{
    if (id == Guid.Empty)
        return Result<User>.Failure("ID inválido", nameof(id));
    
    var user = _repository.Find(id);
    return Result<User>.Success(user);
}

// Uso
var result = GetUser(userId);
if (result.IsSuccess)
{
    var user = result.Value;
}
else
{
    var errors = result.Errors;
}
```

### 2. Inyección de Dependencias Automática

```csharp
using Fudie.DependencyInjection;

[Injectable(ServiceLifetime.Scoped)]
public class UserService : IUserService
{
    // Se registra automáticamente
}

// En Program.cs
builder.Services.AddInjectables(typeof(Program).Assembly);
```

### 3. Entidades de Dominio

```csharp
using Fudie.Domain;
using FluentValidation;

public class User : Entity
{
    public string Name { get; private set; }
    
    private User(Guid id, string name) : base(id)
    {
        Name = name;
    }
    
    public static Result<User> Create(Guid id, string name)
    {
        var user = new User(id, name);
        var validator = new UserValidator();
        var validationResult = ValidateEntity(user, validator);
        
        return validationResult.IsSuccess 
            ? Result<User>.Success(user)
            : Result<User>.Failure(validationResult.Errors);
    }
}
```

### 4. Manejo Global de Excepciones

```csharp
// En Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
```

### 5. Extensiones OpenAPI

```csharp
using Fudie.OpenApi;

app.MapGet("/users/{id}", async (Guid id) => 
{
    // ...
})
.WithStandardOpenApi<UserDto>(
    name: "GetUser",
    summary: "Obtener usuario por ID",
    description: "Retorna un usuario específico",
    tag: "Users"
);
```

### 6. Feature Modules

```csharp
using Fudie;

public class UserFeature : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");
        
        group.MapGet("/{id}", GetUser);
        group.MapPost("/", CreateUser);
    }
}

// En Program.cs
app.MapFeatures();
```

---

## 🔧 Configuración

### Program.cs Completo

```csharp
using Fudie;
using Fudie.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios con [Injectable]
builder.Services.AddInjectables(typeof(Program).Assembly);

// Manejo de excepciones
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<GlobalErrorResponsesOperationFilter>();
});

var app = builder.Build();

// Middleware
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapear features automáticamente
app.MapFeatures();

app.Run();
```

---

## 📝 Respuestas de Error

Todos los errores siguen el estándar **RFC 7807 Problem Details**:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "El ID no puede estar vacío",
  "instance": "/api/users/00000000-0000-0000-0000-000000000000",
  "traceId": "0HN1234567890",
  "timestamp": "2025-11-29T10:30:00Z",
  "errors": {
    "id": ["El ID no puede estar vacío"]
  }
}
```

---

## 🎨 Características

### ✅ Patrón Result
- Manejo funcional de errores
- Sin excepciones para flujo de negocio
- Composable y testeable

### ✅ Dependency Injection
- Registro automático con atributos
- Soporte para Transient, Scoped, Singleton
- Registro de interfaces automático

### ✅ Domain-Driven Design
- Entidades con identidad
- Validación integrada
- Inmutabilidad por defecto

### ✅ Repository Pattern
- Interfaces segregadas (IGet, IAdd, IUpdate, IRemove)
- Soporte para Unit of Work
- Query genérico

### ✅ Global Exception Handling
- Manejo centralizado de excepciones
- Respuestas consistentes
- Logging automático

### ✅ OpenAPI/Swagger
- Documentación automática de errores
- Extensiones para endpoints
- Modelos de error estandarizados

---

## 🧪 Testing

```csharp
[Fact]
public void Result_Success_ShouldHaveValue()
{
    var result = Result<int>.Success(42);
    
    Assert.True(result.IsSuccess);
    Assert.Equal(42, result.Value);
}

[Fact]
public void Result_Failure_ShouldHaveErrors()
{
    var result = Result<int>.Failure("Error de prueba");
    
    Assert.True(result.IsFailure);
    Assert.NotEmpty(result.Errors);
}
```

---

## 📄 Licencia

Este proyecto es parte del ecosistema Fudie.

---

## 🤝 Contribución

Para contribuir al proyecto:
1. Mantener la estructura de namespaces
2. Seguir los patrones establecidos
3. Agregar tests para nueva funcionalidad
4. Documentar cambios en este README

---

## 📚 Recursos

- [RFC 7807 - Problem Details](https://tools.ietf.org/html/rfc7807)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

---

**Versión:** 1.0.0  
**Target Framework:** .NET 8.0  
**Fecha de Creación:** 2025-11-29
