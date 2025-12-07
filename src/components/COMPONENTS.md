# Fudie Web Components - Documentacion API

Suite de Web Components nativos vanilla JS con Shadow DOM.

---

## Convenciones Generales

- **Shadow DOM**: Todos los componentes usan `attachShadow({ mode: 'open' })`
- **Estilos**: `CSSStyleSheet` con `adoptedStyleSheets`
- **Iconos**: Font Awesome 6 (cargado internamente)
- **Tipografia**: `Plus Jakarta Sans`
- **Color primario**: `--primary: #FF4F5A`

---

## 1. Formularios

### `<fudie-text-field>`

Campo de texto estandar.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `label` | string | Etiqueta superior |
| `value` | string | Valor actual |
| `placeholder` | string | Texto placeholder |
| `type` | string | Tipo de input (text, email, password...) |
| `required` | boolean | Campo requerido |
| `disabled` | boolean | Deshabilitado |
| `error` | string | Mensaje de error |
| `hint` | string | Texto de ayuda |

| Propiedad | Tipo | Descripcion |
|-----------|------|-------------|
| `value` | string | Getter/setter del valor |

| Slot | Descripcion |
|------|-------------|
| `icon-right` | Icono a la derecha del input |

| CSS Variables | Default | Descripcion |
|---------------|---------|-------------|
| `--primary` | #FF4F5A | Color de foco |
| `--input-bg` | #F9FAFB | Fondo del input |
| `--input-border` | #E5E7EB | Color del borde |
| `--input-radius` | 0.75rem | Radio del borde |
| `--focus-ring` | var(--primary) | Color anillo de foco |

| Eventos | Detalle |
|---------|---------|
| `input` | `{ value: string }` |
| `change` | `{ value: string }` |

---

### `<fudie-number-field>`

Campo numerico con validacion.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `label` | string | Etiqueta superior |
| `value` | string | Valor actual |
| `min` | number | Valor minimo |
| `max` | number | Valor maximo |
| `step` | number | Incremento |
| `placeholder` | string | Texto placeholder |
| `required` | boolean | Campo requerido |
| `disabled` | boolean | Deshabilitado |
| `error` | string | Mensaje de error |
| `hint` | string | Texto de ayuda |

| Propiedad | Tipo |
|-----------|------|
| `value` | string |

| Slot | Descripcion |
|------|-------------|
| `icon-right` | Icono a la derecha |

| Eventos | Detalle |
|---------|---------|
| `input` | `{ value: string }` |
| `change` | `{ value: string }` |

---

### `<fudie-select-field>`

Campo select/dropdown.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `label` | string | Etiqueta superior |
| `value` | string | Valor seleccionado |
| `placeholder` | string | Opcion por defecto |
| `options` | JSON | Array de opciones `[{label, value}]` o `["opt1", "opt2"]` |
| `required` | boolean | Campo requerido |
| `disabled` | boolean | Deshabilitado |
| `error` | string | Mensaje de error |
| `hint` | string | Texto de ayuda |

| Propiedad | Tipo |
|-----------|------|
| `value` | string |
| `options` | Array | Setter para opciones programaticamente |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ value: string }` |

---

### `<fudie-textarea-field>`

Area de texto multilinea.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `label` | string | Etiqueta superior |
| `value` | string | Valor actual |
| `placeholder` | string | Texto placeholder |
| `rows` | number | Numero de filas (default: 3) |
| `required` | boolean | Campo requerido |
| `disabled` | boolean | Deshabilitado |
| `error` | string | Mensaje de error |
| `hint` | string | Texto de ayuda |

| Propiedad | Tipo |
|-----------|------|
| `value` | string |

| Eventos | Detalle |
|---------|---------|
| `input` | `{ value: string }` |
| `change` | `{ value: string }` |

---

### `<fudie-toggle>`

Interruptor on/off.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `value` | string | Valor cuando esta activo (default: "on") |
| `checked` | boolean | Estado activo |
| `disabled` | boolean | Deshabilitado |
| `label` | string | Texto del toggle |
| `description` | string | Descripcion secundaria |

| Propiedad | Tipo |
|-----------|------|
| `checked` | boolean |

| CSS Variables | Default | Descripcion |
|---------------|---------|-------------|
| `--toggle-width` | 3rem | Ancho del toggle |
| `--toggle-height` | 1.5rem | Alto del toggle |
| `--toggle-bg` | #E5E7EB | Fondo inactivo |
| `--toggle-checked` | #FF4F5A | Fondo activo |
| `--thumb-size` | 1.125rem | Tamano del thumb |
| `--thumb-color` | white | Color del thumb |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ checked: boolean, value: string }` |

