# Comparativa de Métodos de Consulta: Spring Data vs LINQ vs Firestore .NET SDK

## Tabla Comparativa

| Método Spring Data | ✓ | LINQ (EF Core) | Firestore .NET SDK | Notas Firestore |
|---|:---:|---|---|---|
| `findByName(name)` | ✅ | `.Where(x => x.Name == name)` | `.WhereEqualTo("Name", name)` | |
| `findByNameAndAge(name, age)` | ✅ | `.Where(x => x.Name == name && x.Age == age)` | `.WhereEqualTo("Name", name).WhereEqualTo("Age", age)` | Requiere índice compuesto |
| `findByNameOrAge(name, age)` | ⚠️ | `.Where(x => x.Name == name \|\| x.Age == age)` | `.Where(Filter.Or(Filter.EqualTo("Name", name), Filter.EqualTo("Age", age)))` | Máx 30 disjunciones |
| `findByNameIs(name)` / `Equals` | ✅ | `.Where(x => x.Name == name)` | `.WhereEqualTo("Name", name)` | |
| `findByAgeBetween(start, end)` | ✅ | `.Where(x => x.Age >= start && x.Age <= end)` | `.WhereGreaterThanOrEqualTo("Age", start).WhereLessThanOrEqualTo("Age", end)` | Range solo en 1 campo |
| `findByAgeLessThan(age)` | ✅ | `.Where(x => x.Age < age)` | `.WhereLessThan("Age", age)` | |
| `findByAgeLessThanEqual(age)` | ✅ | `.Where(x => x.Age <= age)` | `.WhereLessThanOrEqualTo("Age", age)` | |
| `findByAgeGreaterThan(age)` | ✅ | `.Where(x => x.Age > age)` | `.WhereGreaterThan("Age", age)` | |
| `findByAgeGreaterThanEqual(age)` | ✅ | `.Where(x => x.Age >= age)` | `.WhereGreaterThanOrEqualTo("Age", age)` | |
| `findByStartDateAfter(date)` | ✅ | `.Where(x => x.StartDate > date)` | `.WhereGreaterThan("StartDate", timestamp)` | Usar `Timestamp` de Firestore |
| `findByStartDateBefore(date)` | ✅ | `.Where(x => x.StartDate < date)` | `.WhereLessThan("StartDate", timestamp)` | Usar `Timestamp` de Firestore |
| `findByNameIsNull()` | ❌ | `.Where(x => x.Name == null)` | N/A | No soporta consultar campos inexistentes |
| `findByNameIsNotNull()` | ❌ | `.Where(x => x.Name != null)` | N/A | No hay operador nativo para esto |
| `findByNameLike(pattern)` | ❌ | `.Where(x => EF.Functions.Like(x.Name, pattern))` | N/A | No soporta patrones LIKE |
| `findByNameNotLike(pattern)` | ❌ | `.Where(x => !EF.Functions.Like(x.Name, pattern))` | N/A | No soporta patrones |
| `findByNameStartingWith(prefix)` | ⚠️ | `.Where(x => x.Name.StartsWith(prefix))` | `.WhereGreaterThanOrEqualTo("Name", prefix).WhereLessThan("Name", prefix + "\uf8ff")` | Workaround con range |
| `findByNameEndingWith(suffix)` | ❌ | `.Where(x => x.Name.EndsWith(suffix))` | N/A | Requiere filtrado en cliente |
| `findByNameContaining(text)` | ❌ | `.Where(x => x.Name.Contains(text))` | N/A | Requiere Algolia, Typesense u otro |
| `findByAgeOrderByNameDesc(age)` | ✅ | `.Where(x => x.Age == age).OrderByDescending(x => x.Name)` | `.WhereEqualTo("Age", age).OrderByDescending("Name")` | Requiere índice compuesto |
| `findByNameNot(name)` | ⚠️ | `.Where(x => x.Name != name)` | `.WhereNotEqualTo("Name", name)` | Solo 1 `!=` por query |
| `findByAgeIn(ages)` | ⚠️ | `.Where(x => ages.Contains(x.Age))` | `.WhereIn("Age", ages)` | Máximo 30 valores |
| `findByAgeNotIn(ages)` | ⚠️ | `.Where(x => !ages.Contains(x.Age))` | `.WhereNotIn("Age", ages)` | Máx 10 valores, solo 1 por query |
| `findByActiveTrue()` | ✅ | `.Where(x => x.Active == true)` | `.WhereEqualTo("Active", true)` | |
| `findByActiveFalse()` | ✅ | `.Where(x => x.Active == false)` | `.WhereEqualTo("Active", false)` | |
| `findByNameIgnoreCase(name)` | ❌ | `.Where(x => x.Name.ToLower() == name.ToLower())` | N/A | Workaround: guardar campo en lowercase |
| `countBy...` | ⚠️ | `.Count()` | `.Count().GetSnapshotAsync()` | Agregación con limitaciones |
| `deleteBy...` | ❌ | `.Where(...).ExecuteDelete()` | N/A | No hay delete por query, doc por doc |
| `existsBy...` | ❌ | `.Any(x => ...)` | `.Limit(1).GetSnapshotAsync()` + verificar | Workaround con limit |
| `findFirst...` / `findTopN...` | ✅ | `.Take(n)` | `.Limit(n)` | |
| `findDistinct...` | ❌ | `.Distinct()` | N/A | No soportado nativamente |
| `findByTagsContains(tag)` | ✅ | `.Where(x => x.Tags.Contains(tag))` | `.WhereArrayContains("Tags", tag)` | Solo 1 array-contains por query |
| `findByTagsIn(tags)` | ⚠️ | N/A | `.WhereArrayContainsAny("Tags", tags)` | Máx 30 valores, no combinar con array-contains |

