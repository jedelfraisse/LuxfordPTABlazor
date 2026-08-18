using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>Create/edit payload for a membership milestone.</summary>
public class MembershipMilestoneDTO
{
    public Guid Id { get; set; }
    public int SchoolYearId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MilestoneType MilestoneType { get; set; } = MilestoneType.TotalMembership;
    public int? TargetValue { get; set; }
    public int? ComparisonYearId { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }
}
