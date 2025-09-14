using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class SchoolYearsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    
    public SchoolYearsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<SchoolYear>> Get()
    {
        return await _db.SchoolYears.OrderByDescending(sy => sy.StartDate).ToListAsync();
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

    [HttpGet("{id}")]
    public async Task<ActionResult<SchoolYear>> Get(int id)
    {
        var schoolYear = await _db.SchoolYears.FindAsync(id);
        if (schoolYear == null)
        {
            return NotFound();
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

        var schoolYear = new SchoolYear
        {
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
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
                EndDate = dto.EndDate
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
}

public class TransitionToNewYearDto
{
    public string NewYearName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool ArchiveCurrentYear { get; set; } = true;
    public bool CopyRecurringEvents { get; set; } = true;
}