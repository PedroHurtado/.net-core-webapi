namespace Customer.UnitTests.Helpers;

public record TestableDepositPolicy : DepositPolicy
{
    public TestableDepositPolicy(
        DepositType depositType,
        decimal amount,
        decimal? percentage = null,
        decimal? minimumBillForDeposit = null,
        int? minimumGuestsForDeposit = null)
        : base(depositType, amount, percentage, minimumBillForDeposit, minimumGuestsForDeposit)
    {
    }
}
