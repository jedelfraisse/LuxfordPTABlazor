using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>A milestone plus its computed current value, ready to render as a progress bar.</summary>
public class MilestoneProgressDTO
{
    public Guid Id { get; set; }
    public int SchoolYearId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MilestoneType MilestoneType { get; set; }
    public int? TargetValue { get; set; }
    public int? ComparisonYearId { get; set; }
    public string? ComparisonYearName { get; set; }
    public bool IsVisible { get; set; }
    public int SortOrder { get; set; }

    /// <summary>Current progress toward TargetValue. Null when the milestone can't be evaluated (e.g. no target set).</summary>
    public decimal? CurrentValue { get; set; }

    /// <summary>0-100+, null when TargetValue is unset.</summary>
    public decimal? PercentComplete { get; set; }

    public bool IsAchieved { get; set; }

    /// <summary>Display suffix for CurrentValue/TargetValue — "%" for growth milestones, "" otherwise.</summary>
    public string ValueSuffix { get; set; } = string.Empty;
}
