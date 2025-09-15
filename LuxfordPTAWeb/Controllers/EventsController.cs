using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<EventsController> _logger;
    
    public EventsController(ApplicationDbContext db, ILogger<EventsController> logger) 
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> Get()
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Description = e.Description,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,
                    Link = e.Link,
                    EventType = e.EventType != null ? new EventTypeDto
                    {
                        Id = e.EventType.Id,
                        Name = e.EventType.Name,
                        Slug = e.EventType.Slug,
                        Description = e.EventType.Description,
                        IsMandatory = e.EventType.IsMandatory,
                        DisplayOrder = e.EventType.DisplayOrder,
                        IsActive = e.EventType.IsActive,
                        Size = (int)e.EventType.Size,
                        Icon = e.EventType.Icon,
                        ColorClass = e.EventType.ColorClass
                    } : null,
                    SchoolYear = e.SchoolYear != null ? new SchoolYearDto
                    {
                        Id = e.SchoolYear.Id,
                        Name = e.SchoolYear.Name,
                        StartDate = e.SchoolYear.StartDate,
                        EndDate = e.SchoolYear.EndDate
                    } : null
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all events");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetUpcoming()
    {
        try
        {
            var now = DateTime.UtcNow.Date;
            var in30Days = now.AddDays(30);

            var events = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Where(e => e.Date >= now && e.Date <= in30Days)
                .OrderBy(e => e.Date)
                .Take(5)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Description = e.Description,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,
                    Link = e.Link,
                    EventType = e.EventType != null ? new EventTypeDto
                    {
                        Id = e.EventType.Id,
                        Name = e.EventType.Name,
                        Slug = e.EventType.Slug,
                        Description = e.EventType.Description,
                        IsMandatory = e.EventType.IsMandatory,
                        DisplayOrder = e.EventType.DisplayOrder,
                        IsActive = e.EventType.IsActive,
                        Size = (int)e.EventType.Size,
                        Icon = e.EventType.Icon,
                        ColorClass = e.EventType.ColorClass
                    } : null,
                    SchoolYear = e.SchoolYear != null ? new SchoolYearDto
                    {
                        Id = e.SchoolYear.Id,
                        Name = e.SchoolYear.Name,
                        StartDate = e.SchoolYear.StartDate,
                        EndDate = e.SchoolYear.EndDate
                    } : null
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming events");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("by-type/{eventTypeId}")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetByEventType(int eventTypeId)
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Where(e => e.EventTypeId == eventTypeId)
                .OrderBy(e => e.Date)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Description = e.Description,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,
                    Link = e.Link,
                    EventType = e.EventType != null ? new EventTypeDto
                    {
                        Id = e.EventType.Id,
                        Name = e.EventType.Name,
                        Slug = e.EventType.Slug,
                        Description = e.EventType.Description,
                        IsMandatory = e.EventType.IsMandatory,
                        DisplayOrder = e.EventType.DisplayOrder,
                        IsActive = e.EventType.IsActive,
                        Size = (int)e.EventType.Size,
                        Icon = e.EventType.Icon,
                        ColorClass = e.EventType.ColorClass
                    } : null,
                    SchoolYear = e.SchoolYear != null ? new SchoolYearDto
                    {
                        Id = e.SchoolYear.Id,
                        Name = e.SchoolYear.Name,
                        StartDate = e.SchoolYear.StartDate,
                        EndDate = e.SchoolYear.EndDate
                    } : null
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events by type");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("by-category/{slug}")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetByEventTypeSlug(string slug)
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Where(e => e.EventType!.Slug == slug && e.EventType.IsActive)
                .OrderBy(e => e.Date)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Description = e.Description,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,
                    Link = e.Link,
                    EventType = e.EventType != null ? new EventTypeDto
                    {
                        Id = e.EventType.Id,
                        Name = e.EventType.Name,
                        Slug = e.EventType.Slug,
                        Description = e.EventType.Description,
                        IsMandatory = e.EventType.IsMandatory,
                        DisplayOrder = e.EventType.DisplayOrder,
                        IsActive = e.EventType.IsActive,
                        Size = (int)e.EventType.Size,
                        Icon = e.EventType.Icon,
                        ColorClass = e.EventType.ColorClass
                    } : null,
                    SchoolYear = e.SchoolYear != null ? new SchoolYearDto
                    {
                        Id = e.SchoolYear.Id,
                        Name = e.SchoolYear.Name,
                        StartDate = e.SchoolYear.StartDate,
                        EndDate = e.SchoolYear.EndDate
                    } : null
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events by category slug: {Slug}", slug);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var eventItem = await _db.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            _db.Events.Remove(eventItem);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event");
            return BadRequest(new { Error = ex.Message });
        }
    }
}

// DTO Classes
public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public EventTypeDto? EventType { get; set; }
    public SchoolYearDto? SchoolYear { get; set; }
}

public class EventTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int Size { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;
}

public class SchoolYearDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
