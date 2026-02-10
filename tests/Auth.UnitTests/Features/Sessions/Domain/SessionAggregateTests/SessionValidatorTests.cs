namespace Auth.UnitTests.Features.Sessions.Domain.SessionAggregateTests;

public class SessionValidatorTests
{
    private readonly SessionValidator _validator = new();

    [Fact]
    public void Validate_WithValidSessionWithoutTenant_ReturnsSuccess()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var result = _validator.Validate(session);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidSessionWithTenant_ReturnsSuccess()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRoleId(Guid.NewGuid())
            .WithGroups(["menu:read", "menu:write"])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var result = _validator.Validate(session);

        result.IsValid.Should().BeTrue();
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.Empty)
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var result = _validator.Validate(session);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SessionValidationMessages.IdRequired);
    }

    #endregion

    #region UserId Validation

    [Fact]
    public void UserId_WhenEmpty_ReturnsError()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.Empty)
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var result = _validator.Validate(session);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SessionValidationMessages.UserIdRequired);
    }

    #endregion

    #region RoleId Validation

    [Fact]
    public void RoleId_WhenNullAndTenantIdSet_ReturnsError()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var result = _validator.Validate(session);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SessionValidationMessages.RoleIdRequiredWhenTenantSet);
    }

    [Fact]
    public void RoleId_WhenSetAndTenantIdNull_ReturnsError()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(Guid.NewGuid())
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var result = _validator.Validate(session);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SessionValidationMessages.RoleIdMustBeNullWhenNoTenant);
    }

    #endregion

    #region ExpiresAt Validation

    [Fact]
    public void ExpiresAt_WhenBeforeCreatedAt_ReturnsError()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddMinutes(-1));

        var result = _validator.Validate(session);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SessionValidationMessages.ExpiresAtMustBeAfterCreatedAt);
    }

    [Fact]
    public void ExpiresAt_WhenEqualToCreatedAt_ReturnsError()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now.AddMinutes(-1))
            .WithExpiresAt(now);

        var result = _validator.Validate(session);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SessionValidationMessages.ExpiresAtMustBeAfterCreatedAt);
    }

    [Fact]
    public void ExpiresAt_WhenBeforeLastActivityAt_ReturnsError()
    {
        var now = DateTimeOffset.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now.AddDays(-2))
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddMinutes(-1));

        var result = _validator.Validate(session);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SessionValidationMessages.ExpiresAtMustBeAfterLastActivity);
    }

    #endregion
}
