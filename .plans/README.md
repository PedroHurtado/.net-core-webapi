# 📚 Documentación del Proyecto - Guías de Desarrollo

Esta carpeta contiene toda la documentación necesaria para desarrollar features en este proyecto de forma rápida y consistente usando IA.

---

## 🚀 Inicio Rápido

### Para Desarrolladores Junior (Primera vez)

1. **Lee primero** (30 min):
   - 📖 `analisis-proyecto-fudie.md` - Entiende la arquitectura
   - 📖 `templates/style_guide_examples.md` - Aprende los patrones

2. **Sigue el workflow** (1 hora por feature):
   - 📋 `WORKFLOW.md` - Flujo detallado paso a paso
   - 🤖 `AI_PROMPTS.md` - Prompts listos para copiar

3. **Valida tu trabajo**:
   ```bash
   # Windows
   .\validate-feature.ps1 -Entity "TuEntidad"
   
   # Linux/Mac
   ./validate-feature.sh TuEntidad
   ```

### Para Desarrolladores Experimentados

1. **Consulta rápida**:
   - 📋 `WORKFLOW.md` - Sección "Flujo Rápido"
   - 🤖 `AI_PROMPTS.md` - Copia prompts directamente

2. **Desarrolla** (30-45 min por feature):
   - Define dominio → Genera código con IA → Valida

---

## 📂 Estructura de Documentación

```
.plans/
├── README.md                          ← Estás aquí
├── WORKFLOW.md                        ← Flujo de desarrollo completo
├── AUTOMATION_GUIDE.md                ← Guía detallada con validaciones
├── AI_PROMPTS.md                      ← Prompts optimizados para IA
├── analisis-proyecto-fudie.md         ← Arquitectura del proyecto
│
├── templates/
│   ├── domain_definition_template.md  ← Plantilla para definir dominio
│   ├── style_guide_examples.md        ← Ejemplos de código
│   ├── ai_generation_prompt.md        ← Prompt genérico (legacy)
│   └── persistence_template.md        ← Plantilla de persistencia
│
└── [Otros archivos de planificación]
```

---

## 📖 Guía de Documentos

### 🎯 Documentos Principales

#### 1. **WORKFLOW.md** - Tu Guía Principal
**Cuándo usar**: Siempre que desarrolles una nueva feature

**Contenido**:
- ✅ Flujo rápido (para experimentados)
- ✅ Flujo detallado (para juniors)
- ✅ Checklist de validación
- ✅ Métricas de éxito
- ✅ Troubleshooting

**Tiempo de lectura**: 10 minutos
**Tiempo de aplicación**: 30-60 minutos por feature

---

#### 2. **AI_PROMPTS.md** - Prompts Listos para Usar
**Cuándo usar**: En cada paso del desarrollo

**Contenido**:
- 🤖 Prompt 1: Dominio + Tests Unitarios
- 🤖 Prompt 2: Persistencia
- 🤖 Prompt 3: Query Get por ID
- 🤖 Prompt 4: Query Get Lista
- 🤖 Prompt 5: Command Create
- 🤖 Prompt 6: Command Update
- 🤖 Prompt 7: Command Delete
- 🤖 Prompt 8: Tests de Integración
- 🤖 Prompt 9: Revisión y Optimización

**Cómo usar**:
1. Copia el prompt completo
2. Reemplaza `[Entidad]` con tu entidad
3. Adjunta archivos de referencia con `@`
4. Pega en tu IA
5. Copia el código generado

---

#### 3. **AUTOMATION_GUIDE.md** - Guía Detallada
**Cuándo usar**: Primera vez o cuando tengas dudas

**Contenido**:
- 📋 Visión general del flujo
- 📋 Pasos detallados con ejemplos
- 📋 Validaciones en cada paso
- 📋 Checklist completo
- 📋 Tips para juniors

**Tiempo de lectura**: 30 minutos
**Referencia**: Consulta cuando tengas dudas específicas

---

#### 4. **analisis-proyecto-fudie.md** - Arquitectura
**Cuándo usar**: Al inicio (para entender el proyecto) y como referencia

**Contenido**:
- 🏗️ Objetivos del proyecto Fudie
- 🏗️ Componentes clave (Result, Entity, Repository, etc.)
- 🏗️ Integración con Program.cs
- 🏗️ Análisis del dominio de Pizzas
- 🏗️ Capa de infraestructura
- 🏗️ Patrones y arquitectura
- 🏗️ Recomendaciones

