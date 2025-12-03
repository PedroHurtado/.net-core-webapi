# Domain Specification: Menu

## 1. Estado y Estructura

### Resumen
El **Menu** es el agregado raíz que representa un menú completo de un restaurante (ej: "Carta Principal", "Menú del Día", "Menú de Tapas"). Contiene categorías y éstas a su vez contienen los items. Un restaurante puede tener múltiples menús activos simultáneamente.

Cada menú puede tener una política de fianzas a nivel general, y los items individuales pueden sobrescribir esta política con sus propias reglas específicas.

> **Nota sobre Multiidioma**: El dominio maneja el contenido en el idioma nativo del restaurante. Las traducciones se gestionan a nivel de infraestructura (Cloud Storage) y se referencian desde la entidad Restaurant, no desde Menu.

---

### Propiedades del Agregado Menu

| Propiedad | Type | Modificador | Validaciones | Notas |
|-----------|------|-------------|--------------|-------|
| Id | Guid | protected set | Required | |
| RestaurantId | Guid | protected set | Required | Referencia al restaurante propietario |
| Name | string | protected set | NotEmpty, MaxLength(100) | "Carta Principal", "Menú del Día" |
| Description | string | protected set | MaxLength(500) | Opcional |
| IsActive | bool | protected set | | Menú visible/oculto |
| EffectiveFrom | DateTime? | protected set | | Fecha desde la cual es válido (opcional) |
| EffectiveUntil | DateTime? | protected set | | Fecha hasta la cual es válido (opcional) |
| DisplayOrder | int | protected set | | Orden de visualización |
| CreatedAt | DateTime | protected set | | Auditoría |
| UpdatedAt | DateTime | protected set | | Auditoría |

**Objeto de Valor Anidado**:
- `DepositPolicy`: `DepositPolicy?` (opcional, null = sin política de fianzas)

**Colección**:
- `Categories`: `HashSet<MenuCategory>` (backing field `_categories`)
- Expuesto como: `IReadOnlyCollection<MenuCategory> Categories`

**Invariantes**:
- Al menos una categoría activa
- `EffectiveFrom` debe ser anterior a `EffectiveUntil` si ambos están presentes
- No puede haber dos categorías con el mismo `Name` en el mismo menú
- Si `DepositPolicy` está configurado, debe ser válido (validaciones propias)

---

### DepositPolicy (Value Object)

Define la política de fianzas de reserva a nivel de menú.

| Propiedad | Type | Modificador | Validaciones | Notas |
|-----------|------|-------------|--------------|-------|
| DepositType | DepositType (enum) | protected set | Required | PerPerson, PercentageOfBill, FixedAmount |
| Amount | decimal | protected set | Range(0, 10000) | Importe fijo o € por persona |
| Percentage | decimal? | protected set | Range(0, 100) | Solo si DepositType = PercentageOfBill |
| MinimumBillForDeposit | decimal? | protected set | Range(0, 100000) | Opcional: solo si cuenta supera este importe |
| MinimumGuestsForDeposit | int? | protected set | Range(1, 100) | Opcional: solo si son X+ personas |

**Enums**:
```csharp
public enum DepositType {
    PerPerson = 1,        // €X por persona
    PercentageOfBill = 2, // X% del total estimado
    FixedAmount = 3       // €X importe fijo
}
```

**Propiedades Calculadas**:
- `IsApplicable(int guestCount, decimal estimatedBill)`: `bool` → Determina si aplica según umbrales

**Invariantes**:
- Si `DepositType == PercentageOfBill`, entonces `Percentage` debe tener valor
- Si `DepositType != PercentageOfBill`, entonces `Percentage` debe ser null
- `Amount` debe ser > 0
- Si `Percentage` tiene valor, debe estar entre 1 y 100

**Métodos**:
- `CalculateDeposit(int guestCount, decimal estimatedBill)`: `decimal` → Calcula fianza según tipo

---

### MenuCategory (Entidad dentro del agregado)

| Propiedad | Type | Modificador | Validaciones | Notas |
|-----------|------|-------------|--------------|-------|
| Id | Guid | protected set | Required | |
| Name | string | protected set | NotEmpty, MaxLength(100) | "Entrantes", "Carnes", "Pescados" |
| Description | string | protected set | MaxLength(500) | Opcional |
| DisplayOrder | int | protected set | | Orden dentro del menú |
| IsActive | bool | protected set | | Categoría visible/oculta |

**Colección**:
- `Items`: `HashSet<MenuItem>` (backing field `_items`)
- Expuesto como: `IReadOnlyCollection<MenuItem> Items`

**Invariantes**:
- Al menos un item activo si la categoría está activa
- No puede haber dos items con el mismo `Name` en la misma categoría

---

### MenuItem (Entidad dentro del agregado)

| Propiedad | Type | Modificador | Validaciones | Notas |
|-----------|------|-------------|--------------|-------|
| Id | Guid | protected set | Required | |
| Name | string | protected set | NotEmpty, MaxLength(100) | "Pulpo al Horno" |
| Description | string | protected set | MaxLength(1000) | Opcional |
| ImageUrl | string | protected set | MaxLength(500), ValidUrl | Opcional |
| DisplayOrder | int | protected set | | Orden dentro de la categoría |
| IsActive | bool | protected set | | Item visible/oculto |
| IsAvailable | bool | protected set | | Stock: disponible/agotado HOY |
| IsHighRiskItem | bool | protected set | | Marca item como producto caro/especial |
| RequiresAdvanceOrder | bool | protected set | | Debe pedirse al hacer la reserva |
| MinimumAdvanceOrderQuantity | int? | protected set | Range(1, 100) | Mínimo de porciones para requerir pedido anticipado |
| IsAlwaysAvailable | bool | protected set | | Si true, ignora AvailableDays |

**Objeto de Valor Anidado**:
- `ItemDepositOverride`: `ItemDepositOverride?` (opcional, null = usa política del menú)

**Colecciones**:
- `PriceOptions`: `HashSet<PriceOption>` → `IReadOnlyCollection<PriceOption>`
- `AvailableDays`: `HashSet<DayOfWeek>` → `IReadOnlyCollection<DayOfWeek>`

