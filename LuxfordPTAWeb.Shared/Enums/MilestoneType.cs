namespace LuxfordPTAWeb.Shared.Enums;

public enum MilestoneType
{
    Staff100Percent = 1,   // Reach a known total-staff headcount (TargetValue) with staff members
    StaffVolume = 2,       // Reach a specific number of staff members (TargetValue)
    PercentGrowth = 3,     // Reach a percentage of ComparisonYear's total membership (TargetValue)
    TotalMembership = 4,   // Reach a specific total membership count (TargetValue)
    HistoricalSurpass = 5, // Surpass the highest membership count since ComparisonYear
    Custom = 6             // Freeform milestone with an admin-defined target
}