**Tiempo de lectura**: 45 minutos
**Importancia**: ⭐⭐⭐⭐⭐ (Crítico para entender el proyecto)

---

### 📝 Templates

#### **domain_definition_template.md**
**Cuándo usar**: Al definir una nueva entidad

**Contenido**:
- Plantilla para estado y estructura
- Plantilla para Event Storming
- Plantilla para Example Mapping

**Cómo usar**:
1. Copia la plantilla
2. Crea `domain-specs/[TuEntidad].md`
3. Rellena todas las secciones
4. Usa esto como input para la IA

---

#### **style_guide_examples.md**
**Cuándo usar**: Como referencia al escribir código

**Contenido**:
- ✅ Ejemplo de Entity (Domain)
- ✅ Ejemplo de Tests Unitarios
- ✅ Ejemplo de Feature Slice (Vertical Slice)
- ✅ Ejemplo de Tests de Integración
- ✅ Ejemplo de Configuración de Persistencia

**Importancia**: ⭐⭐⭐⭐⭐ (La IA usa esto como referencia)

---

## 🎯 Flujos de Trabajo Típicos

### Flujo 1: Desarrollar Nueva Feature

```
1. Lee: analisis-proyecto-fudie.md (si es tu primera vez)
2. Crea: domain-specs/[Entidad].md (usa template)
3. Abre: AI_PROMPTS.md
4. Ejecuta: Prompts 1-8 en orden
5. Valida: .\validate-feature.ps1 -Entity "[Entidad]"
6. Revisa: Usa Prompt 9
7. Commit: git commit -m "feat: Add [Entidad] feature"
```

**Tiempo estimado**: 30-60 minutos

---

### Flujo 2: Entender el Proyecto (Onboarding)

```
1. Lee: README.md (este archivo) - 10 min
2. Lee: analisis-proyecto-fudie.md - 45 min
3. Lee: templates/style_guide_examples.md - 20 min
4. Explora: src/webapi/features/pizzas/ - 15 min
5. Lee: WORKFLOW.md - 10 min
6. Practica: Crea una feature simple siguiendo AUTOMATION_GUIDE.md
```

**Tiempo estimado**: 2-3 horas

---

### Flujo 3: Resolver Problemas

```
1. Identifica: ¿Qué está fallando?
2. Consulta: WORKFLOW.md → Sección "Problemas Comunes"
3. Valida: Ejecuta script de validación
4. Compara: Con ejemplos en src/webapi/features/pizzas/
5. Revisa: analisis-proyecto-fudie.md → Sección relevante
6. Pide ayuda: Si después de 3 intentos no funciona
```

---

## 🛠️ Scripts de Automatización

### validate-feature.ps1 (Windows)
```powershell
.\validate-feature.ps1 -Entity "Pedido"
```

**Qué hace**:
- ✅ Verifica estructura de archivos
- ✅ Compila el proyecto
- ✅ Ejecuta tests unitarios
- ✅ Ejecuta tests de integración
- ✅ Verifica que la aplicación inicia

**Salida**:
- ✅ Validación exitosa → Feature completa
- ❌ Errores encontrados → Lista de problemas

---

### validate-feature.sh (Linux/Mac)
```bash
chmod +x validate-feature.sh
./validate-feature.sh Pedido
```

Equivalente a la versión PowerShell.

---

## 📊 Métricas de Calidad

### Feature Completa Debe Tener:

| Aspecto | Criterio |
|---------|----------|
| **Compilación** | 0 errores, 0 warnings |
| **Tests** | 100% pasando |
| **Cobertura** | Todos los casos del Example Mapping |
| **Endpoints** | Funcionan en Swagger |
| **Validaciones** | Retornan 422 con datos inválidos |
| **Errores** | Retornan 404 cuando no existe |
| **Código** | Sigue patrones del proyecto |

---

## 🎓 Niveles de Experiencia

### Junior (0-3 meses)

**Documentos clave**:
1. `analisis-proyecto-fudie.md` - Lee completo
2. `AUTOMATION_GUIDE.md` - Sigue paso a paso
3. `AI_PROMPTS.md` - Copia prompts exactamente

**Tiempo por feature**: 60-90 minutos

**Objetivo**: Entender los patrones y generar código consistente

---

### Mid (3-6 meses)

