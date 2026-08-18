using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.Models;

/// <summary>
/// A membership-drive goal tied to a specific school year. TargetValue is admin-entered
/// (e.g. "100" staff, "120" percent growth) since some targets — like total staff headcount —
/// aren't derivable from membership data alone.
/// </summary>
public class MembershipMilestone
{
    public Guid Id { get; set; }

    public int SchoolYearId { get; set; }
    public SchoolYear SchoolYear { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MilestoneType MilestoneType { get; set; }

    public int? TargetValue { get; set; } // e.g., 100 staff, 120 (%) growth, 200 members

    /// <summary>Comparison school year for PercentGrowth (baseline) or HistoricalSurpass (earliest year considered).</summary>
    public int? ComparisonYearId { get; set; }
    public SchoolYear? ComparisonYear { get; set; }

    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
