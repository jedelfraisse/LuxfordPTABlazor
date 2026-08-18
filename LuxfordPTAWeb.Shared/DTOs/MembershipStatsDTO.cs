namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>Live stats for a school year — powers the admin dashboard and the public stats panel.</summary>
public class MembershipStatsDTO
{
    public int SchoolYearId { get; set; }
    public string SchoolYearName { get; set; } = string.Empty;

    public int CurrentCount { get; set; }
    public int StaffCount { get; set; }

    /// <summary>Percent of the prior school year's total this year has reached. Null if no prior year data exists.</summary>
    public decimal? GrowthPercent { get; set; }
    public string? ComparisonYearName { get; set; }

    public int HistoricalHigh { get; set; }
    public string? HistoricalHighYearName { get; set; }
}