**Propiedades Calculadas**:
- `IsAvailableToday`: `bool` (get only) → `IsAlwaysAvailable || AvailableDays.Contains(DateTime.Today.DayOfWeek)`
- `CanBeOrdered`: `bool` (get only) → `IsActive && IsAvailableToday && IsAvailable`
- `HasDepositOverride`: `bool` (get only) → `ItemDepositOverride != null`

**Invariantes**:
- Al menos una `PriceOption` activa
- Si `IsAlwaysAvailable == false`, debe tener al menos un día en `AvailableDays`
- No puede haber dos `PriceOption` con el mismo `PortionType`
- Si `RequiresAdvanceOrder == true`, entonces `IsHighRiskItem` debe ser true
- Si `MinimumAdvanceOrderQuantity` tiene valor, entonces `RequiresAdvanceOrder` debe ser true

---

### ItemDepositOverride (Value Object)

Define una política de fianza específica para un item, que sobrescribe la política del menú.

| Propiedad | Type | Modificador | Validaciones | Notas |
|-----------|------|-------------|--------------|-------|
| DepositAmount | decimal | protected set | Range(0, 10000) | Fianza específica para este item |
| MinimumQuantityForDeposit | int? | protected set | Range(1, 100) | Solo si se piden X+ porciones |

**Propiedades Calculadas**:
- `IsApplicable(int quantity)`: `bool` → Determina si aplica según cantidad pedida

**Invariantes**:
- `DepositAmount` debe ser > 0
- Si `MinimumQuantityForDeposit` no tiene valor, la fianza aplica siempre

**Lógica de Override**:
- Si un `MenuItem` tiene `ItemDepositOverride` configurado, **ignora completamente** la `DepositPolicy` del menú
- La fianza final de este item será solo la definida en `ItemDepositOverride`

---

### PriceOption (Value Object / Entidad débil)

| Propiedad | Type | Modificador | Validaciones | Notas |
|-----------|------|-------------|--------------|-------|
| Id | Guid | protected set | Required | |
| PortionType | PortionType (enum) | protected set | Required | Tapa, MediaRacion, Racion, SegunMercado |
| Price | decimal? | protected set | Range(0, 10000) | null para SegunMercado sin precio actualizado |
| IsActive | bool | protected set | | Porción disponible/no disponible |

**Enums**:
```csharp
public enum PortionType {
    Tapa = 1,
    MediaRacion = 2,
    Racion = 3,
    SegunMercado = 4
}
```

**Propiedades Calculadas**:
- `RequiresMarketPrice`: `bool` (get only) → `PortionType == SegunMercado && !Price.HasValue`
- `DisplayPrice`: `string` (get only) → `RequiresMarketPrice ? "S/M" : Price.Value.ToString("C")`

**Invariantes**:
- Si `PortionType != SegunMercado`, entonces `Price` debe tener valor
- `Price` debe ser >= 0 si tiene valor

---

## 2. Comportamiento y Reglas (Event Storming)

### Flujo 1: Gestión de Menú

#### 1.1 Crear Menú
```
[RestaurantOwner] -> (CreateMenu) -> [Menu] -> <MenuCreated>
```
- **Input**: RestaurantId, Name, Description, EffectiveFrom?, EffectiveUntil?
- **Validaciones**:
  * RestaurantId debe existir
  * Name no puede estar vacío
  * MaxLength(Name) = 100
  * MaxLength(Description) = 500
  * EffectiveFrom < EffectiveUntil (si ambos presentes)
- **Resultado**: Menu creado con Id único, IsActive = true, sin categorías (ej: "Carta Principal" para restaurante "El Pulpo Feliz")

---

#### 1.2 Configurar Política de Fianzas del Menú
```
[RestaurantOwner] -> (SetDepositPolicy) -> [Menu] -> <DepositPolicyConfigured>
```
- **Input**: DepositType, Amount, Percentage?, MinimumBillForDeposit?, MinimumGuestsForDeposit?
- **Validaciones**:
  * Menu debe existir
  * Amount > 0
  * Si DepositType = PercentageOfBill, Percentage entre 1-100 y no null
  * Si DepositType != PercentageOfBill, Percentage debe ser null
  * MinimumGuestsForDeposit >= 1 (si se especifica)
  * MinimumBillForDeposit >= 0 (si se especifica)
- **Resultado**: DepositPolicy configurado, se aplicará a todos los items sin override (ej: "€15 por persona si son 6+ comensales")

---

#### 1.3 Eliminar Política de Fianzas del Menú
```
[RestaurantOwner] -> (RemoveDepositPolicy) -> [Menu] -> <DepositPolicyRemoved>
```
- **Input**: (ninguno)
- **Validaciones**:
  * Menu debe existir
- **Resultado**: Menu.DepositPolicy = null, solo se aplicarán fianzas de items con override (ej: "Menú del Día sin fianzas")

---

### Flujo 2: Gestión de Categorías

#### 2.1 Agregar Categoría
```
[RestaurantOwner] -> (AddCategory) -> [Menu] -> <CategoryAdded>
```
- **Input**: Name, Description, DisplayOrder
- **Validaciones**:
  * Menu debe existir
  * Name no puede estar vacío
  * MaxLength(Name) = 100
  * MaxLength(Description) = 500
  * Name no duplicado en el menú
- **Resultado**: Nueva categoría agregada a Menu.Categories, IsActive = true (ej: "Pescados y Mariscos")

---

#### 2.2 Renombrar Categoría
```
[RestaurantOwner] -> (RenameCategory) -> [MenuCategory] -> <CategoryRenamed>
```
- **Input**: CategoryId, NewName
- **Validaciones**:
  * Categoría debe existir
  * NewName no puede estar vacío
  * MaxLength(NewName) = 100
  * NewName no duplicado en el menú
- **Resultado**: Categoría renombrada con nuevo nombre

**Validación de Fallo**:
```
[RestaurantOwner] -> (RenameCategory con nombre duplicado) -> [MenuCategory] -> <Error: CategoryNameAlreadyExists>
```

