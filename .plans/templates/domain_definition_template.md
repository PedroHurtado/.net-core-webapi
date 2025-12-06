# Domain Specification: [NombreEntidad]

## 1. Estado y Estructura

### Resumen
*Descripción breve de la responsabilidad de la entidad y su rol en el dominio.*

### Propiedades (Estado)
| Propiedad | Tipo | Modificador | Validaciones (FluentValidation) | Notas |
|-----------|------|-------------|--------------------------------|-------|
| Id | Guid | protected set | Required | |
| Name | string | protected set | NotEmpty, MaxLength(100) | |
| Description | string | protected set | MaxLength(500) | Opcional |
| IsActive | bool | protected set | | |
| CreatedAt | DateTime | protected set | | Auditoría |
| UpdatedAt | DateTime | protected set | | Auditoría |

### Objetos de Valor Anidados
*Definición de Value Objects que pertenecen a esta entidad.*
- **[ValueObject]**: `[Type]?` (opcional/requerido) - Descripción

### Colecciones
*Definición de campos de respaldo (backing fields) y colecciones expuestas.*
```csharp
protected HashSet<[ChildEntity]> _items = [];
public IReadOnlyCollection<[ChildEntity]> Items => _items.ToList().AsReadOnly();
```

### Propiedades Calculadas
- `[PropertyName]`: `[Type]` (get only) → Descripción/Fórmula

### Invariantes / Reglas de Negocio Globales
*Reglas que siempre deben cumplirse para que la entidad sea válida.*
- ✅ Invariante 1
- ✅ Invariante 2
- ✅ Invariante 3

---

## 2. Comportamiento y Reglas (Event Storming)

### Leyenda de Colores
| Color | Elemento | Símbolo | Descripción |
|-------|----------|---------|-------------|
| 🟠 Naranja | Domain Event | `<EventName>` | Algo que ocurrió (pasado) |
| 🔵 Azul | Command | `(CommandName)` | Intención/Acción (imperativo) |
| 🟡 Amarillo | Actor | `[ActorName]` | Usuario o sistema que inicia |
| 🟣 Púrpura | Policy | `{PolicyName}` | Regla de negocio/Política |
| 🟤 Marrón | Aggregate | `[[AggregateName]]` | Entidad raíz del agregado |
| 🔴 Rojo | Hot Spot | `⚠️` | Dudas o conflictos pendientes |
| 🟢 Verde | Read Model | `📊` | Vista/Proyección de datos |
| 🩷 Rosa | External System | `⚡` | Sistema externo |

---

### Flujo 1: [Nombre del Flujo]

#### 1.1 [Nombre del Comando]
```
🟡[Actor] → 🔵(CommandName) → 🟤[[Aggregate]] → 🟠<EventName>
                                      │
                            🟣{PolicyName}
```

**Input**: Param1, Param2, Param3?

**Validaciones** 🟣{PolicyName}:
- Validación 1
- Validación 2
- Validación 3

**Resultado**: Descripción del resultado esperado

---

#### 1.2 [Otro Comando con Error]
```
🟡[Actor] → 🔵(CommandName) → 🟤[[Aggregate]] → 🟠<EventName>
                                      │
                            🟣{PolicyName}
```

**Flujo de Error**:
```
🟡[Actor] → 🔵(CommandName) → 🟤[[Aggregate]] → 🔴<Error: ErrorName>
                                      │
                            🟣{PolicyName} ❌
```

---

### Flujo 2: [Nombre del Flujo]

#### 2.1 [Comando con Sistema Externo]
```
🟡[Actor] → 🔵(CommandName) → 🟤[[Aggregate]] → 🟠<EventName>
                                                      │
                                            🟣{PolicyName}
                                                      │
                                            ⚡ ExternalSystem
                                                      │
                                            🟠<SecondaryEvent>
```

---

### Flujo 3: [Lógica de Negocio Compleja]

#### 3.1 [Cálculo o Proceso]
```
🟡[System] → 🔵(CalculateSomething) → 🟤[[Aggregate]] → 📊 ResultView
                                            │
                                  🟣{CalculationPolicy}
```

**Algoritmo** 🟣{CalculationPolicy}:
```
1. Paso 1
2. Paso 2
3. Paso 3
```

**Ejemplos Visuales**:

```
📊 Ejemplo 1 - Descripción
┌────────────────────────────────────────┐
│ Condición: ...                         │
│ Input: ...                             │
├────────────────────────────────────────┤
│ Cálculo: ...                           │
│ Resultado: ...                         │
└────────────────────────────────────────┘
```

---

### Hot Spots ⚠️ (Preguntas Pendientes)

