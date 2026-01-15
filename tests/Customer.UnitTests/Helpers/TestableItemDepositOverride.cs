namespace Customer.UnitTests.Helpers;

public record TestableItemDepositOverride : ItemDepositOverride
{
    public TestableItemDepositOverride(decimal depositAmount, int? minimumQuantityForDeposit = null)
        : base(depositAmount, minimumQuantityForDeposit) { }
}
