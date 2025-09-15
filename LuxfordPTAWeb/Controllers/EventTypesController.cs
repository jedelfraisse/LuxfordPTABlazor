using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class EventTypesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    
    public EventTypesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<EventType>> Get()
    {
        return await _db.EventTypes.OrderBy(et => et.DisplayOrder).ThenBy(et => et.Name).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventType>> Get(int id)
    {
        var eventType = await _db.EventTypes.FindAsync(id);
        if (eventType == null)
        {
            return NotFound();
        }
        return eventType;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventType>> Post([FromBody] CreateEventTypeDto dto)
    {
        var eventType = new EventType
        {
            Name = dto.Name,
            Slug = !string.IsNullOrEmpty(dto.Slug) ? dto.Slug : GenerateSlug(dto.Name),
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            Size = (EventTypeSize)dto.Size,
            Icon = dto.Icon,
            ColorClass = dto.ColorClass,
            DisplayMode = EventDisplayMode.ShowCategory, // Default value
            MaxEventsToShow = 0,
            ShowViewEventsButton = true,
            ShowInlineOnMainPage = false
        };

        _db.EventTypes.Add(eventType);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = eventType.Id }, eventType);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] CreateEventTypeDto dto)
    {
        var eventType = await _db.EventTypes.FindAsync(id);
        if (eventType == null)
        {
            return NotFound();
        }

        eventType.Name = dto.Name;
        eventType.Slug = !string.IsNullOrEmpty(dto.Slug) ? dto.Slug : GenerateSlug(dto.Name);
        eventType.Description = dto.Description;
        eventType.DisplayOrder = dto.DisplayOrder;
        eventType.IsActive = dto.IsActive;
        eventType.Size = (EventTypeSize)dto.Size;
        eventType.Icon = dto.Icon;
        eventType.ColorClass = dto.ColorClass;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/move-up")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> MoveUp(int id)
    {
        var eventType = await _db.EventTypes.FindAsync(id);
        if (eventType == null)
        {
            return NotFound();
        }

        // Find the event type with the next lower display order
        var previousEventType = await _db.EventTypes
            .Where(et => et.DisplayOrder < eventType.DisplayOrder)
            .OrderByDescending(et => et.DisplayOrder)
            .FirstOrDefaultAsync();

        if (previousEventType != null)
        {
            // Swap display orders
            var tempOrder = eventType.DisplayOrder;
            eventType.DisplayOrder = previousEventType.DisplayOrder;
            previousEventType.DisplayOrder = tempOrder;

            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPost("{id}/move-down")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> MoveDown(int id)
    {
        var eventType = await _db.EventTypes.FindAsync(id);
        if (eventType == null)
        {
            return NotFound();
        }

        // Find the event type with the next higher display order
        var nextEventType = await _db.EventTypes
            .Where(et => et.DisplayOrder > eventType.DisplayOrder)
            .OrderBy(et => et.DisplayOrder)
            .FirstOrDefaultAsync();

        if (nextEventType != null)
        {
            // Swap display orders
            var tempOrder = eventType.DisplayOrder;
            eventType.DisplayOrder = nextEventType.DisplayOrder;
            nextEventType.DisplayOrder = tempOrder;

            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        var eventType = await _db.EventTypes.FindAsync(id);
        if (eventType == null)
        {
            return NotFound();
        }

        // Check if there are any events using this event type
        var hasEvents = await _db.Events.AnyAsync(e => e.EventTypeId == id);
        if (hasEvents)
        {
            return BadRequest("Cannot delete event type that is being used by events.");
        }

        _db.EventTypes.Remove(eventType);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Helper method to generate slug from name
    private string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        return name.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("&", "and")
                   .Replace("/", "-")
                   .Replace("'", "")
                   .Replace("\"", "")
                   .Where(c => char.IsLetterOrDigit(c) || c == '-')
                   .Aggregate("", (current, c) => current + c);
    }
}

public class CreateEventTypeDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Slug is required")]
    [StringLength(100, ErrorMessage = "Slug cannot be longer than 100 characters")]
    public string Slug { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, 999, ErrorMessage = "Display order must be between 0 and 999")]
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    [Range(0, 1, ErrorMessage = "Size must be Half (0) or Full (1)")]
    public int Size { get; set; } = 1; // Default to Full
    
    [StringLength(50, ErrorMessage = "Icon class cannot be longer than 50 characters")]
    public string Icon { get; set; } = string.Empty;
    
    [StringLength(30, ErrorMessage = "Color class cannot be longer than 30 characters")]
    public string ColorClass { get; set; } = string.Empty;
}

