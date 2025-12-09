## Prompt 1.5: Validación + OpenAPI + Wireframe

Revisa el código generado y genera los contratos.

### Contexto
@[domain-specs/[Entidad].md]
@[src/Domain/[Entidad]/Entities/*.cs]
@[src/Domain/[Entidad]/ValueObjects/*.cs]
@[src/Domain/[Entidad]/Enums/*.cs]
@[wireframes/ejemplo-guia.html]

### Tareas

**1. Validación del código generado**
- Entities: DTOs con propiedades según spec
- ValueObjects: factory Create() que lanza excepción si inválido
- Enums: valores según especificación

Si hay errores, lista correcciones antes de continuar.

**2. OpenAPI YAML**
`contracts/[entidad]/openapi.yaml`:
- Paths: comandos del Event Storming
- Schemas: request/response por comando
- Validaciones como constraints
- Responses: 200/201/204, 400, 404, 409, 422

**3. Wireframe HTML**
`wireframes/[entidad]-admin.html`:
- Sigue el estilo del ejemplo adjunto
- Una sola página con todos los comandos y consultas
- Standalone (Tailwind CDN)

### Output
1. Validaciones ✅/❌
2. `openapi.yaml`
3. `[entidad]-admin.html`