---

#### 2.3 Reordenar Categorías
```
[RestaurantOwner] -> (ReorderCategories) -> [Menu] -> <CategoriesReordered>
```
- **Input**: List<CategoryId, NewDisplayOrder>
- **Validaciones**:
  * Todas las categorías deben existir en el menú
  * No puede haber DisplayOrder duplicados
- **Resultado**: DisplayOrder actualizado para cada categoría

---

### Flujo 3: Gestión de Items

#### 3.1 Agregar Item a Categoría
```
[RestaurantOwner] -> (AddItemToCategory) -> [MenuCategory] -> <ItemAdded>
```
- **Input**: CategoryId, Name, Description, ImageUrl, PriceOptions[], DisplayOrder
- **Validaciones**:
  * Categoría debe existir
  * Name no puede estar vacío
  * MaxLength(Name) = 100
  * MaxLength(Description) = 1000
  * MaxLength(ImageUrl) = 500
  * ImageUrl debe ser URL válida (si se proporciona)
  * Debe tener al menos una PriceOption
  * Name no duplicado en la categoría
- **Resultado**: Item agregado a Category.Items, IsActive = true, IsAvailable = true (ej: "Pulpo al Horno")

**Validación de Fallo**:
```
[RestaurantOwner] -> (AddItem sin PriceOptions) -> [MenuCategory] -> <Error: ItemMustHavePriceOptions>
```

---

#### 3.2 Marcar Item como Alto Riesgo
```
[RestaurantOwner] -> (MarkAsHighRisk) -> [MenuItem] -> <ItemMarkedAsHighRisk>
```
- **Input**: ItemId
- **Validaciones**:
  * Item debe existir
- **Resultado**: IsHighRiskItem = true, puede requerir pedido anticipado (ej: "Pulpo al Horno" como alto riesgo)

---

#### 3.3 Requerir Pedido Anticipado
```
[RestaurantOwner] -> (RequireAdvanceOrder) -> [MenuItem] -> <AdvanceOrderRequired>
```
- **Input**: ItemId, MinimumAdvanceOrderQuantity?
- **Validaciones**:
  * Item debe existir
  * IsHighRiskItem debe ser true
  * MinimumAdvanceOrderQuantity >= 1 (si se especifica)
- **Resultado**: RequiresAdvanceOrder = true, MinimumAdvanceOrderQuantity configurado (ej: Pulpo requiere pedido anticipado si son 4+ raciones)

---

#### 3.4 Configurar Override de Fianza para Item
```
[RestaurantOwner] -> (SetItemDepositOverride) -> [MenuItem] -> <ItemDepositOverrideConfigured>
```
- **Input**: ItemId, DepositAmount, MinimumQuantityForDeposit?
- **Validaciones**:
  * Item debe existir
  * DepositAmount > 0
  * MinimumQuantityForDeposit >= 1 (si se especifica)
- **Resultado**: ItemDepositOverride configurado, item ignora DepositPolicy del menú (ej: Pulpo requiere €30 si se piden 4+ raciones)

---

#### 3.5 Eliminar Override de Fianza para Item
```
[RestaurantOwner] -> (RemoveItemDepositOverride) -> [MenuItem] -> <ItemDepositOverrideRemoved>
```
- **Input**: ItemId
- **Validaciones**:
  * Item debe existir
  * ItemDepositOverride debe existir
- **Resultado**: ItemDepositOverride = null, item vuelve a usar DepositPolicy del menú

---

#### 3.6 Configurar Disponibilidad por Días
```
[RestaurantOwner] -> (SetAvailability) -> [MenuItem] -> <AvailabilityConfigured>
```
- **Input**: ItemId, IsAlwaysAvailable, AvailableDays[]
- **Validaciones**:
  * Item debe existir
  * Si IsAlwaysAvailable = false, debe tener al menos un día en AvailableDays
  * AvailableDays debe ser valores válidos de DayOfWeek (0-6)
- **Resultado**: AvailableDays configurado, IsAvailableToday se calcula automáticamente (ej: "Pulpo al Horno" solo los sábados)

**Validación de Fallo**:
```
[RestaurantOwner] -> (SetAvailability sin días cuando IsAlwaysAvailable=false) -> [MenuItem] -> <Error: MustSpecifyAtLeastOneDay>
```

---

#### 3.7 Marcar Item como Agotado/Disponible
```
[Waiter] -> (MarkAsUnavailable) -> [MenuItem] -> <ItemMarkedUnavailable>
```
- **Input**: ItemId
- **Validaciones**:
  * Item debe existir
- **Resultado**: IsAvailable = false, CanBeOrdered = false (ej: "Se acabó el pulpo de hoy")

```
[Waiter] -> (MarkAsAvailable) -> [MenuItem] -> <ItemMarkedAvailable>
```
- **Input**: ItemId
- **Validaciones**:
  * Item debe existir
- **Resultado**: IsAvailable = true, CanBeOrdered recalculado según otras condiciones

---

### Flujo 4: Gestión de Precios

#### 4.1 Agregar Opción de Precio
```
[RestaurantOwner] -> (AddPriceOption) -> [MenuItem] -> <PriceOptionAdded>
```
- **Input**: ItemId, PortionType, Price?
- **Validaciones**:
  * Item debe existir
  * PortionType no duplicado en el item
  * Si PortionType != SegunMercado, Price debe tener valor y ser > 0
  * Si PortionType = SegunMercado, Price puede ser null
- **Resultado**: PriceOption agregada al item (ej: Agregar "Tapa: €3.50" a "Jamón Ibérico")

**Validación de Fallo**:
```
[RestaurantOwner] -> (AddPriceOption con PortionType duplicado) -> [MenuItem] -> <Error: PriceOptionAlreadyExists>
```

---

#### 4.2 Actualizar Precio "Según Mercado"
```
[RestaurantOwner] -> (UpdateMarketPrice) -> [PriceOption] -> <MarketPriceUpdated>
```
- **Input**: PriceOptionId, NewPrice
- **Validaciones**:
  * PriceOption debe existir
  * PortionType debe ser SegunMercado
  * NewPrice >= 0
