# Plan de Implementación: MenuItem Aggregate

**Especificación:** `src/Customer/domain-specs/MenuItem.md`

**Estilos:**
- Agregado: `.plans/templates/styles/style-aggregate.md`
- Value Object: `.plans/templates/styles/style-valueobject.md`
- Enum: `.plans/templates/styles/style-enum.md`
- Comando: `.plans/templates/styles/commands/style-command.md`
- Slice: `.plans/templates/styles/slice/style-slice.md`
- Test Unitario Slice: `.plans/templates/styles/slice/test/style-test-unit-slice.md`
- Test Integración Slice: `.plans/templates/styles/slice/test/style-test-integration-slice.md`

---

## FASE 0: CERRAR EL AGREGADO

Modificaciones base al agregado antes de implementar comandos y slices.

### Tarea 0.1: Modificar MenuItem.cs

| Spec | Sección |
|------|---------|
| Estructura | `MenuItem.md` → §3. Aggregate: MenuItem → Estructura |
| Propiedades | `MenuItem.md` → §3. Aggregate: MenuItem → Propiedades |
| Propiedades Calculadas | `MenuItem.md` → §3. Aggregate: MenuItem → Propiedades Calculadas |
| Validaciones | `MenuItem.md` → §3. Aggregate: MenuItem → Validaciones |

**Cambios:**
- [ ] Name max length: 150 → 100
- [ ] Añadir propiedad calculada: `IsAvailableToday`
- [ ] Añadir propiedad calculada: `CanBeOrdered`
- [ ] Añadir propiedad calculada: `HasDepositOverride`
- [ ] Añadir propiedad calculada: `HasActivePriceOption`
- [ ] Añadir validación: `UniquePortion` para PriceOptions
- [ ] Actualizar mensaje: "Name cannot exceed 100 characters"
- [ ] Actualizar mensaje: "Only high-risk items can require advance order"
- [ ] Actualizar mensaje: "Minimum quantity requires advance order to be enabled"

---

### Tarea 0.2: Modificar ItemDepositOverride.cs

| Spec | Sección |
|------|---------|
| Estructura | `MenuItem.md` → §2.2 ItemDepositOverride → Estructura |
| Validaciones | `MenuItem.md` → §2.2 ItemDepositOverride → Validaciones |
| Propiedades Calculadas | `MenuItem.md` → §2.2 ItemDepositOverride → Propiedades Calculadas |

**Cambios:**
- [ ] Añadir propiedad calculada: `AppliesToAllQuantities`
- [ ] Añadir validación max: `DepositAmount <= 10000`
- [ ] Añadir validación max: `MinimumQuantityForDeposit <= 100`
- [ ] Actualizar mensaje: "Deposit amount must be greater than zero"
- [ ] Actualizar mensaje: "Minimum quantity must be at least 1"
- [ ] Añadir mensaje: "Deposit amount cannot exceed 10000"
- [ ] Añadir mensaje: "Minimum quantity cannot exceed 100"

---

### Tarea 0.3: Modificar NutritionalInfo.cs

| Spec | Sección |
|------|---------|
| Validaciones | `MenuItem.md` → §2.3 NutritionalInfo → Validaciones |

**Cambios:**
- [ ] Actualizar mensaje: "Calories cannot be negative"
- [ ] Actualizar mensaje: "Calories cannot exceed 10000 kcal"
- [ ] Actualizar mensaje: "Serving size must be greater than zero"
- [ ] (y demás mensajes según spec)

---

### Tarea 0.4: Actualizar Tests del Agregado

**Cambios:**
- [ ] `MenuItemValidatorTests.cs` - Tests Name max 100
- [ ] `MenuItemValidatorTests.cs` - Tests UniquePortion
- [ ] `MenuItemTests.cs` - Tests propiedades calculadas
- [ ] `ItemDepositOverrideValidatorTests.cs` - Tests límites max
- [ ] `ItemDepositOverrideTests.cs` - Test `AppliesToAllQuantities`

---

## FASE 1: COMANDOS + SLICES

---

### Tarea 1: POST /menu-items