| # | Pregunta | Estado |
|---|----------|--------|
| 1 | ⚠️ Pregunta 1 | Pendiente |
| 2 | ⚠️ Pregunta 2 | Resuelto |
| 3 | ⚠️ Pregunta 3 | Documentado en Edge Cases |

---

### Resumen de Políticas 🟣

| Política | Trigger | Descripción |
|----------|---------|-------------|
| `{PolicyName1}` | `(Command1)` | Descripción |
| `{PolicyName2}` | `(Command2)` | Descripción |
| `{PolicyName3}` | `(Command3)`, `(Command4)` | Descripción |

---

### Read Models 📊

| Vista | Propósito | Actualizado por |
|-------|-----------|-----------------|
| `ViewModel1` | Descripción | `<Event1>`, `<Event2>` |
| `ViewModel2` | Descripción | `<Event3>` |

---

## 3. Example Mapping

### Story 1: [Nombre de la Historia]

**Rule**: Descripción de la regla de negocio.

✅ **Example (Success)**:
- Descripción del caso de éxito
- **Precondición**: Estado inicial
- **Acción**: `entity.Method(params)`
- **Resultado**: Estado final esperado

❌ **Example (Failure - Razón)**:
- Descripción del caso de fallo
- **Acción**: `entity.Method(params)`
- **Resultado**: Error "Mensaje de error"

---

### Story 2: [Otra Historia]

**Rule**: Descripción de la regla.

✅ **Example (Success)**:
- Caso de éxito
- **Acción**: `entity.Method(params)`
- **Resultado**: Éxito

✅ **Example (Success - Variante)**:
- Variante del caso de éxito
- **Precondición**: Condición diferente
- **Acción**: `entity.Method(params)`
- **Resultado**: Éxito con variante

❌ **Example (Failure - Razón 1)**:
- **Acción**: `entity.Method(invalidParams)`
- **Resultado**: Error "Mensaje 1"

❌ **Example (Failure - Razón 2)**:
- **Acción**: `entity.Method(otherInvalidParams)`
- **Resultado**: Error "Mensaje 2"

---

## 4. Notas de Implementación

### Aggregate Boundary

**[Entidad] es el Aggregate Root**:
```
[Entidad] (Root)
 ├─ [ValueObject1] (Value Object)
 └─ [ChildEntity] (Entity)
     └─ [GrandchildEntity] (Entity)
         ├─ [ValueObject2] (Value Object)
         └─ [Collection] (Collection)
```

**Reglas de Acceso**:
- Toda modificación pasa por `[Entidad]`
- No se puede acceder directamente a `[ChildEntity]` sin pasar por `[Entidad]`
- Los Value Objects son inmutables (reemplazar, no modificar)

---

### Persistencia

**Estructura Recomendada** (Firestore/SQL):
```json
{
  "id": "entity-123",
  "name": "...",
  "childEntities": [
    {
      "id": "child-456",
      "property": "..."
    }
  ],
  "createdAt": "2025-01-01T10:00:00Z",
  "updatedAt": "2025-01-15T14:30:00Z"
}
```

---

### Lógica de Negocio Compleja (Pseudocódigo)

```csharp
public [ReturnType] [MethodName]([Parameters])
{
    // 1. Paso 1
    // 2. Paso 2
    // 3. Paso 3
    
    return result;
}
```

---

## 5. Casos Edge y Consideraciones

### Casos Edge

**Edge 1: [Descripción del caso]**
- **Comportamiento**: Qué pasa
- **Resultado**: Resultado esperado

**Edge 2: [Otro caso]**
- **Comportamiento**: Qué pasa
- **Acción recomendada**: Qué hacer

---

### Consideraciones de Negocio

**¿Pregunta de negocio 1?**
- Respuesta y explicación

**¿Pregunta de negocio 2?**
- Respuesta y explicación

---

## 6. Resumen de Invariantes Críticos

### [Entidad Principal]
- ✅ Invariante 1
- ✅ Invariante 2
- ✅ Invariante 3

### [Entidad Hija]
- ✅ Invariante 1
- ✅ Invariante 2

### [Value Object]
- ✅ Invariante 1
- ✅ Invariante 2

---

## 7. Diagrama Conceptual

```
[Entidad] (Aggregate Root)
├─ Id: Guid
├─ Name: string
├─ [ValueObject]? (Value Object)
│   ├─ Property1: Type
│   └─ Property2: Type
│
└─ [Children]: IReadOnlyCollection<[ChildEntity]>
    └─ [ChildEntity]
        ├─ Id: Guid
        ├─ Property: Type
        └─ [Grandchildren]: IReadOnlyCollection<[GrandchildEntity]>
            └─ [GrandchildEntity]
                ├─ Id: Guid
                └─ Property: Type
```

---

**Fin del Domain Specification**

---

**Fecha**: YYYY-MM-DD  
**Autor**: Fudie ([Nombre])
