using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxfordPTAWeb.Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class EventCatController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EventCatController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<EventCat>> Get()
    {
        return await _db.EventCats
            .OrderBy(et => et.DisplayOrder)
            .ThenBy(et => et.Name)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventCat>> Get(int id)
    {
        var eventCat = await _db.EventCats.FindAsync(id);
        if (eventCat == null)
        {
            return NotFound();
        }
        return eventCat;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventCat>> Post([FromBody] EventCat eventCat)
    {
        // Generate slug if not provided
        if (string.IsNullOrEmpty(eventCat.Slug))
            eventCat.Slug = GenerateSlug(eventCat.Name);

        _db.EventCats.Add(eventCat);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = eventCat.Id }, eventCat);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] EventCat updatedCat)
    {
        var eventCat = await _db.EventCats.FindAsync(id);
        if (eventCat == null)
        {
            return NotFound();
        }

        eventCat.Name = updatedCat.Name;
        eventCat.Slug = string.IsNullOrEmpty(updatedCat.Slug) ? GenerateSlug(updatedCat.Name) : updatedCat.Slug;
        eventCat.Description = updatedCat.Description;
        eventCat.DisplayOrder = updatedCat.DisplayOrder;
        eventCat.IsActive = updatedCat.IsActive;
        eventCat.Size = updatedCat.Size;
        eventCat.Icon = updatedCat.Icon;
        eventCat.ColorClass = updatedCat.ColorClass;
        eventCat.DisplayMode = updatedCat.DisplayMode;
        eventCat.MaxEventsToShow = updatedCat.MaxEventsToShow;
        eventCat.ShowViewEventsButton = updatedCat.ShowViewEventsButton;
        eventCat.ShowInlineOnMainPage = updatedCat.ShowInlineOnMainPage;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/move-up")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> MoveUp(int id)
    {
        var eventCat = await _db.EventCats.FindAsync(id);
        if (eventCat == null)
        {
            return NotFound();
        }

        var previousEventCat = await _db.EventCats
            .Where(et => et.DisplayOrder < eventCat.DisplayOrder)
            .OrderByDescending(et => et.DisplayOrder)
            .FirstOrDefaultAsync();

        if (previousEventCat != null)
        {
            var tempOrder = eventCat.DisplayOrder;
            eventCat.DisplayOrder = previousEventCat.DisplayOrder;
            previousEventCat.DisplayOrder = tempOrder;

            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPost("{id}/move-down")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> MoveDown(int id)
    {
        var eventCat = await _db.EventCats.FindAsync(id);
        if (eventCat == null)
        {
            return NotFound();
        }

        var nextEventCat = await _db.EventCats
            .Where(et => et.DisplayOrder > eventCat.DisplayOrder)
            .OrderBy(et => et.DisplayOrder)
            .FirstOrDefaultAsync();

        if (nextEventCat != null)
        {
            var tempOrder = eventCat.DisplayOrder;
            eventCat.DisplayOrder = nextEventCat.DisplayOrder;
            nextEventCat.DisplayOrder = tempOrder;

            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        var eventCat = await _db.EventCats.FindAsync(id);
        if (eventCat == null)
        {
            return NotFound();
        }

        var hasEvents = await _db.Events.AnyAsync(e => e.EventCatId == id);
        if (hasEvents)
        {
            return BadRequest("Cannot delete event category that is being used by events.");
        }

        _db.EventCats.Remove(eventCat);
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