- **Resultado**: Price actualizado, RequiresMarketPrice = false (ej: "Hoy el pulpo está a €22/kg")

---

#### 4.3 Activar/Desactivar Opción de Precio
```
[RestaurantOwner] -> (TogglePriceOption) -> [PriceOption] -> <PriceOptionToggled>
```
- **Input**: PriceOptionId, IsActive
- **Validaciones**:
  * PriceOption debe existir
  * Si se desactiva, debe quedar al menos una PriceOption activa en el item
- **Resultado**: IsActive actualizado (ej: Temporalmente no ofrecer tapas hoy)

**Validación de Fallo**:
```
[RestaurantOwner] -> (Desactivar última PriceOption activa) -> [MenuItem] -> <Error: MustHaveAtLeastOnePriceOption>
```

---

### Flujo 5: Cálculo de Fianzas (Lógica de Negocio)

#### 5.1 Calcular Fianza de Reserva
```
[System] -> (CalculateReservationDeposit) -> [Menu] -> Resultado: decimal
```
- **Input**: MenuId, GuestCount, EstimatedBill, AdvanceOrderedItems{ItemId, Quantity}
- **Validaciones**:
  * Menu debe existir
  * GuestCount > 0
  * EstimatedBill >= 0
  * Todos los items en AdvanceOrderedItems deben existir
- **Resultado**: Fianza total calculada según algoritmo (ver ejemplos abajo)

**Algoritmo**:
1. Verificar si el menú tiene DepositPolicy
2. Si no tiene, DepositaMenu = 0
3. Si tiene, evaluar IsApplicable(guestCount, estimatedBill)
4. Si aplica, DepositaMenu = CalculateDeposit(guestCount, estimatedBill)
5. Para cada item pedido anticipadamente:
   - Si tiene ItemDepositOverride:
     - Si IsApplicable(quantity): DepositoItem = DepositAmount
     - Sino: DepositoItem = 0
   - Si no tiene override: DepositoItem = 0 (ya cubierto por menú)
6. **FianzaTotal = MAX(DepositoMenu, SUM(DepositosItemsConOverride))**

**Ejemplo 1 - Solo política de menú**:
- Menu: €10/persona si son 6+ comensales
- Reserva: 8 personas
- Items: Ninguno con override
- **Fianza**: €80

**Ejemplo 2 - Solo item con override**:
- Menu: Sin DepositPolicy
- Reserva: 8 personas, piden 2 raciones de Pulpo
- Pulpo: Override €30 si 4+ porciones (2 raciones = 8 porciones)
- **Fianza**: €30

**Ejemplo 3 - Ambos (toma el mayor)**:
- Menu: €10/persona si son 6+ comensales → €80
- Reserva: 8 personas, piden Pulpo
- Pulpo: Override €30 si 4+ porciones
- **Fianza**: MAX(€80, €30) = €80

**Ejemplo 4 - Override mayor que política**:
- Menu: €5/persona si son 4+ comensales → €20
- Reserva: 4 personas, piden Cochinillo entero
- Cochinillo: Override €60
- **Fianza**: MAX(€20, €60) = €60

---

## 3. Example Mapping

### Story 1: Crear un Menú

**Rule**: Un menú debe tener un nombre y pertenecer a un restaurante.

✅ **Example (Success)**: 
- Crear menú "Carta Principal" para restaurante "El Pulpo Feliz"
- **Precondición**: RestaurantId válido
- **Acción**: `Menu.Create(restaurantId, "Carta Principal", "Nuestra carta de especialidades")`
- **Resultado**: Menu creado, IsActive = true, sin categorías

❌ **Example (Failure - Nombre vacío)**:
- Crear menú sin nombre
- **Acción**: `Menu.Create(restaurantId, "", "Descripción")`
- **Resultado**: Error "El nombre del menú es requerido"

❌ **Example (Failure - RestaurantId inválido)**:
- Crear menú con restaurante inexistente
- **Acción**: `Menu.Create(Guid.Empty, "Carta", "Desc")`
- **Resultado**: Error "El restaurante no existe"

---

### Story 2: Configurar Política de Fianzas del Menú

**Rule**: Una política de fianzas debe ser válida según su tipo.

✅ **Example (Success - Por persona)**:
- Configurar €15 por persona si son 6+ comensales
- **Acción**: `menu.SetDepositPolicy(DepositType.PerPerson, amount: 15m, minimumGuests: 6)`
- **Resultado**: DepositPolicy configurado correctamente

✅ **Example (Success - Porcentaje)**:
- Configurar 20% del total si supera €100
- **Acción**: `menu.SetDepositPolicy(DepositType.PercentageOfBill, amount: 0m, percentage: 20m, minimumBill: 100m)`
- **Resultado**: DepositPolicy configurado correctamente

✅ **Example (Success - Importe fijo)**:
- Configurar €50 fijos para cualquier reserva
- **Acción**: `menu.SetDepositPolicy(DepositType.FixedAmount, amount: 50m)`
- **Resultado**: DepositPolicy configurado correctamente

❌ **Example (Failure - Porcentaje sin valor)**:
- Configurar tipo PercentageOfBill sin especificar porcentaje
- **Acción**: `menu.SetDepositPolicy(DepositType.PercentageOfBill, amount: 0m, percentage: null)`
- **Resultado**: Error "Debe especificar el porcentaje"

❌ **Example (Failure - Amount negativo)**:
- Configurar fianza negativa
- **Acción**: `menu.SetDepositPolicy(DepositType.PerPerson, amount: -10m)`
- **Resultado**: Error "El importe debe ser mayor que cero"

---

### Story 3: Agregar Categoría al Menú

**Rule**: No puede haber categorías con nombres duplicados en el mismo menú.

✅ **Example (Success)**:
- Agregar "Entrantes" a menú vacío
- **Acción**: `menu.AddCategory("Entrantes", "Primeros platos")`
- **Resultado**: Categoría agregada, DisplayOrder = 1

