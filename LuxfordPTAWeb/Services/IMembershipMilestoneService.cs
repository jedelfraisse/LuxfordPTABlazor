using LuxfordPTAWeb.Shared.DTOs;

namespace LuxfordPTAWeb.Services;

/// <summary>Computes live membership stats and milestone progress, all scoped to a school year.</summary>
public interface IMembershipMilestoneService
{
    Task<MembershipStatsDTO> GetStatsAsync(int schoolYearId);

    /// <param name="includeHidden">Admin views pass true to see hidden milestones too; the public page passes false.</param>
    Task<List<MilestoneProgressDTO>> EvaluateMilestonesAsync(int schoolYearId, bool includeHidden);
}
