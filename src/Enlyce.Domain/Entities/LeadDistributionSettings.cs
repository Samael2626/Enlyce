namespace Enlyce.Domain.Entities;

public sealed class LeadDistributionSettings
{
    public int Id { get; set; }
    public string Rule { get; set; } = LeadDistributionRule.LeastOpenLeads.ToString();
    public long RoundRobinCursor { get; set; }
}
