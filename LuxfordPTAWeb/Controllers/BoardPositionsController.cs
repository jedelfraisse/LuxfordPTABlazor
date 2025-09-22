using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardPositionsController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	public BoardPositionsController(ApplicationDbContext db) => _db = db;

	[HttpGet("by-schoolyear/{schoolYearId}")]
	public async Task<IEnumerable<BoardPosition>> GetBySchoolYear(int schoolYearId)
	{
		return await _db.BoardPositions
			.Include(bp => bp.BoardPositionTitle)
			.Include(bp => bp.AssignedUser)
			.Where(bp => bp.SchoolYearId == schoolYearId)
			.ToListAsync();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<BoardPosition>> Get(int id)
	{
		var bp = await _db.BoardPositions
			.Include(bp => bp.BoardPositionTitle)
			.Include(bp => bp.AssignedUser)
			.FirstOrDefaultAsync(bp => bp.Id == id);
		if (bp == null) return NotFound();
		return bp;
	}

	[HttpGet("titles")]
	public async Task<IEnumerable<BoardPositionTitle>> GetTitles()
	{
		return await _db.BoardPositionTitles
			.OrderBy(t => t.SortOrder)
			.ToListAsync();
	}

	[HttpGet("required-titles")]
	public async Task<IEnumerable<BoardPositionTitle>> GetRequiredTitles()
	{
		return await _db.BoardPositionTitles
			.Where(t => t.IsRequired)
			.OrderBy(t => t.SortOrder)
			.ToListAsync();
	}

	[HttpGet("all-by-schoolyear/{schoolYearId}")]
	public async Task<IEnumerable<BoardPosition>> GetAllBySchoolYear(int schoolYearId)
	{
		var titles = await _db.BoardPositionTitles
			.OrderBy(t => t.SortOrder)
			.ToListAsync();

		var positions = await _db.BoardPositions
			.Include(bp => bp.BoardPositionTitle)
			.Include(bp => bp.AssignedUser)
			.Where(bp => bp.SchoolYearId == schoolYearId)
			.ToListAsync();

		var result = new List<BoardPosition>();

		foreach (var title in titles)
		{
			var pos = positions.FirstOrDefault(bp => bp.BoardPositionTitleId == title.Id);
			if (pos != null)
			{
				result.Add(pos);
			}
			else
			{
				// Create a placeholder for unfilled positions
				result.Add(new BoardPosition
				{
					BoardPositionTitleId = title.Id,
					BoardPositionTitle = title,
					SchoolYearId = schoolYearId,
					AssignedUser = null,
					UserId = null,
					IsVotingMember = false
				});
			}
		}

		return result;
	}

	[HttpGet("all")]
	[Authorize(Roles = "Admin,BoardMember")]
	public async Task<IEnumerable<BoardPosition>> GetAll()
	{
		return await _db.BoardPositions
			.Include(bp => bp.BoardPositionTitle)
			.Include(bp => bp.AssignedUser)
			.Include(bp => bp.SchoolYear)
			.OrderByDescending(bp => bp.SchoolYear.StartDate)
			.ThenBy(bp => bp.BoardPositionTitle.SortOrder)
			.ToListAsync();
	}

	// --- SECURE ENDPOINTS BELOW ---

	[HttpPost]
	[Authorize(Roles = "Admin,BoardMember")]
	public async Task<ActionResult<BoardPosition>> Post([FromBody] BoardPosition model)
	{
		_db.BoardPositions.Add(model);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
	}

	[HttpPut("{id}")]
	[Authorize(Roles = "Admin,BoardMember")]
	public async Task<IActionResult> Put(int id, [FromBody] BoardPosition model)
	{
		var bp = await _db.BoardPositions.FindAsync(id);
		if (bp == null) return NotFound();

		bp.BoardPositionTitleId = model.BoardPositionTitleId;
		bp.IsVotingMember = model.IsVotingMember;
		bp.SchoolYearId = model.SchoolYearId;
		bp.UserId = model.UserId;

		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin,BoardMember")]
	public async Task<IActionResult> Delete(int id)
	{
		var bp = await _db.BoardPositions.FindAsync(id);
		if (bp == null) return NotFound();
		_db.BoardPositions.Remove(bp);
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPost("titles")]
	[Authorize(Roles = "Admin,BoardMember")]
	public async Task<ActionResult<BoardPositionTitle>> PostTitle([FromBody] BoardPositionTitle model)
	{
		_db.BoardPositionTitles.Add(model);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetTitles), new { id = model.Id }, model);
	}

	[HttpPut("titles/{id}")]
	[Authorize(Roles = "Admin,BoardMember")]
	public async Task<IActionResult> PutTitle(int id, [FromBody] BoardPositionTitle model)
	{
		var title = await _db.BoardPositionTitles.FindAsync(id);
		if (title == null) return NotFound();

		title.Title = model.Title;
		title.RoleType = model.RoleType;
		title.SortOrder = model.SortOrder;
		title.IsRequired = model.IsRequired;
		title.Description = model.Description;

		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("titles/{id}")]
	[Authorize(Roles = "Admin,BoardMember")]
	public async Task<IActionResult> DeleteTitle(int id)
	{
		var title = await _db.BoardPositionTitles.FindAsync(id);
		if (title == null) return NotFound();
		_db.BoardPositionTitles.Remove(title);
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPut("assign-user-override")]
	[Authorize(Roles = "Admin,BoardMember")]
	public async Task<IActionResult> AssignUserOverride([FromBody] AssignUserOverrideDto dto)
	{
		var bp = await _db.BoardPositions
			.FirstOrDefaultAsync(bp => bp.BoardPositionTitleId == dto.BoardPositionTitleId && bp.SchoolYearId == dto.SchoolYearId);

		if (dto.UserId == null)
		{
			// If position exists, delete it (unassign)
			if (bp != null)
			{
				_db.BoardPositions.Remove(bp);
				await _db.SaveChangesAsync();
			}
			return NoContent();
		}

		if (bp == null)
		{
			bp = new BoardPosition
			{
				BoardPositionTitleId = dto.BoardPositionTitleId,
				SchoolYearId = dto.SchoolYearId,
				UserId = dto.UserId,
				IsVotingMember = true
			};
			_db.BoardPositions.Add(bp);
		}
		else
		{
			bp.UserId = dto.UserId;
		}

		await _db.SaveChangesAsync();
		return NoContent();
	}

	public class AssignUserOverrideDto
	{
		public int BoardPositionTitleId { get; set; }
		public string? UserId { get; set; }
		public int SchoolYearId { get; set; }
	}
}