**Endpoint:** `POST /menu-items` → 201 Created → `MenuItemResponse`

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.1 MenuItem.Create |
| Input | `MenuItem.md` → §6.1 MenuItem.Create → Input |
| Lógica | `MenuItem.md` → §6.1 MenuItem.Create → Lógica |
| Request | `MenuItem.md` → §6.1 MenuItem.Create → Slice: POST /menu-items → Request |
| Response | `MenuItem.md` → §4. Response → MenuItemResponse |
| Tests Dominio | `MenuItem.md` → §6.1 MenuItem.Create → Tests Unitarios (Dominio) |
| Tests Servicio | `MenuItem.md` → §6.1 MenuItem.Create → Tests Unitarios (Servicio) |
| Tests Integración | `MenuItem.md` → §6.1 MenuItem.Create → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 1.1 | Comando | `Commands/MenuItem/MenuItem_Create.cs` (MODIFICAR) | [ ] |
| 1.2 | Test dominio | `MenuItemCreateTests.cs` (MODIFICAR) | [ ] |
| 1.3 | Slice | `Api/Commands/MenuItems/CreateMenuItem.cs` | [ ] |
| 1.4 | Test unitario slice | `UnitTests/.../CreateMenuItemTests.cs` | [ ] |
| 1.5 | Test integración | `IntegrationTests/.../CreateMenuItemTests.cs` | [ ] |

**Modificaciones comando:**
- [ ] `IsActive = false` (no `true`)
- [ ] `PriceOptions`: cambiar de `HashSet<PriceOption>` a `CreatePriceOptionCommand[]`
- [ ] Eliminar parámetro `Allergens`
- [ ] Eliminar parámetro `DepositOverride`
- [ ] Eliminar parámetro `NutritionalInfo`
- [ ] Inyectar `PriceOption.Create`

---

### Tarea 2: GET /menu-items/{id}

**Endpoint:** `GET /menu-items/{id}` → 200 OK → `MenuItemResponse`

| Spec | Sección |
|------|---------|
| Query | `MenuItem.md` → §7. Queries → GetMenuItem |
| Response | `MenuItem.md` → §4. Response → MenuItemResponse |
| Tests Integración | `MenuItem.md` → §7. Queries → GetMenuItem → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 2.1 | Slice | `Api/Queries/MenuItems/GetMenuItem.cs` | [ ] |
| 2.2 | Test unitario slice | `UnitTests/.../GetMenuItemTests.cs` | [ ] |
| 2.3 | Test integración | `IntegrationTests/.../GetMenuItemTests.cs` | [ ] |

**Status codes:** 200, 404

---

### Tarea 3: GET /menu-items

**Endpoint:** `GET /menu-items` → 200 OK → `MenuItemResponse[]`

| Spec | Sección |
|------|---------|
| Query | `MenuItem.md` → §7. Queries → ListMenuItems |
| Response | `MenuItem.md` → §4. Response → MenuItemResponse |
| Tests Integración | `MenuItem.md` → §7. Queries → ListMenuItems → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 3.1 | Slice | `Api/Queries/MenuItems/ListMenuItems.cs` | [ ] |
| 3.2 | Test unitario slice | `UnitTests/.../ListMenuItemsTests.cs` | [ ] |
| 3.3 | Test integración | `IntegrationTests/.../ListMenuItemsTests.cs` | [ ] |

**Status codes:** 200 (array), 200 (vacío)

---

### Tarea 4: PUT /menu-items/{id}

**Endpoint:** `PUT /menu-items/{id}` → 204 No Content

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.2 MenuItem.Update |
| Input | `MenuItem.md` → §6.2 MenuItem.Update → Input |
| Lógica | `MenuItem.md` → §6.2 MenuItem.Update → Lógica |
| Request | `MenuItem.md` → §6.2 MenuItem.Update → Slice: PUT /menu-items/{id} → Request |
| Tests Dominio | `MenuItem.md` → §6.2 MenuItem.Update → Tests Unitarios (Dominio) |
| Tests Servicio | `MenuItem.md` → §6.2 MenuItem.Update → Tests Unitarios (Servicio) |
| Tests Integración | `MenuItem.md` → §6.2 MenuItem.Update → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 4.1 | Comando | `Commands/MenuItem/MenuItem_Update.cs` | [ ] |
| 4.2 | Test dominio | `MenuItem_UpdateTests.cs` | [ ] |
| 4.3 | Slice | `Api/Commands/MenuItems/UpdateMenuItem.cs` | [ ] |
| 4.4 | Test unitario slice | `UnitTests/.../UpdateMenuItemTests.cs` | [ ] |
| 4.5 | Test integración | `IntegrationTests/.../UpdateMenuItemTests.cs` | [ ] |

