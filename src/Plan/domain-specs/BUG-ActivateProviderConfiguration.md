# BUG: ActivateProviderConfiguration - Identificador ambiguo

## Problema

El comando `ActivateProviderConfiguration` usa solo `provider` como identificador, pero puede existir múltiples configs del mismo provider en un plan.

## Escenario

1. Plan tiene config Stripe `prod_2024` (activo)
2. Stripe cambia precios → añado config Stripe `prod_2025` (inactivo)
3. Quiero activar `prod_2025` pero el endpoint solo recibe `provider=Stripe`
4. `FirstOrDefault` encuentra una config arbitraria

## Solución propuesta

Cambiar el identificador del endpoint de `provider` a `externalProductId`.

## Archivos afectados

- Comando de dominio: `Plan_ActivateProviderConfiguration.cs`
- Slice: `ActivateProviderConfiguration.cs`
- Tests unitarios e integración
