using LuxfordPTAWeb.Authorization;
using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Services;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Controllers;

[ApiController]
[Route("api/membership/milestones")]
public class MembershipMilestonesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMembershipMilestoneService _milestoneService;

    public MembershipMilestonesController(ApplicationDbContext db, IMembershipMilestoneService milestoneService)
    {
        _db = db;
        _milestoneService = milestoneService;
    }

    /// <summary>
    /// Milestone progress for a school year. Public/anonymous and roleless-authenticated callers
    /// (the /membership-drive page) only ever see visible milestones; includeHidden is honored for
    /// any authenticated PTA role (Admin/BoardMember/Volunteer).
    /// </summary>
    [HttpGet("{schoolYearId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<MilestoneProgressDTO>>> GetForYear(int schoolYearId, [FromQuery] bool includeHidden = false)
    {
        var exists = await _db.SchoolYears.AnyAsync(sy => sy.Id == schoolYearId);
        if (!exists) return NotFound("School year not found.");

        return await _milestoneService.EvaluateMilestonesAsync(schoolYearId, includeHidden && User.IsPtaMember());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<MembershipMilestone>> Create([FromBody] MembershipMilestoneDTO dto)
    {
        var schoolYearExists = await _db.SchoolYears.AnyAsync(sy => sy.Id == dto.SchoolYearId);
        if (!schoolYearExists) return BadRequest("Selected school year does not exist.");

        var milestone = new MembershipMilestone
        {
            Id = Guid.NewGuid(),
            SchoolYearId = dto.SchoolYearId,
            Title = dto.Title,
            Description = dto.Description,
            MilestoneType = dto.MilestoneType,
            TargetValue = dto.TargetValue,
            ComparisonYearId = dto.ComparisonYearId,
            IsVisible = dto.IsVisible,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _db.MembershipMilestones.Add(milestone);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetForYear), new { schoolYearId = milestone.SchoolYearId }, milestone);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MembershipMilestoneDTO dto)
    {
        var milestone = await _db.MembershipMilestones.FindAsync(id);
        if (milestone == null) return NotFound();

        milestone.Title = dto.Title;
        milestone.Description = dto.Description;
        milestone.MilestoneType = dto.MilestoneType;
        milestone.TargetValue = dto.TargetValue;
        milestone.ComparisonYearId = dto.ComparisonYearId;
        milestone.IsVisible = dto.IsVisible;
        milestone.SortOrder = dto.SortOrder;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var milestone = await _db.MembershipMilestones.FindAsync(id);
        if (milestone == null) return NotFound();

        _db.MembershipMilestones.Remove(milestone);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("reorder")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Reorder([FromBody] MilestoneReorderDTO dto)
    {
        var ids = dto.Items.Select(i => i.Id).ToList();
        var milestones = await _db.MembershipMilestones.Where(mm => ids.Contains(mm.Id)).ToListAsync();

        foreach (var item in dto.Items)
        {
            var milestone = milestones.FirstOrDefault(mm => mm.Id == item.Id);
            if (milestone != null) milestone.SortOrder = item.SortOrder;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
