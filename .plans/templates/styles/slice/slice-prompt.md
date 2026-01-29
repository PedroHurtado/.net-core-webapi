## Tarea

Crear slice completa con tests unitarios y de integración.

## Guías de estilo (seguir ESTRICTAMENTE)

- `.plans/templates/styles/slice/style-slice.md`
- `.plans/templates/styles/slice/style-slice-test-unit.md`
- `.plans/templates/styles/slice/style-slice-test-integration.md`

## Especificación

[PEGAR AQUÍ LA ESPECIFICACIÓN]

## PROHIBIDO

- NO explores la estructura del proyecto
- NO leas archivos que no estén indicados en los pasos

## Pasos (en este orden exacto)

1. Leer las 3 guías de estilo
2. Localizar y leer el comando de dominio: `**/{Aggregate}_{Action}.cs`
3. Crear la slice
4. Crear tests unitarios de SERVICIO (no de dominio)
5. Crear tests de integración (DEBE incluir test de persistencia: acción → GET → verificar)

## Verificación
```bash
dotnet build --nologo -v q
dotnet test --filter "FullyQualifiedName~[ClaseTestUnitario]" --nologo -v q
dotnet test --filter "FullyQualifiedName~[ClaseTestIntegracion]" --nologo -v q
```

## Regresión

Solo del proyecto afectado:
```bash
dotnet test tests/[Proyecto].UnitTests --nologo -v q
dotnet test tests/[Proyecto].IntegrationTests --nologo -v q
```

## Entrega

Mensaje de commit.