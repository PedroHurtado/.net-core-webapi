# Flujo de Trabajo Rápido (Fast Track)

Este flujo optimizado se centra en definir el **Dominio** y utilizar la IA para generar la implementación completa (Slice, Tests, Persistencia) basándose en nuestros estándares de estilo.

## 1. Definición del Dominio (Input)
El único paso manual obligatorio. Crea un archivo en la carpeta `domain-specs/` (ej. `domain-specs/Pedido.md`) utilizando la plantilla:
- `.plans/templates/domain_definition_template.md`

Asegúrate de definir bien:
- **Estado**: Propiedades y reglas de validación.
- **Comportamiento**: Métodos públicos necesarios (Event Storming).
- **Casos de Uso**: Ejemplos de éxito y fallo (Example Mapping).

## 2. Generación (Prompt a la IA)
Una vez definido el dominio, solicita a la IA la implementación completa referenciando la guía de estilo.

**Prompt Sugerido:**
> "He definido el dominio en `domain-specs/[Entidad].md`.
> Basándote en el estilo de `.plans/templates/style_guide_examples.md`:
> 1.  Genera la **Clase de Dominio** y sus **Tests Unitarios**.
> 2.  Genera el **Slice (Feature)** para el caso de uso [NombreCaso] (Endpoint + Handler).
> 3.  Genera los **Tests de Integración** para este endpoint.
> 4.  Asegúrate de que compila y sigue los patrones del proyecto."

## 3. Recursos Clave
- **Plantilla de Dominio**: `.plans/templates/domain_definition_template.md`
- **Guía de Estilo (Source of Truth)**: `.plans/templates/style_guide_examples.md`
