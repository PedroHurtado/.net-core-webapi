# Tarea 1: Agregar Fudie a la Solución

## Objetivo
Agregar el proyecto `Fudie.csproj` a la solución `webapi.sln` para que sea reconocido por Visual Studio y el sistema de build.

---

## Estado
- [x] No iniciado
- [x] En progreso
- [x] Completado
- [x] Verificado

---

## Pasos a Seguir

### 1. Abrir la solución
```bash
cd "c:\Users\Home\Documents\Cursos 2025\11-noviembre\03-Asp .met core\webapi"
```

### 2. Agregar el proyecto a la solución
```bash
dotnet sln webapi.sln add src/Fudie/Fudie.csproj
```

### 3. Verificar que se agregó correctamente
```bash
dotnet sln webapi.sln list
```

**Salida esperada:**
```
Proyectos
---------
src\webapi\webapi.csproj
src\generator\CodeGenerator.csproj
src\Fudie\Fudie.csproj          ← NUEVO
tests\...
```

---

## Verificación

### Comando de verificación:
```bash
dotnet sln webapi.sln list | grep -i fudie
```

### Resultado esperado:
Debe mostrar la línea con `src\Fudie\Fudie.csproj`

---

## Problemas Comunes

### Error: "No se encuentra el archivo .csproj"
**Solución:** Verificar que la ruta sea correcta:
```bash
ls src/Fudie/Fudie.csproj
```

### Error: "El proyecto ya existe en la solución"
**Solución:** El proyecto ya fue agregado, continuar con la siguiente tarea.

---

## Notas
- Este paso es necesario para que Visual Studio reconozca el proyecto
- Permite compilar toda la solución con un solo comando
- Facilita la gestión de dependencias entre proyectos
