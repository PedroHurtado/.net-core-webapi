# 🎨 Workflow: Event Storming → Wireframes

**Documento para reunión Producto + UX + Desarrollo**  
**Proyecto:** Fudie Admin  
**Fecha:** Diciembre 2024

---

## 📋 Objetivo de esta Reunión

Establecer un proceso colaborativo donde:

1. **Producto + Dev** definen flujos con Event Storming
2. **UX/Diseño** traduce esos flujos a wireframes interactivos
3. **Todos** validan antes de implementar

**Resultado:** Menos iteraciones, menos malentendidos, desarrollo más rápido.

---

## 🧠 ¿Qué es Event Storming? (2 min)

Es una técnica visual para mapear **qué pasa en el sistema** usando post-its de colores:

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│   🟡 ACTOR        🔵 COMANDO        🟤 AGREGADO      🟠 EVENTO      │
│   (Quién)         (Qué hace)        (Sobre qué)      (Qué pasó)     │
│                                                                     │
│   [Chef]    →    (Crear Menú)   →   [[Menú]]    →   <MenúCreado>   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Los Colores que Usamos

| Color | Elemento | Significado | Ejemplo |
|-------|----------|-------------|---------|
| 🟡 Amarillo | `[Actor]` | Quién inicia la acción | `[Administrador]` |
| 🔵 Azul | `(Comando)` | Intención/Acción | `(Crear Menú)` |
| 🟤 Marrón | `[[Agregado]]` | Entidad principal | `[[Menú]]` |
| 🟠 Naranja | `<Evento>` | Lo que ocurrió (pasado) | `<MenúCreado>` |
| 🟣 Púrpura | `{Política}` | Regla de negocio | `{Nombre requerido}` |
| 🔴 Rojo | `⚠️ Hot Spot` | Duda o conflicto | `⚠️ ¿Límite categorías?` |
| 🟢 Verde | `📊 Read Model` | Vista/Consulta | `📊 Lista de Menús` |

---

## 🔄 El Proceso Completo

```
┌────────────────────────────────────────────────────────────────────┐
│                                                                    │
│   FASE 1: EVENT STORMING                                          │
│   ══════════════════════                                          │
│   👥 Participan: Producto + Dev + UX (opcional)                   │
│   ⏱️ Duración: 1-2 horas por feature                              │
│   📄 Output: domain-specs/[Feature].md                            │
│                                                                    │
│   • Mapeamos TODOS los flujos del usuario                         │
│   • Identificamos comandos, eventos, reglas                       │
│   • Resolvemos dudas (hot spots)                                  │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
                              ↓
┌────────────────────────────────────────────────────────────────────┐
│                                                                    │
│   FASE 2: WIREFRAMES INTERACTIVOS                                 │
│   ═══════════════════════════════                                 │
│   👥 Participan: UX/Diseño (+ Dev para dudas técnicas)            │
│   ⏱️ Duración: 2-4 horas por feature                              │
│   📄 Output: wireframes/[feature]-prototype.html                  │
│                                                                    │
│   • Traducimos cada comando a pantalla/acción                     │
│   • HTML interactivo navegable                                    │
│   • Sin diseño visual final (estructura y flujo)                  │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
                              ↓
┌────────────────────────────────────────────────────────────────────┐
│                                                                    │
│   FASE 3: VALIDACIÓN                                              │
│   ══════════════════                                              │
│   👥 Participan: Todos + Stakeholders                             │
│   ⏱️ Duración: 30 min                                             │
│                                                                    │
│   • Navegamos el prototipo juntos                                 │
│   • Ajustamos si hay gaps                                         │
│   • Aprobamos para implementar                                    │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
                              ↓
┌────────────────────────────────────────────────────────────────────┐
│                                                                    │
│   FASE 4: IMPLEMENTACIÓN                                          │
│   ══════════════════════                                          │
│   👥 Dev implementa, UX refina diseño visual                      │
│                                                                    │
│   • Dev tiene claridad total del flujo                            │
│   • UX puede trabajar diseño final en paralelo                    │
│   • Menos "esto no era lo que pedí"                               │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Cómo Traducir Event Storming a Wireframes

### Tabla de Conversión

| Elemento ES | Se Convierte En | Ejemplo Wireframe |
|-------------|-----------------|-------------------|
| `📊 Read Model` | **Pantalla/Vista** | Lista de menús, Dashboard |
| `(Comando)` | **Botón + Acción** | Botón "Crear", Formulario |
| `<Evento>` | **Feedback** | Toast "Guardado", Redirect |
| `{Política}` | **Validación** | Error inline, Campo requerido |
| `⚠️ Hot Spot` | **Pregunta UX** | ¿Modal o pantalla nueva? |

### Ejemplo Práctico: Gestión de Menús

**Event Storming dice:**
```
🟡[Admin] → 🔵(Crear Menú) → 🟤[[Menú]] → 🟠<MenúCreado>
                  │
           🟣{Nombre requerido}
           🟣{Max 100 caracteres}
