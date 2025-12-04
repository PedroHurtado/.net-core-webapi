using FluentAssertions;
using Fudie.Domain;

namespace Fudie.UnitTests.Domain;

public class EntityTests
{
    #region Test Entity Class

    // Clase de entidad concreta para testing
    private class TestEntity : Entity
    {
        public TestEntity(Guid id) : base(id) { }

        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldSetId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestEntity(id);

        // Assert
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_ShouldAcceptEmptyGuid()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var entity = new TestEntity(id);

        // Assert
        entity.Id.Should().Be(Guid.Empty);
    }

    #endregion

    #region Equals Tests

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act
        var result = entity.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNonEntityObject_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());
        var nonEntity = new object();

        // Act
        var result = entity.Equals(nonEntity);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act
        var result = entity.Equals(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEntityTypes_ButSameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new AnotherTestEntity(id);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeTrue("entities with the same Id should be equal regardless of type");
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithSameId_ShouldReturnSameHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentId_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act
        var hashCode1 = entity.GetHashCode();
        var hashCode2 = entity.GetHashCode();
        var hashCode3 = entity.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
        hashCode2.Should().Be(hashCode3);
    }

    [Fact]
    public void GetHashCode_ShouldMatchIdHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        // Act
        var entityHashCode = entity.GetHashCode();
        var idHashCode = id.GetHashCode();

        // Assert
        entityHashCode.Should().Be(idHashCode);
    }

    #endregion

    #region Equality Operator (==) Tests

    [Fact]
    public void EqualityOperator_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithBothNull_ShouldReturnTrue()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithLeftNull_ShouldReturnFalse()
    {
        // Arrange
        TestEntity? entity1 = null;
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithRightNull_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        TestEntity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act
#pragma warning disable CS1718 // Comparison made to same variable
        var result = entity == entity;
#pragma warning restore CS1718 // Comparison made to same variable

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentEntityTypes_ButSameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        Entity entity1 = new TestEntity(id);
        Entity entity2 = new AnotherTestEntity(id);

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Inequality Operator (!=) Tests

    [Fact]
    public void InequalityOperator_WithSameId_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentId_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithBothNull_ShouldReturnFalse()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithLeftNull_ShouldReturnTrue()
    {
        // Arrange
        TestEntity? entity1 = null;
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithRightNull_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        TestEntity? entity2 = null;

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSameReference_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act
#pragma warning disable CS1718 // Comparison made to same variable
        var result = entity != entity;
#pragma warning restore CS1718 // Comparison made to same variable

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentEntityTypes_ButSameId_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        Entity entity1 = new TestEntity(id);
        Entity entity2 = new AnotherTestEntity(id);

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Operator and Equals Consistency Tests

    [Fact]
    public void EqualityOperator_ShouldBeConsistentWithEquals()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var operatorResult = entity1 == entity2;
        var equalsResult = entity1.Equals(entity2);

        // Assert
        operatorResult.Should().Be(equalsResult);
    }

    [Fact]
    public void InequalityOperator_ShouldBeOppositeOfEqualityOperator()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act
        var equalityResult = entity1 == entity2;
        var inequalityResult = entity1 != entity2;

        // Assert
        inequalityResult.Should().Be(!equalityResult);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Operators_ShouldBeConsistentWithEquals_ForMultipleCases(bool sameId)
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = sameId ? id1 : Guid.NewGuid();
        var entity1 = new TestEntity(id1);
        var entity2 = new TestEntity(id2);

        // Act
        var equalsResult = entity1.Equals(entity2);
        var operatorEqualResult = entity1 == entity2;
        var operatorNotEqualResult = entity1 != entity2;

        // Assert
        operatorEqualResult.Should().Be(equalsResult);
        operatorNotEqualResult.Should().Be(!equalsResult);
    }

    #endregion

    #region Collections and Dictionary Tests

    [Fact]
    public void Entity_ShouldWorkInHashSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);
        var entity3 = new TestEntity(Guid.NewGuid());

        // Act
        var hashSet = new HashSet<TestEntity> { entity1, entity2, entity3 };

        // Assert
        hashSet.Should().HaveCount(2, "entity1 and entity2 have the same Id");
    }

    [Fact]
    public void Entity_ShouldWorkAsDictionaryKey()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);
        var dictionary = new Dictionary<TestEntity, string>();

        // Act
        dictionary[entity1] = "First";
        dictionary[entity2] = "Second";

        // Assert
        dictionary.Should().HaveCount(1, "entity1 and entity2 have the same Id");
        dictionary[entity1].Should().Be("Second");
    }

    [Fact]
    public void Entity_InList_ShouldFindByEquals()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id) { Name = "First" };
        var entity2 = new TestEntity(id) { Name = "Second" };
        var list = new List<TestEntity> { entity1 };

        // Act
        var contains = list.Contains(entity2);

        // Assert
        contains.Should().BeTrue("entity2 has the same Id as entity1");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Entity_WithEmptyGuid_ShouldStillSupportEquality()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.Empty);
        var entity2 = new TestEntity(Guid.Empty);

        // Act & Assert
        entity1.Should().Be(entity2);
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();
    }

    [Fact]
    public void Entity_MultipleInstancesWithSameId_ShouldAllBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entities = Enumerable.Range(0, 10)
            .Select(_ => new TestEntity(id))
            .ToList();

        // Act & Assert
        for (int i = 0; i < entities.Count - 1; i++)
        {
            for (int j = i + 1; j < entities.Count; j++)
            {
                entities[i].Should().Be(entities[j]);
                (entities[i] == entities[j]).Should().BeTrue();
            }
        }
    }

    #endregion

    #region Additional Test Entity Class

    private class AnotherTestEntity : Entity
    {
        public AnotherTestEntity(Guid id) : base(id) { }

        public int Value { get; set; }
    }

    #endregion
}