❌ **Example (Failure - Nombre duplicado)**:
- Agregar "Entrantes" cuando ya existe "Entrantes"
- **Precondición**: Menu ya tiene categoría "Entrantes"
- **Acción**: `menu.AddCategory("Entrantes", "Otra descripción")`
- **Resultado**: Error "Ya existe una categoría con ese nombre"

✅ **Example (Success - Diferentes categorías)**:
- Agregar "Pescados" después de "Entrantes"
- **Precondición**: Menu tiene categoría "Entrantes"
- **Acción**: `menu.AddCategory("Pescados", "Pescados frescos")`
- **Resultado**: Categoría agregada, DisplayOrder = 2

---

### Story 4: Agregar Item con Opciones de Precio

**Rule**: Un item debe tener al menos una opción de precio activa.

✅ **Example (Success - Múltiples porciones)**:
- Agregar "Jamón Ibérico" con Tapa, Media, Ración
- **Acción**: `category.AddItem("Jamón Ibérico", priceOptions: [Tapa(3.50), Media(7.00), Racion(14.00)])`
- **Resultado**: Item agregado con 3 PriceOptions

✅ **Example (Success - Una sola porción)**:
- Agregar "Cochinillo Asado" solo con Ración
- **Acción**: `category.AddItem("Cochinillo Asado", priceOptions: [Racion(45.00)])`
- **Resultado**: Item agregado con 1 PriceOption

❌ **Example (Failure - Sin opciones de precio)**:
- Agregar item sin PriceOptions
- **Acción**: `category.AddItem("Jamón Ibérico", priceOptions: [])`
- **Resultado**: Error "El item debe tener al menos una opción de precio"

❌ **Example (Failure - Nombre duplicado)**:
- Agregar "Jamón Ibérico" cuando ya existe en la categoría
- **Precondición**: Categoría ya tiene item "Jamón Ibérico"
- **Acción**: `category.AddItem("Jamón Ibérico", priceOptions: [...])`
- **Resultado**: Error "Ya existe un item con ese nombre en esta categoría"

---

### Story 5: Configurar Precio "Según Mercado"

**Rule**: Un item con precio "Según Mercado" puede tener precio null hasta que se actualice.

✅ **Example (Success - Crear sin precio)**:
- Agregar "Pulpo al Horno" con precio "Según Mercado" sin especificar precio
- **Acción**: `item.AddPriceOption(PortionType.SegunMercado, price: null)`
- **Resultado**: PriceOption creada, RequiresMarketPrice = true, DisplayPrice = "S/M"

✅ **Example (Success - Actualizar precio)**:
- Actualizar precio del pulpo a €22.00
- **Precondición**: Item "Pulpo" con precio SegunMercado sin valor
- **Acción**: `priceOption.UpdateMarketPrice(22.00m)`
- **Resultado**: Price = 22.00, RequiresMarketPrice = false, DisplayPrice = "€22,00"

✅ **Example (Success - Actualizar precio múltiples veces)**:
- Actualizar precio del pulpo de €22 a €25
- **Precondición**: Price = 22.00
- **Acción**: `priceOption.UpdateMarketPrice(25.00m)`
- **Resultado**: Price = 25.00

❌ **Example (Failure - Precio negativo)**:
- Actualizar con precio negativo
- **Acción**: `priceOption.UpdateMarketPrice(-10.00m)`
- **Resultado**: Error "El precio no puede ser negativo"

❌ **Example (Failure - Actualizar precio en porción fija)**:
- Intentar usar UpdateMarketPrice en una Tapa (precio fijo)
- **Precondición**: PriceOption con PortionType = Tapa
- **Acción**: `priceOption.UpdateMarketPrice(5.00m)`
- **Resultado**: Error "Solo se puede actualizar precios de tipo Según Mercado"

---

### Story 6: Configurar Disponibilidad por Días

**Rule**: Si un item no está siempre disponible, debe tener al menos un día configurado.

✅ **Example (Success - Disponible solo sábados)**:
- Configurar "Pulpo al Horno" disponible solo sábados
- **Acción**: `item.SetAvailability(isAlwaysAvailable: false, days: [DayOfWeek.Saturday])`
- **Resultado**: IsAlwaysAvailable = false, AvailableDays = [Saturday]

✅ **Example (Success - Disponible viernes y sábado)**:
- Configurar "Paella" disponible viernes y sábado
- **Acción**: `item.SetAvailability(false, [DayOfWeek.Friday, DayOfWeek.Saturday])`
- **Resultado**: AvailableDays = [Friday, Saturday]

✅ **Example (Success - Siempre disponible)**:
- Configurar "Croquetas" siempre disponibles
- **Acción**: `item.SetAvailability(isAlwaysAvailable: true, days: [])`
- **Resultado**: IsAlwaysAvailable = true, AvailableDays vacío

❌ **Example (Failure - Sin días cuando no es siempre disponible)**:
- Configurar item sin días cuando IsAlwaysAvailable = false
- **Acción**: `item.SetAvailability(isAlwaysAvailable: false, days: [])`
- **Resultado**: Error "Si no está siempre disponible, debe tener al menos un día"

✅ **Example (Check - Hoy es sábado y el pulpo está disponible)**:
- Verificar si el pulpo está disponible hoy (sábado)
- **Precondición**: Hoy es sábado, Item configurado solo para sábados
- **Resultado**: IsAvailableToday = true

✅ **Example (Check - Hoy es lunes y el pulpo NO está disponible)**:
- Verificar si el pulpo está disponible hoy (lunes)
- **Precondición**: Hoy es lunes, Item configurado solo para sábados
- **Resultado**: IsAvailableToday = false

---

### Story 7: Marcar Item como Alto Riesgo y Requerir Pedido Anticipado

**Rule**: Si un item requiere pedido anticipado, debe ser marcado como alto riesgo primero.

✅ **Example (Success - Marcar como alto riesgo)**:
- Marcar "Pulpo al Horno" como alto riesgo
- **Acción**: `item.MarkAsHighRisk()`
- **Resultado**: IsHighRiskItem = true

