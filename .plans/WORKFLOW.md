# 🚀 Workflow de Desarrollo Automatizado

**Objetivo**: Desarrollar features completas (dominio → tests → endpoints) usando IA para automatizar el 90% del trabajo.

---

## 📋 Recursos Principales

| Documento | Propósito |
|-----------|-----------|
| [AUTOMATION_GUIDE.md](./AUTOMATION_GUIDE.md) | Guía completa paso a paso con validaciones |
| [AI_PROMPTS.md](./AI_PROMPTS.md) | Prompts listos para copiar y pegar |
| [ANALISIS_PROYECTO_FUDIE.md](./ANALISIS_PROYECTO_FUDIE.md) | Arquitectura y patrones del proyecto |
| [style_guide_examples.md](./templates/style_guide_examples.md) | Ejemplos de código y estilo |
| [domain_definition_template.md](./templates/domain_definition_template.md) | Plantilla para definir dominio |

---

## ⚡ Flujo Rápido (Para Desarrolladores Experimentados)

### 1️⃣ Definir Dominio (Manual - 15 min)
```bash
# Crear especificación
code domain-specs/[Entidad].md
# Usa: .plans/templates/domain_definition_template.md
```

### 2️⃣ Generar Código (IA - 5 min por paso)
```bash
# Copia prompts de: .plans/AI_PROMPTS.md
# Ejecuta en orden:
# - Prompt 1: Dominio + Tests
# - Prompt 2: Persistencia
# - Prompt 3: Query Get por ID
# - Prompt 4: Query Get Lista
# - Prompt 5: Command Create
# - Prompt 6: Command Update
# - Prompt 7: Command Delete (opcional)
# - Prompt 8: Tests de Integración
```

### 3️⃣ Validar (Automático - 2 min)
```bash
# Windows
.\validate-feature.ps1 -Entity "Pedido"

# Linux/Mac
chmod +x validate-feature.sh
./validate-feature.sh Pedido
```

### 4️⃣ Revisar y Optimizar (IA - 5 min)
```bash
# Usa Prompt 9 de [AI_PROMPTS.md](./AI_PROMPTS.md)
# Revisa sugerencias y aplica mejoras
```

**Tiempo total estimado**: 30-45 minutos por feature completa

---

## 📚 Flujo Detallado (Para Desarrolladores Junior)

### Paso 0: Preparación

1. **Lee la documentación**:
   - [ANALISIS_PROYECTO_FUDIE.md](./ANALISIS_PROYECTO_FUDIE.md) - Entiende la arquitectura
   - [style_guide_examples.md](./templates/style_guide_examples.md) - Aprende los patrones

2. **Crea estructura de carpetas**:
   ```bash
   # Reemplaza [entidad] con tu entidad en minúsculas
   mkdir -p src/webapi/features/[entidad]/models
   mkdir -p src/webapi/features/[entidad]/queries
   mkdir -p src/webapi/features/[entidad]/commands
   mkdir -p tests/WebApi.UnitTests/Features/[Entidad]
   mkdir -p tests/WebApi.IntegrationTests/Features/[Entidad]
   mkdir -p domain-specs
   ```

### Paso 1: Definir Dominio (MANUAL)

**Tiempo estimado**: 15-30 minutos

1. Crea `domain-specs/[Entidad].md`
2. Usa plantilla: [domain_definition_template.md](./templates/domain_definition_template.md)
3. Completa:
   - ✅ Propiedades y validaciones
   - ✅ Relaciones
   - ✅ Event Storming (flujo temporal)
   - ✅ Example Mapping (casos de éxito/fallo)

**Criterio de éxito**: Especificación completa y clara

### Paso 2: Generar Dominio + Tests (IA)

**Tiempo estimado**: 5 minutos

1. Abre [AI_PROMPTS.md](./AI_PROMPTS.md)
2. Copia **Prompt 1: Generar Dominio + Tests Unitarios**
3. Reemplaza `[Entidad]` y `[entidad]`
4. Pega en tu IA (asegúrate de adjuntar archivos con @)
5. Copia el código generado

**Validación**:
```bash
dotnet build
dotnet test tests/WebApi.UnitTests/Features/[Entidad]/[Entidad]Tests.cs
```

