using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxfordPTAWeb.Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class SchoolYearsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    
    public SchoolYearsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<SchoolYear>> Get()
    {
        // Only return years that are not hidden, or all if user is board/admin
        var isBoard = User.IsInRole("Admin") || User.IsInRole("BoardMember");
        return await _db.SchoolYears
            .Where(sy => isBoard || sy.IsVisibleToPublic)
            .OrderByDescending(sy => sy.StartDate)
            .ToListAsync();
    }

    [HttpGet("current")]
    public async Task<ActionResult<SchoolYear>> GetCurrent()
    {
        var currentDate = DateTime.Now;
        var currentYear = await _db.SchoolYears
            .FirstOrDefaultAsync(sy => currentDate >= sy.StartDate && currentDate <= sy.EndDate);
        if (currentYear == null)
        {
            return NotFound("No current school year found");
        }
        return currentYear;
    }

    [HttpGet("last")]
    public async Task<ActionResult<SchoolYear>> GetLastYear()
    {
        var currentDate = DateTime.Now;
        var lastYear = await _db.SchoolYears
            .Where(sy => sy.EndDate < currentDate)
            .OrderByDescending(sy => sy.StartDate)
            .FirstOrDefaultAsync();
        if (lastYear == null)
        {
            return NotFound("No previous school year found");
        }
        return lastYear;
    }

    [HttpGet("next")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<SchoolYear>> GetNextYear()
    {
        var currentDate = DateTime.Now;
        var nextYear = await _db.SchoolYears
            .Where(sy => sy.StartDate > currentDate)
            .OrderBy(sy => sy.StartDate)
            .FirstOrDefaultAsync();
        if (nextYear == null)
        {
            return NotFound("No future school year found");
        }
        return nextYear;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SchoolYear>> Get(int id)
    {
        var schoolYear = await _db.SchoolYears.FindAsync(id);
        if (schoolYear == null)
        {
            return NotFound();
        }
        // Only allow non-board to view if visible
        var isBoard = User.IsInRole("Admin") || User.IsInRole("BoardMember");
        if (!isBoard && !schoolYear.IsVisibleToPublic)
        {
            return Forbid();
        }
        return schoolYear;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<SchoolYear>> Post([FromBody] CreateSchoolYearDto dto)
    {
        // Validate date range
        if (dto.StartDate >= dto.EndDate)
        {
            return BadRequest("Start date must be before end date");
        }

        // Check for overlapping school years
        var overlapping = await _db.SchoolYears
            .AnyAsync(sy => (dto.StartDate >= sy.StartDate && dto.StartDate <= sy.EndDate) ||
                           (dto.EndDate >= sy.StartDate && dto.EndDate <= sy.EndDate) ||
                           (dto.StartDate <= sy.StartDate && dto.EndDate >= sy.EndDate));
        if (overlapping)
        {
            return BadRequest("Date range overlaps with an existing school year");
        }

        // Only allow creation of last year or next year
        var today = DateTime.Today;
        int currentYear = today.Month >= 7 ? today.Year : today.Year - 1;
        var lastYearStart = new DateTime(currentYear - 1, 7, 1);
        var lastYearEnd = new DateTime(currentYear, 6, 30);
        var nextYearStart = new DateTime(currentYear + 1, 7, 1);
        var nextYearEnd = new DateTime(currentYear + 2, 6, 30);

        bool isLastYear = dto.StartDate == lastYearStart && dto.EndDate == lastYearEnd;
        bool isNextYear = dto.StartDate == nextYearStart && dto.EndDate == nextYearEnd;

        if (!isLastYear && !isNextYear)
        {
            return BadRequest("Can only create last year or next year.");
        }

        var schoolYear = new SchoolYear
        {
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = isNextYear ? SchoolYearStatus.FutureYear : SchoolYearStatus.PrevYear,
            IsVisibleToPublic = !isNextYear // Hide future year from public by default
        };

        _db.SchoolYears.Add(schoolYear);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = schoolYear.Id }, schoolYear);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] CreateSchoolYearDto dto)
    {
        var schoolYear = await _db.SchoolYears.FindAsync(id);
        if (schoolYear == null)
        {
            return NotFound();
        }

        // Validate date range
        if (dto.StartDate >= dto.EndDate)
        {
            return BadRequest("Start date must be before end date");
        }

        // Check for overlapping school years (excluding current one)
        var overlapping = await _db.SchoolYears
            .Where(sy => sy.Id != id)
            .AnyAsync(sy => (dto.StartDate >= sy.StartDate && dto.StartDate <= sy.EndDate) ||
                           (dto.EndDate >= sy.StartDate && dto.EndDate <= sy.EndDate) ||
                           (dto.StartDate <= sy.StartDate && dto.EndDate >= sy.EndDate));
        if (overlapping)
        {
            return BadRequest("Date range overlaps with an existing school year");
        }

        schoolYear.Name = dto.Name;
        schoolYear.StartDate = dto.StartDate;
        schoolYear.EndDate = dto.EndDate;
        // Optionally allow updating status/visibility
        // schoolYear.Status = dto.Status;
        // schoolYear.IsVisibleToPublic = dto.IsVisibleToPublic;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        var schoolYear = await _db.SchoolYears.FindAsync(id);
        if (schoolYear == null)
        {
            return NotFound();
        }

        // Check if there are any events in this school year
        var hasEvents = await _db.Events.AnyAsync(e => e.SchoolYearId == id);
        if (hasEvents)
        {
            return BadRequest("Cannot delete school year that contains events.");
        }

        _db.SchoolYears.Remove(schoolYear);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/transition")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> TransitionToNewYear(int id, [FromBody] TransitionToNewYearDto dto)
    {
        var currentYear = await _db.SchoolYears.FindAsync(id);
        if (currentYear == null)
        {
            return NotFound("Current school year not found");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Create new school year
            var newYear = new SchoolYear
            {
                Name = dto.NewYearName,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = SchoolYearStatus.FutureYear,
                IsVisibleToPublic = false // Hide future year from public by default
            };

            _db.SchoolYears.Add(newYear);
            await _db.SaveChangesAsync();

            // TODO: Add year-end transition logic here
            // - Copy recurring event types
            // - Archive old year data
            // - Reset treasurer checklists
            // - etc.

            await transaction.CommitAsync();
            return CreatedAtAction(nameof(Get), new { id = newYear.Id }, newYear);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public class CreateSchoolYearDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    // Optionally: public SchoolYearStatus Status { get; set; }
    // Optionally: public bool IsVisibleToPublic { get; set; }
}

public class TransitionToNewYearDto
{
    public string NewYearName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool ArchiveCurrentYear { get; set; } = true;
    public bool CopyRecurringEvents { get; set; } = true;
}