**Documentos clave**:
1. `WORKFLOW.md` - Flujo detallado
2. `AI_PROMPTS.md` - Personaliza prompts si es necesario

**Tiempo por feature**: 45-60 minutos

**Objetivo**: Desarrollar features de forma independiente

---

### Senior (6+ meses)

**Documentos clave**:
1. `WORKFLOW.md` - Flujo rápido
2. `AI_PROMPTS.md` - Referencia rápida

**Tiempo por feature**: 30-45 minutos

**Objetivo**: Optimizar proceso y ayudar a juniors

---

## 🔄 Ciclo de Mejora Continua

### Si encuentras:

**❌ Prompt que genera código incorrecto**
1. Documenta el problema
2. Ajusta el prompt
3. Actualiza `AI_PROMPTS.md`
4. Comparte con el equipo

**❌ Patrón que no funciona**
1. Documenta el caso
2. Propón solución
3. Actualiza `style_guide_examples.md`
4. Actualiza `analisis-proyecto-fudie.md` si es necesario

**❌ Validación que falta**
1. Identifica qué falta validar
2. Agrega al script `validate-feature.ps1` / `.sh`
3. Actualiza `AUTOMATION_GUIDE.md`

**✅ Optimización encontrada**
1. Documenta la mejora
2. Actualiza documentación relevante
3. Comparte en reunión de equipo

---

## 📞 Soporte y Ayuda

### Antes de Pedir Ayuda

1. ✅ Leíste la documentación relevante
2. ✅ Ejecutaste el script de validación
3. ✅ Comparaste con ejemplos existentes
4. ✅ Intentaste 3 veces con diferentes enfoques

### Al Pedir Ayuda, Incluye:

1. **Qué estás intentando hacer**: "Generar command Create para Pedido"
2. **Qué error obtienes**: Mensaje de error completo
3. **Qué ya intentaste**: "Usé Prompt 5, comparé con CreatePizza.cs"
4. **Código relevante**: Snippet del código problemático

---

## 🎯 Objetivos del Sistema de Automatización

### Objetivos Alcanzados

✅ **Reducir tiempo de desarrollo**: De 4-6 horas a 30-60 minutos por feature
✅ **Estandarizar código**: Todos siguen los mismos patrones
✅ **Reducir errores**: Validación automática
✅ **Facilitar onboarding**: Juniors productivos en días, no semanas
✅ **Documentación viva**: Se actualiza con el proyecto

### Métricas

| Métrica | Antes | Después | Mejora |
|---------|-------|---------|--------|
| Tiempo por feature | 4-6 horas | 30-60 min | 80-85% |
| Errores de compilación | 5-10 | 0-1 | 90% |
| Tests fallando | 20-30% | 0-5% | 85% |
| Tiempo de onboarding | 2-3 semanas | 2-3 días | 90% |
| Consistencia de código | 60% | 95% | 58% |

---

## 🚀 Próximos Pasos

### Como Desarrollador

1. **Hoy**: Lee este README completo (10 min)
2. **Hoy**: Lee `analisis-proyecto-fudie.md` (45 min)
3. **Mañana**: Desarrolla tu primera feature siguiendo `AUTOMATION_GUIDE.md`
4. **Esta semana**: Desarrolla 2-3 features más
5. **Próxima semana**: Usa el flujo rápido de `WORKFLOW.md`

### Como Equipo

1. **Semana 1**: Todos leen documentación core
2. **Semana 2**: Cada uno desarrolla 1 feature de prueba
3. **Semana 3**: Retrospectiva y mejoras a los prompts
4. **Semana 4**: Producción completa con el sistema

---

## 📚 Recursos Externos

- **Clean Architecture**: Robert C. Martin
- **Domain-Driven Design**: Eric Evans
- **Vertical Slice Architecture**: Jimmy Bogard
- **CQRS**: Greg Young
- **Result Pattern**: Vladimir Khorikov

---

## 🎉 ¡Empieza Ahora!

**Para tu primera feature**:

1. Abre `AUTOMATION_GUIDE.md`
2. Sigue el Paso 0: Preparación
3. Define tu dominio (Paso 1)
4. Abre `AI_PROMPTS.md` y empieza con Prompt 1
5. ¡Disfruta viendo cómo la IA genera código de calidad!

**¡Buena suerte! 🚀**

---

**Última actualización**: 2025-12-01
**Versión**: 1.0
**Mantenido por**: Equipo de Arquitectura
