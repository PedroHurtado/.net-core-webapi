Continúo desarrollo Fudie con patrón MicroDomain validado.

Resumen del patrón:
- Entity = DTO (2 constructores: protected + public con Guid, props públicas, HashSet para colecciones)
- ValueObject = record con factory Create() → Result<T>, constructor private
- Validator = FluentValidation en mismo archivo que Entity, clase separada
- Command = [Injectable], implementa ICreateCommand<TCmd,TEntity> o IModifyCommand<TCmd,TEntity>, valida al final con Entity.ValidateEntity()
- Inyección = clase concreta (UpdateFoo), no interfaz genérica
- Interfaces = solo contrato, no para DI

Flujo:
- Fase 1: Modelo completo (Enums → ValueObjects → Entities → Aggregate) - equipo junto, valida Domain Spec
- Fase 2: Commands en paralelo

Archivos de referencia en proyecto:
- .plans/MICRODOMAIN_TEAM_NOTES.md
- Domain Specs del agregado a implementar

¿Qué agregado o comando continuamos?