**Criterio de éxito**: ✅ Compila sin errores, ✅ Todos los tests pasan

### Paso 3: Generar Persistencia (IA)

**Tiempo estimado**: 3 minutos

1. Usa **Prompt 2: Generar Persistencia**
2. Crea archivo de configuración
3. Actualiza `ApplicationDbContext.cs`

**Validación**:
```bash
dotnet build
dotnet run --project src/webapi
# Debe iniciar sin errores
```

**Criterio de éxito**: ✅ Aplicación inicia correctamente

### Paso 4: Generar Queries (IA)

**Tiempo estimado**: 5 minutos

1. Usa **Prompt 3: Generar Query Get por ID**
2. Usa **Prompt 4: Generar Query Get Lista**

**Validación**:
```bash
dotnet build
dotnet run --project src/webapi
# Probar en Swagger: https://localhost:5001/swagger
```

**Criterio de éxito**: ✅ Endpoints funcionan en Swagger

### Paso 5: Generar Commands (IA)

**Tiempo estimado**: 10 minutos

1. Usa **Prompt 5: Generar Command Create**
2. Usa **Prompt 6: Generar Command Update**
3. Usa **Prompt 7: Generar Command Delete** (opcional)

**Validación**:
```bash
dotnet build
# Probar en Swagger: POST, PUT, DELETE
```

**Criterio de éxito**: ✅ Todos los endpoints funcionan

### Paso 6: Generar Tests de Integración (IA)

**Tiempo estimado**: 5 minutos

1. Usa **Prompt 8: Generar Tests de Integración**

**Validación**:
```bash
dotnet test
```

**Criterio de éxito**: ✅ Todos los tests pasan (unitarios + integración)

### Paso 7: Validación Automática

**Tiempo estimado**: 2 minutos

```bash
# Windows
.\validate-feature.ps1 -Entity "Pedido"

# Linux/Mac
./validate-feature.sh Pedido
```

**Criterio de éxito**: ✅ Script reporta validación exitosa

### Paso 8: Revisión y Optimización (IA)

**Tiempo estimado**: 5 minutos

1. Usa **Prompt 9: Revisar y Optimizar**
2. Aplica sugerencias críticas
3. Considera sugerencias opcionales

**Criterio de éxito**: ✅ Código revisado y optimizado

---

## ✅ Checklist Final

Antes de considerar la feature completa:

### Estructura de Archivos
- [ ] `src/webapi/features/[entidad]/models/[Entidad].cs`
- [ ] `src/webapi/features/[entidad]/queries/Get[Entidad].cs`
- [ ] `src/webapi/features/[entidad]/queries/Get[Entidades].cs`
- [ ] `src/webapi/features/[entidad]/commands/Create[Entidad].cs`
- [ ] `src/webapi/features/[entidad]/commands/Update[Entidad].cs`
- [ ] `src/webapi/infrastructure/Configurations/[Entidad]Configuration.cs`
- [ ] `tests/WebApi.UnitTests/Features/[Entidad]/[Entidad]Tests.cs`
- [ ] `tests/WebApi.IntegrationTests/Features/[Entidad]/Create[Entidad]Tests.cs`
- [ ] `tests/WebApi.IntegrationTests/Features/[Entidad]/Get[Entidad]Tests.cs`
- [ ] `tests/WebApi.IntegrationTests/Features/[Entidad]/Get[Entidades]Tests.cs`
- [ ] `tests/WebApi.IntegrationTests/Features/[Entidad]/Update[Entidad]Tests.cs`

### Compilación y Tests
- [ ] `dotnet build` - Sin errores ni warnings
- [ ] `dotnet test` - Todos los tests pasan
- [ ] Aplicación inicia sin errores

### Funcionalidad
- [ ] GET /[entidades]/{id} - Funciona en Swagger
- [ ] GET /[entidades] - Funciona con paginación y filtrado
- [ ] POST /[entidades] - Crea correctamente
- [ ] PUT /[entidades]/{id} - Actualiza correctamente
- [ ] Validaciones funcionan (retorna 422 con datos inválidos)
- [ ] Errores 404 cuando no existe

### Calidad de Código
- [ ] Sigue patrones del proyecto (Vertical Slice, CQRS, Result)
- [ ] Nombres descriptivos y consistentes
- [ ] Sin código duplicado
- [ ] Sin TODOs pendientes