---

### `<fudie-checkbox>`

Casilla de verificacion.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `value` | string | Valor cuando esta activo |
| `checked` | boolean | Estado activo |
| `disabled` | boolean | Deshabilitado |
| `label` | string | Texto del checkbox |
| `description` | string | Descripcion secundaria |

| Propiedad | Tipo |
|-----------|------|
| `checked` | boolean |

| CSS Variables | Default |
|---------------|---------|
| `--checkbox-size` | 1.25rem |
| `--checkbox-bg` | white |
| `--checkbox-border` | #E5E7EB |
| `--checkbox-checked` | #FF4F5A |
| `--checkmark-color` | white |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ checked: boolean, value: string }` |

---

### `<fudie-radio-grid>`

Grid de opciones tipo radio con iconos.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del grupo |
| `label` | string | Etiqueta del grupo |
| `value` | string | Valor seleccionado |
| `options` | JSON | `[{value, label, icon}]` |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ value: string }` |

---

### `<fudie-input-date>`

Selector de fecha nativo.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `label` | string | Etiqueta |
| `value` | string | Fecha en formato YYYY-MM-DD |
| `min` | string | Fecha minima |
| `max` | string | Fecha maxima |
| `required` | boolean | Requerido |
| `disabled` | boolean | Deshabilitado |
| `error` | string | Mensaje de error |
| `hint` | string | Texto de ayuda |

| Propiedad | Tipo |
|-----------|------|
| `value` | string |

| Eventos | Detalle |
|---------|---------|
| `input` | `{ value: string }` |
| `change` | `{ value: string }` |

---

### `<fudie-input-time>`

Selector de hora nativo.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `label` | string | Etiqueta |
| `value` | string | Hora en formato HH:MM |
| `min` | string | Hora minima |
| `max` | string | Hora maxima |
| `required` | boolean | Requerido |
| `disabled` | boolean | Deshabilitado |
| `error` | string | Mensaje de error |
| `hint` | string | Texto de ayuda |

| Propiedad | Tipo |
|-----------|------|
| `value` | string |

| Eventos | Detalle |
|---------|---------|
| `input` | `{ value: string }` |
| `change` | `{ value: string }` |

---

### `<fudie-search>`

Campo de busqueda con icono y boton limpiar.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `placeholder` | string | Texto placeholder (default: "Buscar...") |
| `value` | string | Valor de busqueda |

| Eventos | Detalle |
|---------|---------|
| `search` | `{ value: string }` |
| `clear` | - |

---

### `<fudie-file-upload>`

Zona de carga de archivos con drag & drop.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `multiple` | boolean | Permite multiples archivos |
| `accept` | string | Tipos de archivo aceptados |
| `label` | string | Texto principal |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ files: File[] }` |

---

### `<fudie-stepper>`

Control numerico con botones +/-.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `value` | number | Valor actual |
| `min` | number | Valor minimo |
| `max` | number | Valor maximo |
| `step` | number | Incremento (default: 1) |
| `size` | string | `sm`, `md`, `lg` |
| `disabled` | boolean | Deshabilitado |

| Propiedad | Tipo |
|-----------|------|
| `value` | number |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ value: number }` |

---

## 2. Navegacion

### `<fudie-nav-item>`

Item de navegacion lateral.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `href` | string | URL destino |
| `icon` | string | Nombre de icono Font Awesome |
| `label` | string | Texto del item |
| `active` | boolean | Estado activo |

| Slot | Descripcion |
|------|-------------|
| default | Contenido adicional (ej: badge) |

| CSS Variables | Default |
|---------------|---------|
| `--primary` | #FF4F5A |
| `--primary-light` | #FFF1F2 |

---

### `<fudie-breadcrumb>`

Contenedor de migas de pan.

| Slot | Descripcion |
|------|-------------|
| default | Elementos `<fudie-breadcrumb-item>` |

### `<fudie-breadcrumb-item>`

Item individual de breadcrumb.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `href` | string | URL (omitir para ultimo item) |
| `active` | boolean | Item actual (sin link) |

| Slot | Descripcion |
|------|-------------|
| default | Texto del item |

---

### `<fudie-pagination>`

Control de paginacion.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `current` | number | Pagina actual |
| `total` | number | Total de paginas |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ page: number }` |

---

### `<fudie-tabs>`

Contenedor de tabs.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `active` | number | Indice del tab activo |

**Uso**: Los hijos deben tener atributo `data-tab="Nombre Tab"`:

```html
<fudie-tabs>
  <div data-tab="General">Contenido 1</div>
  <div data-tab="Ajustes">Contenido 2</div>