## Leyenda

- ✅ Soportado completamente
- ⚠️ Soportado con limitaciones
- ❌ No soportado

## Ejemplos de Código C# (Firestore .NET SDK)

### Query Compuesta
```csharp
CollectionReference citiesRef = db.Collection("cities");

Query query = citiesRef
    .WhereEqualTo("State", "CA")
    .WhereGreaterThan("Population", 100000)
    .OrderByDescending("Population")
    .Limit(10);

QuerySnapshot snapshot = await query.GetSnapshotAsync();

foreach (DocumentSnapshot doc in snapshot.Documents)
{
    var city = doc.ConvertTo<City>();
}
```

### OR con Filter
```csharp
Query query = citiesRef.Where(
    Filter.Or(
        Filter.EqualTo("Capital", true),
        Filter.GreaterThanOrEqualTo("Population", 1000000)
    )
);
```

### AND + OR Combinados
```csharp
Query query = citiesRef.Where(
    Filter.And(
        Filter.EqualTo("State", "CA"),
        Filter.Or(
            Filter.EqualTo("Capital", true),
            Filter.GreaterThanOrEqualTo("Population", 1000000)
        )
    )
);
```

### StartsWith Workaround
```csharp
var prefix = "San";
Query query = citiesRef
    .WhereGreaterThanOrEqualTo("Name", prefix)
    .WhereLessThan("Name", prefix + "\uf8ff");
```

## Limitaciones Críticas de Firestore

| Limitación | Impacto |
|---|---|
| Solo 1 `!=` o `not-in` por query | No puedes combinar múltiples `Not` |
| Range (`<`, `>`, `>=`, `<=`) solo en 1 campo | `Between` en un campo está OK, pero no mezclar con otros |
| `In` / `NotIn` máx 30 / 10 valores | Validar tamaño de colecciones |
| Máx 30 disjunciones OR | Limitar combinaciones OR |
| `WhereArrayContains` máx 1 por query | No combinar múltiples array-contains |
| Case-sensitive siempre | Guardar campo adicional en lowercase para búsquedas |
| No soporta JOIN | Desnormalizar datos o múltiples queries |
| No soporta GROUP BY | Usar agregaciones limitadas o procesar en cliente |