**Status codes:** 204, 404, 422

---

### Tarea 5: POST /menu-items/{id}/activate

**Endpoint:** `POST /menu-items/{id}/activate` → 200 OK → `MenuItemResponse`

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.3 MenuItem.Activate |
| Guards | `MenuItem.md` → §6.3 MenuItem.Activate → Guards |
| Lógica | `MenuItem.md` → §6.3 MenuItem.Activate → Lógica |
| Tests Dominio | `MenuItem.md` → §6.3 MenuItem.Activate → Tests Unitarios (Dominio) |
| Tests Servicio | `MenuItem.md` → §6.3 MenuItem.Activate → Tests Unitarios (Servicio) |
| Tests Integración | `MenuItem.md` → §6.3 MenuItem.Activate → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 5.1 | Comando | `Commands/MenuItem/MenuItem_Activate.cs` | [ ] |
| 5.2 | Test dominio | `MenuItem_ActivateTests.cs` | [ ] |
| 5.3 | Slice | `Api/Commands/MenuItems/ActivateMenuItem.cs` | [ ] |
| 5.4 | Test unitario slice | `UnitTests/.../ActivateMenuItemTests.cs` | [ ] |
| 5.5 | Test integración | `IntegrationTests/.../ActivateMenuItemTests.cs` | [ ] |

**Status codes:** 200, 404, 409, 422

---

### Tarea 6: POST /menu-items/{id}/deactivate

**Endpoint:** `POST /menu-items/{id}/deactivate` → 200 OK → `MenuItemResponse`

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.4 MenuItem.Deactivate |
| Guards | `MenuItem.md` → §6.4 MenuItem.Deactivate → Guards |
| Lógica | `MenuItem.md` → §6.4 MenuItem.Deactivate → Lógica |
| Tests Dominio | `MenuItem.md` → §6.4 MenuItem.Deactivate → Tests Unitarios (Dominio) |
| Tests Servicio | `MenuItem.md` → §6.4 MenuItem.Deactivate → Tests Unitarios (Servicio) |
| Tests Integración | `MenuItem.md` → §6.4 MenuItem.Deactivate → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 6.1 | Comando | `Commands/MenuItem/MenuItem_Deactivate.cs` | [ ] |
| 6.2 | Test dominio | `MenuItem_DeactivateTests.cs` | [ ] |
| 6.3 | Slice | `Api/Commands/MenuItems/DeactivateMenuItem.cs` | [ ] |
| 6.4 | Test unitario slice | `UnitTests/.../DeactivateMenuItemTests.cs` | [ ] |
| 6.5 | Test integración | `IntegrationTests/.../DeactivateMenuItemTests.cs` | [ ] |

**Status codes:** 200, 404, 409

---

### Tarea 7: POST /menu-items/{id}/mark-available

**Endpoint:** `POST /menu-items/{id}/mark-available` → 200 OK → `MenuItemResponse`

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.5 MenuItem.MarkAsAvailable |
| Lógica | `MenuItem.md` → §6.5 MenuItem.MarkAsAvailable → Lógica |
| Tests Dominio | `MenuItem.md` → §6.5 MenuItem.MarkAsAvailable → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.5 MenuItem.MarkAsAvailable → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 7.1 | Comando | `Commands/MenuItem/MenuItem_MarkAsAvailable.cs` | [ ] |
| 7.2 | Test dominio | `MenuItem_MarkAsAvailableTests.cs` | [ ] |
| 7.3 | Slice | `Api/Commands/MenuItems/MarkMenuItemAvailable.cs` | [ ] |
| 7.4 | Test unitario slice | `UnitTests/.../MarkMenuItemAvailableTests.cs` | [ ] |
| 7.5 | Test integración | `IntegrationTests/.../MarkMenuItemAvailableTests.cs` | [ ] |

**Status codes:** 200, 404

---

### Tarea 8: POST /menu-items/{id}/mark-unavailable