✅ **Example (Success - Requerir pedido anticipado)**:
- Configurar que el pulpo requiere pedido anticipado si son 4+ porciones
- **Precondición**: IsHighRiskItem = true
- **Acción**: `item.RequireAdvanceOrder(minimumQuantity: 4)`
- **Resultado**: RequiresAdvanceOrder = true, MinimumAdvanceOrderQuantity = 4

❌ **Example (Failure - Requerir pedido sin ser alto riesgo)**:
- Intentar requerir pedido anticipado sin marcar como alto riesgo
- **Precondición**: IsHighRiskItem = false
- **Acción**: `item.RequireAdvanceOrder(minimumQuantity: 4)`
- **Resultado**: Error "Solo items de alto riesgo pueden requerir pedido anticipado"

✅ **Example (Success - Requerir pedido anticipado sin cantidad mínima)**:
- Configurar que el cochinillo siempre requiere pedido anticipado
- **Precondición**: IsHighRiskItem = true
- **Acción**: `item.RequireAdvanceOrder(minimumQuantity: null)`
- **Resultado**: RequiresAdvanceOrder = true, MinimumAdvanceOrderQuantity = null (siempre requiere)

---

### Story 8: Configurar Override de Fianza para Item

**Rule**: Un item puede tener su propia fianza que sobrescribe la del menú.

✅ **Example (Success - Override sin cantidad mínima)**:
- Configurar fianza de €30 para el pulpo (siempre aplica)
- **Acción**: `item.SetItemDepositOverride(depositAmount: 30m, minimumQuantity: null)`
- **Resultado**: ItemDepositOverride configurado, aplica siempre

✅ **Example (Success - Override con cantidad mínima)**:
- Configurar fianza de €30 para el pulpo si se piden 4+ porciones
- **Acción**: `item.SetItemDepositOverride(depositAmount: 30m, minimumQuantity: 4)`
- **Resultado**: ItemDepositOverride configurado, aplica solo si quantity >= 4

✅ **Example (Success - Eliminar override)**:
- Eliminar la fianza específica del pulpo
- **Precondición**: Item tiene ItemDepositOverride
- **Acción**: `item.RemoveItemDepositOverride()`
- **Resultado**: ItemDepositOverride = null, vuelve a usar política del menú

❌ **Example (Failure - Fianza negativa)**:
- Configurar fianza negativa
- **Acción**: `item.SetItemDepositOverride(depositAmount: -10m)`
- **Resultado**: Error "La fianza debe ser mayor que cero"

❌ **Example (Failure - Fianza cero)**:
- Configurar fianza de €0
- **Acción**: `item.SetItemDepositOverride(depositAmount: 0m)`
- **Resultado**: Error "La fianza debe ser mayor que cero" (usar RemoveItemDepositOverride en su lugar)

---

### Story 9: Control de Stock (Disponible/Agotado)

**Rule**: Un item puede marcarse como agotado durante el servicio.

✅ **Example (Success - Marcar como agotado)**:
- Marcar "Pulpo" como agotado durante el servicio
- **Precondición**: IsAvailable = true
- **Acción**: `item.MarkAsUnavailable()`
- **Resultado**: IsAvailable = false, CanBeOrdered = false

✅ **Example (Success - Marcar como disponible nuevamente)**:
- Volver a marcar "Pulpo" como disponible
- **Precondición**: IsAvailable = false
- **Acción**: `item.MarkAsAvailable()`
- **Resultado**: IsAvailable = true, CanBeOrdered = true (si cumple otras condiciones)

✅ **Example (Check - Item puede pedirse)**:
- Verificar si las croquetas pueden pedirse
- **Precondición**: IsActive = true, IsAvailableToday = true, IsAvailable = true
- **Resultado**: CanBeOrdered = true

✅ **Example (Check - Item NO puede pedirse por agotado)**:
- Verificar si el pulpo puede pedirse cuando está agotado
- **Precondición**: IsActive = true, IsAvailableToday = true, IsAvailable = false
- **Resultado**: CanBeOrdered = false

✅ **Example (Check - Item NO puede pedirse por día)**:
- Verificar si el pulpo puede pedirse un lunes (solo disponible sábados)
- **Precondición**: IsActive = true, IsAvailableToday = false, IsAvailable = true
- **Resultado**: CanBeOrdered = false

---

### Story 10: Cálculo de Fianzas con Lógica de Override

**Rule**: Si un item tiene override, se ignora la política del menú y se usa la mayor fianza.

✅ **Example (Fianza solo de menú)**:
- Menu con política: €10/persona si 6+ comensales
- Reserva: 8 personas, sin items con override
- **Cálculo**: 
  - DepositoMenu = €10 × 8 = €80
  - DepositosItems = 0
  - **FianzaTotal = MAX(€80, €0) = €80**

✅ **Example (Fianza solo de item con override)**:
- Menu sin DepositPolicy
- Reserva: 8 personas, piden 2 raciones de Pulpo (8 porciones)
- Pulpo: Override €30 si 4+ porciones
- **Cálculo**:
  - DepositoMenu = 0
  - DepositosItems = €30 (aplica porque 8 >= 4)
  - **FianzaTotal = MAX(€0, €30) = €30**

✅ **Example (Ambos - Menú mayor)**:
- Menu: €10/persona si 6+ comensales → €80
- Reserva: 8 personas, piden Pulpo (8 porciones)
- Pulpo: Override €30 si 4+ porciones
- **Cálculo**:
  - DepositoMenu = €80
  - DepositosItems = €30
  - **FianzaTotal = MAX(€80, €30) = €80**

✅ **Example (Ambos - Item override mayor)**:
- Menu: €5/persona si 4+ comensales → €20
- Reserva: 4 personas, piden Cochinillo entero
- Cochinillo: Override €60
- **Cálculo**:
  - DepositoMenu = €20
  - DepositosItems = €60
  - **FianzaTotal = MAX(€20, €60) = €60**

✅ **Example (Item con override pero no aplica cantidad)**:
- Menu: €10/persona si 6+ comensales → €80
- Reserva: 8 personas, piden 1 ración de Pulpo (4 porciones)
- Pulpo: Override €30 si 8+ porciones (no aplica)
- **Cálculo**:
  - DepositoMenu = €80
  - DepositosItems = 0 (no aplica porque 4 < 8)
  - **FianzaTotal = MAX(€80, €0) = €80**