---

## 🎯 Métricas de Éxito

| Métrica | Objetivo |
|---------|----------|
| **Tiempo por feature** | 30-45 minutos |
| **Cobertura de tests** | 100% de casos del Example Mapping |
| **Compilación** | 0 errores, 0 warnings |
| **Tests pasando** | 100% |
| **Código generado por IA** | ~90% |
| **Código manual** | ~10% (solo definición de dominio) |

---

## 🚨 Problemas Comunes y Soluciones

### La IA genera código que no compila

**Solución**:
1. Verifica que adjuntaste los archivos de referencia con `@`
2. Compara con ejemplos existentes (Pizza, Ingredient)
3. Pide a la IA que corrija específicamente el error de compilación

### Los tests no pasan

**Solución**:
1. Lee el mensaje de error con atención
2. Verifica que el Example Mapping esté completo
3. Compara con tests existentes
4. Pide a la IA que corrija el test específico

### El script de validación falla

**Solución**:
1. Lee qué archivo falta o qué test falló
2. Ejecuta manualmente el comando que falló
3. Corrige el problema específico
4. Vuelve a ejecutar el script

### La aplicación no inicia

**Solución**:
1. Verifica que agregaste el `DbSet` a `ApplicationDbContext`
2. Verifica la configuración de EF Core
3. Revisa la consola para ver el error específico

---

## 📞 Soporte

Si te atascas:

1. **Revisa la documentación**:
   - [AUTOMATION_GUIDE.md](./AUTOMATION_GUIDE.md) - Guía detallada
   - [AI_PROMPTS.md](./AI_PROMPTS.md) - Prompts optimizados
   - [ANALISIS_PROYECTO_FUDIE.md](./ANALISIS_PROYECTO_FUDIE.md) - Arquitectura

2. **Compara con ejemplos**:
   - `src/webapi/features/pizzas/` - Feature completa
   - `src/webapi/features/ingredients/` - Otro ejemplo

3. **Ejecuta validación**:
   ```bash
   .\validate-feature.ps1 -Entity "Pedido"
   ```

4. **Pide ayuda**:
   - Después de 3 intentos fallidos
   - Con información específica del error
   - Con lo que ya intentaste

---

## 🎓 Aprendizaje Continuo

### Para Juniors

1. **Semana 1-2**: Sigue el flujo detallado al pie de la letra
2. **Semana 3-4**: Empieza a entender por qué funciona cada paso
3. **Mes 2**: Usa el flujo rápido, ya entiendes los patrones
4. **Mes 3+**: Puedes modificar prompts y optimizar el proceso

### Para Seniors

1. Usa el flujo rápido desde el inicio
2. Personaliza prompts según necesidades
3. Mejora las plantillas cuando encuentres patrones
4. Ayuda a juniors a entender el "por qué"

---

## 🔄 Mejora Continua

Si encuentras:
- **Patrón que no funciona**: Documéntalo y propón mejora
- **Prompt que genera código incorrecto**: Ajústalo y comparte
- **Validación que falta**: Agrégala al script
- **Documentación confusa**: Clarifica y actualiza

---

## 📊 Ejemplo de Timeline

**Feature: Pedido (entidad compleja con relaciones)**

| Paso | Tiempo | Acumulado |
|------|--------|-----------|
| 0. Preparación | 5 min | 5 min |
| 1. Definir dominio | 20 min | 25 min |
| 2. Dominio + Tests | 5 min | 30 min |
| 3. Persistencia | 3 min | 33 min |
| 4. Queries | 5 min | 38 min |
| 5. Commands | 10 min | 48 min |
| 6. Tests integración | 5 min | 53 min |
| 7. Validación | 2 min | 55 min |
| 8. Revisión | 5 min | 60 min |

**Total**: ~1 hora para feature completa con tests

---

## 🎉 ¡Listo para Empezar!

1. Lee [AUTOMATION_GUIDE.md](./AUTOMATION_GUIDE.md) completo (primera vez)
2. Abre [AI_PROMPTS.md](./AI_PROMPTS.md) en una pestaña
3. Crea tu `domain-specs/[Entidad].md`
4. ¡Empieza a generar código!

**¡Buena suerte! 🚀**