**Endpoint:** `POST /menu-items/{id}/mark-unavailable` → 200 OK → `MenuItemResponse`

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.6 MenuItem.MarkAsUnavailable |
| Lógica | `MenuItem.md` → §6.6 MenuItem.MarkAsUnavailable → Lógica |
| Tests Dominio | `MenuItem.md` → §6.6 MenuItem.MarkAsUnavailable → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.6 MenuItem.MarkAsUnavailable → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 8.1 | Comando | `Commands/MenuItem/MenuItem_MarkAsUnavailable.cs` | [ ] |
| 8.2 | Test dominio | `MenuItem_MarkAsUnavailableTests.cs` | [ ] |
| 8.3 | Slice | `Api/Commands/MenuItems/MarkMenuItemUnavailable.cs` | [ ] |
| 8.4 | Test unitario slice | `UnitTests/.../MarkMenuItemUnavailableTests.cs` | [ ] |
| 8.5 | Test integración | `IntegrationTests/.../MarkMenuItemUnavailableTests.cs` | [ ] |

**Status codes:** 200, 404

---

### Tarea 9: PUT /menu-items/{id}/deposit-override

**Endpoint:** `PUT /menu-items/{id}/deposit-override` → 204 No Content

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.7 MenuItem.SetDepositOverride |
| Input | `MenuItem.md` → §6.7 MenuItem.SetDepositOverride → Input |
| Lógica | `MenuItem.md` → §6.7 MenuItem.SetDepositOverride → Lógica |
| Request | `MenuItem.md` → §6.7 MenuItem.SetDepositOverride → Slice → Request |
| Tests Dominio | `MenuItem.md` → §6.7 MenuItem.SetDepositOverride → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.7 MenuItem.SetDepositOverride → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 9.1 | Comando | `Commands/MenuItem/MenuItem_SetDepositOverride.cs` | [ ] |
| 9.2 | Test dominio | `MenuItem_SetDepositOverrideTests.cs` | [ ] |
| 9.3 | Slice | `Api/Commands/MenuItems/SetDepositOverride.cs` | [ ] |
| 9.4 | Test unitario slice | `UnitTests/.../SetDepositOverrideTests.cs` | [ ] |
| 9.5 | Test integración | `IntegrationTests/.../SetDepositOverrideTests.cs` | [ ] |

**Status codes:** 204, 404, 422

---

### Tarea 10: DELETE /menu-items/{id}/deposit-override

**Endpoint:** `DELETE /menu-items/{id}/deposit-override` → 204 No Content

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.8 MenuItem.RemoveDepositOverride |
| Lógica | `MenuItem.md` → §6.8 MenuItem.RemoveDepositOverride → Lógica |
| Tests Dominio | `MenuItem.md` → §6.8 MenuItem.RemoveDepositOverride → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.8 MenuItem.RemoveDepositOverride → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 10.1 | Comando | `Commands/MenuItem/MenuItem_RemoveDepositOverride.cs` | [ ] |
| 10.2 | Test dominio | `MenuItem_RemoveDepositOverrideTests.cs` | [ ] |
| 10.3 | Slice | `Api/Commands/MenuItems/RemoveDepositOverride.cs` | [ ] |
| 10.4 | Test unitario slice | `UnitTests/.../RemoveDepositOverrideTests.cs` | [ ] |
| 10.5 | Test integración | `IntegrationTests/.../RemoveDepositOverrideTests.cs` | [ ] |

**Status codes:** 204, 404

---

### Tarea 11: PUT /menu-items/{id}/nutritional-info

**Endpoint:** `PUT /menu-items/{id}/nutritional-info` → 204 No Content

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.9 MenuItem.SetNutritionalInfo |
| Input | `MenuItem.md` → §6.9 MenuItem.SetNutritionalInfo → Input |
| Lógica | `MenuItem.md` → §6.9 MenuItem.SetNutritionalInfo → Lógica |
| Request | `MenuItem.md` → §6.9 MenuItem.SetNutritionalInfo → Slice → Request |
| Tests Dominio | `MenuItem.md` → §6.9 MenuItem.SetNutritionalInfo → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.9 MenuItem.SetNutritionalInfo → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 11.1 | Comando | `Commands/MenuItem/MenuItem_SetNutritionalInfo.cs` | [ ] |
| 11.2 | Test dominio | `MenuItem_SetNutritionalInfoTests.cs` | [ ] |
| 11.3 | Slice | `Api/Commands/MenuItems/SetNutritionalInfo.cs` | [ ] |
| 11.4 | Test unitario slice | `UnitTests/.../SetNutritionalInfoTests.cs` | [ ] |
| 11.5 | Test integración | `IntegrationTests/.../SetNutritionalInfoTests.cs` | [ ] |