```

**Wireframe traduce:**

```
┌─────────────────────────────────────────────────────────┐
│  PANTALLA: Crear Menú                                   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ← Menús / Nuevo Menú                    [Cancelar]     │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │  Nombre del Menú *                              │   │
│  │  ┌─────────────────────────────────────────┐   │   │
│  │  │ Carta Principal                    45/100│   │   │  ← {Max 100 chars}
│  │  └─────────────────────────────────────────┘   │   │
│  │  ⚠️ El nombre es requerido (si vacío)          │   │  ← {Nombre requerido}
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│                              [Crear Menú] ← (Comando)   │
│                                                         │
│  Al guardar → Toast "Menú creado" ← <MenúCreado>       │
│            → Redirect a detalle                         │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 📝 Ejemplo Real: Feature "Menús"

### Event Storming Resumido

```
FLUJO 1: CRUD Básico
══════════════════════════════════════════════════════════

📊 Lista de Menús (Read Model)
    │
    ├── 🟡[Admin] → 🔵(Crear Menú) → 🟤[[Menú]] → 🟠<MenúCreado>
    │                    │
    │              🟣{Nombre requerido}
    │              🟣{Vigencia: indefinida o fechas}
    │
    ├── 🟡[Admin] → 🔵(Ver Menú) → 📊 Detalle Menú
    │
    ├── 🟡[Admin] → 🔵(Editar Info) → 🟤[[Menú]] → 🟠<InfoActualizada>
    │
    └── 🟡[Admin] → 🔵(Desactivar Menú) → 🟤[[Menú]] → 🟠<MenúDesactivado>


FLUJO 2: Gestión de Categorías
══════════════════════════════════════════════════════════

📊 Detalle Menú
    │
    ├── 🟡[Admin] → 🔵(Añadir Categoría) → 🟤[[Menú]] → 🟠<CategoríaAñadida>
    │                    │
    │              🟣{Nombre único en menú}
    │              ⚠️ ¿Límite de categorías? → Decidido: No
    │
    ├── 🟡[Admin] → 🔵(Reordenar Categorías) → 🟤[[Menú]] → 🟠<CategoriasReordenadas>
    │
    └── 🟡[Admin] → 🔵(Eliminar Categoría) → 🟤[[Menú]] → 🟠<CategoríaEliminada>
                         │
                   🟣{Confirmar si tiene items}


FLUJO 3: Gestión de Items
══════════════════════════════════════════════════════════

📊 Detalle Menú (vista categorías)
    │
    └── 🟡[Admin] → 🔵(Añadir Item) → 🟤[[Menú]] → 🟠<ItemAñadido>
                         │
                   🟣{Precio > 0}
                   🟣{Categoría debe existir}
```

### Wireframes Resultantes

| Read Model / Comando | Wireframe | Tipo |
|----------------------|-----------|------|
| `📊 Lista de Menús` | Vista principal con cards | Pantalla |
| `(Crear Menú)` | Formulario nombre + vigencia | Pantalla |
| `📊 Detalle Menú` | Árbol categorías + panel config | Pantalla |
| `(Añadir Categoría)` | Modal con nombre + icono | Popup |
| `(Reordenar Categorías)` | Modal con drag & drop | Popup |
| `(Editar Info)` | Mismo form que crear | Pantalla (reutiliza) |

---

## 🤝 Decisiones UX Comunes

### ¿Cuándo Popup vs Pantalla Nueva?

