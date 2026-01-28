## Tarea

Crear slice completa con tests unitarios y de integración.

## Guías de estilo (seguir ESTRICTAMENTE)

- `.plans/templates/styles/slice/style-slice.md`
- `.plans/templates/styles/slice/style-slice-test-unit.md`
- `.plans/templates/styles/slice/style-slice-test-integration.md`

## Especificación

[PEGAR AQUÍ LA ESPECIFICACIÓN]

## Pasos

1. Leer las 3 guías de estilo antes de escribir código
2. Crear la slice
3. Crear tests unitariosa
4. Crear tests de integración

## Verificación
```bash
dotnet build
dotnet test --filter "FullyQualifiedName~[NombreDelTestUnitarioCreado]"
dotnet test --filter "FullyQualifiedName~[NombreDelTestIntegracionCreado]"
```

## Regresión completa

Ejecutar todos los tests del proyecto.

## Entrega

Proporcionar mensaje de commit.