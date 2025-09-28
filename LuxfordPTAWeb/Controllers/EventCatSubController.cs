using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class EventCatSubController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EventCatSubController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<EventCatSub>> Get([FromQuery] int? eventCatId)
    {
        var query = _db.EventCatSubs
            .Include(s => s.EventCat)
            .OrderBy(s => s.EventCat.DisplayOrder)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name);

        if (eventCatId.HasValue)
        {
            query = (IOrderedQueryable<EventCatSub>)query.Where(s => s.EventCatId == eventCatId.Value);
        }

        return await query.ToListAsync();
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<IEnumerable<EventCatSub>> GetByCategory(int categoryId)
    {
        return await _db.EventCatSubs
            .Include(s => s.EventCat)
            .Where(s => s.EventCatId == categoryId)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventCatSub>> Get(int id)
    {
        var eventCatSub = await _db.EventCatSubs
            .Include(s => s.EventCat)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (eventCatSub == null)
        {
            return NotFound();
        }
        return eventCatSub;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventCatSubCreateDTO dto)
    {
        var eventCatSub = new EventCatSub
        {
            EventCatId = dto.EventCatId,
            Name = dto.Name,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
            // Do not set EventCat here; let EF Core handle the relationship via EventCatId
        };

        // Add to DB and save
        _db.EventCatSubs.Add(eventCatSub);
        await _db.SaveChangesAsync();

        return Ok(eventCatSub);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] EventCatSubCreateDTO dto)
    {
        var eventCatSub = await _db.EventCatSubs.FindAsync(id);
        if (eventCatSub == null)
        {
            return NotFound();
        }

        eventCatSub.Name = dto.Name;
        eventCatSub.Description = dto.Description;
        eventCatSub.DisplayOrder = dto.DisplayOrder;
        eventCatSub.IsActive = dto.IsActive;
        eventCatSub.EventCatId = dto.EventCatId;
        // Only set Icon and ColorClass if present in DTO
        if (!string.IsNullOrWhiteSpace(dto.Icon))
            eventCatSub.Icon = dto.Icon;
        if (!string.IsNullOrWhiteSpace(dto.ColorClass))
            eventCatSub.ColorClass = dto.ColorClass;

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

    [HttpPost("{id}/move-up")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> MoveUp(int id)
    {
        var eventCatSub = await _db.EventCatSubs.FindAsync(id);
        if (eventCatSub == null)
        {
            return NotFound();
        }

        var previousEventCatSub = await _db.EventCatSubs
            .Where(ecs => ecs.EventCatId == eventCatSub.EventCatId && ecs.DisplayOrder < eventCatSub.DisplayOrder)
            .OrderByDescending(ecs => ecs.DisplayOrder)
            .FirstOrDefaultAsync();

        if (previousEventCatSub != null)
        {
            var tempOrder = eventCatSub.DisplayOrder;
            eventCatSub.DisplayOrder = previousEventCatSub.DisplayOrder;
            previousEventCatSub.DisplayOrder = tempOrder;

            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPost("{id}/move-down")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> MoveDown(int id)
    {
        var eventCatSub = await _db.EventCatSubs.FindAsync(id);
        if (eventCatSub == null)
        {
            return NotFound();
        }

        var nextEventCatSub = await _db.EventCatSubs
            .Where(ecs => ecs.EventCatId == eventCatSub.EventCatId && ecs.DisplayOrder > eventCatSub.DisplayOrder)
            .OrderBy(ecs => ecs.DisplayOrder)
            .FirstOrDefaultAsync();

        if (nextEventCatSub != null)
        {
            var tempOrder = eventCatSub.DisplayOrder;
            eventCatSub.DisplayOrder = nextEventCatSub.DisplayOrder;
            nextEventCatSub.DisplayOrder = tempOrder;

            await _db.SaveChangesAsync();
        }

        return NoContent();
    }
}