✅ **Example (Múltiples items con override)**:
- Menu: €5/persona si 4+ comensales → €20
- Reserva: 6 personas, piden:
  - Pulpo (10 porciones): Override €30 si 4+ porciones → Aplica €30
  - Cochinillo entero: Override €60 → Aplica €60
- **Cálculo**:
  - DepositoMenu = €30 (€5 × 6)
  - DepositosItems = €30 + €60 = €90
  - **FianzaTotal = MAX(€30, €90) = €90**

---

### Story 11: Validación de Item Pedible (Reglas Combinadas)

**Rule**: Un item solo puede pedirse si cumple todas las condiciones.

✅ **Example (Item pedible - Todas las condiciones cumplidas)**:
- Item: "Croquetas"
- **Estado**: IsActive = true, IsAvailableToday = true, IsAvailable = true
- **Resultado**: CanBeOrdered = true

❌ **Example (Item NO pedible - Inactivo)**:
- Item: "Croquetas"
- **Estado**: IsActive = false, IsAvailableToday = true, IsAvailable = true
- **Resultado**: CanBeOrdered = false

❌ **Example (Item NO pedible - No disponible hoy)**:
- Item: "Pulpo al Horno" (solo sábados)
- **Estado**: IsActive = true, IsAvailableToday = false (hoy es lunes), IsAvailable = true
- **Resultado**: CanBeOrdered = false

❌ **Example (Item NO pedible - Agotado)**:
- Item: "Pulpo al Horno"
- **Estado**: IsActive = true, IsAvailableToday = true, IsAvailable = false (agotado)
- **Resultado**: CanBeOrdered = false

---

## 4. Notas de Implementación

### Arquitectura Multiidioma

**En el Dominio C# (Menu aggregate)**:
- Solo se almacena el contenido en el idioma nativo del restaurante
- No hay campos específicos de idioma en Menu, MenuCategory, MenuItem

**A Nivel de Infraestructura (Restaurant)**:
```json
// En Restaurant collection (Firestore)
{
  "id": "restaurant-123",
  "name": "El Pulpo Feliz",
  "menuTranslations": {
    "en": "gs://bucket/restaurants/restaurant-123/menus/en.json",
    "fr": "gs://bucket/restaurants/restaurant-123/menus/fr.json"
  }
}
```

**Flujo de Traducción** (fuera del alcance del dominio Menu):
1. Usuario crea/edita menú en su idioma nativo → Se guarda en Firestore
2. Usuario solicita traducción a idioma X → IA genera traducción
3. Traducción se guarda en Cloud Storage → URL se guarda en `Restaurant.menuTranslations`
4. Cliente accede vía QR → CDN sirve el archivo traducido desde Cloud Storage

---

### Aggregate Boundary

**Menu es el Aggregate Root**:
```
Menu (Root)
 ├─ DepositPolicy (Value Object)
 └─ MenuCategory (Entity)
     └─ MenuItem (Entity)
         ├─ ItemDepositOverride (Value Object)
         ├─ PriceOption (Value Object / Entity débil)
         └─ AvailableDays (Collection)
```

**Reglas de Acceso**:
- Toda modificación pasa por `Menu`
- No se puede acceder directamente a `MenuItem` sin pasar por `Menu` y `MenuCategory`
- `DepositPolicy` y `ItemDepositOverride` son Value Objects inmutables (reemplazar, no modificar)

---

### Persistencia en Firestore

**Estructura Recomendada**:
```
/restaurants/{restaurantId}/menus/{menuId}
{
  "id": "menu-123",
  "restaurantId": "restaurant-456",
  "name": "Carta Principal",
  "description": "...",
  "isActive": true,
  "effectiveFrom": "2025-01-01T00:00:00Z",
  "effectiveUntil": null,
  "displayOrder": 1,
  "depositPolicy": {
    "depositType": 1, // PerPerson
    "amount": 15.00,
    "percentage": null,
    "minimumBillForDeposit": null,
    "minimumGuestsForDeposit": 6
  },
  "categories": [
    {
      "id": "category-789",
      "name": "Pescados y Mariscos",
      "description": "...",
      "displayOrder": 1,
      "isActive": true,
      "items": [
        {
          "id": "item-101",
          "name": "Pulpo al Horno",
          "description": "...",
          "imageUrl": "...",
          "displayOrder": 1,
          "isActive": true,
          "isAvailable": true,
          "isHighRiskItem": true,
          "requiresAdvanceOrder": true,
          "minimumAdvanceOrderQuantity": 4,
          "isAlwaysAvailable": false,
          "availableDays": [6], // Saturday
          "itemDepositOverride": {
            "depositAmount": 30.00,
            "minimumQuantityForDeposit": 4
          },
          "priceOptions": [
            {
              "id": "price-202",
              "portionType": 4, // SegunMercado
              "price": 22.00,
              "isActive": true
            }
          ]
        }
      ]
    }
  ],
  "createdAt": "2025-01-01T10:00:00Z",
  "updatedAt": "2025-01-15T14:30:00Z"
}
```

---

### Lógica de Cálculo de Fianzas (Pseudocódigo)

```csharp
public decimal CalculateReservationDeposit(
    int guestCount, 
    decimal estimatedBill,
    Dictionary<Guid, int> advanceOrderedItems) // itemId -> quantity
{
    decimal menuDeposit = 0;
    decimal itemsDeposit = 0;
    
    // 1. Calcular fianza del menú
    if (DepositPolicy != null && DepositPolicy.IsApplicable(guestCount, estimatedBill))
    {
        menuDeposit = DepositPolicy.CalculateDeposit(guestCount, estimatedBill);
    }
    
    // 2. Calcular fianzas de items con override
    foreach (var (itemId, quantity) in advanceOrderedItems)
    {
        var item = FindItemById(itemId);
        if (item.ItemDepositOverride != null && 
            item.ItemDepositOverride.IsApplicable(quantity))
        {
            itemsDeposit += item.ItemDepositOverride.DepositAmount;
        }
    }
    
    // 3. Retornar el máximo
    return Math.Max(menuDeposit, itemsDeposit);
}
```

