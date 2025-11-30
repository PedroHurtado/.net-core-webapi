# Persistence Specification: [Entity/Aggregate Name]

## 1. Entity Framework Configuration

### Table Mapping
- **Table Name**: `[TableName]`
- **Key**: `[PrimaryKey]` (usually Id)

### Property Configuration
| Property | DB Column | Type | Required | MaxLength | Notes |
|----------|-----------|------|----------|-----------|-------|
| Name     | Name      | nvarchar(100) | Yes | 100 | |
| ...      | ...       | ...  | ...      | ...       | |

### Value Objects (Complex Types)
*If the entity uses Value Objects, describe how they are mapped (Owned Types, Flattened, etc.)*
- **[ValueObject]**: Mapped as Owned Entity.

## 2. Relationships
*Define how this entity relates to others.*

- **Target Entity**: `[EntityName]`
- **Type**: One-to-Many / Many-to-Many
- **Foreign Key**: `[FK_Property]`
- **Navigation Property**: `[PropName]`
- **Delete Behavior**: Restrict / Cascade

## 3. DbContext Registration
- Ensure `DbSet<[Entity]>` is added to the main DbContext.