| Usar Popup (Modal) | Usar Pantalla Nueva |
|--------------------|---------------------|
| ✅ Formulario simple (2-3 campos) | ✅ Formulario complejo (5+ campos) |
| ✅ Acción rápida sin perder contexto | ✅ Necesita espacio para preview |
| ✅ Confirmaciones | ✅ Flujo multi-paso |
| **Ejemplo:** Crear categoría | **Ejemplo:** Crear menú, Editar item |

### ¿Cuándo Guardar Automático vs Botón?

| Guardar Automático | Botón Guardar Explícito |
|--------------------|-------------------------|
| ✅ Toggles (on/off) | ✅ Formularios con múltiples campos |
| ✅ Cambios de estado simple | ✅ Cambios que afectan facturación |
| ✅ Reordenar (drag & drop) | ✅ Datos sensibles |
| **Ejemplo:** Toggle disponibilidad | **Ejemplo:** Editar precios |

### Estados de Items a Mostrar

| Estado | Visual | Badge |
|--------|--------|-------|
| Disponible | Normal | 🟢 DISPONIBLE |
| Agotado | Opacidad 50% + overlay | ⚫ AGOTADO |
| Alto Riesgo | Badge amarillo | 🟡 ALTO RIESGO |

---

## 📐 Estructura del Wireframe HTML

```html
<!-- Wireframe interactivo standalone -->
<!DOCTYPE html>
<html>
<head>
    <!-- Tailwind + FontAwesome + Google Fonts -->
</head>
<body>
    <!-- Sidebar fijo con navegación -->
    <aside>...</aside>
    
    <!-- Contenido principal -->
    <main>
        <!-- VISTA 1: Lista -->
        <div id="view-list" class="view active">...</div>
        
        <!-- VISTA 2: Crear/Editar -->
        <div id="view-create" class="view">...</div>
        
        <!-- VISTA 3: Detalle -->
        <div id="view-detail" class="view">...</div>
    </main>
    
    <!-- Modales -->
    <div id="modal-categoria">...</div>
    
    <!-- JS para navegación -->
    <script>
        function showView(name) { ... }
    </script>
</body>
</html>
```

**Características:**
- ✅ Un solo archivo HTML (fácil compartir)
- ✅ Navegación real entre vistas
- ✅ Modales funcionales
- ✅ Estados interactivos
- ✅ Sidebar con accesos directos para demo

---

## ✅ Checklist para UX/Diseño

Antes de dar por terminado un wireframe:

### Estructura
- [ ] ¿Cada comando del ES tiene su representación?
- [ ] ¿Los read models tienen su vista?
- [ ] ¿Las políticas se reflejan como validaciones?

### Navegación
- [ ] ¿Se puede navegar todo el flujo?
- [ ] ¿Hay forma de volver atrás?
- [ ] ¿Los breadcrumbs son correctos?

### Feedback
- [ ] ¿Cada acción tiene feedback visual?
- [ ] ¿Los errores se muestran claramente?
- [ ] ¿Los estados de carga están indicados?

### Interactividad
- [ ] ¿Los botones hacen algo (aunque sea alert)?
- [ ] ¿Los modales abren y cierran?
- [ ] ¿Los formularios tienen validación visual?

---

## 📁 Entregables por Feature

```
/wireframes
  └── /menus
      ├── menus-prototype.html      ← Wireframe interactivo
      └── menus-notas.md            ← Decisiones UX tomadas

/domain-specs
  └── Menu.md                       ← Event Storming documentado
```

---

## 🚀 Próximos Pasos

1. **Hoy:** Alinear proceso ES → Wireframes
2. **Esta semana:** Completar wireframe Menús (ya avanzado)
3. **Siguiente:** Aplicar proceso a próxima feature (¿Reservas? ¿Items?)

---

## 💬 Preguntas para Discutir

1. ¿Tiene sentido el proceso para el equipo?
2. ¿El nivel de detalle del wireframe es suficiente o excesivo?
3. ¿Preferimos wireframes en HTML o herramienta visual (Figma)?
4. ¿Quién lidera cada fase?

---

**Documento preparado para reunión**  
**Próxima acción:** Validar proceso con el equipo