---

### Validaciones Importantes

**A Nivel de Menu**:
- Al menos una categoría activa
- Nombres de categorías únicos
- EffectiveFrom < EffectiveUntil
- Si DepositPolicy existe, debe ser válido

**A Nivel de MenuCategory**:
- Al menos un item activo (si categoría está activa)
- Nombres de items únicos en la categoría

**A Nivel de MenuItem**:
- Al menos una PriceOption activa
- No duplicar PortionTypes
- Si RequiresAdvanceOrder = true, entonces IsHighRiskItem = true
- Si IsAlwaysAvailable = false, entonces AvailableDays no vacío
- Si ItemDepositOverride existe, DepositAmount > 0

**A Nivel de PriceOption**:
- Si PortionType != SegunMercado, Price debe tener valor
- Price >= 0 si tiene valor

**A Nivel de DepositPolicy**:
- Si DepositType = PercentageOfBill, Percentage entre 1-100
- Si DepositType != PercentageOfBill, Percentage = null
- Amount > 0
- MinimumGuestsForDeposit >= 1 si se especifica

---

## 5. Casos Edge y Consideraciones

### Casos Edge

**Edge 1: Menu sin DepositPolicy, item sin override**
- **Comportamiento**: No se requiere fianza de reserva
- **FianzaTotal**: €0

**Edge 2: Menu con DepositPolicy, pero no se cumplen umbrales**
- **Ejemplo**: Política requiere 6+ comensales, reserva de 4 personas
- **Comportamiento**: DepositPolicy no aplica (IsApplicable = false)
- **FianzaTotal**: €0 (a menos que haya items con override)

**Edge 3: Item con override, pero no se cumple cantidad mínima**
- **Ejemplo**: Override requiere 4+ porciones, se piden 2 porciones
- **Comportamiento**: Override no aplica
- **FianzaTotal**: Solo fianza del menú (si existe)

**Edge 4: Todas las PriceOptions inactivas**
- **Comportamiento**: Invariante violado, no se permite
- **Acción**: Debe haber al menos una PriceOption activa

**Edge 5: Menú con fechas de vigencia expiradas**
- **Comportamiento**: IsActive se mantiene true, pero cliente puede filtrar por fechas
- **Recomendación**: Verificar EffectiveFrom/EffectiveUntil al listar menús

**Edge 6: Actualizar precio "Según Mercado" múltiples veces al día**
- **Comportamiento**: Permitido, se sobrescribe el valor anterior

**Edge 7: Item con override, menú sin DepositPolicy**
- **Comportamiento**: FianzaTotal = DepositoItem (si aplica)
- **Ejemplo**: Item requiere €30, menú sin política → €30

---

### Consideraciones de Negocio

**¿Cuándo se cobra la fianza?**
- Al confirmar la reserva (momento del pedido anticipado)
- **No** al llegar al restaurante

**¿Cómo se descuenta la fianza?**
- Se resta del total de la cuenta al pagar
- **Ejemplo**: Cuenta €120, Fianza €30 → Cliente paga €90

**¿Qué pasa si no se consumen los items reservados?**
- Restaurante retiene la fianza
- **Ejemplo**: Reservaron pulpo (€30), no vinieron → Restaurante conserva €30

**¿Se puede cambiar la política de fianzas con reservas pendientes?**
- Sí, pero las reservas ya confirmadas mantienen la fianza original
- Nueva política aplica solo a nuevas reservas

---

## 6. Resumen de Invariantes Críticos

### Menu
- ✅ Al menos una categoría activa
- ✅ Nombres de categorías únicos
- ✅ EffectiveFrom < EffectiveUntil (si ambos presentes)

### MenuCategory
- ✅ Al menos un item activo (si categoría activa)
- ✅ Nombres de items únicos en categoría

### MenuItem
- ✅ Al menos una PriceOption activa
- ✅ PortionTypes únicos
- ✅ Si RequiresAdvanceOrder = true → IsHighRiskItem = true
- ✅ Si IsAlwaysAvailable = false → AvailableDays no vacío

### PriceOption
- ✅ Si PortionType != SegunMercado → Price debe tener valor
- ✅ Price >= 0 (si tiene valor)

### DepositPolicy
- ✅ Si DepositType = PercentageOfBill → Percentage entre 1-100
- ✅ Si DepositType != PercentageOfBill → Percentage = null
- ✅ Amount > 0

### ItemDepositOverride
- ✅ DepositAmount > 0
- ✅ MinimumQuantityForDeposit >= 1 (si se especifica)

---

## 7. Diagrama Conceptual

```
Menu (Aggregate Root)
├─ Id: Guid
├─ RestaurantId: Guid
├─ Name: string
├─ DepositPolicy? (Value Object)
│   ├─ DepositType: enum
│   ├─ Amount: decimal
│   ├─ Percentage?: decimal
│   ├─ MinimumBillForDeposit?: decimal
│   └─ MinimumGuestsForDeposit?: int
│
└─ Categories: IReadOnlyCollection<MenuCategory>
    └─ MenuCategory
        ├─ Id: Guid
        ├─ Name: string
        └─ Items: IReadOnlyCollection<MenuItem>
            └─ MenuItem
                ├─ Id: Guid
                ├─ Name: string
                ├─ IsHighRiskItem: bool
                ├─ RequiresAdvanceOrder: bool
                ├─ IsAvailable: bool (stock)
                ├─ IsAlwaysAvailable: bool
                ├─ AvailableDays: IReadOnlyCollection<DayOfWeek>
                ├─ ItemDepositOverride? (Value Object)
                │   ├─ DepositAmount: decimal
                │   └─ MinimumQuantityForDeposit?: int
                │
                └─ PriceOptions: IReadOnlyCollection<PriceOption>
                    └─ PriceOption
                        ├─ Id: Guid
                        ├─ PortionType: enum
                        └─ Price?: decimal
```

---

**Fin del Domain Specification**
