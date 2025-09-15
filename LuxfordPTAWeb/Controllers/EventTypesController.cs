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
        return await _db.EventTypes.OrderBy(et => et.Name).ToListAsync();
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
            Description = dto.Description,
            IsMandatory = dto.IsMandatory
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
        eventType.Description = dto.Description;
        eventType.IsMandatory = dto.IsMandatory;

        await _db.SaveChangesAsync();
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
}

public class CreateEventTypeDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Slug cannot be longer than 100 characters")]
    public string Slug { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    public string Description { get; set; } = string.Empty;
    
    public bool IsMandatory { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int Size { get; set; } = 1;
    public string Icon { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;


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