**Status codes:** 204, 404, 422

---

### Tarea 12: DELETE /menu-items/{id}/nutritional-info

**Endpoint:** `DELETE /menu-items/{id}/nutritional-info` → 204 No Content

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.10 MenuItem.RemoveNutritionalInfo |
| Lógica | `MenuItem.md` → §6.10 MenuItem.RemoveNutritionalInfo → Lógica |
| Tests Dominio | `MenuItem.md` → §6.10 MenuItem.RemoveNutritionalInfo → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.10 MenuItem.RemoveNutritionalInfo → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 12.1 | Comando | `Commands/MenuItem/MenuItem_RemoveNutritionalInfo.cs` | [ ] |
| 12.2 | Test dominio | `MenuItem_RemoveNutritionalInfoTests.cs` | [ ] |
| 12.3 | Slice | `Api/Commands/MenuItems/RemoveNutritionalInfo.cs` | [ ] |
| 12.4 | Test unitario slice | `UnitTests/.../RemoveNutritionalInfoTests.cs` | [ ] |
| 12.5 | Test integración | `IntegrationTests/.../RemoveNutritionalInfoTests.cs` | [ ] |

**Status codes:** 204, 404

---

### Tarea 13: POST /menu-items/{id}/price-options

**Endpoint:** `POST /menu-items/{id}/price-options` → 201 Created → `MenuItemResponse`

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.11 MenuItem.AddPriceOption |
| Input | `MenuItem.md` → §6.11 MenuItem.AddPriceOption → Input |
| Guards | `MenuItem.md` → §6.11 MenuItem.AddPriceOption → Guards |
| Lógica | `MenuItem.md` → §6.11 MenuItem.AddPriceOption → Lógica |
| Request | `MenuItem.md` → §6.11 MenuItem.AddPriceOption → Slice → Request |
| Tests Dominio | `MenuItem.md` → §6.11 MenuItem.AddPriceOption → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.11 MenuItem.AddPriceOption → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 13.1 | Comando | `Commands/MenuItem/MenuItem_AddPriceOption.cs` | [ ] |
| 13.2 | Test dominio | `MenuItem_AddPriceOptionTests.cs` | [ ] |
| 13.3 | Slice | `Api/Commands/MenuItems/AddPriceOption.cs` | [ ] |
| 13.4 | Test unitario slice | `UnitTests/.../AddPriceOptionTests.cs` | [ ] |
| 13.5 | Test integración | `IntegrationTests/.../AddPriceOptionTests.cs` | [ ] |

**Status codes:** 201, 404, 409, 422

---

### Tarea 14: PUT /menu-items/{id}/price-options/{portionType}

**Endpoint:** `PUT /menu-items/{id}/price-options/{portionType}` → 204 No Content

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.12 MenuItem.UpdatePriceOption |
| Input | `MenuItem.md` → §6.12 MenuItem.UpdatePriceOption → Input |
| Guards | `MenuItem.md` → §6.12 MenuItem.UpdatePriceOption → Guards |
| Lógica | `MenuItem.md` → §6.12 MenuItem.UpdatePriceOption → Lógica |
| Request | `MenuItem.md` → §6.12 MenuItem.UpdatePriceOption → Slice → Request |
| Tests Dominio | `MenuItem.md` → §6.12 MenuItem.UpdatePriceOption → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.12 MenuItem.UpdatePriceOption → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 14.1 | Comando | `Commands/MenuItem/MenuItem_UpdatePriceOption.cs` | [ ] |
| 14.2 | Test dominio | `MenuItem_UpdatePriceOptionTests.cs` | [ ] |
| 14.3 | Slice | `Api/Commands/MenuItems/UpdatePriceOption.cs` | [ ] |
| 14.4 | Test unitario slice | `UnitTests/.../UpdatePriceOptionTests.cs` | [ ] |
| 14.5 | Test integración | `IntegrationTests/.../UpdatePriceOptionTests.cs` | [ ] |

**Status codes:** 204, 404, 422

---

### Tarea 15: DELETE /menu-items/{id}/price-options/{portionType}

