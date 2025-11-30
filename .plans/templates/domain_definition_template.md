# Domain Definition: [NombreEntidad]

## 1. Estado y Estructura

### Resumen
*Descripción breve de la responsabilidad de la entidad y su rol en el dominio.*

### Propiedades (Estado)
| Propiedad | Tipo | Modificador | Validaciones (FluentValidation) | Notas |
|-----------|------|-------------|--------------------------------|-------|
| Name      | string | protected set | NotEmpty, MaxLength(100) | |
| Url       | string | protected set | NotEmpty, ValidUrl | |
| Price     | decimal | get only | | Calculado: Suma de ingredientes * 1.2 |

### Relaciones
*Definición de campos de respaldo (backing fields) y colecciones expuestas.*
- **Ingredients**: `HashSet<Ingredient>` (backing field), expuesto como `IReadOnlyCollection<Ingredient>`.

### Invariantes / Reglas de Negocio Globales
*Reglas que siempre deben cumplirse para que la entidad sea válida.*
- El precio nunca puede ser negativo.
- Una pizza debe tener al menos una base (ejemplo).

---

## 2. Comportamiento y Reglas (Event Storming & Example Mapping)

### Event Storming (Textual)
*Flujo temporal del sistema: [Actor] -> (Comando) -> [Agregado] -> <Evento>*

#### Flujo Principal
1. [Chef] -> (Crear Pizza) -> [Pizza] -> <PizzaCreada>
2. [Chef] -> (Añadir Ingrediente) -> [Pizza] -> <IngredienteAñadido>
   *Constraint*: El ingrediente no debe existir ya.
3. [Chef] -> (Añadir Ingrediente) -> [Pizza] -> <Error: IngredienteDuplicado>
4. [Chef] -> (Eliminar Ingrediente) -> [Pizza] -> <IngredienteEliminado>

### Example Mapping
*Desglose de reglas de negocio en ejemplos concretos para TDD.*

#### Story: Crear una nueva Pizza
**Rule**: La pizza debe tener datos básicos válidos (Nombre, Descripción, URL).
- *Example (Success)*: Crear pizza "Margarita" con descripción y URL válidas.
- *Example (Failure)*: Intentar crear pizza sin nombre (retorna Error "El nombre es requerido").
- *Example (Failure)*: Intentar crear pizza con URL inválida (retorna Error "URL inválida").

#### Story: Gestión de Ingredientes
**Rule**: No se pueden añadir ingredientes nulos.
- *Example (Failure)*: Añadir `null` como ingrediente (retorna Error "El ingrediente no puede ser nulo").

**Rule**: Los ingredientes deben ser únicos en una pizza.
- *Example (Success)*: Añadir "Queso" a una pizza vacía.
- *Example (Failure)*: Añadir "Queso" a una pizza que ya tiene "Queso" (retorna Error "El ingrediente ya existe").
