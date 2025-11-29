# Tarea 2: Actualizar Referencias del Proyecto webapi

## Objetivo
Agregar la referencia al proyecto `Fudie` en el archivo `webapi.csproj` para que el proyecto web pueda usar las clases del DLL.

---

## Estado
- [ ] No iniciado
- [ ] En progreso
- [x] Completado
- [x] Verificado

---

## Pasos a Seguir

### 1. Abrir el archivo webapi.csproj
```bash
code "src/webapi/webapi.csproj"
```

### 2. Agregar la referencia al proyecto Fudie

Buscar la sección `<ItemGroup>` que contiene las referencias a proyectos (donde está `CodeGenerator.csproj`).

**Agregar esta línea:**
```xml
<ProjectReference Include="..\Fudie\Fudie.csproj" />
```

**Resultado esperado:**
```xml
<ItemGroup>
  <ProjectReference Include="..\Fudie\Fudie.csproj" />
  <ProjectReference Include="..\generator\CodeGenerator.csproj"
    OutputItemType="Analyzer"
    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 3. Guardar el archivo

---

## Verificación

### Comando de verificación:
```bash
dotnet list src/webapi/webapi.csproj reference
```

**Salida esperada:**
```
Referencia de proyectos
-----------------------
..\Fudie\Fudie.csproj
..\generator\CodeGenerator.csproj
```

### Compilación de prueba:
```bash
dotnet build src/webapi/webapi.csproj
```

Si hay errores relacionados con namespaces, es normal. Se resolverán en la siguiente tarea.

---

## Archivo Completo Esperado

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.1.0" />
    <PackageReference Include="Microsoft.AspNetCore.Diagnostics.HealthChecks" Version="2.2.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Fudie\Fudie.csproj" />
    <ProjectReference Include="..\generator\CodeGenerator.csproj"
      OutputItemType="Analyzer"
      ReferenceOutputAssembly="false" />
  </ItemGroup>

</Project>
```

---

## Problemas Comunes

### Error: "No se puede resolver la referencia"
**Solución:** Verificar que la ruta relativa sea correcta:
```bash
ls src/Fudie/Fudie.csproj
```

### Error de compilación por namespaces
**Solución:** Es esperado. Se resolverá en la tarea 3 (Actualizar Namespaces).

---

## Notas
- Esta referencia permite que `webapi` use todas las clases públicas de `Fudie`
- Las dependencias de `Fudie` se heredan automáticamente
- No es necesario duplicar los paquetes NuGet que ya están en `Fudie`