</fudie-tabs>
```

---

## 3. Contenedores

### `<fudie-card>`

Tarjeta contenedora.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `title` | string | Titulo de la cabecera |
| `subtitle` | string | Subtitulo |
| `no-padding` | boolean | Quitar padding del contenido |
| `no-border` | boolean | Quitar borde y sombra |

| Slot | Descripcion |
|------|-------------|
| default | Contenido principal |
| `header-actions` | Acciones en cabecera |
| `footer` | Contenido del footer |

| CSS Variables | Default |
|---------------|---------|
| `--bg-color` | #FFFFFF |
| `--border-color` | #E5E7EB |
| `--shadow` | 0 1px 3px... |
| `--radius` | 1rem |
| `--padding` | 1.5rem |

---

### `<fudie-modal>`

Dialogo modal.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `open` | boolean | Mostrar modal |
| `title` | string | Titulo del modal |
| `size` | string | `sm`, `md`, `lg`, `full` |
| `close-on-overlay` | string | "false" para deshabilitar cierre al click en overlay |

| Propiedad/Metodo | Descripcion |
|------------------|-------------|
| `isOpen` | Getter: boolean |
| `open()` | Abrir modal |
| `close()` | Cerrar modal |

| Slot | Descripcion |
|------|-------------|
| default | Contenido del body |
| `footer` | Contenido del footer (botones) |

| CSS Variables | Default |
|---------------|---------|
| `--overlay-bg` | rgba(0,0,0,0.5) |
| `--modal-bg` | #ffffff |
| `--radius` | 1.5rem |
| `--z-index` | 50 |

| Eventos |
|---------|
| `fudie-open` |
| `fudie-close` |

---

### `<fudie-accordion>`

Contenedor de acordeon.

| Slot | Descripcion |
|------|-------------|
| default | Elementos `<fudie-accordion-item>` |

### `<fudie-accordion-item>`

Item de acordeon.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `title` | string | Titulo del item |
| `icon` | string | Icono Font Awesome |
| `open` | boolean | Estado abierto |

| Slot | Descripcion |
|------|-------------|
| default | Contenido expandible |

---

### `<fudie-table>`

Tabla de datos.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `columns` | JSON | `[{key, label, type?}]` - types: `image`, `badge`, `actions` |
| `data` | JSON | Array de objetos con datos |

| Eventos | Detalle |
|---------|---------|
| `edit` | `{ id: string }` |
| `delete` | `{ id: string }` |

**Ejemplo**:
```html
<fudie-table
  columns='[{"key":"name","label":"Nombre"},{"key":"status","label":"Estado","type":"badge"}]'
  data='[{"name":"Item 1","status":"Active"}]'>
</fudie-table>
```

---

## 4. Feedback

### `<fudie-progress>`

Barra de progreso.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `value` | number | Valor actual |
| `max` | number | Valor maximo (default: 100) |
| `label` | string | Etiqueta |
| `show-value` | boolean | Mostrar porcentaje |
| `size` | string | `sm`, `md`, `lg`, `xl` |
| `variant` | string | `primary`, `success`, `warning`, `info`, `danger` |
| `striped` | boolean | Patron rayado |
| `animated` | boolean | Animacion de rayas |

| CSS Variables | Default |
|---------------|---------|
| `--bar-bg` | #E5E7EB |
| `--fill-color` | #FF4F5A |
| `--height` | 0.5rem |

---

### `<fudie-spinner>`

Indicador de carga.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `size` | string | `sm`, `md`, `lg` |
| `variant` | string | `primary`, `white`, `secondary` |

---

### `<fudie-alert>`

Mensaje de alerta.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `variant` | string | `info`, `success`, `warning`, `danger` |
| `title` | string | Titulo del mensaje |
| `icon` | string | Icono personalizado |
| `dismissible` | boolean | Mostrar boton cerrar |

| Metodo | Descripcion |
|--------|-------------|
| `dismiss()` | Cerrar alerta con animacion |

| Slot | Descripcion |
|------|-------------|
| default | Contenido del mensaje |

| Eventos |
|---------|
| `dismiss` |

---

### `<fudie-badge>`

Etiqueta/insignia.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `variant` | string | `default`, `primary`, `success`, `warning`, `danger`, `info` |
| `size` | string | `sm`, `md`, `lg` |
| `shape` | string | `pill` (default), `square` |
| `label` | string | Texto del badge |
| `icon` | string | Icono Font Awesome |

| Slot | Descripcion |
|------|-------------|
| default | Contenido adicional |

---

### `<fudie-rating>`

Estrellas de valoracion.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `value` | number | Valor actual |
| `max` | number | Maximo de estrellas (default: 5) |
| `readonly` | boolean | Solo lectura |
| `size` | string | `sm`, `md` (default), `lg` |

| Propiedad | Tipo |
|-----------|------|
| `value` | number |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ value: number }` |

