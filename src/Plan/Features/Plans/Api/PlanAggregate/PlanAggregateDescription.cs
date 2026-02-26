namespace Plans.Features.Plans.Api.PlanAggregate;

public class PlanAggregateDescription : IAggregateDescription
{
    public string Id => "plan";
    public string DisplayName => "Plans";
    public string? Icon => "credit-card";
    public string ReadDescription => "View plans";
    public string WriteDescription => "Manage plans";
}
