namespace Plans.Features.Plans.Api.PlanAggregate;

public class PlanAggregateDescription : IAggregateDescription
{
    public string Id => "plan";
    public string DisplayName => "Planes";
    public string? Icon => "credit-card";
}
