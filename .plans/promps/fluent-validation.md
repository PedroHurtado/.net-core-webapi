# Genera tests para validador FluentValidation

## Reglas

1. **Antes de codificar**: muéstrame tabla con tests propuestos (nombre, tipo, casos). Espera mi aprobación.

2. **Nomenclatura**: `{Propiedad}_When{Condición}_Should{Resultado}`

3. **Diseño**:
   - Separar tests válidos e inválidos (nunca mezclar)
   - Incluir valores límite exactos
   - Probar null en propiedades nullable
   - [Theory] para múltiples casos, [Fact] para caso único

4. **Helper con reflexión** para crear instancias sin pasar por factory methods

5. **Usar** `FluentValidation.TestHelper`

6. **Respetar** estructura de carpetas del proyecto de test

## Validador
[PEGAR CÓDIGO]