---

## 5. Elementos UI

### `<fudie-button>`

Boton estandar.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `variant` | string | `primary`, `secondary`, `danger`, `ghost` |
| `size` | string | `sm`, `md`, `lg`, `icon` |
| `type` | string | `button`, `submit` |
| `disabled` | boolean | Deshabilitado |
| `full-width` | boolean | Ancho completo |

| Slot | Descripcion |
|------|-------------|
| default | Texto del boton |
| `icon-left` | Icono izquierdo |
| `icon-right` | Icono derecho |

| CSS Variables | Default |
|---------------|---------|
| `--primary` | #FF4F5A |
| `--primary-dark` | #E63E49 |

---

### `<fudie-avatar>`

Avatar de usuario.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `src` | string | URL de imagen |
| `alt` | string | Texto alternativo |
| `initials` | string | Iniciales si no hay imagen |
| `size` | string | `sm`, `md`, `lg`, `xl` |
| `shape` | string | `circle`, `square` |
| `status` | string | `online`, `busy`, `away`, `offline` |

---

### `<fudie-calendar>`

Selector de fecha tipo calendario.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `selected` | string | Fecha seleccionada YYYY-MM-DD |
| `min-date` | string | Fecha minima seleccionable |
| `locale` | string | Locale para formato (default: "es-ES") |

| CSS Variables | Default |
|---------------|---------|
| `--primary` | #FF4F5A |
| `--primary-light` | #FFF1F2 |
| `--radius` | 1rem |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ value: string }` |

---

### `<fudie-grid-picker>`

Selector de opciones en grid.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `name` | string | Nombre del campo |
| `label` | string | Etiqueta |
| `options` | JSON | `[{value, label, icon}]` |
| `value` | string/JSON | Valor seleccionado (JSON si multiple) |
| `multiple` | boolean | Seleccion multiple |
| `columns` | number | Numero de columnas (default: 4) |
| `variant` | string | `fill` para estilo relleno al seleccionar |

| Eventos | Detalle |
|---------|---------|
| `change` | `{ value: string|array, name: string }` |

---

## 6. Cards Especializadas

### `<fudie-product-card>`

Tarjeta de producto.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `image` | string | URL de imagen |
| `title` | string | Nombre del producto |
| `description` | string | Descripcion |
| `price` | string | Precio |
| `tag` | string | Etiqueta/badge (ej: "Nuevo") |

| Slot | Descripcion |
|------|-------------|
| `action` | Boton o accion en el footer |

---

### `<fudie-stat-card>`

Tarjeta de estadistica/KPI.

| Atributo | Tipo | Descripcion |
|----------|------|-------------|
| `label` | string | Etiqueta de la metrica |
| `value` | string | Valor principal |
| `trend` | number | Porcentaje de tendencia |
| `trend-label` | string | Texto de tendencia (default: "vs. mes pasado") |
| `icon` | string | Icono Font Awesome |
| `variant` | string | `primary`, `success`, `info`, `warning` |

---

## Ejemplos de Uso

```html
<!-- Campo de texto con error -->
<fudie-text-field
  label="Email"
  name="email"
  type="email"
  placeholder="tu@email.com"
  error="Email invalido"
  required>
</fudie-text-field>

<!-- Toggle con descripcion -->
<fudie-toggle
  label="Notificaciones"
  description="Recibir alertas por email"
  checked>
</fudie-toggle>

<!-- Modal -->
<fudie-modal id="mi-modal" title="Confirmar accion" size="sm">
  <p>¿Estas seguro?</p>
  <div slot="footer">
    <fudie-button variant="secondary">Cancelar</fudie-button>
    <fudie-button variant="primary">Confirmar</fudie-button>
  </div>
</fudie-modal>

<!-- Abrir modal -->
<script>
  document.getElementById('mi-modal').open();
</script>

<!-- Tabla de datos -->
<fudie-table
  columns='[
    {"key":"avatar","label":"","type":"image"},
    {"key":"name","label":"Nombre"},
    {"key":"status","label":"Estado","type":"badge"},
    {"key":"actions","label":"","type":"actions"}
  ]'
  data='[
    {"id":"1","avatar":"/img/user.jpg","name":"Juan","status":"Active"},
    {"id":"2","avatar":"/img/user2.jpg","name":"Maria","status":"Pending"}
  ]'>
</fudie-table>
```
