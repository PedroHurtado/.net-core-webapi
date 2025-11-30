# Prompt para Generación con IA

Copia y pega este prompt en tu chat con la IA, reemplazando los corchetes `[...]` con tu información.

---

**Rol**: Eres un Arquitecto de Software experto en .NET 8, DDD y Vertical Slices.

**Contexto**:
Estoy desarrollando un microservicio y necesito implementar una nueva funcionalidad siguiendo estrictamente los estándares del proyecto.

**Inputs**:
1.  **Definición del Dominio**:
    *(Copia aquí el contenido de tu `domain-specs/Entidad.md`)*

2.  **Especificación de Persistencia** (Opcional):
    *(Copia aquí el contenido de tu `persistence_template.md` relleno, o indica "Infiérelo del dominio")*

3.  **Guía de Estilo**:
    Por favor, lee y sigue estrictamente los patrones definidos en el archivo `.plans/templates/style_guide_examples.md` que tienes en tu contexto.

**Tarea**:
Genera el código necesario para implementar esta funcionalidad completa. Debes entregar:

1.  **Capa de Dominio**:
    - La clase `Entity` completa con validaciones y métodos fábrica.
    - **Tests Unitarios** de Dominio (xUnit) cubriendo los casos del Example Mapping.

2.  **Capa de Infraestructura**:
    - La clase `IEntityTypeConfiguration<T>` de EF Core.

3.  **Capa de Aplicación (Feature Slice)**:
    - El comando/query `Create[Entidad]` (o el que corresponda).
    - El Endpoint (Minimal API) y el Handler.
    - DTOs de Request y Response.

4.  **Tests de Integración**:
    - Un test de integración completo probando el Happy Path y errores de validación, usando `WebApplicationFactory`.

**Restricciones**:
- Usa `FluentValidation` anidado en la entidad.
- Usa `Result<T>` para el manejo de errores.
- No uses Controladores, usa Minimal APIs con `IFeatureModule`.
- Asegúrate de que el código compile y sea consistente con los ejemplos de la guía de estilo.