**Endpoint:** `DELETE /menu-items/{id}/price-options/{portionType}` → 204 No Content

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.13 MenuItem.RemovePriceOption |
| Guards | `MenuItem.md` → §6.13 MenuItem.RemovePriceOption → Guards |
| Lógica | `MenuItem.md` → §6.13 MenuItem.RemovePriceOption → Lógica |
| Tests Dominio | `MenuItem.md` → §6.13 MenuItem.RemovePriceOption → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.13 MenuItem.RemovePriceOption → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 15.1 | Comando | `Commands/MenuItem/MenuItem_RemovePriceOption.cs` | [ ] |
| 15.2 | Test dominio | `MenuItem_RemovePriceOptionTests.cs` | [ ] |
| 15.3 | Slice | `Api/Commands/MenuItems/RemovePriceOption.cs` | [ ] |
| 15.4 | Test unitario slice | `UnitTests/.../RemovePriceOptionTests.cs` | [ ] |
| 15.5 | Test integración | `IntegrationTests/.../RemovePriceOptionTests.cs` | [ ] |

**Status codes:** 204, 404, 422

---

### Tarea 16: POST /menu-items/{id}/allergens

**Endpoint:** `POST /menu-items/{id}/allergens` → 201 Created → `MenuItemResponse`

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.14 MenuItem.AddAllergen |
| Input | `MenuItem.md` → §6.14 MenuItem.AddAllergen → Input |
| Guards | `MenuItem.md` → §6.14 MenuItem.AddAllergen → Guards |
| Lógica | `MenuItem.md` → §6.14 MenuItem.AddAllergen → Lógica |
| Request | `MenuItem.md` → §6.14 MenuItem.AddAllergen → Slice → Request |
| Tests Dominio | `MenuItem.md` → §6.14 MenuItem.AddAllergen → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.14 MenuItem.AddAllergen → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 16.1 | Comando | `Commands/MenuItem/MenuItem_AddAllergen.cs` | [ ] |
| 16.2 | Test dominio | `MenuItem_AddAllergenTests.cs` | [ ] |
| 16.3 | Slice | `Api/Commands/MenuItems/AddAllergen.cs` | [ ] |
| 16.4 | Test unitario slice | `UnitTests/.../AddAllergenTests.cs` | [ ] |
| 16.5 | Test integración | `IntegrationTests/.../AddAllergenTests.cs` | [ ] |

**Status codes:** 201, 404, 409

---

### Tarea 17: DELETE /menu-items/{id}/allergens/{allergenId}

**Endpoint:** `DELETE /menu-items/{id}/allergens/{allergenId}` → 204 No Content

| Spec | Sección |
|------|---------|
| Comando | `MenuItem.md` → §6.15 MenuItem.RemoveAllergen |
| Guards | `MenuItem.md` → §6.15 MenuItem.RemoveAllergen → Guards |
| Lógica | `MenuItem.md` → §6.15 MenuItem.RemoveAllergen → Lógica |
| Tests Dominio | `MenuItem.md` → §6.15 MenuItem.RemoveAllergen → Tests Unitarios (Dominio) |
| Tests Integración | `MenuItem.md` → §6.15 MenuItem.RemoveAllergen → Tests Integración |

| Paso | Tipo | Archivo | Estado |
|------|------|---------|--------|
| 17.1 | Comando | `Commands/MenuItem/MenuItem_RemoveAllergen.cs` | [ ] |
| 17.2 | Test dominio | `MenuItem_RemoveAllergenTests.cs` | [ ] |
| 17.3 | Slice | `Api/Commands/MenuItems/RemoveAllergen.cs` | [ ] |
| 17.4 | Test unitario slice | `UnitTests/.../RemoveAllergenTests.cs` | [ ] |
| 17.5 | Test integración | `IntegrationTests/.../RemoveAllergenTests.cs` | [ ] |

**Status codes:** 204, 404

---

## RESUMEN

| Fase | Tareas | Descripción |
|------|--------|-------------|
| **Fase 0** | 0.1 - 0.4 | Cerrar agregado (MenuItem, ItemDepositOverride, NutritionalInfo, tests base) |
| **Fase 1** | 1 - 17 | 17 endpoints verticales (comando + tests + slice + tests) |

**Totales:**
- Comandos de dominio: 14 nuevos + 1 modificación
- Slices: 17
- Tests dominio: 14 nuevos + modificaciones
- Tests unitarios slice: 17
- Tests integración: 17
