using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxfordPTAWeb.Shared.Models;

[ApiController]
[Route("api/[controller]")]
public class EventCatSubController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EventCatSubController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<EventCatSub>> Get()
    {
        return await _db.EventCatSubs
            .Include(s => s.EventType)
            .OrderBy(s => s.EventType.DisplayOrder)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<IEnumerable<EventCatSub>> GetByCategory(int categoryId)
    {
        return await _db.EventCatSubs
            .Include(s => s.EventType)
            .Where(s => s.EventTypeId == categoryId)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventCatSub>> Get(int id)
    {
        var eventCatSub = await _db.EventCatSubs
            .Include(s => s.EventType)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (eventCatSub == null)
        {
            return NotFound();
        }
        return eventCatSub;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventCatSub>> Post([FromBody] EventCatSub eventCatSub)
    {
        // Generate slug if not provided
        if (string.IsNullOrEmpty(eventCatSub.Slug))
            eventCatSub.Slug = EventCatSub.GenerateSlug(eventCatSub.Name);

        _db.EventCatSubs.Add(eventCatSub);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = eventCatSub.Id }, eventCatSub);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] EventCatSub updatedSub)
    {
        var eventCatSub = await _db.EventCatSubs.FindAsync(id);
        if (eventCatSub == null)
        {
            return NotFound();
        }

        eventCatSub.Name = updatedSub.Name;
        eventCatSub.Slug = string.IsNullOrEmpty(updatedSub.Slug) ? EventCatSub.GenerateSlug(updatedSub.Name) : updatedSub.Slug;
        eventCatSub.Description = updatedSub.Description;
        eventCatSub.DisplayOrder = updatedSub.DisplayOrder;
        eventCatSub.IsActive = updatedSub.IsActive;
        eventCatSub.Icon = updatedSub.Icon;
        eventCatSub.ColorClass = updatedSub.ColorClass;
        eventCatSub.EventTypeId = updatedSub.EventTypeId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        var eventCatSub = await _db.EventCatSubs.FindAsync(id);
        if (eventCatSub == null)
        {
            return NotFound();
        }

        var hasEvents = await _db.Events.AnyAsync(e => e.EventSubTypeId == id);
        if (hasEvents)
        {
            return BadRequest("Cannot delete event subcategory that is being used by events.");
        }

        _db.EventCatSubs.Remove(eventCatSub);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}