# Tarea 5: Limpieza Final

## Objetivo
Eliminar la carpeta `common/` del proyecto `webapi` y limpiar archivos temporales generados durante el desarrollo.

---

## Estado
- [ ] No iniciado
- [ ] En progreso
- [ ] Completado
- [ ] Verificado

---

## ⚠️ IMPORTANTE - Verificar Antes de Eliminar

**NO proceder con esta tarea hasta que:**
- ✅ Todas las tareas anteriores estén completadas
- ✅ La compilación sea exitosa
- ✅ Todos los tests pasen
- ✅ La aplicación funcione correctamente

---

## Pasos de Limpieza

### 1. Verificación Final Pre-Eliminación

```bash
cd "c:\Users\Home\Documents\Cursos 2025\11-noviembre\03-Asp .met core\webapi"

# Verificar que no hay referencias a webapi.common
grep -r "webapi\.common" src/webapi/ --include="*.cs" | grep -v "bin\|obj\|common"
```

**Resultado esperado:** Sin resultados

### 2. Crear Backup (Opcional pero Recomendado)

```bash
# Crear backup de la carpeta common
cp -r "src/webapi/common" "src/webapi/common.backup"
```

O comprimir:
```bash
tar -czf common-backup-$(date +%Y%m%d).tar.gz src/webapi/common
```

### 3. Eliminar la Carpeta common/

```bash
# Eliminar la carpeta common
rm -rf "src/webapi/common"
```

**En Windows PowerShell:**
```powershell
Remove-Item -Path "src\webapi\common" -Recurse -Force
```

### 4. Limpiar Archivos Temporales

```bash
# Limpiar builds
dotnet clean

# Eliminar carpetas bin y obj
find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null
find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null
```

**En Windows PowerShell:**
```powershell
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
```

### 5. Restaurar y Recompilar

```bash
# Restaurar paquetes
dotnet restore

# Compilar toda la solución
dotnet build webapi.sln
```

---

## Verificación Post-Eliminación

### 1. Verificar estructura del proyecto
```bash
ls src/webapi/
```

**NO debe aparecer:**
- ❌ `common/`
- ❌ `common.backup/` (si no se quiere mantener)

**Debe aparecer:**
- ✅ `features/`
- ✅ `infrastructure/`
- ✅ `Program.cs`
- ✅ `webapi.csproj`

### 2. Compilación final
```bash
dotnet build webapi.sln
```

**Resultado esperado:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 3. Ejecutar tests finales
```bash
dotnet test
```

**Resultado esperado:**
```
Passed!  - Failed:     0, Passed:    XX, Skipped:     0, Total:    XX
```

### 4. Ejecutar la aplicación
```bash
dotnet run --project src/webapi/webapi.csproj
```

**Verificar:**
- [ ] Inicia sin errores
- [ ] Swagger funciona
- [ ] Endpoints responden

---

## Limpieza de Git (Si aplica)

### 1. Verificar estado de Git
```bash
git status
```

### 2. Agregar cambios
```bash
# Agregar proyecto Fudie
git add src/Fudie/

# Agregar cambios en webapi
git add src/webapi/

# Agregar cambios en solución
git add webapi.sln
```

### 3. Eliminar common/ del repositorio
```bash
git rm -r src/webapi/common/
```

### 4. Commit de la migración
```bash
git commit -m "Migrar carpeta common a proyecto Fudie DLL

- Creado proyecto Fudie como DLL independiente
- Actualizados namespaces de webapi.common a Fudie
- Eliminada carpeta common/ del proyecto webapi
- Todos los tests pasando
"
```

---

## Checklist Final

### Estructura del Proyecto
- [ ] Carpeta `src/Fudie/` existe y contiene todos los archivos
- [ ] Carpeta `src/webapi/common/` ha sido eliminada
- [ ] Archivo `webapi.sln` incluye el proyecto Fudie
- [ ] Archivo `webapi.csproj` referencia a Fudie

### Funcionalidad
- [ ] Compilación exitosa
- [ ] Tests pasando
- [ ] Aplicación funciona correctamente
- [ ] No hay referencias a `webapi.common` en el código

### Documentación
- [ ] Plan de migración completado
- [ ] Todas las tareas marcadas como completadas
- [ ] Cambios documentados (commit, changelog, etc.)

---

## Archivos a Revisar

### Verificar que estos archivos NO contengan `webapi.common`:
```bash
# Program.cs
grep "webapi.common" src/webapi/Program.cs

# Archivos de features
grep -r "webapi.common" src/webapi/features/

# Archivos de infrastructure
grep -r "webapi.common" src/webapi/infrastructure/
```

**Todos deben devolver:** Sin resultados

---

## Rollback (En caso de problemas)

Si algo sale mal después de eliminar `common/`:

### 1. Restaurar desde backup
```bash
cp -r src/webapi/common.backup src/webapi/common
```

### 2. O desde Git
```bash
git checkout src/webapi/common/
```

### 3. Revertir cambios
```bash
git reset --hard HEAD~1
```

---

## Problemas Comunes

### Error: "No se encuentra el tipo X"
**Causa:** Se eliminó `common/` pero aún hay referencias sin actualizar  
**Solución:** Restaurar backup y revisar tarea 3 (Actualizar Namespaces)

### Error de compilación después de eliminar
**Causa:** Falta la referencia a Fudie en webapi.csproj  
**Solución:** Revisar tarea 2 (Actualizar Referencias)

### Git no detecta la eliminación
**Solución:**
```bash
git add -A
git status
```

---

## Resultado Final Esperado

```
webapi/
├── src/
│   ├── Fudie/              ✅ NUEVO - DLL independiente
│   │   ├── Fudie.csproj
│   │   ├── *.cs
│   │   └── ...
│   ├── webapi/
│   │   ├── features/       ✅ Usa Fudie via using
│   │   ├── infrastructure/ ✅ Usa Fudie via using
│   │   ├── Program.cs      ✅ Usa Fudie via using
│   │   └── webapi.csproj   ✅ Referencia a Fudie
│   └── generator/
├── tests/
└── webapi.sln              ✅ Incluye Fudie
```

---

## Métricas de Éxito

✅ **Limpieza completada:**
- Carpeta `common/` eliminada
- Sin archivos temporales
- Repositorio limpio

✅ **Funcionalidad mantenida:**
- Misma funcionalidad que antes
- Todos los tests pasan
- Sin regresiones

✅ **Código mejorado:**
- Mejor organización
- DLL reutilizable
- Namespaces más claros

---

## Notas Finales
- Mantener el backup por al menos una semana
- Documentar cualquier problema encontrado
- Actualizar README del proyecto si es necesario
- Considerar crear un tag de Git para esta versión
