using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Enums;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Services;

public class MembershipMilestoneService : IMembershipMilestoneService
{
    private readonly ApplicationDbContext _db;

    // "Historical surpass since 2019-2020" from the original spec, used only when a milestone
    // doesn't specify its own ComparisonYear anchor.
    private static readonly DateTime DefaultHistoricalAnchor = new(2019, 7, 1);

    public MembershipMilestoneService(ApplicationDbContext db)
    {
        _db = db;
    }

    private readonly record struct YearCounts(int Total, int Staff);

    private async Task<Dictionary<int, YearCounts>> BuildYearCountsAsync()
    {
        return await _db.MembershipRecords
            .GroupBy(mr => mr.SchoolYearId)
            .Select(g => new
            {
                SchoolYearId = g.Key,
                Total = g.Count(),
                Staff = g.Count(mr => mr.IsStaff)
            })
            .ToDictionaryAsync(g => g.SchoolYearId, g => new YearCounts(g.Total, g.Staff));
    }

    public async Task<MembershipStatsDTO> GetStatsAsync(int schoolYearId)
    {
        var schoolYear = await _db.SchoolYears.FindAsync(schoolYearId)
            ?? throw new InvalidOperationException($"School year {schoolYearId} not found.");

        var counts = await BuildYearCountsAsync();
        var allYears = await _db.SchoolYears.OrderBy(sy => sy.StartDate).ToListAsync();

        var current = counts.GetValueOrDefault(schoolYearId);

        var priorYear = allYears
            .Where(sy => sy.StartDate < schoolYear.StartDate)
            .OrderByDescending(sy => sy.StartDate)
            .FirstOrDefault();

        decimal? growthPercent = null;
        if (priorYear != null && counts.TryGetValue(priorYear.Id, out var priorCounts) && priorCounts.Total > 0)
        {
            // Percent CHANGE from last year — e.g. going from 116 members to 61 is a -47.4% change,
            // not "current/prior*100" (52.6%), which is a ratio and would misreport a decline as growth.
            growthPercent = Math.Round((current.Total - priorCounts.Total) / (decimal)priorCounts.Total * 100, 1);
        }

        var (historicalHigh, historicalHighYear) = GetHistoricalHigh(allYears, counts, DefaultHistoricalAnchor, excludeYearId: null);

        return new MembershipStatsDTO
        {
            SchoolYearId = schoolYear.Id,
            SchoolYearName = schoolYear.Name,
            CurrentCount = current.Total,
            StaffCount = current.Staff,
            GrowthPercent = growthPercent,
            ComparisonYearName = priorYear?.Name,
            HistoricalHigh = historicalHigh,
            HistoricalHighYearName = historicalHighYear?.Name
        };
    }

    private static (int High, SchoolYear? Year) GetHistoricalHigh(
        List<SchoolYear> allYears, Dictionary<int, YearCounts> counts, DateTime anchor, int? excludeYearId)
    {
        SchoolYear? bestYear = null;
        int best = 0;
        foreach (var year in allYears.Where(y => y.StartDate >= anchor && y.Id != excludeYearId))
        {
            var total = counts.GetValueOrDefault(year.Id).Total;
            if (total >= best)
            {
                best = total;
                bestYear = year;
            }
        }
        return (best, bestYear);
    }

    public async Task<List<MilestoneProgressDTO>> EvaluateMilestonesAsync(int schoolYearId, bool includeHidden)
    {
        var query = _db.MembershipMilestones
            .Include(mm => mm.ComparisonYear)
            .Where(mm => mm.SchoolYearId == schoolYearId);

        if (!includeHidden)
            query = query.Where(mm => mm.IsVisible);

        var milestones = await query.OrderBy(mm => mm.SortOrder).ToListAsync();
        if (milestones.Count == 0) return [];

        var counts = await BuildYearCountsAsync();
        var allYears = await _db.SchoolYears.OrderBy(sy => sy.StartDate).ToListAsync();
        var current = counts.GetValueOrDefault(schoolYearId);

        var results = new List<MilestoneProgressDTO>();
        foreach (var m in milestones)
        {
            var dto = new MilestoneProgressDTO
            {
                Id = m.Id,
                SchoolYearId = m.SchoolYearId,
                Title = m.Title,
                Description = m.Description,
                MilestoneType = m.MilestoneType,
                TargetValue = m.TargetValue,
                ComparisonYearId = m.ComparisonYearId,
                ComparisonYearName = m.ComparisonYear?.Name,
                IsVisible = m.IsVisible,
                SortOrder = m.SortOrder
            };

            switch (m.MilestoneType)
            {
                case MilestoneType.Staff100Percent:
                case MilestoneType.StaffVolume:
                    dto.CurrentValue = current.Staff;
                    ApplyTargetProgress(dto, m.TargetValue);
                    break;

                case MilestoneType.TotalMembership:
                    dto.CurrentValue = current.Total;
                    ApplyTargetProgress(dto, m.TargetValue);
                    break;

                case MilestoneType.PercentGrowth:
                    {
                        dto.ValueSuffix = "%";
                        var comparisonTotal = m.ComparisonYearId.HasValue
                            ? counts.GetValueOrDefault(m.ComparisonYearId.Value).Total
                            : 0;
                        if (comparisonTotal > 0)
                        {
                            dto.CurrentValue = Math.Round(current.Total / (decimal)comparisonTotal * 100, 1);
                            ApplyTargetProgress(dto, m.TargetValue);
                        }
                        // else: comparison year has no data — leave CurrentValue/PercentComplete null (undefined, not zero)
                        break;
                    }

                case MilestoneType.HistoricalSurpass:
                    {
                        var anchor = m.ComparisonYear?.StartDate ?? DefaultHistoricalAnchor;
                        var (historicalHigh, _) = GetHistoricalHigh(allYears, counts, anchor, excludeYearId: schoolYearId);
                        dto.CurrentValue = current.Total;
                        // Admin-entered TargetValue overrides the dynamic historical high when set.
                        dto.TargetValue = m.TargetValue ?? historicalHigh;
                        ApplyTargetProgress(dto, dto.TargetValue);
                        break;
                    }

                case MilestoneType.Custom:
                default:
                    dto.CurrentValue = current.Total;
                    ApplyTargetProgress(dto, m.TargetValue);
                    break;
            }

            results.Add(dto);
        }

        return results;
    }

    private static void ApplyTargetProgress(MilestoneProgressDTO dto, int? target)
    {
        if (!target.HasValue || target.Value <= 0 || !dto.CurrentValue.HasValue)
            return;

        dto.PercentComplete = Math.Round(dto.CurrentValue.Value / target.Value * 100, 1);
        dto.IsAchieved = dto.CurrentValue.Value >= target.Value;
    }
}
