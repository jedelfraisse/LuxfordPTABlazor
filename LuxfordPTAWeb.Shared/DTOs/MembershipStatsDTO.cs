namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>Live stats for a school year — powers the admin dashboard and the public stats panel.</summary>
public class MembershipStatsDTO
{
    public int SchoolYearId { get; set; }
    public string SchoolYearName { get; set; } = string.Empty;

    public int CurrentCount { get; set; }
    public int StaffCount { get; set; }

    /// <summary>Percent CHANGE from the prior school year's total (negative = decline). Null if no prior year data exists.</summary>
    public decimal? GrowthPercent { get; set; }
    public string? ComparisonYearName { get; set; }
}
